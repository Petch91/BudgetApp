namespace Entities.Domain.Models;

public class TransactionVariable : Transaction
{
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
}