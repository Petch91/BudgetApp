namespace Entities.Domain.Models;

/// <summary>
/// Nature d'un mouvement bancaire, deduite du libelle de l'extrait.
/// Sert a decider si le mouvement doit etre importe comme transaction variable
/// ou s'il correspond deja a une depense fixe.
/// </summary>
public enum MouvementBancaireType
{
    /// <summary>Achat par carte (Bancontact, DebitMastercard).</summary>
    AchatCarte,

    /// <summary>Retrait d'especes.</summary>
    Cash,

    /// <summary>Virement sortant ponctuel.</summary>
    Virement,

    /// <summary>Versement entrant (salaire, remboursement, Wero...).</summary>
    Versement,

    /// <summary>Remboursement d'un achat carte.</summary>
    Remboursement,

    /// <summary>Domiciliation europeenne : correspond generalement a une depense fixe.</summary>
    Domiciliation,

    /// <summary>Ordre permanent : correspond generalement a une depense fixe.</summary>
    OrdrePermanent,

    /// <summary>Remboursement de credit : correspond generalement a une depense fixe.</summary>
    Credit,

    /// <summary>Frais bancaires.</summary>
    Frais,

    /// <summary>Interets crediteurs ou debiteurs.</summary>
    Interet,

    /// <summary>Type non reconnu.</summary>
    Autre
}
