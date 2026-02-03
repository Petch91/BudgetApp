using System.Security.Claims;
using Application.Interfaces;
using Serilog;

namespace Front_BudgetApp.Api.Endpoints;

public static class RapportEndpoints
{
    public static void MapRapport(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/rapport")
            .WithTags("Rapport").RequireAuthorization("Connected");

        /* =======================
         * GET RAPPORT BY MONTH
         * ======================= */

        group.MapGet("/{annee:int}/{mois:int}", async (int annee, int mois, ClaimsPrincipal user, IRapportService service) =>
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (claim is null || !int.TryParse(claim.Value, out var userId))
            {
                Log.Warning("Échec extraction userId du JWT — claim NameIdentifier absent ou invalide");
                return Results.Unauthorized();
            }
            var result = await service.GetRapportMois(annee, mois, userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });
    }
}
