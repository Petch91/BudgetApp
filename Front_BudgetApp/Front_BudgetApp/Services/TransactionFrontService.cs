using System.Net.Http.Json;
using BudgetApp.Shared.Interfaces.Http;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using FluentResults;
using Serilog;

namespace Front_BudgetApp.Services;

public class TransactionFrontService(IHttpClientFactory factory) : IHttpTransaction
{
    private HttpClient Client => factory.CreateClient("Api");

    public async Task<Result<IReadOnlyList<TransactionVariableDto>>> GetByMonth(int month, int year)
    {
        try
        {
            var revenusTask = GetRevenuesByMonth(month, year);
            var depensesTask = GetDepensesByMonth(month, year);

            await Task.WhenAll(revenusTask, depensesTask);

            var revenus = await revenusTask;
            var depenses = await depensesTask;

            if (revenus.IsFailed || depenses.IsFailed)
            {
                return Result.Fail("Impossible de recuperer les transactions");
            }

            var all = revenus.Value.Concat(depenses.Value)
                .OrderByDescending(t => t.Date)
                .ToList();

            return Result.Ok<IReadOnlyList<TransactionVariableDto>>(all);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur lors de la recuperation des transactions du mois {Month}/{Year}", month, year);
            return Result.Fail("Erreur technique lors de la recuperation des transactions");
        }
    }

    public async Task<Result<IReadOnlyList<TransactionVariableDto>>> GetRevenuesByMonth(int month, int year)
    {
        try
        {
            var response = await Client.GetAsync($"transaction/revenubymonth/{month}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur recuperation revenus ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail("Impossible de recuperer les revenus");
            }

            var revenus = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<TransactionVariableDto>>();

            // Filtrer par annee cote client car l'API ne le fait pas
            var filtered = (revenus ?? [])
                .Where(t => t.Date.Year == year)
                .ToList();

            return Result.Ok<IReadOnlyList<TransactionVariableDto>>(filtered);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la recuperation des revenus");
            return Result.Fail("Erreur technique lors de la recuperation des revenus");
        }
    }

    public async Task<Result<IReadOnlyList<TransactionVariableDto>>> GetDepensesByMonth(int month, int year)
    {
        try
        {
            var response = await Client.GetAsync($"transaction/depensebymonth/{month}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur recuperation depenses ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail("Impossible de recuperer les depenses");
            }

            var depenses = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<TransactionVariableDto>>();

            // Filtrer par annee cote client car l'API ne le fait pas
            var filtered = (depenses ?? [])
                .Where(t => t.Date.Year == year)
                .ToList();

            return Result.Ok<IReadOnlyList<TransactionVariableDto>>(filtered);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la recuperation des depenses");
            return Result.Fail("Erreur technique lors de la recuperation des depenses");
        }
    }

    public async Task<Result<TransactionVariableDto>> Add(TransactionVariableForm form)
    {
        try
        {
            var response = await Client.PostAsJsonAsync("transaction", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur ajout transaction ({StatusCode}) : {Error}",
                    response.StatusCode, error);

                return Result.Fail("Impossible d'ajouter la transaction");
            }

            var created = await response.Content
                .ReadFromJsonAsync<TransactionVariableDto>();

            if (created is null)
                return Result.Fail("Reponse invalide du serveur");

            return Result.Ok(created);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de l'ajout de la transaction {@Form}", form);
            return Result.Fail("Erreur technique lors de l'ajout de la transaction");
        }
    }

    public async Task<Result> Update(int id, TransactionVariableForm form)
    {
        try
        {
            var response = await Client.PutAsJsonAsync($"transaction/{id}", form);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur update transaction {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail("Impossible de mettre a jour la transaction");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la mise a jour transaction {Id}", id);
            return Result.Fail("Erreur technique lors de la mise a jour de la transaction");
        }
    }

    public async Task<Result> Delete(int id)
    {
        try
        {
            var response = await Client.DeleteAsync($"transaction/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Log.Warning(
                    "Erreur suppression transaction {Id} ({StatusCode}) : {Error}",
                    id, response.StatusCode, error);

                return Result.Fail("Impossible de supprimer la transaction");
            }

            return Result.Ok();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la suppression transaction {Id}", id);
            return Result.Fail("Erreur technique lors de la suppression de la transaction");
        }
    }
}
