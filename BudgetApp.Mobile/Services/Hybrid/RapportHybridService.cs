using BudgetApp.Mobile.Models.Local;
using BudgetApp.Mobile.Services.Auth;
using BudgetApp.Mobile.Services.Offline;
using Entities.Domain.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BudgetApp.Mobile.Services.Hybrid;

public class RapportHybridService : IRapportHybridService
{
    private readonly LocalDbContext _db;
    private readonly IConnectivityService _connectivity;
    private readonly ISyncService _sync;
    private readonly IMobileAuthStateService _authState;

    public RapportHybridService(
        LocalDbContext db,
        IConnectivityService connectivity,
        ISyncService sync,
        IMobileAuthStateService authState)
    {
        _db = db;
        _connectivity = connectivity;
        _sync = sync;
        _authState = authState;
    }

    public async Task<Result<MobileRapportDto>> GetRapportAsync(int year, int month)
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
            var isOffline = !_connectivity.IsOnline;
            if (!isOffline)
            {
                var syncResult = await _sync.SyncAllAsync();
                if (syncResult.IsFailed)
                {
                    isOffline = true;
                }
            }

            // Get local data
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

            var rapport = new MobileRapportDto
            {
                Year = year,
                Month = month,
                IsOffline = isOffline,
                TotalRevenus = transactions
                    .Where(t => t.TransactionType == TransactionType.Revenu)
                    .Sum(t => t.Montant),
                TotalDepenses = transactions
                    .Where(t => t.TransactionType == TransactionType.Depense)
                    .Sum(t => t.Montant),
                Transactions = transactions.Select(t => new MobileTransactionDto
                {
                    LocalId = t.Id,
                    ServerId = t.ServerId,
                    Intitule = t.Intitule,
                    Montant = t.Montant,
                    Date = t.Date,
                    TransactionType = t.TransactionType,
                    CategorieName = t.Categorie?.Name,
                    IsPending = t.SyncState != SyncState.Synced
                }).ToList()
            };

            return Result.Ok(rapport);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting rapport");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }
}
