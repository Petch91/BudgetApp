using Datas.Services.Interfaces;
using Datas.Services.MapperProjection;
using Datas.Tools;
using Entities.Dtos;
using Entities.Forms;
using Entities.Mappers;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;


namespace Datas.Services;

public class DepenseFixeService(MyDbContext context) : IDepenseFixeService
{
    public async Task<DepenseFixeDto> GetById(int id)
    {
        Log.Information("Récupération de la dépense fixe avec ID {Id}", id);
        var depense = await context.DepenseFixes.Where(d => d.Id == id).Select(ProjectionDto.DepenseFixeAsDto)
            .FirstOrDefaultAsync();
        if (depense == null)
        {
            Log.Warning("Aucune dépense fixe trouvée avec ID {Id}", id);
            return null;
        }

        Log.Information("Dépense fixe récupérée avec succès (ID: {Id})", id);
        return depense;
    }

    public async Task<IEnumerable<DepenseFixeDto>> GetDepenseFixes()
    {
        Log.Information("Récupération de toutes les dépenses fixes");
        var depenses = await context.DepenseFixes.Select(ProjectionDto.DepenseFixeAsDto).ToListAsync();
        Log.Information("{Count} dépense(s) fixe(s) récupérée(s)", depenses.Count);
        return depenses;
    }

    public async Task<Result> Add(DepenseFixeForm entity)
    {
        Log.Information("Ajout d'une nouvelle dépense fixe");

        if (!await context.Categories.AnyAsync(c =>
                c.Id == entity.Categorie.Id) || entity.BeginDate == default || entity.ReminderDaysBefore < 0 ||
            !Enum.IsDefined(typeof(Frequence), entity.Frequence))
        {
            Log.Warning("Échec de l'ajout : données invalides ou catégorie inexistante");
            return Result.BadRequest;
        }

        var depense = SetDates(entity.ToDb(), entity.BeginDate);
        context.DepenseFixes.Add(depense);
        var result = await context.SaveChangesAsync();
        Log.Information("Dépense fixe ajoutée : {Result}", result > 0);
        return result > 0 ? Result.Success : Result.Failure;
    }

    public async Task<Result> Update(int id, DepenseFixeForm entity)
    {
        Log.Information("Mise à jour de la dépense fixe ID {Id}", id);

        if (!await context.Categories.AnyAsync(c => c.Id == entity.Categorie.Id) || entity.BeginDate == default ||
            entity.ReminderDaysBefore < 0 || !Enum.IsDefined(typeof(Frequence), entity.Frequence))
        {
            Log.Warning("Échec de la mise à jour : données invalides pour la dépense fixe ID {Id}", id);
            return Result.BadRequest;
        }

        var depense = await context.DepenseFixes.Include(x => x.DueDates).Include(x => x.Rappels)
            .FirstOrDefaultAsync(d => d.Id == id);
        if (depense == null)
        {
            Log.Warning("Dépense fixe non trouvée pour mise à jour (ID {Id})", id);
            return Result.NotFound;
        }

        var newdepense = new DepenseFixe();

        if (depense.DueDates.IsNullOrEmpty() ||
            depense.DueDates.Select(d => d.Date).Min(date => date.Date) != entity.BeginDate.Date ||
            depense.Frequence != entity.Frequence)
        {
            newdepense = depense.ToDb(entity);
            //await context.Rappels.Where(r => r.DepenseFixeId == id).ExecuteDeleteAsync();
            //await context.depenseDueDates.Where(d => d.DepenseId == id).ExecuteDeleteAsync();
            depense = SetDates(depense, entity.BeginDate);
        }
        else if (depense.EstDomiciliée != entity.EstDomiciliée ||
                 depense.ReminderDaysBefore != entity.ReminderDaysBefore)
        {
            newdepense = depense.ToDb(entity);
            if (!entity.EstDomiciliée)
            {
                //if (depense.Rappels.Count > 0) await context.Rappels.Where(r => r.DepenseFixeId == id).ExecuteDeleteAsync();
                var rappels = new List<Rappel>();
                foreach (var date in depense.DueDates)
                {
                    rappels.AddRange(SetRappels(date.Date, entity.ReminderDaysBefore));
                }

                depense.Rappels = rappels;
            }
            else
            {
                //await context.Rappels.Where(r => r.DepenseFixeId == id).ExecuteDeleteAsync();
                depense.Rappels = new List<Rappel>();
            }
        }


        context.DepenseFixes.Update(newdepense);
        var result = await context.SaveChangesAsync();

        Log.Information("Dépense fixe mise à jour : succès = {Success} (ID {Id})", result > 0, id);
        return result > 0 ? Result.Success : Result.NotFound;
    }

    public async Task<bool> Delete(int id)
    {
        Log.Information("Suppression de la dépense fixe ID {Id}", id);
        var result = await context.DepenseFixes.Where(d => d.Id == id).ExecuteDeleteAsync();
        Log.Information("Suppression effectuée : {Success}", result > 0);
        return result > 0;
    }

    public async Task<bool> ChangeVuRappel(int id)
    {
        Log.Information("Changement d'état de vu pour le rappel ID {Id}", id);
        var rappel = await context.Rappels.FindAsync(id);
        if (rappel == null)
        {
            Log.Warning("Rappel non trouvé (ID {Id})", id);
            return false;
        }

        var result = await context.Rappels
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(p => p.SetProperty(f => f.Vu, !rappel.Vu));
        Log.Information("État de vu modifié pour rappel ID {Id} : {Success}", id, result > 0);
        return result > 0;
    }

    public async Task<Result> ChangeCategorie(int depenseId, int categorieId)
    {
        Log.Information("Changement de catégorie pour la dépense fixe ID {Id} vers la catégorie {CategorieId}",
            depenseId, categorieId);
        var depense = await context.DepenseFixes.FindAsync(depenseId);
        if (depense == null)
        {
            Log.Warning("Dépense fixe non trouvée pour changement de catégorie (ID {Id})", depenseId);
            return Result.NotFound;
        }

        var categorie = await context.Categories.FindAsync(categorieId);
        if (categorie == null)
        {
            Log.Warning("Catégorie non trouvée (ID {Id})", categorieId);
            return Result.BadRequest;
        }

        var result = await context.DepenseFixes
            .Where(r => r.Id == depenseId)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(f => f.CategorieId, categorieId));

        Log.Information("Catégorie changée pour dépense ID {Id} : succès = {Success}", depenseId, result > 0);
        return result > 0 ? Result.Success : Result.Failure;
    }

    public async Task<bool> ChangeBeginDate(int id, DateTime beginDate)
    {
        Log.Information("Changement de date de début pour la dépense fixe ID {Id}", id);
        var depense = await context.DepenseFixes.Include(x => x.DueDates).FirstOrDefaultAsync(x => x.Id == id);
        if (depense == null)
        {
            Log.Warning("Dépense fixe non trouvée pour changement de date (ID {Id})", id);
            return false;
        }

        // await context.Rappels.Where(r => r.DepenseFixeId == id).ExecuteDeleteAsync();  il ne faut pas supprimer de soit meme car on fais par les includes 
        // await context.depenseDueDates.Where(d => d.DepenseId == id).ExecuteDeleteAsync(); ceci est fait quand on le gere nous meme revoir les video formation EF mommer 


        depense = SetDates(depense, beginDate);

        context.DepenseFixes.Update(depense);
        var result = await context.SaveChangesAsync();
        Log.Information("Date de début modifiée pour ID {Id} : succès = {Success}", id, result > 0);
        return result > 0;
    }

    private DepenseFixe SetDates(DepenseFixe depense, DateTime beginDate)
    {
        var date = beginDate;
        var year = date.Year;
        var rappels = new List<Rappel>();

        depense.DueDates = new List<DepenseDueDate>();

        bool shouldBreak = false;

        for (int i = 0; i < (int)depense.Frequence; i++)
        {
            if (year >= date.Year)
            {
                depense.DueDates.Add(new DepenseDueDate { Date = date });
                if (!depense.EstDomiciliée)
                {
                    rappels.AddRange(SetRappels(date, depense.ReminderDaysBefore));
                }

                switch (depense.Frequence)
                {
                    case Frequence.Biannuel:
                    {
                        date = date.AddMonths(6);
                        break;
                    }
                    case Frequence.Trimestriel:
                    {
                        date = date.AddMonths(3);
                        break;
                    }
                    case Frequence.Mensuel:
                    {
                        date = date.AddMonths(1);
                        break;
                    }
                    case Frequence.Annuel:
                    {
                        shouldBreak = true;
                        break;
                    }

                    default: throw new ArgumentOutOfRangeException();
                }

                if (shouldBreak) break;
            }
            else break;
        }

        depense.Rappels = rappels;
        return depense;
    }

    private List<Rappel> SetRappels(DateTime date, int reminderDaysBefore)
    {
        var rappels = new List<Rappel>
        {
            new Rappel { RappelDate = date.AddDays(-reminderDaysBefore) },
            new Rappel { RappelDate = date.AddDays(-1) },
            new Rappel { RappelDate = date.AddDays(0) }
        };
        return rappels;
    }
}