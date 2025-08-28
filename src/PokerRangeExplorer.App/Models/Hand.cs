namespace PokerRangeExplorer.App.Models;

public class Hand
{
    public string Name { get; set; } = string.Empty;
    public bool? Suited { get; set; }
    public string Action { get; set; } = string.Empty;
    public int Percentage { get; set; }

    public string DisplayName => Suited switch
    {
        true => $"{Name}s",
        false => $"{Name}o",
        null => Name
    };
}
