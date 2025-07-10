using Datas.Services.Interfaces;
using Datas.Services.MapperProjection;
using Datas.Tools;
using Entities.Dtos;
using Entities.Forms;
using Entities.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Datas.Services;

public class CategorieService(MyDbContext context) : ICategorieService
{
    public async Task<CategorieDto> GetById(int id)
    {
        Log.Information("Récupération de la catégorie avec l'ID {Id}", id);

        var categorie = await context.Categories
            .Where(d => d.Id == id)
            .Select(ProjectionDto.CategorieAsDto)
            .FirstOrDefaultAsync();

        if (categorie == null)
        {
            Log.Warning("Catégorie avec ID {Id} non trouvée", id);
        }

        return categorie;
    }

    public async Task<IEnumerable<CategorieDto>> GetCategories()
    {
        Log.Information("Récupération de toutes les catégories");

        var categories = await context.Categories
            .Select(ProjectionDto.CategorieAsDto)
            .ToListAsync();

        Log.Information("Nombre de catégories récupérées : {Count}", categories.Count);

        return categories;
    }

    public async Task<Result<CategorieDto>> Add(CategorieForm entity)
    {
        Log.Information("Tentative d'ajout d'une catégorie : {Name}", entity.Name);

        try
        {
            var categorie = new Categorie
            {
                Name = entity.Name,
                Icon = entity.Icon
            };

            context.Categories.Add(categorie);
            var saved = await context.SaveChangesAsync();

            if (saved > 0)
            {
                Log.Information("Catégorie créée avec succès : ID {Id}", categorie.Id);

                var dto = new CategorieDto
                {
                    Id = categorie.Id,
                    Name = categorie.Name,
                    Icon = categorie.Icon
                };

                return Result.Ok(dto);
            }
            else
            {
                Log.Warning("Aucune modification enregistrée lors de l'ajout de la catégorie : {Name}", entity.Name);
                return Result.Fail<CategorieDto>("Aucune modification n'a été enregistrée en base.");
            }
        }
        catch (DbUpdateException dbEx)
        {
            Log.Error(dbEx, "Erreur base de données lors de l'ajout de la catégorie : {Name}", entity.Name);
            return Result.Fail<CategorieDto>("Erreur base de données : " + dbEx.Message)
                         .WithError(dbEx.InnerException?.Message ?? string.Empty);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Erreur inattendue lors de la création de la catégorie : {Name}", entity.Name);
            return Result.Fail<CategorieDto>("Erreur inattendue lors de la création de la catégorie.")
                         .WithError(ex.Message);
        }
    }

    public async Task<ResultEnum> Update(int id, CategorieForm entity)
    {
        Log.Information("Mise à jour de la catégorie ID {Id}", id);

        var categorie = await context.TransactionsVariables.FindAsync(id);
        if (categorie == null)
        {
            Log.Warning("Catégorie non trouvée pour mise à jour : ID {Id}", id);
            return ResultEnum.NotFound;
        }

        var result = await context.Categories
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(f => f
                .SetProperty(a => a.Name, entity.Name)
                .SetProperty(c => c.Icon, entity.Icon));

        if (result > 0)
        {
            Log.Information("Catégorie mise à jour avec succès : ID {Id}", id);
            return ResultEnum.Success;
        }
        else
        {
            Log.Warning("Échec de la mise à jour de la catégorie : ID {Id}", id);
            return ResultEnum.NotFound;
        }
    }

    public async Task<bool> Delete(int id)
    {
        Log.Information("Suppression de la catégorie ID {Id}", id);

        var result = await context.Categories.Where(d => d.Id == id).ExecuteDeleteAsync();

        if (result > 0)
        {
            Log.Information("Catégorie supprimée avec succès : ID {Id}", id);
            return true;
        }
        else
        {
            Log.Warning("Catégorie non trouvée ou déjà supprimée : ID {Id}", id);
            return false;
        }
    }
}
