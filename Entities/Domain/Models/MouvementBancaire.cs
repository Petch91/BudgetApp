namespace Entities.Domain.Models;

/// <summary>
/// Une ligne brute d'extrait de compte, telle que produite par un parser bancaire.
/// Aucune logique metier : c'est la representation fidele du mouvement lu dans le fichier.
/// </summary>
public class MouvementBancaire
{
    /// <summary>Date valeur du mouvement (celle utilisee pour la transaction).</summary>
    public DateTime DateValeur { get; set; }

    /// <summary>Date de comptabilisation.</summary>
    public DateTime DateComptabilisation { get; set; }

    /// <summary>Montant signe : negatif pour un debit, positif pour un credit.</summary>
    public decimal Montant { get; set; }

    /// <summary>Libelle complet de la transaction (colonne "Transaction").</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Nom de la contrepartie / du marchand quand la banque le fournit.</summary>
    public string? NomContrepartie { get; set; }

    /// <summary>IBAN de la contrepartie quand il est fourni.</summary>
    public string? CompteContrepartie { get; set; }

    /// <summary>Communication structuree ou libre.</summary>
    public string? Communication { get; set; }

    /// <summary>
    /// Identifiant unique du mouvement, derive de la reference bancaire quand elle existe.
    /// Sert de cle anti-doublon lors des imports successifs.
    /// </summary>
    public string ExternalRef { get; set; } = string.Empty;

    /// <summary>Nature deduite du mouvement.</summary>
    public MouvementBancaireType Type { get; set; }

    /// <summary>True si le mouvement est une entree d'argent.</summary>
    public bool IsRevenu => Montant > 0;
}
