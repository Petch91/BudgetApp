using System.Net.Http.Json;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;
using Front_BudgetApp.Services.Sécurité;
using Serilog;

namespace Front_BudgetApp.Services;

public class DepenseFixeFrontService(IHttpClientFactory factory, AuthStateService authState) : IHttpDepenseFixe
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


    /* =======================
     * GET ALL
     * ======================= */

    public async Task<Result<IReadOnlyList<DepenseFixeDto>>> GetDepenses()
    {
        try
        {
            var Client = await GetClientAsync();
            var response = await Client.GetAsync("depensefixe");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur récupération dépenses fixes ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail("Impossible de récupérer les dépenses fixes");
            }

            var depenses = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<DepenseFixeDto>>();

            return Result.Ok(depenses ?? []);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la récupération des dépenses fixes");
            return Result.Fail("Erreur technique lors de la récupération des dépenses fixes");
        }
    }

    /* =======================
     * ADD
     * ======================= */

    public async Task<Result<DepenseFixeDto>> Add(DepenseFixeForm depenseForm)
    {
        try
        {
            var Client = await GetClientAsync();
            var response = await Client.PostAsJsonAsync("depensefixe", depenseForm);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur ajout dépense fixe ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail("Impossible d'ajouter la dépense fixe");
            }

            var created = await response.Content
                .ReadFromJsonAsync<DepenseFixeDto>();

            if (created is null)
                return Result.Fail("Réponse invalide du serveur");

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "Erreur inattendue lors de l'ajout de la dépense fixe {@Form}",
                depenseForm);

            return Result.Fail("Erreur technique lors de l'ajout de la dépense fixe");
        }
    }

    /* =======================
     * UPDATE
     * ======================= */

    public async Task<Result> Update(int id, DepenseFixeForm depenseForm)
    {
        try
        {
            var Client = await GetClientAsync();
            var response = await Client.PutAsJsonAsync($"depensefixe/{id}", depenseForm);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur update dépense fixe {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail("Impossible de mettre à jour la dépense fixe");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "Erreur inattendue lors de la mise à jour dépense fixe {Id}",
                id);

            return Result.Fail("Erreur technique lors de la mise à jour de la dépense fixe");
        }
    }

    /* =======================
     * DELETE
     * ======================= */

    public async Task<Result> Delete(int id)
    {
        try
        {
            var Client = await GetClientAsync();
            var response = await Client.DeleteAsync($"depensefixe/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur suppression dépense fixe {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail("Impossible de supprimer la dépense fixe");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "Erreur inattendue lors de la suppression dépense fixe {Id}",
                id);

            return Result.Fail("Erreur technique lors de la suppression de la dépense fixe");
        }
    }

    /* =======================
     * CHANGE RAPPEL VU
     * ======================= */

    public async Task<Result> ChangeVuRappel(int id)
    {
        try
        {
            var Client = await GetClientAsync();
            var response = await Client.PatchAsync($"depensefixe/rappels/{id}/vu", null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur PATCH rappel vu {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail("Impossible de modifier l'état du rappel");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "Erreur inattendue lors du PATCH rappel vu {Id}",
                id);

            return Result.Fail("Erreur technique lors de la modification du rappel");
        }
    }
}
