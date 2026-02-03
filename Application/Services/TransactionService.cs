using Application.Interfaces;
using Application.Persistence;
using Application.Projections;
using Entities.Contracts.Dtos;
using Entities.Contracts.Forms;
using Entities.Domain.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Application.Services;

public class TransactionService(MyDbContext context) : ITranscationService
{
    /* =======================
     * READ
     * ======================= */

    public async Task<Result<TransactionVariableDto>> GetById(int id, int userId)
    {
        Log.Information("Récupération transaction variable ID {Id}", id);

        var transaction = await context.TransactionsVariables
            .Where(t => t.Id == id && t.UserId == userId)
            .Select(ProjectionDto.TransactionAsDto)
            .SingleOrDefaultAsync();

        if (transaction is null)
        {
            Log.Warning("Transaction variable ID {Id} non trouvée pour userId {UserId}", id, userId);
            return Result.Fail("Transaction non trouvée");
        }

        return Result.Ok(transaction);
    }

    public async Task<Result<IReadOnlyList<TransactionVariableDto>>> GetRevenuesByMonth(int month, int userId)
    {
        var transactions = await context.TransactionsVariables
            .Where(t =>
                t.UserId == userId &&
                t.TransactionType == TransactionType.Revenu &&
                t.Date.Month == month)
            .Select(ProjectionDto.TransactionAsDto)
            .ToListAsync();

        return Result.Ok<IReadOnlyList<TransactionVariableDto>>(transactions);
    }

    public async Task<Result<IReadOnlyList<TransactionVariableDto>>> GetDepensesByMonth(int month, int userId)
    {
        var transactions = await context.TransactionsVariables
            .Where(t =>
                t.UserId == userId &&
                t.TransactionType == TransactionType.Depense &&
                t.Date.Month == month)
            .Select(ProjectionDto.TransactionAsDto)
            .ToListAsync();

        return Result.Ok<IReadOnlyList<TransactionVariableDto>>(transactions);
    }

    /* =======================
     * ADD
     * ======================= */

    public async Task<Result<TransactionVariableDto>> Add(TransactionVariableForm form, int userId)
    {
        Log.Information("Ajout transaction variable {@Form}", form);

        if (!await context.Categories.AnyAsync(c => c.Id == form.CategorieId))
            return Result.Fail("Catégorie inexistante");

        var transaction = new TransactionVariable
        {
            Intitule = form.Intitule,
            Montant = form.Montant,
            Date = form.Date,
            TransactionType = form.TransactionType,
            CategorieId = form.CategorieId,
            UserId = userId
        };

        context.TransactionsVariables.Add(transaction);
        await context.SaveChangesAsync();

        return await GetById(transaction.Id, userId);
    }

    /* =======================
     * UPDATE
     * ======================= */

    public async Task<Result> Update(int id, TransactionVariableForm form, int userId)
    {
        Log.Information("Mise à jour transaction variable ID {Id}", id);

        if (!await context.Categories.AnyAsync(c => c.Id == form.CategorieId))
            return Result.Fail("Catégorie inexistante");

        var result = await context.TransactionsVariables
            .Where(t => t.Id == id && t.UserId == userId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(t => t.Intitule, form.Intitule)
                .SetProperty(t => t.Montant, form.Montant)
                .SetProperty(t => t.Date, form.Date)
                .SetProperty(t => t.TransactionType, form.TransactionType)
                .SetProperty(t => t.CategorieId, form.CategorieId));

        if (result <= 0)
        {
            Log.Warning("Transaction variable ID {Id} non trouvée pour mise à jour (userId {UserId})", id, userId);
            return Result.Fail("Transaction non trouvée");
        }
        return Result.Ok();
    }

    /* =======================
     * DELETE
     * ======================= */

    public async Task<Result> Delete(int id, int userId)
    {
        Log.Information("Suppression transaction variable ID {Id} (userId {UserId})", id, userId);

        var result = await context.TransactionsVariables
            .Where(t => t.Id == id && t.UserId == userId)
            .ExecuteDeleteAsync();

        if (result <= 0)
        {
            Log.Warning("Transaction variable ID {Id} non trouvée pour suppression (userId {UserId})", id, userId);
            return Result.Fail("Transaction non trouvée");
        }
        return Result.Ok();
    }

    /* =======================
     * ACTION MÉTIER
     * ======================= */

    public async Task<Result> ChangeCategorie(int transactionId, int categorieId)
    {
        Log.Information(
            "Changement catégorie transaction {TransactionId} → {CategorieId}",
            transactionId, categorieId);

        if (!await context.Categories.AnyAsync(c => c.Id == categorieId))
            return Result.Fail("Catégorie inexistante");

        var result = await context.TransactionsVariables
            .Where(t => t.Id == transactionId)
            .ExecuteUpdateAsync(u =>
                u.SetProperty(t => t.CategorieId, categorieId));

        return result > 0
            ? Result.Ok()
            : Result.Fail("Transaction non trouvée");
    }
}
