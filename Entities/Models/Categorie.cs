using Entities.Interfaces;

namespace Entities.Models;

public class Categorie : IModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public IEnumerable<Transaction>  Transactions { get; set; } = new List<Transaction>(); 
}