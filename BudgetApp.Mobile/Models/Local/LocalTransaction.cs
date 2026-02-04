using Entities.Domain.Models;

namespace BudgetApp.Mobile.Models.Local;

public class LocalTransaction
{
    public int Id { get; set; }

    /// <summary>
    /// GUID generated offline for new transactions
    /// </summary>
    public string LocalId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Server ID - NULL until synced
    /// </summary>
    public int? ServerId { get; set; }

    public string Intitule { get; set; } = string.Empty;

    public decimal Montant { get; set; }

    public DateTime Date { get; set; } = DateTime.Today;

    public TransactionType TransactionType { get; set; } = TransactionType.Depense;

    public int? CategorieId { get; set; }

    public int UserId { get; set; }

    // Sync tracking
    public SyncState SyncState { get; set; } = SyncState.Synced;

    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public DateTime? ServerLastModified { get; set; }

    // Navigation property
    public LocalCategorie? Categorie { get; set; }
}

public enum SyncState
{
    Synced = 0,
    PendingCreate = 1,
    PendingUpdate = 2,
    PendingDelete = 3
}
