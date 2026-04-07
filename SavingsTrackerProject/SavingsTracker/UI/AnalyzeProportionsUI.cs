using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

/// <summary>
/// Provides a visual breakdown of user spending habits using charts and tables.
/// </summary>
public static class AnalyzeProportionsUI
{
    /// <summary>
    /// Calculates spending per category and renders a breakdown chart and summary table.
    /// </summary>
    /// <param name="user">The user whose transactions will be analyzed.</param>
    /// <param name="renderHeader">A delegate to draw the standard application header.</param>
    public static void Show(User user, Action renderHeader)
    {
        AnsiConsole.Clear();
        renderHeader();

        if (user.DailyTransactions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]! No transactions found to analyze.[/]");
            AnsiConsole.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            return;
        }

        var proportions = user.DailyTransactions
            .GroupBy(t => t.Category)
            .Select(g => new 
            { 
                Category = g.Key, 
                TotalAmount = g.Sum(t => t.Amount) 
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToList();

        double grandTotal = proportions.Sum(x => x.TotalAmount);

        var chart = new BreakdownChart()
            .Width(60)
            .FullSize();

        Color[] proportionColors = { 
            Color.Teal, Color.MediumPurple, Color.Gold1, 
            Color.IndianRed, Color.Lime, Color.DeepSkyBlue1 
        };

        var table = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        table.AddColumn("[grey]Category[/]");
        table.AddColumn(new TableColumn("[grey]Total Expenditure[/]").RightAligned());
        table.AddColumn(new TableColumn("[grey]Proportion (%)[/]").RightAligned());

        int colorIdx = 0;
        foreach (var item in proportions)
        {
            var color = proportionColors[colorIdx % proportionColors.Length];
            double percent = (item.TotalAmount / grandTotal) * 100;

            chart.AddItem(item.Category, item.TotalAmount, color);
            table.AddRow(item.Category, $"{item.TotalAmount:C2}", $"{percent:F1}%");
            
            colorIdx++;
        }

        AnsiConsole.Write(new Panel(new Rows(
            new Text($"Total Expenditure Analysis: {grandTotal:C2}", new Style(foreground: Palette.Brand, decoration: Decoration.Bold)),
            new Padder(chart, new Padding(0, 1, 0, 1)),
            table
        ))
        .Header(" Spending Proportions ")
        .BorderColor(Palette.Border)
        .Padding(2, 1, 2, 1));

        AnsiConsole.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }
}
