using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class MonthlyPerformanceUI
{
    public static void Show(User user, Action renderHeader)
    {
        AnsiConsole.Clear();
        renderHeader();

        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Monthly Performance Review[/]").Centered());

        var monthlyIncome = AnsiConsole.Prompt(
            new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Enter your total monthly income: $")
                .Validate(n => n >= 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Income cannot be negative[/]")));

        DateTime now = DateTime.Now;
        
        double currentMonthExpenses = user.DailyTransactions
            .Where(t => t.Date.Month == now.Month && t.Date.Year == now.Year)
            .Sum(t => t.Amount);

        double netCashFlow = monthlyIncome - currentMonthExpenses;

        RenderCashFlowSummary(monthlyIncome, currentMonthExpenses, netCashFlow);

        RenderSavingsComparison(user);

        AnsiConsole.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }

    private static void RenderCashFlowSummary(double income, double expenses, double net)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        table.AddColumn("[grey]Description[/]");
        table.AddColumn(new TableColumn("[grey]Amount[/]").RightAligned());

        table.AddRow("Total Monthly Income", $"{income:C2}");
        table.AddRow("Total Monthly Expenses", $"[red]-{expenses:C2}[/]");
        table.AddEmptyRow();
        
        string netColor = net >= 0 ? "green" : "red";
        table.AddRow("[bold]Remaining Net Cash Flow[/]", $"[bold {netColor}]{net:C2}[/]");

        AnsiConsole.Write(new Panel(table)
            .Header(" Current Month Cash Flow ")
            .BorderColor(Palette.Brand));
    }

    private static void RenderSavingsComparison(User user)
    {
        var chart = new BarChart()
            .Width(60)
            .Label("[white]Savings Comparison (Last 4 Months)[/]")
            .CenterLabel();

        // Calculate data for the last 4 months
        for (int i = 3; i >= 0; i--)
        {
            var monthDate = DateTime.Now.AddMonths(-i);
            
            // Calculate total contributions (savings) for that specific month
            double monthlySavings = user.Contributions
                .Where(c => c.Date.Month == monthDate.Month && c.Date.Year == monthDate.Year)
                .Sum(c => c.Amount);

            string label = monthDate.ToString("MMM yyyy");
            
            Color barColor = (i == 0) ? Palette.Brand : Color.Grey; 

            chart.AddItem(label, Math.Round(monthlySavings, 2), barColor);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(chart)
            .Header(" Monthly Savings Trends ")
            .BorderColor(Palette.Border));
    }
}
