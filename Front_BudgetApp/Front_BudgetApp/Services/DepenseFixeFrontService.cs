using System.Text.Json;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Dtos;
using Entities.Forms;
using Serilog;

namespace Front_BudgetApp.Services;

public class DepenseFixeFrontService(IHttpClientFactory factory) : IHttpDepenseFixe
{
    public async Task<IEnumerable<DepenseFixeDto>> GetDepenses()
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.GetAsync("depenseFixe");

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Échec de la récupération des dépenses. Statut: {StatusCode}, Réponse: {Content}", response.StatusCode, content);
                return [];
            }
            var json = await response.Content.ReadAsStringAsync();
            var depenseFixes = await response.Content.ReadFromJsonAsync<IEnumerable<DepenseFixeDto>>();
            Log.Information("Dépenses récupérées avec succès.");
            return depenseFixes ?? [];
        }
        catch (Exception e)
        {
            Log.Error(e, "Erreur inattendue lors de la récupération des Dépenses.");
            return [];
        };
    }

    public async Task<DepenseFixeDto?> Add(DepenseFixeForm depenseForm)
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("depenseFixe", depenseForm);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Lecture possible du message d'erreur JSON
                string errorMessage;
                try
                {
                    var errorObj = JsonSerializer.Deserialize<Dictionary<string, string>>(content);
                    errorMessage = errorObj?["error"] + " | " + errorObj?["details"];
                }
                catch
                {
                    errorMessage = content;
                }

                Log.Warning("Échec de l'ajout de la depenseFixe. Statut: {StatusCode}, Erreur: {Error}, Données: {@DepenseFixeForm}",
                    response.StatusCode, errorMessage, depenseForm);
                return null;
            }

            var createdDto = await response.Content.ReadFromJsonAsync<DepenseFixeDto>();

            Log.Information("depense ajoutée avec succès : {@CreatedDto}", createdDto);
            return createdDto;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de l'ajout de la depenseFixe : {@CategorieForm}", depenseForm);
            return null;
        }
    }

    public async Task<bool> Update(int id, DepenseFixeForm depenseForm)
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.PutAsJsonAsync($"depenseFixe/{id}", depenseForm);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Échec de la mise à jour de la depense. Statut: {StatusCode}, Réponse: {Content}, Id: {Id}, Données: {@DepenseForm}",
                    response.StatusCode, content, id, depenseForm);
                return false;
            }

            Log.Information("depense mise à jour avec succès : Id={Id}, {@DepenseForm}", id, depenseForm);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Erreur inattendue lors de la mise à jour de la depense Id={Id} : {@DepenseForm}", id, depenseForm);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.DeleteAsync($"depensefixe/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Échec de la suppression de la dépense. Statut: {StatusCode}, Réponse: {Content}, Id: {Id}",
                    response.StatusCode, content, id);
                return false;
            }

            Log.Information("Dépense supprimée avec succès (Id={Id})", id);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Erreur inattendue lors de la suppression de la dépense Id={Id}", id);
            return false;
        }
    }
    
    public async Task<bool> ChangeVuRappel(int id)
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.PatchAsync($"rappels/{id}/vu", null);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Échec du PATCH /rappels/{Id}/vu - Status: {StatusCode}, Response: {Response}", id, response.StatusCode, content);
                return false;
            }

            Log.Information("PATCH /rappels/{Id}/vu effectué avec succès", id);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Erreur lors du PATCH /rappels/{Id}/vu", id);
            return false;
        }
    }
}