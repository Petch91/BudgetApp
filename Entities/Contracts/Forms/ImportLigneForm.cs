namespace Entities.Contracts.Forms;

/// <summary>
/// Une ligne d'extrait validee par l'utilisateur, prete a etre inseree.
/// </summary>
public class ImportLigneForm
{
    public string ExternalRef { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Intitule { get; set; } = string.Empty;

    /// <summary>Montant absolu (le sens est porte par <see cref="IsRevenu"/>).</summary>
    public decimal Montant { get; set; }

    public bool IsRevenu { get; set; }
    public int CategorieId { get; set; }
}
