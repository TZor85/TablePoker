using PokerRangeExplorer.App.Models;
using System.IO;
using System.Text.Json;

namespace PokerRangeExplorer.App.Services;

public class RangeService
{
    private readonly Random _random = new();

    public async Task<List<PokerRange>> LoadMultipleRangesAsync(string[] filePaths)
    {
        var ranges = new List<PokerRange>();
        var tasks = filePaths.Select(LoadRangeFromFileAsync);

        var results = await Task.WhenAll(tasks);

        foreach (var range in results.Where(r => r != null))
        {
            ranges.Add(range!);
        }

        return ranges;
    }

    public async Task<PokerRange?> LoadRangeFromFileAsync(string filePath)
    {
        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var range = JsonSerializer.Deserialize<PokerRange>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (range != null && string.IsNullOrEmpty(range.Name))
            {
                range.Name = Path.GetFileNameWithoutExtension(filePath);
            }

            return range;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando {filePath}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<PokerRange>> LoadRangesFromDirectoryAsync(string directoryPath)
    {
        var jsonFiles = Directory.GetFiles(directoryPath, "*.json");
        return await LoadMultipleRangesAsync(jsonFiles);
    }

    public List<HandCell> GroupHandsToMatrix(List<Hand> hands)
    {
        var groupedHands = hands
            .GroupBy(h => new { h.Name, h.Suited })
            .Select(g => new HandCell
            {
                Name = g.Key.Name,
                Suited = g.Key.Suited,
                Options = g.Select(x => new ActionOption
                {
                    Action = x.Action,
                    Percentage = x.Percentage
                }).ToList()
            })
            .ToList();

        return groupedHands;
    }

    public ActionOption? GetRandomAction(List<ActionOption> options)
    {
        if (!options.Any()) return null;

        var totalWeight = options.Sum(o => o.Percentage);
        if (totalWeight == 0) return options.First();

        var roll = _random.NextDouble() * totalWeight;
        var cumulative = 0.0;

        foreach (var option in options)
        {
            cumulative += option.Percentage;
            if (roll <= cumulative)
                return option;
        }

        return options.Last();
    }

    public List<string> Generate13x13HandGrid()
    {
        var hands = new List<string>();
        var ranks = new[] { "A", "K", "Q", "J", "T", "9", "8", "7", "6", "5", "4", "3", "2" };

        for (int row = 0; row < 13; row++)
        {
            for (int col = 0; col < 13; col++)
            {
                if (row == col)
                {
                    // Parejas en la diagonal
                    hands.Add($"{ranks[row]}{ranks[col]}");
                }
                else if (row < col)
                {
                    // Suited en la parte superior
                    hands.Add($"{ranks[row]}{ranks[col]}s");
                }
                else
                {
                    // Offsuit en la parte inferior
                    hands.Add($"{ranks[col]}{ranks[row]}o");
                }
            }
        }

        return hands;
    }
}

