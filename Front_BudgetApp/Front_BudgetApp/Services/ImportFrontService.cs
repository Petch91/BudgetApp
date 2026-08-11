using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;
using Front_BudgetApp.Services.Sécurité;
using Serilog;

namespace Front_BudgetApp.Services;

public class ImportFrontService(IHttpClientFactory factory, AuthStateService authState) : IHttpImport
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task<HttpClient> GetClientAsync()
    {
        var client = factory.CreateClient("Api");
        var token = await authState.GetAccessTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        return client;
    }

    public async Task<Result<ImportPreviewDto>> Analyser(Stream fichier, string nomFichier)
    {
        try
        {
            var client = await GetClientAsync();

            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fichier);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            content.Add(fileContent, "fichier", nomFichier);

            var response = await client.PostAsync("import/analyze", content);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await authState.ForceLogoutAsync();
                return Result.Fail("Session expirée");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur analyse extrait ({StatusCode}) : {Error}", response.StatusCode, error);
                return Result.Fail(ExtraireMessage(error) ?? HttpErrorHelper.GetUserMessage(response, "Analyse de l'extrait"));
            }

            var preview = await response.Content.ReadFromJsonAsync<ImportPreviewDto>(JsonOptions);

            if (preview is null)
                return Result.Fail("Reponse invalide du serveur");

            return Result.Ok(preview);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de l'analyse de l'extrait {NomFichier}", nomFichier);
            return Result.Fail("Erreur technique lors de l'analyse de l'extrait");
        }
    }

    public async Task<Result<ImportResultDto>> Confirmer(ImportConfirmForm form)
    {
        try
        {
            var client = await GetClientAsync();
            var response = await client.PostAsJsonAsync("import/confirm", form);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await authState.ForceLogoutAsync();
                return Result.Fail("Session expirée");
            }

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning("Erreur import ({StatusCode}) : {Error}", response.StatusCode, error);
                return Result.Fail(ExtraireMessage(error) ?? HttpErrorHelper.GetUserMessage(response, "Import des transactions"));
            }

            var resultat = await response.Content.ReadFromJsonAsync<ImportResultDto>(JsonOptions);

            if (resultat is null)
                return Result.Fail("Reponse invalide du serveur");

            return Result.Ok(resultat);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de l'import des transactions");
            return Result.Fail("Erreur technique lors de l'import");
        }
    }

    /// <summary>L'API renvoie ses erreurs metier sous forme de tableau JSON de messages.</summary>
    private static string? ExtraireMessage(string corps)
    {
        if (string.IsNullOrWhiteSpace(corps))
            return null;

        try
        {
            var messages = JsonSerializer.Deserialize<List<string>>(corps, JsonOptions);
            return messages is { Count: > 0 } ? string.Join(" | ", messages) : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
