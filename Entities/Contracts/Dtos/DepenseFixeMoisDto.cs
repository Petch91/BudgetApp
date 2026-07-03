namespace Entities.Contracts.Dtos;

/// <summary>
/// Une occurrence de dépense fixe tombant sur un mois donné (pour l'export CSV).
/// Couvre les dépenses récurrentes (mensuel/trimestriel/biannuel/annuel dont l'échéance
/// tombe ce mois) et les échéances échelonnées prévues ce mois.
/// </summary>
public record DepenseFixeMoisDto(
    DateTime Date,
    string Intitule,
    string Categorie,
    string Frequence,
    int? NumeroEcheance,
    int? TotalEcheances,
    decimal Montant
);
