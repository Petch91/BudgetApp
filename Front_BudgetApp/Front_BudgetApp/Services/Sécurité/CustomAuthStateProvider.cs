using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Front_BudgetApp.Services.Sécurité;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthStateService _authStateService;

    public CustomAuthStateProvider(AuthStateService authStateService)
    {
        _authStateService = authStateService;
        _authStateService.OnAuthStateChanged += NotifyAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var session = await _authStateService.GetSessionAsync();

            if (session?.IsValid == true)
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, session.Username ?? "User"),
                    new(ClaimTypes.Email, session.Email ?? ""),
                    new("UserId", session.UserId.ToString())
                };

                var identity = new ClaimsIdentity(claims, "BudgetAppAuth");
                return new AuthenticationState(new ClaimsPrincipal(identity));
            }
        }
        catch
        {
            // Erreur d'acces au ProtectedLocalStorage
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public void NotifyAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
