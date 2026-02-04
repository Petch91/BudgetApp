using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BudgetApp.Mobile.Services.Auth;
using Entities.Contracts.Dtos;
using FluentResults;
using Serilog;

namespace BudgetApp.Mobile.Services.Api;

public class MobileCategorieService : IMobileCategorieService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMobileAuthStateService _authState;

    public MobileCategorieService(
        IHttpClientFactory httpClientFactory,
        IMobileAuthStateService authState)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
    }

    private async Task<HttpClient> GetClientAsync()
    {
        var client = _httpClientFactory.CreateClient("Api");
        var token = await _authState.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    public async Task<Result<List<CategorieDto>>> GetAllAsync()
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync("/api/categorie");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authState.ForceLogoutAsync();
                return Result.Fail("Session expiree");
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail($"Erreur serveur: {response.StatusCode}");
            }

            var categories = await response.Content.ReadFromJsonAsync<List<CategorieDto>>();
            return Result.Ok(categories ?? []);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching categories");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }
}
