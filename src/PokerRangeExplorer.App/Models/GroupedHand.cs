namespace PokerRangeExplorer.App.Models;

public class GroupedHand
{
    public string Name { get; set; } = string.Empty;
    public bool? Suited { get; set; }
    public List<ActionOption> Options { get; set; } = new();

    public string DisplayName => Suited switch
    {
        true => $"{Name}s",
        false => $"{Name}o",
        null => Name
    };
}
