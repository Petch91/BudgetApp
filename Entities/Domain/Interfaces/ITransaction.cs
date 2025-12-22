using Entities.Domain.Models;

namespace Entities.Domain.Interfaces;

public interface ITransaction : IModel
{
    public string Intitule { get; set; } 
    public decimal Montant { get; set; }
    public int CategorieId { get; set; }
    public Categorie Categorie { get; set; }
}