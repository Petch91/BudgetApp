namespace BudgetApp.Mobile.Models.Local;

public class SyncMetadata
{
    public string EntityType { get; set; } = string.Empty;

    public DateTime? LastSyncTimestamp { get; set; }

    public DateTime? LastServerTimestamp { get; set; }
}
