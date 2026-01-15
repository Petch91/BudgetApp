namespace Entities.Contracts.Dtos;

public record RapportLigneDto(
    DateTime Date,
    string Intitule,
    decimal Montant,
    CategorieDto Categorie,
    bool IsRevenu,
    bool IsDepenseFixe
);
