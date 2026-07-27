namespace CybsClient.Model.CyberStore;

public class FilterGroup
{
    public string Name { get; set; } = "";
    public bool IsOpen { get; set; } = false;
    public bool HasSearch { get; set; } = false;
    public string SearchText { get; set; } = "";
    public List<FilterOption> Options { get; set; } = new();
}

public class FilterOption
{
    public string Label { get; set; } = "";
    public int? Count { get; set; }
    public bool IsChecked { get; set; }
}
