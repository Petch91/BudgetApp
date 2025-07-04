using Entities.Interfaces;

namespace Entities.Models;

public class DepenseDueDate : IModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int DepenseId { get; set; }
    public DepenseFixe Depense { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}