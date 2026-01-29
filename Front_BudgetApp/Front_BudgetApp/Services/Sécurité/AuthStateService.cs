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
    private static readonly TimeSpan RefreshThreshold = TimeSpan.FromMinutes(2);

    private AuthSession? _currentSession;
    private bool _isRefreshing;

    public event Action? OnAuthStateChanged;

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
            }
            catch
            {
                return null;
            }
        }

        if (_currentSession == null)
            return null;

        // Token expiré → tenter un refresh
        if (_currentSession.IsExpired)
        {
            var refreshed = await RefreshSessionAsync();
            if (!refreshed)
            {
                await LogoutAsync();
                return null;
            }
            return _currentSession;
        }

        // Token bientôt expiré → refresh proactif
        if (_currentSession.ExpiresAt - DateTime.UtcNow < RefreshThreshold)
        {
            _ = RefreshSessionAsync(); // fire-and-forget, on utilise le token actuel en attendant
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
        OnAuthStateChanged?.Invoke();
    }

    public async Task LogoutAsync()
    {
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