

using Datas.Services.Interfaces;
using Datas.Services.MapperProjection;
using Datas.Tools;
using Entities.Dtos;
using Entities.Forms;
using Entities.Mappers;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Result> Add(TransactionVariableForm entity)
    {
        var t = entity.ToDb();
        context.TransactionsVariables.Add(t);
        var result = await context.SaveChangesAsync();
        return result > 0 ? Result.Success : Result.Failure;
    }

    public async Task<Result> Update(int id, TransactionVariableForm entity)
    {
        if(entity.CategorieId == 0 || !await context.Categories.AnyAsync(c => c.Id == entity.CategorieId)) return Result.BadRequest; //return Badrequest
        var transaction = await context.TransactionsVariables.FindAsync(id);
        if (transaction == null) return Result.NotFound; //return not found
      
        context.TransactionsVariables.Update(transaction.ToDb(entity));
        var result = await context.SaveChangesAsync();
        return result > 0 ? Result.Success : Result.NotFound; 
    }

    public async Task<bool> Delete(int id)
    {
        var result = await context.TransactionsVariables.Where(d => d.Id == id).ExecuteDeleteAsync();
        return result > 0;
    }

    public async Task<Result> ChangeCategorie(int transactionId, int categorieId)
    {
        var transaction = await context.DepenseFixes.FindAsync(transactionId);
        if (transaction == null)
            return Result.NotFound;

        var categorie = await context.Categories.FindAsync(categorieId);
        if (categorie == null)
            return Result.BadRequest; 

        var result = await context.DepenseFixes
            .ExecuteUpdateAsync(p => 
                p.SetProperty(f => f.CategorieId, categorieId));

        return result > 0 ? Result.Success : Result.Failure;
    }
}