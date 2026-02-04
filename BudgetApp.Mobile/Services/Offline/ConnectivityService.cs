using Serilog;

namespace BudgetApp.Mobile.Services.Offline;

public class ConnectivityService : IConnectivityService, IDisposable
{
    public bool IsOnline => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public event Action<bool>? OnConnectivityChanged;

    public ConnectivityService()
    {
        Connectivity.Current.ConnectivityChanged += OnConnectivityChangedHandler;
    }

    private void OnConnectivityChangedHandler(object? sender, ConnectivityChangedEventArgs e)
    {
        var isOnline = e.NetworkAccess == NetworkAccess.Internet;
        Log.Information("Connectivity changed: {IsOnline}", isOnline);
        OnConnectivityChanged?.Invoke(isOnline);
    }

    public void Dispose()
    {
        Connectivity.Current.ConnectivityChanged -= OnConnectivityChangedHandler;
    }
}
