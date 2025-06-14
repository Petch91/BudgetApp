using Entities.Interfaces;

namespace Entities.Models;

public class DepenseVariable : ITransaction
{
    public int Id { get; set; }
    public string Intitule { get; set; }
    public decimal Montant { get; set; }
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}