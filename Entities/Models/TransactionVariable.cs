namespace Entities.Models;

public class TransactionVariable : Transaction
{
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
}