using System.Security.Claims;
using Application.Interfaces;
using Entities.Contracts.Forms;
using Serilog;

namespace Front_BudgetApp.Api.Endpoints;

public static class ImportEndpoints
{
    /// <summary>Taille maximale acceptee pour un extrait de compte (5 Mo).</summary>
    private const long TailleMaxFichier = 5 * 1024 * 1024;

    public static void MapImport(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/import")
            .WithTags("Import").RequireAuthorization("Connected");

        /* =======================
         * ANALYSE (aucune ecriture)
         * ======================= */

        group.MapPost("/analyze", async (IFormFile fichier, ClaimsPrincipal user, IImportService service) =>
        {
            var userId = GetUserId(user);
            if (userId == 0)
                return Results.Unauthorized();

            if (fichier.Length == 0)
                return Results.BadRequest(new[] { "Fichier vide" });

            if (fichier.Length > TailleMaxFichier)
                return Results.BadRequest(new[] { "Fichier trop volumineux (5 Mo maximum)" });

            await using var stream = fichier.OpenReadStream();
            var result = await service.Analyser(stream, userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors.Select(e => e.Message));
        })
        .DisableAntiforgery() // Protege par JWT Bearer : pas de cookie, donc pas de risque CSRF.
        .WithSummary("Analyse un extrait de compte et renvoie les transactions proposees");

        /* =======================
         * CONFIRMATION (ecriture)
         * ======================= */

        group.MapPost("/confirm", async (ImportConfirmForm form, ClaimsPrincipal user, IImportService service) =>
        {
            var userId = GetUserId(user);
            if (userId == 0)
                return Results.Unauthorized();

            var result = await service.Confirmer(form, userId);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors.Select(e => e.Message));
        })
        .WithSummary("Importe les transactions validees par l'utilisateur");
    }

    private static int GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || !int.TryParse(claim.Value, out var userId))
        {
            Log.Warning("Échec extraction userId du JWT — claim NameIdentifier absent ou invalide");
            return 0;
        }
        return userId;
    }
}
