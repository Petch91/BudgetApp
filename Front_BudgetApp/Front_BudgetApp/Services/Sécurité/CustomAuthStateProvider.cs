using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Front_BudgetApp.Services.Sécurité;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthStateService _authStateService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime _jsRuntime;

    public const string AUTH_COOKIE = "budgetapp_auth";

    public CustomAuthStateProvider(
        AuthStateService authStateService,
        IHttpContextAccessor httpContextAccessor,
        IJSRuntime jsRuntime)
    {
        _authStateService = authStateService;
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
        _authStateService.OnAuthStateChanged += NotifyAuthStateChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Verifier le cookie (fonctionne pendant le prerendu)
        var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[AUTH_COOKIE];

        if (string.IsNullOrEmpty(cookie))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Pendant le prerendu, on fait confiance au cookie
        // Apres le rendu interactif, on verifie la vraie session
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

            // Session null ou invalide mais cookie existe = prerendu, faire confiance au cookie
            var tempIdentity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "User") },
                "BudgetAppAuth");
            return new AuthenticationState(new ClaimsPrincipal(tempIdentity));
        }
        catch
        {
            // Erreur (JS interop pas disponible pendant le prerendu, etc.)
            // On fait confiance au cookie temporairement
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "User") },
                "BudgetAppAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
    }

    public async Task SetAuthCookieAsync(bool authenticated)
    {
        try
        {
            if (authenticated)
            {
                await _jsRuntime.InvokeVoidAsync("eval",
                    $"document.cookie = '{AUTH_COOKIE}=true; path=/; max-age=604800; SameSite=Strict'");
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("eval",
                    $"document.cookie = '{AUTH_COOKIE}=; path=/; max-age=0; SameSite=Strict'");
            }
        }
        catch
        {
            // Ignorer si JS interop pas disponible
        }
    }

    public void NotifyAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
