using System.Security.Claims;
using Application.Interfaces;
using Entities.Contracts.Forms;

namespace Front_BudgetApp.Api.Endpoints;

public static class DepenseFixesEndpoints
{
    public static void MapDepenseFixe(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/depensefixe")
            .WithTags("DepenseFixes").RequireAuthorization("Connected");

        /* =======================
         * GET BY ID
         * ======================= */

        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, IDepenseFixeService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.GetById(id, userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Errors);
        });

        /* =======================
         * GET ALL
         * ======================= */

        group.MapGet("/", async (ClaimsPrincipal user, IDepenseFixeService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.GetDepenseFixes(userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * ADD
         * ======================= */

        group.MapPost("/", async (DepenseFixeForm form, ClaimsPrincipal user, IDepenseFixeService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.Add(form, userId);

            if (result.IsSuccess)
            {
                var created = result.Value;
                return Results.Created($"/api/depensefixe/{created.Id}", created);
            }

            return Results.BadRequest(result.Errors);
        });

        /* =======================
         * UPDATE
         * ======================= */

        group.MapPut("/{id:int}", async (int id, DepenseFixeForm form, ClaimsPrincipal user, IDepenseFixeService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.Update(id, form, userId);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * DELETE
         * ======================= */

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, IDepenseFixeService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.Delete(id, userId);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(result.Errors);
        });

        /* =======================
         * RAPPEL → VU
         * ======================= */

        group.MapPatch("/rappels/{id:int}/vu", async (int id, IDepenseFixeService service) =>
        {
            var result = await service.ChangeVuRappel(id);

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
            IDepenseFixeService service) =>
        {
            var result = await service.ChangeCategorie(id, categorieId);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * CHANGE BEGIN DATE
         * ======================= */

        group.MapPatch("/{id:int}/duedate", async (
            int id,
            DateTime beginDate,
            IDepenseFixeService service) =>
        {
            var result = await service.ChangeBeginDate(id, beginDate);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(result.Errors);
        });
        /* =======================
         * ACTIVER ECHELONNEMENT
         * ======================= */

        group.MapPatch("/{id:int}/echelonnement", async (
            int id,
            EchelonnementRequest request,
            ClaimsPrincipal user,
            IDepenseFixeService service) =>
        {
            var userId = GetUserId(user);
            var result = await service.ActiverEchelonnement(id, request.NombreEcheances, request.MontantParEcheance, userId);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.BadRequest(result.Errors);
        });
    }

    private static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
}
