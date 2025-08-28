namespace PokerRangeExplorer.App.Models;

public class Position
{
    public string Name { get; set; } = string.Empty;
    public string? HeroPosition { get; set; }
    public string? OpenRaiser { get; set; }
    public string? ThreeBetPosition { get; set; }
    public string? Caller { get; set; }
    public string? Limper { get; set; }
    public string? Squeezer { get; set; }
    public string? BetSize { get; set; }
    public string? RaiserFolds { get; set; }
    public bool? IsGreater { get; set; }

    public List<Hand> Hands { get; set; } = new();

    // 🔎 Propiedad para mostrar el contexto completo
    public string DisplayContext
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(HeroPosition)) parts.Add($"Hero: {HeroPosition}");
            if (!string.IsNullOrEmpty(OpenRaiser)) parts.Add($"OR: {OpenRaiser}");
            if (!string.IsNullOrEmpty(ThreeBetPosition)) parts.Add($"3Bet: {ThreeBetPosition}");
            if (!string.IsNullOrEmpty(Caller)) parts.Add($"Caller: {Caller}");
            if (!string.IsNullOrEmpty(Limper)) parts.Add($"Limper: {Limper}");
            if (!string.IsNullOrEmpty(Squeezer)) parts.Add($"Squeezer: {Squeezer}");
            if (!string.IsNullOrEmpty(BetSize)) parts.Add($"Bet: {BetSize}");
            if (!string.IsNullOrEmpty(RaiserFolds)) parts.Add($"RFolds: {RaiserFolds}");
            if (IsGreater.HasValue) parts.Add($"IsGreater: {IsGreater}");

            // fallback si no tiene nada
            if (parts.Count == 0 && !string.IsNullOrEmpty(Name))
                return Name;

            return string.Join(" | ", parts);
        }
    }

    public string VillainContext
    {
        get
        {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(OpenRaiser)) parts.Add($"OR: {OpenRaiser}");
            if (!string.IsNullOrEmpty(ThreeBetPosition)) parts.Add($"3Bet: {ThreeBetPosition}");
            if (!string.IsNullOrEmpty(Caller)) parts.Add($"Caller: {Caller}");
            if (!string.IsNullOrEmpty(Limper)) parts.Add($"Limper: {Limper}");
            if (!string.IsNullOrEmpty(Squeezer)) parts.Add($"Squeezer: {Squeezer}");
            if (!string.IsNullOrEmpty(BetSize)) parts.Add($"Bet: {BetSize}");
            if (!string.IsNullOrEmpty(RaiserFolds)) parts.Add($"RFolds: {RaiserFolds}");
            if (IsGreater.HasValue) parts.Add($"IsGreater: {IsGreater}");

            if (parts.Count == 0 && !string.IsNullOrEmpty(Name))
                return Name;

            return string.Join(" | ", parts);
        }
    }
}
