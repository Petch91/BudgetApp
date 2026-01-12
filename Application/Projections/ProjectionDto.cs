using System.Linq.Expressions;
using Entities.Contracts.Dtos;
using Entities.Domain.Models;

namespace Application.Projections;

public static class ProjectionDto
{
    public static Expression<Func<DepenseFixe, DepenseFixeDto>> DepenseFixeAsDto =>
        d => new DepenseFixeDto(
            d.Id,
            d.Intitule,
            d.Montant,
            new CategorieDto(
                d.Categorie.Id,
                d.Categorie.Name,
                d.Categorie.Icon
            ),
            d.Frequence,
            d.EstDomiciliee,
            d.DueDates
                .Select(dd => new DepenseDueDateDto(dd.Date))
                .ToList(),
            d.ReminderDaysBefore,
            d.Rappels
                .Select(r => new RappelDto(
                    r.Id,
                    r.RappelDate,
                    r.Vu
                ))
                .ToList()
        );
    
    public static Expression<Func<TransactionVariable, TransactionVariableDto>> TransactionAsDto =>
        t => new TransactionVariableDto(
            t.Id,
            t.Intitule,
            t.Montant,
            t.Date,
            t.TransactionType,
            new CategorieDto(
                t.Categorie.Id,
                t.Categorie.Name,
                t.Categorie.Icon
            )
        );
    public static Expression<Func<Categorie, CategorieDto>> CategorieAsDto =>
        c => new CategorieDto(
            c.Id,
            c.Name,
            c.Icon
        );

    public static Expression<Func<Rappel, RappelDto>> RappelAsDto =>
        r => new RappelDto(
            r.Id,
            r.RappelDate,
            r.Vu
        );
}