namespace BudgetApp.Mobile.Services.Auth;

public interface IMobileAuthStateService
{
    event Action? OnAuthStateChanged;
    event Action? OnSessionExpired;

    Task<bool> IsAuthenticatedAsync();
    Task<AuthSession?> GetSessionAsync();
    Task<string?> GetAccessTokenAsync();
    Task SaveSessionAsync(AuthSession session);
    Task<bool> RefreshSessionAsync();
    Task LogoutAsync();
    Task ForceLogoutAsync();

    Task<bool> IsBiometricEnabledAsync();
    Task SetBiometricEnabledAsync(bool enabled);
}

public class AuthSession
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
