using System.Security.Claims;
using Application.Interfaces;
using Entities.Contracts.Forms;

namespace Front_BudgetApp.Api.Endpoints;

public static class TransactionVariableEndpoints
{
    public static void MapTransactionVariable(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transaction")
            .WithTags("Transactions").RequireAuthorization("Connected");

        /* =======================
         * GET BY ID
         * ======================= */

        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, ITranscationService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.GetById(id, userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Errors);
        });

        /* =======================
         * REVENUS BY MONTH
         * ======================= */

        group.MapGet("/revenubymonth/{month:int}", async (int month, ClaimsPrincipal user, ITranscationService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.GetRevenuesByMonth(month, userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * DEPENSES BY MONTH
         * ======================= */

        group.MapGet("/depensebymonth/{month:int}", async (int month, ClaimsPrincipal user, ITranscationService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.GetDepensesByMonth(month, userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * ADD
         * ======================= */

        group.MapPost("/", async (TransactionVariableForm form, ClaimsPrincipal user, ITranscationService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.Add(form, userId);

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
            ClaimsPrincipal user,
            ITranscationService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.Update(id, form, userId);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(result.Errors);
        });

        /* =======================
         * DELETE
         * ======================= */

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, ITranscationService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.Delete(id, userId);

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

    private static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
