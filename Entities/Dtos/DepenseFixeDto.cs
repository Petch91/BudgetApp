using Entities.Models;

namespace Entities.Dtos;

public class DepenseFixeDto
{
    public int Id { get; set; }
    public string Intitule { get; set; } = String.Empty;
    public decimal Montant { get; set; }
    public CategorieDto Categorie { get; set; }
    public Frequence Frequence { get; set; }
    public bool EstDomiciliee { get; set; }
    public List<DateTime> DueDates { get; set; }
    public int ReminderDaysBefore { get; set; } = 3;
    public List<RappelDto> Rappels { get; set; } = new List<RappelDto>();
}