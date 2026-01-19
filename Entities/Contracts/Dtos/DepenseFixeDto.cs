using Entities.Domain.Models;

namespace Entities.Contracts.Dtos;

public record DepenseFixeDto
(
    int Id,
    string Intitule,
    decimal Montant,
    CategorieDto Categorie,
    Frequence Frequence,
    bool EstDomiciliee,
    IReadOnlyList<DepenseDueDateDto> DueDates,
    int ReminderDaysBefore,
    IReadOnlyList<RappelDto> Rappels,
    DateTime? DateFin,
    bool IsActive
);