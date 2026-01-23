using Entities.Domain.Models.Front;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace Front_BudgetApp.Services.Sécurité;

public class AuthStateService
{
    private readonly ProtectedLocalStorage _localStorage;
    private const string AUTH_KEY = "budgetapp_auth_session";
    
    private AuthSession? _currentSession;
    
    public event Action? OnAuthStateChanged;
    
    public AuthStateService(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }
    
    public async Task<AuthSession?> GetSessionAsync()
    {
        if (_currentSession != null)
            return _currentSession;
            
        try
        {
            var result = await _localStorage.GetAsync<AuthSession>(AUTH_KEY);
            _currentSession = result.Success ? result.Value : null;
            
            // Vérifier si expiré
            if (_currentSession?.IsExpired == true)
            {
                await LogoutAsync();
                return null;
            }
            
            return _currentSession;
        }
        catch
        {
            return null;
        }
    }
    
    public async Task SaveSessionAsync(AuthSession session)
    {
        _currentSession = session;
        await _localStorage.SetAsync(AUTH_KEY, session);
        OnAuthStateChanged?.Invoke();
    }
    
    public async Task LogoutAsync()
    {
        _currentSession = null;
        await _localStorage.DeleteAsync(AUTH_KEY);
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
}