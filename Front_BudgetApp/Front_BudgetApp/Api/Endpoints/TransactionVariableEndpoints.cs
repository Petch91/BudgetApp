using Datas.Interfaces;
using Entities.Contracts.Forms;

namespace Front_BudgetApp.Api.Endpoints;

public static class TransactionVariableEndpoints
{
    public static void MapTransactionVariable(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transaction")
            .WithTags("Transactions");

        /* =======================
         * GET BY ID
         * ======================= */

        group.MapGet("/{id:int}", async (int id, ITranscationService service) =>
        {
            var result = await service.GetById(id);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Errors);
        });

        /* =======================
         * REVENUS BY MONTH
         * ======================= */

        group.MapGet("/revenubymonth/{month:int}", async (int month, ITranscationService service) =>
        {
            var result = await service.GetRevenuesByMonth(month);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * DEPENSES BY MONTH
         * ======================= */

        group.MapGet("/depensebymonth/{month:int}", async (int month, ITranscationService service) =>
        {
            var result = await service.GetDepensesByMonth(month);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * ADD
         * ======================= */

        group.MapPost("/", async (TransactionVariableForm form, ITranscationService service) =>
        {
            var result = await service.Add(form);

            if (result.IsSuccess)
            {
                var created = result.Value;
                return Results.Created($"/api/transaction/{created.Id}", created);
            }

            return Results.BadRequest(result.Errors);
        });

        /* =======================
         * UPDATE
         * ======================= */

        group.MapPut("/{id:int}", async (
            int id,
            TransactionVariableForm form,
            ITranscationService service) =>
        {
            var result = await service.Update(id, form);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(result.Errors);
        });

        /* =======================
         * DELETE
         * ======================= */

        group.MapDelete("/{id:int}", async (int id, ITranscationService service) =>
        {
            var result = await service.Delete(id);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(result.Errors);
        });

        /* =======================
         * CHANGE CATEGORIE
         * ======================= */

        group.MapPatch("/{id:int}/categorie", async (
            int id,
            int categorieId,
            ITranscationService service) =>
        {
            var result = await service.ChangeCategorie(id, categorieId);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Errors);
        });
    }
}
