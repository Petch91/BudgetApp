using System.Linq.Expressions;
using Entities.Dtos;
using Entities.Models;

namespace Entities.Mappers;

public static class MapperDto
{
    public static DepenseFixeDto ToDto(this DepenseFixe depenseFixe)
    {
        return new DepenseFixeDto
        {
            Id = depenseFixe.Id,
            Intitule = depenseFixe.Intitule,
            Montant = depenseFixe.Montant,
            EstDomiciliee = depenseFixe.EstDomiciliée,
            ReminderDaysBefore = depenseFixe.ReminderDaysBefore,
        };
    }

    public static DepenseFixe ToDb(this DepenseFixeDto depenseFixe)
    {
        return new DepenseFixe
        {
            Id = depenseFixe.Id,
            Intitule = depenseFixe.Intitule,
            Montant = depenseFixe.Montant,
            EstDomiciliée = depenseFixe.EstDomiciliee,
            ReminderDaysBefore = depenseFixe.ReminderDaysBefore,
            
        };
    }

    public static RappelDto ToDto(this Rappel rappel)
    {
        return new RappelDto
        {
            Id = rappel.Id,
            RappelDate = rappel.RappelDate,
        };
    }

}