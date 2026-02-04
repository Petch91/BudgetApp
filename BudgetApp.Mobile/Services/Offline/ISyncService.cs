using FluentResults;

namespace BudgetApp.Mobile.Services.Offline;

public interface ISyncService
{
    bool IsSyncing { get; }
    event Action<bool>? OnSyncStateChanged;

    Task<Result> SyncAllAsync();
    Task<Result> SyncTransactionsAsync(int year, int month);
    Task<Result> SyncCategoriesAsync();
    Task<Result> PushPendingChangesAsync();
}
