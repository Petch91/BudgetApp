using Entities.Contracts.Dtos;
using Entities.Domain.Models;

namespace Entities.Contracts.Forms;

public class DepenseFixeForm
{
    public string Intitule { get; set; } = String.Empty;
    public decimal Montant { get; set; }
    public CategorieDto Categorie { get; set; }
    public Frequence Frequence { get; set; }
    public bool EstDomiciliee { get; set; }
    public DateTime BeginDate { get; set; }
    public int ReminderDaysBefore { get; set; } = 3;

    /// <summary>
    /// Date de fin optionnelle pour les dépenses temporaires (crédits, abonnements limités).
    /// Null = dépense permanente.
    /// </summary>
    public DateTime? DateFin { get; set; }

    public bool IsEchelonne { get; set; }
    public int? NombreEcheances { get; set; }
    public decimal? MontantParEcheance { get; set; }
}