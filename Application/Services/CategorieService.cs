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

public class CategorieService(MyDbContext context) : ICategorieService
{
    public async Task<Result<CategorieDto>> GetById(int id)
    {
        Log.Information("Récupération de la catégorie avec l'ID {Id}", id);

        var categorie = await context.Categories
            .Where(c => c.Id == id)
            .Select(ProjectionDto.CategorieAsDto)
            .SingleOrDefaultAsync();

        if (categorie is null)
        {
            Log.Warning("Catégorie avec ID {Id} non trouvée", id);
            return Result.Fail("Catégorie non trouvée");
        }

        return Result.Ok(categorie);
    }

    public async Task<Result<IReadOnlyList<CategorieDto>>> GetCategories()
    {
        Log.Information("Récupération de toutes les catégories");

        var categories = await context.Categories
            .Select(ProjectionDto.CategorieAsDto)
            .ToListAsync();

        Log.Information("Nombre de catégories récupérées : {Count}", categories.Count);

        return Result.Ok<IReadOnlyList<CategorieDto>>(categories);
    }

    public async Task<Result<CategorieDto>> Add(CategorieForm form)
    {
        Log.Information("Tentative d'ajout d'une catégorie : {Name}", form.Name);

        try
        {
            var categorie = new Categorie
            {
                Name = form.Name,
                Icon = form.Icon
            };

            context.Categories.Add(categorie);
            var saved = await context.SaveChangesAsync();

            if (saved <= 0)
            {
                Log.Warning("Aucune modification enregistrée pour la catégorie : {Name}", form.Name);
                return Result.Fail("Aucune modification n'a été enregistrée");
            }

            Log.Information("Catégorie créée avec succès : ID {Id}", categorie.Id);

            var dto = new CategorieDto(
                categorie.Id,
                categorie.Name,
                categorie.Icon
            );

            return Result.Ok(dto);
        }
        catch (DbUpdateException dbEx)
        {
            Log.Error(dbEx, "Erreur DB lors de l'ajout de la catégorie : {Name}", form.Name);
            return Result.Fail("Erreur base de données")
                .WithError(dbEx.InnerException?.Message ?? dbEx.Message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la création de la catégorie : {Name}", form.Name);
            return Result.Fail("Erreur inattendue")
                .WithError(ex.Message);
        }
    }

    public async Task<Result> Update(int id, CategorieForm form)
    {
        Log.Information("Mise à jour de la catégorie ID {Id}", id);

        var result = await context.Categories
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(c => c.Name, form.Name)
                .SetProperty(c => c.Icon, form.Icon));

        if (result <= 0)
        {
            Log.Warning("Catégorie non trouvée pour mise à jour : ID {Id}", id);
            return Result.Fail("Catégorie non trouvée");
        }

        Log.Information("Catégorie mise à jour avec succès : ID {Id}", id);
        return Result.Ok();
    }

    public async Task<Result> Delete(int id)
    {
        Log.Information("Suppression de la catégorie ID {Id}", id);

        var result = await context.Categories
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync();

        if (result <= 0)
        {
            Log.Warning("Catégorie non trouvée ou déjà supprimée : ID {Id}", id);
            return Result.Fail("Catégorie non trouvée");
        }

        Log.Information("Catégorie supprimée avec succès : ID {Id}", id);
        return Result.Ok();
    }
}
