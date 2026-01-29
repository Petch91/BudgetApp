using System.Security.Claims;
using Application.Interfaces;

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
            var userId = int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await service.GetRapportMois(annee, mois, userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });
    }
}
