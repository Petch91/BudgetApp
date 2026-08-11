using Entities.Domain.Models;

namespace Entities.Contracts.Dtos;

/// <summary>
/// Une ligne d'extrait analysee, proposee a l'utilisateur avant import.
/// Rien n'est ecrit en base tant que l'utilisateur n'a pas valide.
/// </summary>
public record ImportedTransactionDto(
    string ExternalRef,
    DateTime Date,
    string Intitule,
    string Description,
    decimal Montant,
    bool IsRevenu,
    MouvementBancaireType Type,
    string TypeLabel,
    int CategorieId,
    string CategorieName,
    bool IsDuplicate,
    bool IsLikelyDepenseFixe,
    bool Include
);
