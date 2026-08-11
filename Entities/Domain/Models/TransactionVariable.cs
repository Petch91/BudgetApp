namespace Entities.Domain.Models;

public class TransactionVariable : Transaction
{
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }

    /// <summary>
    /// Reference unique du mouvement bancaire d'origine, quand la transaction provient
    /// d'un import d'extrait de compte. Null pour une saisie manuelle.
    /// Sert de cle anti-doublon lors des imports successifs.
    /// </summary>
    public string? ExternalRef { get; set; }
}