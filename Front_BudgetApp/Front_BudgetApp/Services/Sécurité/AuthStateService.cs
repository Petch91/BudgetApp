using System.Net.Http.Json;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models.Front;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Serilog;

namespace Front_BudgetApp.Services.Sécurité;

public class AuthStateService
{
    private readonly ProtectedLocalStorage _localStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string AUTH_KEY = "budgetapp_auth_session";
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(3);

    private AuthSession? _currentSession;
    private bool _isRefreshing;
    private CancellationTokenSource? _refreshTimerCts;

    public event Action? OnAuthStateChanged;
    public event Action? OnSessionExpired;

    public AuthStateService(ProtectedLocalStorage localStorage, IHttpClientFactory httpClientFactory)
    {
        _localStorage = localStorage;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<AuthSession?> GetSessionAsync()
    {
        if (_currentSession == null)
        {
            try
            {
                var result = await _localStorage.GetAsync<AuthSession>(AUTH_KEY);
                _currentSession = result.Success ? result.Value : null;

                // Premier chargement depuis storage → démarrer le timer
                if (_currentSession is { IsExpired: false })
                    ScheduleRefresh();
            }
            catch
            {
                return null;
            }
        }

        if (_currentSession == null)
            return null;

        // Token expiré → tenter un refresh synchrone
        if (_currentSession.IsExpired)
        {
            var refreshed = await RefreshSessionAsync();
            if (!refreshed)
            {
                await ForceLogoutAsync();
                return null;
            }
            return _currentSession;
        }

        return _currentSession;
    }

    public async Task SaveSessionAsync(AuthSession session)
    {
        _currentSession = session;
        try
        {
            await _localStorage.SetAsync(AUTH_KEY, session);
        }
        catch
        {
            // JS interop not available during static rendering
        }
        ScheduleRefresh();
        OnAuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
        CancelRefreshTimer();

        // Révoquer le refresh token côté serveur
        if (_currentSession?.RefreshToken != null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("Api");
                await client.PostAsJsonAsync("auth/logout",
                    new RefreshTokenForm(_currentSession.RefreshToken));
            }
            catch
            {
                // Best effort
            }
        }

        _currentSession = null;
        try
        {
            await _localStorage.DeleteAsync(AUTH_KEY);
        }
        catch
        {
            // JS interop not available during static rendering
        }
        OnAuthStateChanged?.Invoke();
    }

    /// <summary>
    /// Logout + signal OnSessionExpired pour redirection automatique.
    /// Appelé quand un 401 est reçu ou quand le refresh échoue.
    /// </summary>
    public async Task ForceLogoutAsync()
    {
        await LogoutAsync();
        OnSessionExpired?.Invoke();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var session = await GetSessionAsync();
        return session?.IsValid == true;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var session = await GetSessionAsync();
        return session?.AccessToken;
    }

    private void ScheduleRefresh()
    {
        CancelRefreshTimer();

        if (_currentSession == null)
            return;

        var delay = _currentSession.ExpiresAt - DateTime.UtcNow - RefreshMargin;
        if (delay <= TimeSpan.Zero)
            delay = TimeSpan.FromSeconds(5); // refresh ASAP si déjà proche

        _refreshTimerCts = new CancellationTokenSource();
        var token = _refreshTimerCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, token);
                if (token.IsCancellationRequested) return;

                Log.Information("Timer refresh déclenché (délai: {Delay})", delay);
                var success = await RefreshSessionAsync();
                if (!success)
                {
                    Log.Warning("Refresh proactif échoué → forceLogout");
                    await ForceLogoutAsync();
                }
            }
            catch (TaskCanceledException)
            {
                // Timer annulé (logout ou nouveau login)
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Erreur dans le timer de refresh");
            }
        }, token);
    }

    private void CancelRefreshTimer()
    {
        if (_refreshTimerCts != null)
        {
            _refreshTimerCts.Cancel();
            _refreshTimerCts.Dispose();
            _refreshTimerCts = null;
        }
    }

    private async Task<bool> RefreshSessionAsync()
    {
        if (_isRefreshing || _currentSession?.RefreshToken == null)
            return false;

        _isRefreshing = true;
        try
        {
            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("auth/refresh",
                new RefreshTokenForm(_currentSession.RefreshToken));

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Refresh token échoué ({StatusCode})", response.StatusCode);
                return false;
            }

            var dto = await response.Content.ReadFromJsonAsync<AuthenticatedUserDto>();
            if (dto == null)
                return false;

            var newSession = new AuthSession
            {
                AccessToken = dto.Token,
                RefreshToken = dto.RefreshToken,
                ExpiresAt = dto.ExpiresAt,
                UserId = dto.Id,
                Username = dto.Username,
                Email = dto.Email
            };

            await SaveSessionAsync(newSession);
            Log.Information("Session rafraîchie avec succès");
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur lors du refresh de la session");
            return false;
        }
        finally
        {
            _isRefreshing = false;
        }
    }
}
