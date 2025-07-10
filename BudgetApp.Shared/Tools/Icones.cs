using System.Text.Json;

namespace BudgetApp.Shared.Tools;

public static class BootstrapIconList
{
    public static readonly List<string> Icons;

    static BootstrapIconList()
    {
        string path = Path.Combine("icons-names.json");
        string json = File.ReadAllText(path);
        Icons = JsonSerializer.Deserialize<List<string>>(json);
    }
}