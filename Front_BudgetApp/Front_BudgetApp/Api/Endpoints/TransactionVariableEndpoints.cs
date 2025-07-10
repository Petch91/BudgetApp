using Datas.Services.Interfaces;
using Datas.Tools;
using Entities.Forms;
using Microsoft.AspNetCore.Mvc;

namespace API_BudgetApp.Endpoints;

public static class TransactionVariableEndpoints
{
    public static void MapTransactionVariable(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/transaction").WithTags("Transactions");
        
        group.MapGet("/{id:int}", async (int id, ITranscationService service) =>
        {
            var result = await service.GetById(id);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });
        group.MapGet("/revenubymonth/{month:int}", async (int month, ITranscationService service) =>
        {
            var result = await service.GetRevenuesByMonth(month);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });
        group.MapGet("/depensebymonth/{month:int}", async (int month, ITranscationService service) =>
        {
            var result = await service.GetDepensesByMonth(month);
            return result is not null ? Results.Ok(result) : Results.NotFound();
        });

        group.MapPost("/", async (TransactionVariableForm transaction, ITranscationService service) =>
        {
            var result = await service.Add(transaction);

            if (result.IsSuccess)
            {
                var created = result.Value;
                return Results.Created($"/transaction/{created.Id}", created); // Renvoie le DTO avec l'ID
            }

            var errorMessages = string.Join(" | ", result.Errors.Select(e => e.Message));
            return Results.BadRequest(new { Error = "Échec lors de l'ajout", Details = errorMessages });
        });
        
        group.MapPut("/{id:int}", async (int id,TransactionVariableForm form, ITranscationService service) =>
        {
            var result = await service.Update(id,form);
            if (result == ResultEnum.NotFound)
                return Results.NotFound();
            if (result == ResultEnum.Success) return Results.NoContent();
            return Results.NotFound();
        });
        
        group.MapDelete("/{id:int}", async (int id, ITranscationService service) =>
        {
            return await service.Delete(id)
                ? Results.NoContent()
                : Results.NotFound();
        });

        group.MapPatch("/{id}/categorie", async (
            int id,
            [FromBody] int categorieId,
            ITranscationService service) =>
        {
            var result = await service.ChangeCategorie(id, categorieId);
            return result switch
            {
                ResultEnum.Success => Results.NoContent(),
                ResultEnum.NotFound => Results.NotFound(),
                ResultEnum.BadRequest => Results.BadRequest(),
                _ => Results.StatusCode(500)
            };
        });
    }
}