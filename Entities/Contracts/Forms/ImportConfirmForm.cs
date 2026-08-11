namespace Entities.Contracts.Forms;

/// <summary>
/// Requete de confirmation d'import : uniquement les lignes cochees par l'utilisateur.
/// </summary>
public class ImportConfirmForm
{
    public List<ImportLigneForm> Lignes { get; set; } = [];
}
