namespace Entities.Domain.Models;

public class DepenseFixe : Transaction
{
    public Frequence Frequence { get; set; }
    public bool EstDomiciliee { get; set; }
    public ICollection<DepenseDueDate> DueDates { get; set; }
    public int ReminderDaysBefore { get; set; } = 3;

    /// <summary>
    /// Date de fin pour les dépenses temporaires (crédits, abonnements limités).
    /// Null = dépense permanente/à vie.
    /// </summary>
    public DateTime? DateFin { get; set; }

    public ICollection<Rappel> Rappels { get; set; } = new List<Rappel>();

    /// <summary>
    /// Indique si la dépense est encore active (pas de DateFin ou DateFin non atteinte).
    /// </summary>
    public bool IsActive => DateFin == null || DateFin.Value >= DateTime.Today;
}

public enum Frequence
{
    Mensuel =12,
    Trimestriel = 4,
    Biannuel = 2,
    Annuel = 1
}