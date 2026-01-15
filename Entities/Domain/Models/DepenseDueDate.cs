using Entities.Domain.Interfaces;

namespace Entities.Domain.Models;

public class DepenseDueDate : IModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal? MontantEffectif { get; set; }
    public int DepenseId { get; set; }
    public DepenseFixe Depense { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}