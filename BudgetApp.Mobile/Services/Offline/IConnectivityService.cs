namespace BudgetApp.Mobile.Services.Offline;

public interface IConnectivityService
{
    bool IsOnline { get; }
    event Action<bool>? OnConnectivityChanged;
}
