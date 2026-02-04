using BudgetApp.Mobile.Models.Local;
using BudgetApp.Mobile.Services.Api;
using BudgetApp.Mobile.Services.Auth;
using BudgetApp.Mobile.Services.Offline;
using Entities.Contracts.Forms;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BudgetApp.Mobile.Services.Hybrid;

public class TransactionHybridService : ITransactionHybridService
{
    private readonly LocalDbContext _db;
    private readonly IConnectivityService _connectivity;
    private readonly IMobileTransactionService _api;
    private readonly ISyncService _sync;
    private readonly IMobileAuthStateService _authState;

    public TransactionHybridService(
        LocalDbContext db,
        IConnectivityService connectivity,
        IMobileTransactionService api,
        ISyncService sync,
        IMobileAuthStateService authState)
    {
        _db = db;
        _connectivity = connectivity;
        _api = api;
        _sync = sync;
        _authState = authState;
    }

    public async Task<Result<List<LocalTransaction>>> GetByMonthAsync(int year, int month)
    {
        try
        {
            var session = await _authState.GetSessionAsync();
            if (session is null)
            {
                return Result.Fail("Non authentifie");
            }

            await _db.InitializeDatabaseAsync();

            // Try to sync if online
            if (_connectivity.IsOnline)
            {
                await _sync.SyncTransactionsAsync(year, month);
            }

            // Return local data
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var transactions = await _db.Transactions
                .Include(t => t.Categorie)
                .Where(t => t.UserId == session.UserId &&
                           t.Date >= startDate &&
                           t.Date < endDate &&
                           t.SyncState != SyncState.PendingDelete)
                .OrderByDescending(t => t.Date)
                .ThenByDescending(t => t.Id)
                .ToListAsync();

            return Result.Ok(transactions);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting transactions");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<LocalTransaction>> CreateAsync(TransactionVariableForm form)
    {
        try
        {
            var session = await _authState.GetSessionAsync();
            if (session is null)
            {
                return Result.Fail("Non authentifie");
            }

            await _db.InitializeDatabaseAsync();

            var transaction = new LocalTransaction
            {
                LocalId = Guid.NewGuid().ToString(),
                Intitule = form.Intitule,
                Montant = form.Montant,
                Date = form.Date,
                TransactionType = form.TransactionType,
                CategorieId = form.CategorieId,
                UserId = session.UserId,
                SyncState = _connectivity.IsOnline ? SyncState.Synced : SyncState.PendingCreate,
                LastModified = DateTime.UtcNow
            };

            if (_connectivity.IsOnline)
            {
                // Create on server first
                var result = await _api.CreateAsync(form);
                if (result.IsSuccess)
                {
                    transaction.ServerId = result.Value.Id;
                    transaction.ServerLastModified = DateTime.UtcNow;
                }
                else
                {
                    // Failed to create on server, mark as pending
                    transaction.SyncState = SyncState.PendingCreate;
                }
            }

            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();

            return Result.Ok(transaction);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating transaction");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(int localId, TransactionVariableForm form)
    {
        try
        {
            await _db.InitializeDatabaseAsync();

            var transaction = await _db.Transactions.FindAsync(localId);
            if (transaction is null)
            {
                return Result.Fail("Transaction non trouvee");
            }

            transaction.Intitule = form.Intitule;
            transaction.Montant = form.Montant;
            transaction.Date = form.Date;
            transaction.TransactionType = form.TransactionType;
            transaction.CategorieId = form.CategorieId;
            transaction.LastModified = DateTime.UtcNow;

            if (_connectivity.IsOnline && transaction.ServerId.HasValue)
            {
                // Update on server
                form.Id = transaction.ServerId.Value;
                var result = await _api.UpdateAsync(form);
                if (result.IsSuccess)
                {
                    transaction.SyncState = SyncState.Synced;
                    transaction.ServerLastModified = DateTime.UtcNow;
                }
                else
                {
                    transaction.SyncState = SyncState.PendingUpdate;
                }
            }
            else
            {
                // Mark as pending update (unless it's still pending create)
                if (transaction.SyncState == SyncState.Synced)
                {
                    transaction.SyncState = SyncState.PendingUpdate;
                }
            }

            await _db.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating transaction");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int localId)
    {
        try
        {
            await _db.InitializeDatabaseAsync();

            var transaction = await _db.Transactions.FindAsync(localId);
            if (transaction is null)
            {
                return Result.Fail("Transaction non trouvee");
            }

            if (_connectivity.IsOnline && transaction.ServerId.HasValue)
            {
                // Delete on server
                var result = await _api.DeleteAsync(transaction.ServerId.Value);
                if (result.IsSuccess)
                {
                    _db.Transactions.Remove(transaction);
                }
                else
                {
                    // Failed to delete on server, mark as pending
                    transaction.SyncState = SyncState.PendingDelete;
                }
            }
            else if (transaction.SyncState == SyncState.PendingCreate)
            {
                // Never synced, just remove locally
                _db.Transactions.Remove(transaction);
            }
            else
            {
                // Mark as pending delete
                transaction.SyncState = SyncState.PendingDelete;
            }

            await _db.SaveChangesAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting transaction");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }
}
