using BudgetApp.Shared.Interfaces.Http;
using Entities.Dtos;
using Entities.Forms;
using Serilog;

namespace Front_BudgetApp.Services;

public class CategorieFrontService(IHttpClientFactory factory) : IHttpCategorie
{
    public async Task<IEnumerable<CategorieDto>> GetCategories()
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.GetAsync("categorie");

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Échec de la récupération des catégories. Statut: {StatusCode}, Réponse: {Content}", response.StatusCode, content);
                return [];
            }
            var json = await response.Content.ReadAsStringAsync();
            var categories = await response.Content.ReadFromJsonAsync<IEnumerable<CategorieDto>>();
            Log.Information("Catégories récupérées avec succès.");
            return categories ?? [];
        }
        catch (Exception e)
        {
            Log.Error(e, "Erreur inattendue lors de la récupération des catégories.");
            return [];
        }
    }

    public async Task<bool> Add(CategorieForm categorieForm)
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.PostAsJsonAsync("categorie", categorieForm);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Échec de l'ajout de la catégorie. Statut: {StatusCode}, Réponse: {Content}, Données: {@CategorieForm}",
                            response.StatusCode, content, categorieForm);
                return false;
            }

            Log.Information("Catégorie ajoutée avec succès : {@CategorieForm}", categorieForm);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Erreur inattendue lors de l'ajout de la catégorie : {@CategorieForm}", categorieForm);
            return false;
        }
    }

    public async Task<bool> Update(int id, CategorieForm categorieForm)
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.PutAsJsonAsync($"categorie/{id}", categorieForm);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Échec de la mise à jour de la catégorie. Statut: {StatusCode}, Réponse: {Content}, Id: {Id}, Données: {@CategorieForm}",
                            response.StatusCode, content, id, categorieForm);
                return false;
            }

            Log.Information("Catégorie mise à jour avec succès : Id={Id}, {@CategorieForm}", id, categorieForm);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Erreur inattendue lors de la mise à jour de la catégorie Id={Id} : {@CategorieForm}", id, categorieForm);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var client = factory.CreateClient("Api");
            var response = await client.DeleteAsync($"categorie/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Échec de la suppression de la catégorie. Statut: {StatusCode}, Réponse: {Content}, Id: {Id}",
                            response.StatusCode, content, id);
                return false;
            }

            Log.Information("Catégorie supprimée avec succès (Id={Id})", id);
            return true;
        }
        catch (Exception e)
        {
            Log.Error(e, "Erreur inattendue lors de la suppression de la catégorie Id={Id}", id);
            return false;
        }
    }
}
