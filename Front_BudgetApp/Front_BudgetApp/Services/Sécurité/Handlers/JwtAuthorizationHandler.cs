using System.Net.Http.Headers;

namespace Front_BudgetApp.Services.Sécurité.Handlers;

public class JwtAuthorizationHandler : DelegatingHandler
{
    private readonly AuthStateService _authState;
    
    public JwtAuthorizationHandler(AuthStateService authState)
    {
        _authState = authState;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        // Récupérer le token
        var token = await _authState.GetAccessTokenAsync();
        
        // Ajouter le header Authorization si token présent
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }
        
        var response = await base.SendAsync(request, cancellationToken);
        
        // Si 401, déconnecter automatiquement
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            await _authState.LogoutAsync();
        }
        
        return response;
    }
}