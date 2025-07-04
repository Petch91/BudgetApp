using Datas.Services.Interfaces;
using Datas.Tools;
using Entities.Forms;
using Microsoft.AspNetCore.Mvc;

namespace API_BudgetApp.Endpoints;

public static class DepenseFixesEndpoints
{
    public static void MapDepenseFixe(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/depensefixe").WithTags("DepenseFixes");
        
        group.MapGet("/{id:int}", async (int id, IDepenseFixeService service) =>
        {
            var result = await service.GetById(id);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });
        
        group.MapGet("/", async (IDepenseFixeService service) =>
        {
            var result = await service.GetDepenseFixes();
            return Results.Ok(result);
        });
        group.MapPost("/", async (DepenseFixeForm form, IDepenseFixeService service) =>
        {
            var success = await service.Add(form);
            return success == Result.Success ? Results.Created($"/depensefixe", null) : Results.BadRequest("Échec lors de l'ajout");
        });
        
        
        group.MapPut("/{id:int}", async (int id,DepenseFixeForm form, IDepenseFixeService service) =>
        {
            var result = await service.Update(id,form);
            if (result == Result.NotFound)
                return Results.NotFound();
            if (result == Result.BadRequest)
                return Results.BadRequest("Categorie n'existe pas");
            if (result == Result.Success) return Results.NoContent();
            return Results.NotFound();
        });
        
        group.MapDelete("/{id:int}", async (int id, IDepenseFixeService service) =>
        {
            return await service.Delete(id)
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapPatch("/rappels/{id:int}/vu", async (int id, IDepenseFixeService service) =>
        {
            return await service.ChangeVuRappel(id)
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapPatch("/{id}/categorie", async (
            int id,
            [FromBody] int categorieId,
            IDepenseFixeService service) =>
        {
            var result = await service.ChangeCategorie(id, categorieId);
            return result switch
            {
                Result.Success => Results.NoContent(),
                Result.NotFound => Results.NotFound(),
                Result.BadRequest => Results.BadRequest(),
                _ => Results.StatusCode(500)
            };
        });
        group.MapPatch("/{id:int}/duedate", async (
            int id,
            [FromBody] DateTime due,
            IDepenseFixeService service) =>
        {
            var success = await service.ChangeBeginDate(id, due);
            return success ? Results.NoContent() : Results.NotFound();
        });
    }

}

