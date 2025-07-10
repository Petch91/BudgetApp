

using Datas.Services.Interfaces;
using Datas.Services.MapperProjection;
using Datas.Tools;
using Entities.Dtos;
using Entities.Forms;
using Entities.Mappers;
using Entities.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Datas.Services;

public class TransactionService(MyDbContext context) : ITranscationService
{
    public async Task<TransactionVariableDto> GetById(int id)
    {
        var transaction = await context.TransactionsVariables.Where(t => t.Id == id).Select(ProjectionDto.TransactionAsDto).FirstOrDefaultAsync();
        if (transaction == null) return null;
        return transaction;
    }

    public async Task<IEnumerable<TransactionVariableDto>> GetRevenuesByMonth(int month)
    {
        var transactions = await context.TransactionsVariables
            .Where(t => t.TransactionType == TransactionType.Revenu && t.Date.Month == month)
            .Select(ProjectionDto.TransactionAsDto).ToListAsync();
        //if (transaction == null) return null;
        return transactions;
    }

    public async Task<IEnumerable<TransactionVariableDto>> GetDepensesByMonth(int month)
    {
        var transactions = await context.TransactionsVariables
            .Where(t => t.TransactionType == TransactionType.Depense && t.Date.Month == month)
            .Select(ProjectionDto.TransactionAsDto).ToListAsync();
        //if (transaction == null) return null;
        return transactions;;
    }

    public async Task<Result<TransactionVariableDto>> Add(TransactionVariableForm entity)
    {
        Log.Information("Ajout d'une nouvelle transaction variable");

        try
        {
            var transaction = entity.ToDb();
            context.TransactionsVariables.Add(transaction);
            var saved = await context.SaveChangesAsync();

            if (saved > 0)
            {
                var dto = await GetById(transaction.Id);

                Log.Information("Transaction variable ajoutée avec succès (ID: {Id})", transaction.Id);
                return Result.Ok(dto);
            }

            Log.Warning("Aucune transaction variable n'a été ajoutée");
            return Result.Fail<TransactionVariableDto>("Aucune ligne n’a été insérée en base.");
        }
        catch (DbUpdateException dbEx)
        {
            Log.Error(dbEx, "Erreur base de données lors de l'ajout d'une transaction variable");
            return Result.Fail<TransactionVariableDto>("Erreur base de données : " + dbEx.Message)
                .WithError(dbEx.InnerException?.Message ?? string.Empty);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de l'ajout d'une transaction variable");
            return Result.Fail<TransactionVariableDto>("Erreur inattendue.")
                .WithError(ex.Message);
        }
    }


    public async Task<ResultEnum> Update(int id, TransactionVariableForm entity)
    {
        if(entity.CategorieId == 0 || !await context.Categories.AnyAsync(c => c.Id == entity.CategorieId)) return ResultEnum.BadRequest; //return Badrequest
        var transaction = await context.TransactionsVariables.FindAsync(id);
        if (transaction == null) return ResultEnum.NotFound; //return not found
      
        context.TransactionsVariables.Update(transaction.ToDb(entity));
        var result = await context.SaveChangesAsync();
        return result > 0 ? ResultEnum.Success : ResultEnum.NotFound; 
    }

    public async Task<bool> Delete(int id)
    {
        var result = await context.TransactionsVariables.Where(d => d.Id == id).ExecuteDeleteAsync();
        return result > 0;
    }

    public async Task<ResultEnum> ChangeCategorie(int transactionId, int categorieId)
    {
        var transaction = await context.DepenseFixes.FindAsync(transactionId);
        if (transaction == null)
            return ResultEnum.NotFound;

        var categorie = await context.Categories.FindAsync(categorieId);
        if (categorie == null)
            return ResultEnum.BadRequest; 

        var result = await context.DepenseFixes
            .ExecuteUpdateAsync(p => 
                p.SetProperty(f => f.CategorieId, categorieId));

        return result > 0 ? ResultEnum.Success : ResultEnum.Failure;
    }
}