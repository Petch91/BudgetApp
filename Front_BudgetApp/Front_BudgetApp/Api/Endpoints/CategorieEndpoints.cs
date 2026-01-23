using Application.Interfaces;
using Entities.Contracts.Forms;
using FluentResults;

namespace Front_BudgetApp.Api.Endpoints;

public static class CategorieEndpoints
{
    public static void MapCategorie(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categorie")
            .WithTags("Catégories");

        /* =======================
         * GET BY ID
         * ======================= */

        group.MapGet("/{id:int}", async (int id, ICategorieService service) =>
        {
            var result = await service.GetById(id);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(result.Errors);
        });

        /* =======================
         * GET ALL
         * ======================= */

        group.MapGet("/", async (ICategorieService service) =>
        {
            var result = await service.GetCategories();

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * ADD
         * ======================= */

        group.MapPost("/", async (CategorieForm form, ICategorieService service) =>
        {
            var result = await service.Add(form);

            if (result.IsSuccess)
            {
                var created = result.Value;
                return Results.Created($"/api/categorie/{created.Id}", created);
            }

            return Results.BadRequest(result.Errors);
        }).RequireAuthorization("Connected");

        /* =======================
         * UPDATE
         * ======================= */

        group.MapPut("/{id:int}", async (int id, CategorieForm form, ICategorieService service) =>
        {
            var result = await service.Update(id, form);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(result.Errors);
        }).RequireAuthorization("Connected");

        /* =======================
         * DELETE
         * ======================= */

        group.MapDelete("/{id:int}", async (int id, ICategorieService service) =>
        {
            var result = await service.Delete(id);

            return result.IsSuccess
                ? Results.NoContent()
                : Results.NotFound(result.Errors);
        }).RequireAuthorization("Connected");
    }
}
