using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using Entities.Contracts.Forms;
using Serilog;

namespace BudgetApp.Mobile.Services.Auth;

public class MobileAuthStateService : IMobileAuthStateService
{
    private const string AccessTokenKey = "budgetapp_access_token";
    private const string RefreshTokenKey = "budgetapp_refresh_token";
    private const string UserIdKey = "budgetapp_user_id";
    private const string UserEmailKey = "budgetapp_user_email";
    private const string ExpiresAtKey = "budgetapp_expires_at";
    private const string BiometricEnabledKey = "budgetapp_biometric";

    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(3);

    private readonly IHttpClientFactory _httpClientFactory;
    private AuthSession? _cachedSession;
    private CancellationTokenSource? _refreshCts;

    public event Action? OnAuthStateChanged;
    public event Action? OnSessionExpired;

    public MobileAuthStateService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var session = await GetSessionAsync();
        return session is not null && session.ExpiresAt > DateTime.UtcNow;
    }

    public async Task<AuthSession?> GetSessionAsync()
    {
        if (_cachedSession is not null && _cachedSession.ExpiresAt > DateTime.UtcNow)
        {
            return _cachedSession;
        }

        try
        {
            var accessToken = await SecureStorage.Default.GetAsync(AccessTokenKey);
            if (string.IsNullOrEmpty(accessToken))
            {
                return null;
            }

            var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
            var userIdStr = await SecureStorage.Default.GetAsync(UserIdKey);
            var userEmail = await SecureStorage.Default.GetAsync(UserEmailKey);
            var expiresAtStr = await SecureStorage.Default.GetAsync(ExpiresAtKey);

            if (string.IsNullOrEmpty(refreshToken) ||
                !int.TryParse(userIdStr, out var userId) ||
                !DateTime.TryParse(expiresAtStr, out var expiresAt))
            {
                return null;
            }

            _cachedSession = new AuthSession
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                UserId = userId,
                UserEmail = userEmail ?? string.Empty,
                ExpiresAt = expiresAt
            };

            // Schedule refresh if session is still valid
            if (_cachedSession.ExpiresAt > DateTime.UtcNow)
            {
                ScheduleRefresh();
            }

            return _cachedSession;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error reading session from SecureStorage");
            return null;
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var session = await GetSessionAsync();
        return session?.AccessToken;
    }

    public async Task SaveSessionAsync(AuthSession session)
    {
        try
        {
            await SecureStorage.Default.SetAsync(AccessTokenKey, session.AccessToken);
            await SecureStorage.Default.SetAsync(RefreshTokenKey, session.RefreshToken);
            await SecureStorage.Default.SetAsync(UserIdKey, session.UserId.ToString());
            await SecureStorage.Default.SetAsync(UserEmailKey, session.UserEmail);
            await SecureStorage.Default.SetAsync(ExpiresAtKey, session.ExpiresAt.ToString("O"));

            _cachedSession = session;

            ScheduleRefresh();
            OnAuthStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error saving session to SecureStorage");
            throw;
        }
    }

    public async Task<bool> RefreshSessionAsync()
    {
        try
        {
            var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
            if (string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var client = _httpClientFactory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenForm
            {
                RefreshToken = refreshToken
            });

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Token refresh failed with status {StatusCode}", response.StatusCode);
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            var newAccessToken = root.GetProperty("accessToken").GetString()!;
            var newRefreshToken = root.GetProperty("refreshToken").GetString()!;

            // Parse JWT to get expiration
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(newAccessToken);
            var expiresAt = jwt.ValidTo;

            var session = new AuthSession
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserId = _cachedSession?.UserId ?? 0,
                UserEmail = _cachedSession?.UserEmail ?? string.Empty,
                ExpiresAt = expiresAt
            };

            await SaveSessionAsync(session);
            Log.Information("Token refreshed successfully, expires at {ExpiresAt}", expiresAt);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error refreshing token");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        CancelRefreshTimer();

        try
        {
            // Call server logout to invalidate refresh token
            var refreshToken = await SecureStorage.Default.GetAsync(RefreshTokenKey);
            if (!string.IsNullOrEmpty(refreshToken))
            {
                var client = _httpClientFactory.CreateClient("Api");
                await client.PostAsJsonAsync("/api/auth/logout", new RefreshTokenForm
                {
                    RefreshToken = refreshToken
                });
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Error calling server logout");
        }

        SecureStorage.Default.RemoveAll();
        _cachedSession = null;

        OnAuthStateChanged?.Invoke();
    }

    public async Task ForceLogoutAsync()
    {
        await LogoutAsync();
        OnSessionExpired?.Invoke();
    }

    public async Task<bool> IsBiometricEnabledAsync()
    {
        var value = await SecureStorage.Default.GetAsync(BiometricEnabledKey);
        return value == "true";
    }

    public async Task SetBiometricEnabledAsync(bool enabled)
    {
        await SecureStorage.Default.SetAsync(BiometricEnabledKey, enabled ? "true" : "false");
    }

    private void ScheduleRefresh()
    {
        CancelRefreshTimer();

        if (_cachedSession is null)
        {
            return;
        }

        var delay = _cachedSession.ExpiresAt - DateTime.UtcNow - RefreshMargin;
        if (delay <= TimeSpan.Zero)
        {
            // Token is about to expire or already expired, refresh immediately
            _ = Task.Run(async () => await RefreshSessionAsync());
            return;
        }

        _refreshCts = new CancellationTokenSource();
        var token = _refreshCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay, token);
                if (!token.IsCancellationRequested)
                {
                    var success = await RefreshSessionAsync();
                    if (!success)
                    {
                        await ForceLogoutAsync();
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Expected when logout is called
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in scheduled token refresh");
            }
        }, token);

        Log.Debug("Scheduled token refresh in {Delay}", delay);
    }

    private void CancelRefreshTimer()
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();
        _refreshCts = null;
    }
}
