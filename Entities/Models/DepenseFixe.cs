using Entities.Interfaces;

namespace Entities.Models;

public class DepenseFixe : Transaction
{
   
    public Frequence Frequence { get; set; }
    public bool EstDomiciliée { get; set; }
    public DateTime DueDate { get; set; }
    public int ReminderDaysBefore { get; set; } = 3;

    public ICollection<Rappel> Rappels { get; set; } = new List<Rappel>();
}

public enum Frequence
{
    Mensuel =12,
    Trimestriel = 4,
    Biannuel = 2,
    Annuel = 1
}