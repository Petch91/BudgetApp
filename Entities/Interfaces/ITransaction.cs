namespace Entities.Interfaces;

public interface ITransaction : IModel
{
    public string Intitule { get; set; } 
    public decimal Montant { get; set; }
}