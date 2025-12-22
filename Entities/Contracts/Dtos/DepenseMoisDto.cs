namespace Entities.Contracts.Dtos;

public record DepenseMoisDto
(
    int Id,
    decimal Montant,
    int Mois,
    int Annee,
    string Type
);
