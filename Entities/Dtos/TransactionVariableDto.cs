using Entities.Models;

namespace Entities.Dtos;

public class TransactionVariableDto
{
    public int Id { get; set; }
    public string Intitule { get; set; } = String.Empty;
    public decimal Montant { get; set; }
    public CategorieDto Categorie { get; set; }
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
}