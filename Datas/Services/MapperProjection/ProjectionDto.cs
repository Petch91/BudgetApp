using System.Linq.Expressions;
using Entities.Dtos;
using Entities.Models;

namespace Datas.Services.MapperProjection;

public static class ProjectionDto
{
    public static Expression<Func<DepenseFixe, DepenseFixeDto>> DepenseFixeAsDto => d => new DepenseFixeDto
    {
        Id = d.Id,
        Intitule = d.Intitule,
        Montant = d.Montant,
        Frequence = d.Frequence,
        Categorie = new CategorieDto{ Id = d.Categorie.Id, Name = d.Categorie.Name},
        EstDomiciliée = d.EstDomiciliée,
        ReminderDaysBefore = d.ReminderDaysBefore,
        DueDates = d.DueDates.Select(date => date.Date).ToList(),
        Rappels = d.Rappels.Select(r => new RappelDto
        {
            Id = r.Id,
            Vu = r.Vu,
            RappelDate = r.RappelDate,
        }).ToList()
    };
    
    public static Expression<Func<TransactionVariable, TransactionVariableDto>> TransactionAsDto => t => new TransactionVariableDto
    {
        Id = t.Id,
        Intitule = t.Intitule,
        Montant = t.Montant,
        Categorie = new CategorieDto{ Id = t.Categorie.Id, Name = t.Categorie.Name},
        TransactionType = t.TransactionType,
        Date = t.Date,
    };
    public static Expression<Func<Categorie, CategorieDto>> CategorieAsDto => categorie => new CategorieDto
    {
        Id = categorie.Id,
        Name = categorie.Name,
    };

    public static Expression<Func<Rappel, RappelDto>> RappelAsDto => r => new RappelDto
    {
        Id = r.Id,
        RappelDate = r.RappelDate,
        Vu = r.Vu,
    };
}