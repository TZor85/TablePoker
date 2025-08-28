using Microsoft.Win32;
using PokerRangeExplorer.App.Models;
using PokerRangeExplorer.App.Services;
using System.Windows;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace PokerRangeExplorer.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly RangeService _rangeService = new();
    private readonly List<PokerRange> _ranges = new();
    private List<HandCell> _currentHandCells = new();

    public MainWindow()
    {
        InitializeComponent();
        UpdateUI();
    }

    private async void LoadFiles_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            Multiselect = true,
            Title = "Seleccionar archivos JSON de rangos"
        };

        if (dialog.ShowDialog() == true)
        {
            await LoadRangesAsync(dialog.FileNames);
        }
    }

    private async void LoadDirectory_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Seleccionar carpeta con archivos JSON",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            var ranges = await _rangeService.LoadRangesFromDirectoryAsync(dialog.SelectedPath);

            if (ranges.Any())
            {
                _ranges.AddRange(ranges);
                UpdateUI();
                //StatusText.Text = $"{ranges.Count} rangos cargados desde carpeta";
                //ErrorText.Text = "";
            }
            else
            {
                //ErrorText.Text = "No se encontraron archivos JSON válidos en la carpeta";
                //StatusText.Text = "";
            }
        }
    }

    private async Task LoadRangesAsync(string[] filePaths)
    {
        //StatusText.Text = "Cargando...";
        //ErrorText.Text = "";

        var newRanges = await _rangeService.LoadMultipleRangesAsync(filePaths);
        var errorCount = filePaths.Length - newRanges.Count;

        if (newRanges.Any())
        {
            _ranges.AddRange(newRanges);
            UpdateUI();
            //StatusText.Text = $"{newRanges.Count} rangos cargados correctamente";

            if (errorCount > 0)
            {
                //ErrorText.Text = $"{errorCount} archivos no pudieron cargarse";
            }
            else
            {
                //ErrorText.Text = "";
            }
        }
        else
        {
            //ErrorText.Text = "No se pudo cargar ningún archivo";
            //StatusText.Text = "";
        }
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _ranges.Clear();
        UpdateUI();
        //StatusText.Text = "Rangos eliminados";
        //ErrorText.Text = "";
    }

    private void UpdateUI()
    {
        RangeListBox.ItemsSource = null;
        RangeListBox.ItemsSource = _ranges.Select(r => r.Name).ToList();

        RangeTitle.Text = "";
        ClearHandMatrix();
        SelectedHandResult.Text = "";
    }

    private void RangeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RangeListBox.SelectedIndex >= 0)
        {
            var selectedRange = _ranges[RangeListBox.SelectedIndex];
            RangeTitle.Text = selectedRange.Name;

            // HERO buttons
            HeroPanel.Children.Clear();
            var heroPositions = selectedRange.Positions
                .Select(p => p.HeroPosition)
                .Where(h => !string.IsNullOrEmpty(h))
                .Distinct()
                .ToList();

            foreach (var hero in heroPositions)
            {
                var btn = new Button
                {
                    Content = hero,
                    Margin = new Thickness(3),
                    Tag = hero
                };
                btn.Click += HeroButton_Click;
                HeroPanel.Children.Add(btn);
            }
        }
    }

    private void HeroButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            var hero = button.Tag.ToString();
            var selectedRange = _ranges[RangeListBox.SelectedIndex];

            // Villain buttons
            VillainPanel.Children.Clear();
            var contexts = selectedRange.Positions
                .Where(p => p.HeroPosition == hero)
                .ToList();

            foreach (var pos in contexts)
            {
                var btn = new Button
                {
                    Content = pos.VillainContext,
                    Margin = new Thickness(2),
                    Padding = new Thickness(5),
                    Tag = pos
                };
                btn.Click += VillainButton_Click;
                VillainPanel.Children.Add(btn);
            }
        }
    }

    private void VillainButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Position pos)
        {
            _currentHandCells = _rangeService.GroupHandsToMatrix(pos.Hands);
            RenderHandMatrix();
            SelectedHandResult.Text = "";
        }
    }

    private void Hero_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RangeListBox.SelectedIndex >= 0 && HeroComboBox.SelectedIndex >= 0)
        {
            var selectedRange = _ranges[RangeListBox.SelectedIndex];
            var hero = HeroComboBox.SelectedItem?.ToString();

            // Filtrar solo las posiciones de ese héroe
            var villanos = selectedRange.Positions
                .Where(p => p.HeroPosition == hero)
                .ToList();

            VillainComboBox.ItemsSource = villanos;
            VillainComboBox.DisplayMemberPath = "VillainContext";
            if (villanos.Any())
                VillainComboBox.SelectedIndex = 0;
        }
    }

    private void Villain_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RangeListBox.SelectedIndex >= 0 &&
            HeroComboBox.SelectedIndex >= 0 &&
            VillainComboBox.SelectedIndex >= 0)
        {
            var selectedRange = _ranges[RangeListBox.SelectedIndex];
            var selectedPosition = (Position)VillainComboBox.SelectedItem;

            _currentHandCells = _rangeService.GroupHandsToMatrix(selectedPosition.Hands);
            RenderHandMatrix();
            SelectedHandResult.Text = "";
        }
    }

    private void RenderHandMatrix()
    {
        HandGrid.Children.Clear();

        var allPossibleHands = _rangeService.Generate13x13HandGrid();

        foreach (var handName in allPossibleHands)
        {
            var matchingCell = _currentHandCells.FirstOrDefault(h => h.DisplayName == handName);

            var button = new System.Windows.Controls.Button
            {
                Content = handName,
                Background = matchingCell != null && matchingCell.CellBrush != null ? matchingCell.CellBrush : System.Windows.Media.Brushes.White,
                Margin = new Thickness(1),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Tag = matchingCell
            };

            if (matchingCell != null)
            {
                button.Click += HandCell_Click;
                button.ToolTip = CreateHandTooltip(matchingCell);
            }
            else
            {
                button.IsEnabled = false;
                button.Opacity = 0.3;
            }

            HandGrid.Children.Add(button);
        }
    }

    private string CreateHandTooltip(HandCell handCell)
    {
        if (!handCell.Options.Any())
            return $"{handCell.DisplayName}: Sin acciones";

        var tooltip = $"{handCell.DisplayName}:\n";
        foreach (var option in handCell.Options)
        {
            tooltip += $"• {option.Action} ({option.Percentage}%)\n";
        }

        return tooltip.TrimEnd('\n');
    }

    private void HandCell_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button btn && btn.Tag is HandCell handCell)
        {
            if (!handCell.Options.Any())
            {
                SelectedHandResult.Text = $"{handCell.DisplayName} → Sin acciones";
                return;
            }

            var selectedAction = _rangeService.GetRandomAction(handCell.Options);
            if (selectedAction != null)
            {
                SelectedHandResult.Text = $"{handCell.DisplayName} → {selectedAction.Action}";
            }
            else
            {
                SelectedHandResult.Text = $"{handCell.DisplayName} → Error en selección";
            }
        }
    }

    private void ClearHandMatrix()
    {
        HandGrid.Children.Clear();
        _currentHandCells.Clear();
    }
}