using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class ViewOrganizedUI
{
    public static void ViewOrganizedSpending(User user, Action renderHeader)
    {
        if (user.DailyTransactions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]! No transactions recorded yet.[/]");
            Thread.Sleep(2000);
            return;
        }

        AnsiConsole.Clear();
        renderHeader();
        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Spending Analysis[/]").Centered());

        var categoryTotals = user.DailyTransactions
            .GroupBy(t => t.Category)
            .Select(group => new { 
                Category = group.Key, 
                Total = group.Sum(t => t.Amount) 
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        double grandTotal = categoryTotals.Sum(x => x.Total);

        var chart = new BreakdownChart().FullSize();
        
        Color[] chartColors = { Palette.Brand, Palette.Accent, Palette.Border, Palette.StatusBar, Color.Grey };
        int colorIndex = 0;

        foreach (var item in categoryTotals)
        {
            chart.AddItem(item.Category, item.Total, chartColors[colorIndex % chartColors.Length]);
            colorIndex++;
        }

        //Summary Table
        var summaryTable = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        summaryTable.AddColumn("Category");
        summaryTable.AddColumn(new TableColumn("Amount").RightAligned());
        summaryTable.AddColumn(new TableColumn("Percent").RightAligned());

        foreach (var item in categoryTotals)
        {
            double percent = (item.Total / grandTotal) * 100;
            summaryTable.AddRow(item.Category, $"{item.Total:C2}", $"{percent:F1}%");
        }

        AnsiConsole.Write(new Panel(new Rows(
            new Text($"Total Categorized Spending: {grandTotal:C2}", new Style(Palette.Brand, decoration: Decoration.Bold)),
            new Padder(chart, new Padding(0, 1, 0, 1)),
            summaryTable
        ))
        .Header(" Spending by Category ")
        .BorderColor(Palette.Border)
        .Padding(2, 1, 2, 1));

        AnsiConsole.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }
}
