using System.Net.Http.Json;
using System.Text.Json;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using FluentResults;
using Serilog;

namespace Front_BudgetApp.Services;

public class RapportFrontService(IHttpClientFactory factory) : IHttpRapport
{
    private HttpClient Client => factory.CreateClient("Api");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<Result<RapportMoisDto>> GetRapportMois(int annee, int mois)
    {
        try
        {
            var response = await Client.GetAsync($"rapport/{annee}/{mois}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur recuperation rapport {Mois}/{Annee} ({StatusCode}) : {Error}",
                    mois, annee, response.StatusCode, error);

                return Result.Fail("Impossible de recuperer le rapport");
            }

            var rapport = await response.Content.ReadFromJsonAsync<RapportMoisDto>(JsonOptions);

            if (rapport is null)
                return Result.Fail("Reponse invalide du serveur");

            return Result.Ok(rapport);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la recuperation du rapport {Mois}/{Annee}", mois, annee);
            return Result.Fail("Erreur technique lors de la recuperation du rapport");
        }
    }
}
