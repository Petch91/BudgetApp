using Entities.Domain.Models;

namespace Entities.Contracts.Dtos;

public record TransactionVariableDto
(
    int Id,
    string Intitule,
    decimal Montant,
    DateTime Date,
    TransactionType TransactionType,
    CategorieDto Categorie
);
