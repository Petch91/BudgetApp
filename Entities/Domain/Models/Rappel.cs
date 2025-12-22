using Entities.Domain.Interfaces;

namespace Entities.Domain.Models;

public class Rappel : IModel
{
    public int Id { get; set; }
    public int DepenseFixeId { get; set; }
    public DepenseFixe DepenseFixe { get; set; } = null!;

    public DateTime RappelDate { get; set; }
    public bool Vu { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}