using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;

namespace PokerRangeExplorer.App.Models;

public class HandCell
{
    public string Name { get; set; } = string.Empty;   // AK, AQ, 99...
    public bool? Suited { get; set; }
    public List<ActionOption> Options { get; set; } = new();

    public string DisplayName => Suited switch
    {
        true => $"{Name}s",
        false => $"{Name}o",
        null => Name
    };

    public Brush CellBrush
    {
        get
        {
            if (!Options.Any()) return System.Windows.Media.Brushes.LightGray;

            // Si una sola acción => color único
            if (Options.Count == 1)
                return GetBrushForAction(Options[0].Action);

            // Si varias => crear un brush compuesto
            return CreateMultiColorBrush();
        }
    }

    public Brush TextBrush
    {
        get
        {
            // Si no hay opciones, el fondo es LightGray, el texto negro está bien.
            if (!Options.Any()) return System.Windows.Media.Brushes.Black;

            // Para una sola opción, calculamos la luminancia del color de fondo
            if (Options.Count == 1)
            {
                var color = ((SolidColorBrush)GetBrushForAction(Options[0].Action)).Color;
                return GetContrastColor(color);
            }

            // Para múltiples opciones, podemos usar un color fijo o intentar algo más complejo.
            // Por simplicidad, para múltiples colores, usaremos negro por defecto.
            // Si se quiere más precisión, habría que calcular la luminancia promedio o dominante.
            return System.Windows.Media.Brushes.Black;
        }
    }

    private Brush GetBrushForAction(string action)
    {
        // Mapeo básico acción -> color
        return action.ToLower() switch
        {
            var a when a.Contains("raise") => new SolidColorBrush(System.Windows.Media.Color.FromRgb(173, 216, 230)), // Azul claro
            var a when a.Contains("call") => new SolidColorBrush(System.Windows.Media.Color.FromRgb(144, 238, 144)), // Verde claro
            var a when a.Contains("fold") => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 182, 193)), // Rosa claro
            var a when a.Contains("check") => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 224)), // Amarillo claro
            _ => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 228, 181)) // Naranja pastel
        };
    }

    private Brush CreateMultiColorBrush()
    {
        var brush = new LinearGradientBrush();
        brush.StartPoint = new System.Windows.Point(0, 0);
        brush.EndPoint = new System.Windows.Point(1, 0); // Horizontal

        double cumulative = 0;
        foreach (var option in Options)
        {
            var color = ((SolidColorBrush)GetBrushForAction(option.Action)).Color;
            double stop = cumulative + option.Percentage / 100.0;
            brush.GradientStops.Add(new GradientStop(color, cumulative));
            brush.GradientStops.Add(new GradientStop(color, stop));
            cumulative = stop;
        }

        return brush;
    }

    private Brush GetContrastColor(Color backgroundColor)
    {
        // Fórmula de luminancia (ITU-R BT.709)
        double luminance = (0.2126 * backgroundColor.R + 0.7152 * backgroundColor.G + 0.0722 * backgroundColor.B) / 255;
        return luminance > 0.5 ? Brushes.Black : Brushes.White;
    }
}
