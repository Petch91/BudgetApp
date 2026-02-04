using BlazorBootstrap;
using BudgetApp.Mobile.Services.Api;
using BudgetApp.Mobile.Services.Auth;
using BudgetApp.Mobile.Services.Hybrid;
using BudgetApp.Mobile.Services.Offline;
using Microsoft.Extensions.Logging;
using Serilog;

namespace BudgetApp.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Debug()
            .CreateLogger();

        builder.Logging.AddSerilog();

        // Blazor Bootstrap
        builder.Services.AddBlazorBootstrap();

        // SQLite Database
        builder.Services.AddSingleton<LocalDbContext>();

        // Connectivity
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();

        // Auth Services
        builder.Services.AddSingleton<IMobileAuthStateService, MobileAuthStateService>();
        builder.Services.AddSingleton<IBiometricService, BiometricService>();

        // API Configuration
        var apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android
            ? "http://10.0.2.2:5201" // Android emulator localhost
            : "http://localhost:5201"; // iOS simulator

#if !DEBUG
        apiBaseUrl = "https://budget.yourdomain.com"; // Production URL
#endif

        builder.Services.AddHttpClient("Api", client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // API Services
        builder.Services.AddScoped<IMobileTransactionService, MobileTransactionService>();
        builder.Services.AddScoped<IMobileRapportService, MobileRapportService>();
        builder.Services.AddScoped<IMobileCategorieService, MobileCategorieService>();

        // Sync Service
        builder.Services.AddScoped<ISyncService, SyncService>();

        // Hybrid Services (orchestrate online/offline)
        builder.Services.AddScoped<ITransactionHybridService, TransactionHybridService>();
        builder.Services.AddScoped<IRapportHybridService, RapportHybridService>();

        return builder.Build();
    }
}
