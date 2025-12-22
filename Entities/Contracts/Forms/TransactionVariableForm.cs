using Entities.Domain.Models;

namespace Entities.Contracts.Forms;

public class TransactionVariableForm
{
    public string Intitule { get; set; } = String.Empty;
    public decimal Montant { get; set; }
    public int CategorieId { get; set; }
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
}