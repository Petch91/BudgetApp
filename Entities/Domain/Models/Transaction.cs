using Entities.Domain.Interfaces;

namespace Entities.Domain.Models;

public class Transaction : ITransaction
{
    public int Id { get; set; }
    public string Intitule { get; set; } = String.Empty;
    public decimal Montant { get; set; }
    public int CategorieId { get; set; }
    public Categorie Categorie { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public enum TransactionType
{
    Revenu,
    Depense,
}