using System.Text.Json;

namespace BudgetApp.Shared.Tools;

public static class BootstrapIconList
{
    public static readonly List<BootIcone> Icons;

    static BootstrapIconList()
    {
        string path = Path.Combine("icons-names.json");
        string json = File.ReadAllText(path);
        Icons = JsonSerializer.Deserialize<List<BootIcone>>(json);
    }
}

public class BootIcone
{
    public string Name { get; set; }
    public string IconeName { get; set; }
}