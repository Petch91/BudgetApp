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
}