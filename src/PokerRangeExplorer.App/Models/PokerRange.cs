namespace PokerRangeExplorer.App.Models;

public class PokerRange
{
    public string Name { get; set; } = string.Empty;
    public List<Position> Positions { get; set; } = new();
}
