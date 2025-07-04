using Datas.Services.Interfaces;
using Datas.Tools;
using Entities.Forms;

namespace API_BudgetApp.Endpoints;

public static class CategorieEndpoints
{
    public static void MapCategorie(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/categorie")
            .WithTags("Catégories");
        
        group.MapGet("/{id:int}", async (int id, ICategorieService service) =>
        {
            var result = await service.GetById(id);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });
        
        group.MapGet("/", async (ICategorieService service) =>
        {
            var result = await service.GetCategories();
            return Results.Ok(result);
        });

        group.MapPost("/", async (CategorieForm form, ICategorieService service) =>
        {
            var success = await service.Add(form);
            return success == Result.Success ? Results.Created($"/categorie", null) : Results.BadRequest("Échec lors de l'ajout");
        });
        
        group.MapPut("/{id:int}", async (int id,CategorieForm form, ICategorieService service) =>
        {
            var result = await service.Update(id,form);
            if (result == Result.NotFound)
                return Results.NotFound();
            if (result == Result.Success) return Results.NoContent();
            return Results.NotFound();
        });
        
        group.MapDelete("/{id:int}", async (int id, ICategorieService service) =>
        {
            return await service.Delete(id)
                ? Results.NoContent()
                : Results.NotFound();
        });
    }
}