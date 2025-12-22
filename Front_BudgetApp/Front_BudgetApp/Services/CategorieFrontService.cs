using System.Net.Http.Json;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;
using Serilog;

namespace Front_BudgetApp.Services;

public class CategorieFrontService(IHttpClientFactory factory) : IHttpCategorie
{
    private HttpClient Client => factory.CreateClient("Api");

    public async Task<Result<IReadOnlyList<CategorieDto>>> GetCategories()
    {
        try
        {
            var response = await Client.GetAsync("categorie");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur récupération catégories ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail("Impossible de récupérer les catégories");
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
            var response = await Client.PostAsJsonAsync("categorie", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur ajout catégorie ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail("Impossible d'ajouter la catégorie");
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
            var response = await Client.PutAsJsonAsync($"categorie/{id}", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur update catégorie {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail("Impossible de mettre à jour la catégorie");
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
            var response = await Client.DeleteAsync($"categorie/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur suppression catégorie {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail("Impossible de supprimer la catégorie");
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
