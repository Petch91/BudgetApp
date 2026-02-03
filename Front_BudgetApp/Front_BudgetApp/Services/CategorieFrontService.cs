using System.Net;
using System.Net.Http.Json;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;
using Front_BudgetApp.Services.Sécurité;
using Serilog;

namespace Front_BudgetApp.Services;

public class CategorieFrontService(IHttpClientFactory factory, AuthStateService authState) : IHttpCategorie
{
    private async Task<HttpClient> GetClientAsync()
    {
        var client = factory.CreateClient("Api");
        var token = await authState.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    public async Task<Result<IReadOnlyList<CategorieDto>>> GetCategories()
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.GetAsync("categorie");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await authState.ForceLogoutAsync();
                return Result.Fail("Session expirée");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur récupération catégories ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail(HttpErrorHelper.GetUserMessage(response, "Récupération des catégories"));
            }

            var categories = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<CategorieDto>>();

            return Result.Ok(categories ?? []);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la récupération des catégories");
            return Result.Fail("Erreur technique lors de la récupération des catégories");
        }
    }

    public async Task<Result<CategorieDto>> Add(CategorieForm form)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.PostAsJsonAsync("categorie", form);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await authState.ForceLogoutAsync();
                return Result.Fail("Session expirée");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur ajout catégorie ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail(HttpErrorHelper.GetUserMessage(response, "Ajout catégorie"));
            }

            var created = await response.Content.ReadFromJsonAsync<CategorieDto>();

            if (created is null)
                return Result.Fail("Réponse invalide du serveur");

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de l'ajout catégorie {@Form}", form);
            return Result.Fail("Erreur technique lors de l'ajout de la catégorie");
        }
    }

    public async Task<Result> Update(int id, CategorieForm form)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.PutAsJsonAsync($"categorie/{id}", form);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await authState.ForceLogoutAsync();
                return Result.Fail("Session expirée");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur update catégorie {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail(HttpErrorHelper.GetUserMessage(response, "Mise à jour catégorie"));
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la mise à jour catégorie {Id}", id);
            return Result.Fail("Erreur technique lors de la mise à jour de la catégorie");
        }
    }

    public async Task<Result> Delete(int id)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.DeleteAsync($"categorie/{id}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await authState.ForceLogoutAsync();
                return Result.Fail("Session expirée");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur suppression catégorie {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail(HttpErrorHelper.GetUserMessage(response, "Suppression catégorie"));
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la suppression catégorie {Id}", id);
            return Result.Fail("Erreur technique lors de la suppression de la catégorie");
        }
    }
}
