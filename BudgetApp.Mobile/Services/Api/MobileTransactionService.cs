using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BudgetApp.Mobile.Services.Auth;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;
using Serilog;

namespace BudgetApp.Mobile.Services.Api;

public class MobileTransactionService : IMobileTransactionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMobileAuthStateService _authState;

    public MobileTransactionService(
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

    public async Task<Result<List<TransactionVariableDto>>> GetByMonthAsync(int year, int month)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync($"/api/transaction?year={year}&month={month}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authState.ForceLogoutAsync();
                return Result.Fail("Session expiree");
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result.Fail($"Erreur serveur: {response.StatusCode}");
            }

            var transactions = await response.Content.ReadFromJsonAsync<List<TransactionVariableDto>>();
            return Result.Ok(transactions ?? []);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching transactions");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result<TransactionVariableDto>> CreateAsync(TransactionVariableForm form)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.PostAsJsonAsync("/api/transaction", form);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authState.ForceLogoutAsync();
                return Result.Fail("Session expiree");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result.Fail($"Erreur: {error}");
            }

            var transaction = await response.Content.ReadFromJsonAsync<TransactionVariableDto>();
            return Result.Ok(transaction!);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating transaction");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(TransactionVariableForm form)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.PutAsJsonAsync($"/api/transaction/{form.Id}", form);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authState.ForceLogoutAsync();
                return Result.Fail("Session expiree");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result.Fail($"Erreur: {error}");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating transaction");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(int id)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.DeleteAsync($"/api/transaction/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authState.ForceLogoutAsync();
                return Result.Fail("Session expiree");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return Result.Fail($"Erreur: {error}");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting transaction");
            return Result.Fail($"Erreur: {ex.Message}");
        }
    }
}
