using Entities.Dtos;
using Entities.Models;

namespace Entities.Forms;

public class TransactionVariableForm
{
    public string Intitule { get; set; } = String.Empty;
    public decimal Montant { get; set; }
    public CategorieForm Categorie { get; set; }
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
}