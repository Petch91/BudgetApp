using BudgetApp.Mobile.Models.Local;
using BudgetApp.Mobile.Services.Api;
using BudgetApp.Mobile.Services.Auth;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Interfaces;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BudgetApp.Mobile.Services.Offline;

public class SyncService : ISyncService
{
    private readonly LocalDbContext _db;
    private readonly IConnectivityService _connectivity;
    private readonly IMobileTransactionService _transactionApi;
    private readonly IMobileCategorieService _categorieApi;
    private readonly IMobileAuthStateService _authState;
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    private bool _isSyncing;
    public bool IsSyncing
    {
        get => _isSyncing;
        private set
        {
            if (_isSyncing != value)
            {
                _isSyncing = value;
                OnSyncStateChanged?.Invoke(value);
            }
        }
    }

    public event Action<bool>? OnSyncStateChanged;

    public SyncService(
        LocalDbContext db,
        IConnectivityService connectivity,
        IMobileTransactionService transactionApi,
        IMobileCategorieService categorieApi,
        IMobileAuthStateService authState)
    {
        _db = db;
        _connectivity = connectivity;
        _transactionApi = transactionApi;
        _categorieApi = categorieApi;
        _authState = authState;
    }

    public async Task<Result> SyncAllAsync()
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Fail("Pas de connexion internet");
        }

        if (!await _syncLock.WaitAsync(0))
        {
            return Result.Ok(); // Already syncing
        }

        try
        {
            IsSyncing = true;

            // Sync categories first (read-only)
            var catResult = await SyncCategoriesAsync();
            if (catResult.IsFailed)
            {
                return catResult;
            }

            // Push pending changes
            var pushResult = await PushPendingChangesAsync();
            if (pushResult.IsFailed)
            {
                Log.Warning("Some pending changes failed to sync");
            }

            // Pull current month transactions
            var now = DateTime.Today;
            var pullResult = await SyncTransactionsAsync(now.Year, now.Month);

            return pullResult;
        }
        finally
        {
            IsSyncing = false;
            _syncLock.Release();
        }
    }

    public async Task<Result> SyncTransactionsAsync(int year, int month)
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Fail("Pas de connexion internet");
        }

        try
        {
            var session = await _authState.GetSessionAsync();
            if (session is null)
            {
                return Result.Fail("Non authentifie");
            }

            // Get transactions from server for the given month
            var rapportResult = await _transactionApi.GetByMonthAsync(year, month);
            if (rapportResult.IsFailed)
            {
                return Result.Fail(rapportResult.Errors);
            }

            var serverTransactions = rapportResult.Value;

            // Get local transactions for this month
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var localTransactions = await _db.Transactions
                .Where(t => t.UserId == session.UserId &&
                           t.Date >= startDate &&
                           t.Date < endDate &&
                           t.SyncState == SyncState.Synced)
                .ToListAsync();

            // Sync from server to local (Last-Write-Wins)
            foreach (var serverTx in serverTransactions)
            {
                var localTx = localTransactions.FirstOrDefault(l => l.ServerId == serverTx.Id);

                if (localTx is null)
                {
                    // New transaction from server
                    _db.Transactions.Add(new LocalTransaction
                    {
                        ServerId = serverTx.Id,
                        LocalId = Guid.NewGuid().ToString(),
                        Intitule = serverTx.Intitule,
                        Montant = serverTx.Montant,
                        Date = serverTx.Date,
                        TransactionType = serverTx.TransactionType,
                        CategorieId = serverTx.CategorieId,
                        UserId = session.UserId,
                        SyncState = SyncState.Synced,
                        LastModified = DateTime.UtcNow,
                        ServerLastModified = DateTime.UtcNow
                    });
                }
                else
                {
                    // Update existing local transaction
                    localTx.Intitule = serverTx.Intitule;
                    localTx.Montant = serverTx.Montant;
                    localTx.Date = serverTx.Date;
                    localTx.TransactionType = serverTx.TransactionType;
                    localTx.CategorieId = serverTx.CategorieId;
                    localTx.ServerLastModified = DateTime.UtcNow;
                }
            }

            // Remove local transactions that no longer exist on server
            var serverIds = serverTransactions.Select(s => s.Id).ToHashSet();
            var toRemove = localTransactions.Where(l => l.ServerId.HasValue && !serverIds.Contains(l.ServerId.Value));
            _db.Transactions.RemoveRange(toRemove);

            await _db.SaveChangesAsync();

            // Update sync metadata
            await UpdateSyncMetadataAsync("Transaction", year, month);

            Log.Information("Synced {Count} transactions for {Year}/{Month}",
                serverTransactions.Count, year, month);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error syncing transactions");
            return Result.Fail($"Erreur de synchronisation: {ex.Message}");
        }
    }

    public async Task<Result> SyncCategoriesAsync()
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Fail("Pas de connexion internet");
        }

        try
        {
            var categoriesResult = await _categorieApi.GetAllAsync();
            if (categoriesResult.IsFailed)
            {
                return Result.Fail(categoriesResult.Errors);
            }

            var serverCategories = categoriesResult.Value;

            // Replace all local categories with server categories
            var localCategories = await _db.Categories.ToListAsync();
            _db.Categories.RemoveRange(localCategories);

            foreach (var cat in serverCategories)
            {
                _db.Categories.Add(new LocalCategorie
                {
                    Id = cat.Id,
                    Name = cat.Name,
                    Icon = cat.Icon
                });
            }

            await _db.SaveChangesAsync();

            Log.Information("Synced {Count} categories", serverCategories.Count);

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error syncing categories");
            return Result.Fail($"Erreur de synchronisation des categories: {ex.Message}");
        }
    }

    public async Task<Result> PushPendingChangesAsync()
    {
        if (!_connectivity.IsOnline)
        {
            return Result.Fail("Pas de connexion internet");
        }

        try
        {
            var pendingTransactions = await _db.Transactions
                .Where(t => t.SyncState != SyncState.Synced)
                .ToListAsync();

            var errors = new List<string>();

            foreach (var tx in pendingTransactions)
            {
                var result = tx.SyncState switch
                {
                    SyncState.PendingCreate => await PushCreateAsync(tx),
                    SyncState.PendingUpdate => await PushUpdateAsync(tx),
                    SyncState.PendingDelete => await PushDeleteAsync(tx),
                    _ => Result.Ok()
                };

                if (result.IsFailed)
                {
                    errors.Add($"{tx.Intitule}: {result.Errors.FirstOrDefault()?.Message}");
                }
            }

            await _db.SaveChangesAsync();

            if (errors.Count > 0)
            {
                Log.Warning("Some pending changes failed: {Errors}", string.Join(", ", errors));
                return Result.Fail(errors);
            }

            Log.Information("Pushed {Count} pending changes", pendingTransactions.Count);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error pushing pending changes");
            return Result.Fail($"Erreur d'envoi: {ex.Message}");
        }
    }

    private async Task<Result> PushCreateAsync(LocalTransaction tx)
    {
        var form = new TransactionVariableForm
        {
            Intitule = tx.Intitule,
            Montant = tx.Montant,
            Date = tx.Date,
            TransactionType = tx.TransactionType,
            CategorieId = tx.CategorieId
        };

        var result = await _transactionApi.CreateAsync(form);
        if (result.IsSuccess)
        {
            tx.ServerId = result.Value.Id;
            tx.SyncState = SyncState.Synced;
            tx.ServerLastModified = DateTime.UtcNow;
        }

        return result.ToResult();
    }

    private async Task<Result> PushUpdateAsync(LocalTransaction tx)
    {
        if (!tx.ServerId.HasValue)
        {
            return Result.Fail("No server ID for update");
        }

        var form = new TransactionVariableForm
        {
            Id = tx.ServerId.Value,
            Intitule = tx.Intitule,
            Montant = tx.Montant,
            Date = tx.Date,
            TransactionType = tx.TransactionType,
            CategorieId = tx.CategorieId
        };

        var result = await _transactionApi.UpdateAsync(form);
        if (result.IsSuccess)
        {
            tx.SyncState = SyncState.Synced;
            tx.ServerLastModified = DateTime.UtcNow;
        }

        return result;
    }

    private async Task<Result> PushDeleteAsync(LocalTransaction tx)
    {
        if (!tx.ServerId.HasValue)
        {
            // Never synced, just remove locally
            _db.Transactions.Remove(tx);
            return Result.Ok();
        }

        var result = await _transactionApi.DeleteAsync(tx.ServerId.Value);
        if (result.IsSuccess)
        {
            _db.Transactions.Remove(tx);
        }

        return result;
    }

    private async Task UpdateSyncMetadataAsync(string entityType, int year, int month)
    {
        var metadata = await _db.SyncMetadata.FindAsync(entityType);
        if (metadata is null)
        {
            metadata = new SyncMetadata { EntityType = entityType };
            _db.SyncMetadata.Add(metadata);
        }

        metadata.LastSyncTimestamp = DateTime.UtcNow;
        metadata.LastServerTimestamp = DateTime.UtcNow;
    }
}
