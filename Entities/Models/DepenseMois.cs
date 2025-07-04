using Entities.Interfaces;

namespace Entities.Models;

public class DepenseMois : IModel
{
    public int Id { get; set; }
    public int Montant { get; set; }
    public int Mois { get; set; }
    public int Annee {get; set;}
    public TransactionType TransactionType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}