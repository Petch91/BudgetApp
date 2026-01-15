namespace Entities.Contracts.Dtos;

public record RapportMoisDto(
    int Mois,
    int Annee,
    decimal TotalRevenus,
    decimal TotalDepenses,
    decimal Solde,
    IReadOnlyList<RapportLigneDto> Lignes
);
