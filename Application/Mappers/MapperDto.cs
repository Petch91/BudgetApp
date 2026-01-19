using Entities.Contracts.Dtos;
using Entities.Domain.Models;

namespace Application.Mappers;

public static class MapperDto
{
    public static DepenseFixeDto ToDto(this DepenseFixe d)
        => new(
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
                .Select(dd => new DepenseDueDateDto(dd.Id, dd.Date, dd.MontantEffectif))
                .ToList(),
            d.ReminderDaysBefore,
            d.Rappels
                .Select(r => r.ToDto())
                .ToList(),
            d.DateFin,
            d.IsActive
        );

    public static RappelDto ToDto(this Rappel r)
        => new(
            r.Id,
            r.RappelDate,
            r.Vu
        );
}