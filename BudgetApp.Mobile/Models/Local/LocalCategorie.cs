namespace BudgetApp.Mobile.Models.Local;

public class LocalCategorie
{
    /// <summary>
    /// Same ID as server - categories are read-only cache
    /// </summary>
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Icon { get; set; }

    // Navigation property
    public ICollection<LocalTransaction> Transactions { get; set; } = [];
}
