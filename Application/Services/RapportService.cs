using Application.Interfaces;
using Application.Persistence;
using Entities.Contracts.Dtos;
using Entities.Domain.Models;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Application.Services;

public class RapportService(MyDbContext context) : IRapportService
{
    public async Task<Result<RapportMoisDto>> GetRapportMois(int annee, int mois, int userId)
    {
        Log.Information("Récupération rapport {Mois}/{Annee}", mois, annee);

        var lignes = new List<RapportLigneDto>();

        // 1. Récupérer les DueDates du mois (dépenses fixes)
        var dueDates = await context.depenseDueDates
            .Include(dd => dd.Depense)
                .ThenInclude(d => d.Categorie)
            .Where(dd => dd.Date.Month == mois && dd.Date.Year == annee && dd.Depense.UserId == userId && dd.Depense.IsEchelonne == false)
            .ToListAsync();

        foreach (var dueDate in dueDates)
        {
            var montant = dueDate.MontantEffectif ?? dueDate.Depense.Montant;

            lignes.Add(new RapportLigneDto(
                Date: dueDate.Date,
                Intitule: dueDate.Depense.Intitule,
                Montant: montant,
                Categorie: new CategorieDto(
                    dueDate.Depense.Categorie.Id,
                    dueDate.Depense.Categorie.Name,
                    dueDate.Depense.Categorie.Icon
                ),
                IsRevenu: false,
                IsDepenseFixe: true
            ));
        }

        // 2. Récupérer les TransactionVariable du mois
        var transactions = await context.TransactionsVariables
            .Include(t => t.Categorie)
            .Where(t => t.Date.Month == mois && t.Date.Year == annee && t.UserId == userId)
            .ToListAsync();

        foreach (var transaction in transactions)
        {
            lignes.Add(new RapportLigneDto(
                Date: transaction.Date,
                Intitule: transaction.Intitule,
                Montant: transaction.Montant,
                Categorie: new CategorieDto(
                    transaction.Categorie.Id,
                    transaction.Categorie.Name,
                    transaction.Categorie.Icon
                ),
                IsRevenu: transaction.TransactionType == TransactionType.Revenu,
                IsDepenseFixe: false
            ));
        }

        // 3. Trier par date
        lignes = lignes.OrderBy(l => l.Date).ToList();

        // 4. Calculer les totaux
        var totalRevenus = lignes.Where(l => l.IsRevenu).Sum(l => l.Montant);
        var totalDepenses = lignes.Where(l => !l.IsRevenu).Sum(l => l.Montant);
        var solde = totalRevenus - totalDepenses;

        return Result.Ok(new RapportMoisDto(
            Mois: mois,
            Annee: annee,
            TotalRevenus: totalRevenus,
            TotalDepenses: totalDepenses,
            Solde: solde,
            Lignes: lignes
        ));
    }
}
