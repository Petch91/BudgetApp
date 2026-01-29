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

public class DepenseFixeService(MyDbContext context) : IDepenseFixeService
{
    /* =======================
     * READ
     * ======================= */

    public async Task<Result<DepenseFixeDto>> GetById(int id, int userId)
    {
        Log.Information("Récupération de la dépense fixe ID {Id}", id);

        var depense = await context.DepenseFixes
            .Where(d => d.Id == id && d.UserId == userId)
            .Select(ProjectionDto.DepenseFixeAsDto)
            .SingleOrDefaultAsync();

        if (depense is null)
        {
            Log.Warning("Dépense fixe non trouvée (ID {Id})", id);
            return Result.Fail("Dépense fixe non trouvée");
        }

        return Result.Ok(depense);
    }

    public async Task<Result<IReadOnlyList<DepenseFixeDto>>> GetDepenseFixes(int userId)
    {
        Log.Information("Récupération de toutes les dépenses fixes");

        var depenses = await context.DepenseFixes
            .Where(d => d.UserId == userId)
            .Select(ProjectionDto.DepenseFixeAsDto)
            .ToListAsync();

        return Result.Ok<IReadOnlyList<DepenseFixeDto>>(depenses);
    }

    /* =======================
     * ADD
     * ======================= */

    public async Task<Result<DepenseFixeDto>> Add(DepenseFixeForm form, int userId)
    {
        Log.Information("Ajout d'une dépense fixe {@Form}", form);

        if (!await context.Categories.AnyAsync(c => c.Id == form.Categorie.Id))
            return Result.Fail("Catégorie inexistante");

        if (form.BeginDate == default)
            return Result.Fail("Date de début invalide");

        if (form.ReminderDaysBefore < 0)
            return Result.Fail("Nombre de jours de rappel invalide");

        if (form.DateFin.HasValue && form.DateFin.Value <= form.BeginDate)
            return Result.Fail("La date de fin doit être postérieure à la date de début");

        var depense = new DepenseFixe
        {
            Intitule = form.Intitule,
            Montant = form.Montant,
            Frequence = form.Frequence,
            EstDomiciliee = form.EstDomiciliee,
            ReminderDaysBefore = form.ReminderDaysBefore,
            CategorieId = form.Categorie.Id,
            DateFin = form.DateFin,
            UserId = userId,
            IsEchelonne = form.IsEchelonne,
            NombreEcheances = form.NombreEcheances,
            MontantParEcheance = form.MontantParEcheance,
            EcheancesRestantes = form.IsEchelonne ? form.NombreEcheances : null,
            DueDates = [],
            Rappels = []
        };

        SetDates(depense, form.BeginDate);

        context.DepenseFixes.Add(depense);
        await context.SaveChangesAsync();

        return await GetById(depense.Id, userId);
    }

    /* =======================
     * UPDATE
     * ======================= */

    public async Task<Result> Update(int id, DepenseFixeForm form, int userId)
    {
        Log.Information("Mise à jour dépense fixe ID {Id}", id);

        var depense = await context.DepenseFixes
            .Include(d => d.DueDates)
            .Include(d => d.Rappels)
            .SingleOrDefaultAsync(d => d.Id == id && d.UserId == userId);

        if (depense is null)
            return Result.Fail("Dépense fixe non trouvée");

        depense.Intitule = form.Intitule;
        depense.Montant = form.Montant;
        depense.Frequence = form.Frequence;
        depense.EstDomiciliee = form.EstDomiciliee;
        depense.ReminderDaysBefore = form.ReminderDaysBefore;
        depense.CategorieId = form.Categorie.Id;
        depense.DateFin = form.DateFin;
        depense.IsEchelonne = form.IsEchelonne;
        depense.NombreEcheances = form.NombreEcheances;
        depense.MontantParEcheance = form.MontantParEcheance;
        if (form.IsEchelonne && depense.EcheancesRestantes == null)
            depense.EcheancesRestantes = form.NombreEcheances;

        SetDates(depense, form.BeginDate);

        await context.SaveChangesAsync();
        return Result.Ok();
    }

    /* =======================
     * DELETE
     * ======================= */

    public async Task<Result> Delete(int id, int userId)
    {
        Log.Information("Suppression dépense fixe ID {Id}", id);

        var result = await context.DepenseFixes
            .Where(d => d.Id == id && d.UserId == userId)
            .ExecuteDeleteAsync();

        return result > 0
            ? Result.Ok()
            : Result.Fail("Dépense fixe non trouvée");
    }


    public async Task<Result> ChangeVuRappel(int id)
    {
        Log.Information("Changement d'état vu du rappel ID {Id}", id);

        var rappel = await context.Rappels
            .Where(r => r.Id == id)
            .Select(r => new { r.Id, r.Vu })
            .SingleOrDefaultAsync();

        if (rappel is null)
        {
            Log.Warning("Rappel non trouvé (ID {Id})", id);
            return Result.Fail("Rappel non trouvé");
        }

        var result = await context.Rappels
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(u =>
                u.SetProperty(r => r.Vu, !rappel.Vu));

        return result > 0
            ? Result.Ok()
            : Result.Fail("Impossible de modifier l'état du rappel");
    }

    public async Task<Result> ChangeCategorie(int depenseId, int categorieId)
    {
        Log.Information(
            "Changement de catégorie pour la dépense {DepenseId} vers {CategorieId}",
            depenseId, categorieId);

        var categorieExiste = await context.Categories
            .AnyAsync(c => c.Id == categorieId);

        if (!categorieExiste)
            return Result.Fail("Catégorie inexistante");

        var result = await context.DepenseFixes
            .Where(d => d.Id == depenseId)
            .ExecuteUpdateAsync(u =>
                u.SetProperty(d => d.CategorieId, categorieId));

        return result > 0
            ? Result.Ok()
            : Result.Fail("Dépense fixe non trouvée");
    }

    public async Task<Result> ChangeBeginDate(int id, DateTime beginDate)
    {
        Log.Information("Changement de date de début dépense fixe ID {Id}", id);

        var depense = await context.DepenseFixes
            .Include(d => d.DueDates)
            .Include(d => d.Rappels)
            .SingleOrDefaultAsync(d => d.Id == id);

        if (depense is null)
            return Result.Fail("Dépense fixe non trouvée");

        SetDates(depense, beginDate);
        await context.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> ActiverEchelonnement(int id, int nombreEcheances, decimal montantParEcheance, int userId)
    {
        Log.Information("Activation échelonnement dépense fixe ID {Id} : {Nb}x{Montant}", id, nombreEcheances, montantParEcheance);

        var depense = await context.DepenseFixes
            .SingleOrDefaultAsync(d => d.Id == id && d.UserId == userId);

        if (depense is null)
            return Result.Fail("Dépense fixe non trouvée");

        depense.IsEchelonne = true;
        depense.NombreEcheances = nombreEcheances;
        depense.MontantParEcheance = montantParEcheance;
        depense.EcheancesRestantes = nombreEcheances;

        await context.SaveChangesAsync();
        return Result.Ok();
    }

    public void GenerateNextDates(DepenseFixe depense, DateTime startDate)
    {
        // Ne pas générer si la dépense a une date de fin dépassée
        if (depense.DateFin.HasValue && depense.DateFin.Value < DateTime.Today)
            return;

        var date = startDate;

        for (int i = 0; i < 3; i++) // horizon de sécurité
        {
            date = depense.Frequence switch
            {
                Frequence.Mensuel => date.AddMonths(1),
                Frequence.Trimestriel => date.AddMonths(3),
                Frequence.Biannuel => date.AddMonths(6),
                Frequence.Annuel => date.AddYears(1),
                _ => throw new ArgumentOutOfRangeException()
            };

            // Ne pas générer de dates au-delà de DateFin
            if (depense.DateFin.HasValue && date > depense.DateFin.Value)
                break;

            depense.DueDates.Add(new DepenseDueDate
            {
                Date = date,
                MontantEffectif = depense.Montant
            });

            if (!depense.EstDomiciliee)
            {
                foreach (var rappel in SetRappels(date, depense.ReminderDaysBefore))
                {
                    depense.Rappels.Add(rappel);
                }
            }
        }
    }

    /* =======================
     * PRIVATE LOGIC
     * ======================= */

    private void SetDates(DepenseFixe depense, DateTime beginDate)
    {
        depense.DueDates.Clear();
        depense.Rappels.Clear();

        var date = beginDate;

        for (int i = 0; i < (int)depense.Frequence; i++)
        {
            // Ne pas générer de dates au-delà de DateFin
            if (depense.DateFin.HasValue && date > depense.DateFin.Value)
                break;

            depense.DueDates.Add(new DepenseDueDate
            {
                Date = date,
                MontantEffectif = depense.Montant
            });

            if (!depense.EstDomiciliee)
            {
                foreach (var rappel in SetRappels(date, depense.ReminderDaysBefore))
                {
                    depense.Rappels.Add(rappel);
                }
            }

            date = depense.Frequence switch
            {
                Frequence.Mensuel => date.AddMonths(1),
                Frequence.Trimestriel => date.AddMonths(3),
                Frequence.Biannuel => date.AddMonths(6),
                Frequence.Annuel => date.AddYears(1),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private static List<Rappel> SetRappels(DateTime date, int reminderDaysBefore)
        => new()
        {
            new() { RappelDate = date.AddDays(-reminderDaysBefore) },
            new() { RappelDate = date.AddDays(-1) },
            new() { RappelDate = date },
        };
}
