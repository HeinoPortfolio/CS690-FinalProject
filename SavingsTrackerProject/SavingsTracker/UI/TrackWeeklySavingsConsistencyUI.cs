using Spectre.Console;
using SavingsTracker.Data;
using SavingsTracker.Logic;

namespace SavingsTracker.UI;

public static class TrackWeeklySavingsConsistencyUI
{
    public static void Show(User user, Action renderHeader)
    {
        AnsiConsole.Clear();
        renderHeader();

        if (user.ActiveGoal == null)
        {
            AnsiConsole.MarkupLine("[yellow]! An active goal is required to track consistency.[/]");
            AnsiConsole.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            return;
        }

        var prediction = SavingsPredictor.CalculateRequiredRate(user);
        double weeklyTarget = 0;

        if (prediction != null)
        {
            // Normalize the required rate to a seven-day (weekly) value
            weeklyTarget = prediction.Unit.ToLower() switch
            {
                "day" => prediction.RequiredRate * 7,
                "week" => prediction.RequiredRate,
                "month" => (prediction.RequiredRate * 12) / 52,
                "year" => prediction.RequiredRate / 52,
                _ => 0
            };
        }

        int weeksMet = 0;
        double totalSavedInPeriod = 0;
        DateTime now = DateTime.Now;

        var table = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        table.AddColumn("[grey]7-Day Window[/]");
        table.AddColumn(new TableColumn("[grey]Amount Saved[/]").RightAligned());
        table.AddColumn("[grey]Status[/]");

        for (int i = 0; i < 8; i++)
        {
            DateTime weekEnd = now.AddDays(-(i * 7));
            DateTime weekStart = weekEnd.AddDays(-7);

            double savedInWindow = user.Contributions
                .Where(c => c.Date >= weekStart && c.Date < weekEnd)
                .Sum(c => c.Amount);

            totalSavedInPeriod += savedInWindow;
            
            bool isMet = savedInWindow >= weeklyTarget;
            if (isMet) weeksMet++;

            string status = isMet ? "[bold green]MET[/]" : "[bold red]MISSED[/]";

            table.AddRow(
                $"{weekStart:MMM dd} - {weekEnd:MMM dd}",
                $"{savedInWindow:C2}",
                status
            );
        }

        // Add the total summary as a footer for robustness
        table.Columns[1].Footer = new Markup($"[bold white]{totalSavedInPeriod:C2}[/]");
        table.Columns[0].Footer = new Markup("[bold white]Total Saved (8w):[/]");


        var summaryGrid = new Grid().AddColumns(2);
        summaryGrid.AddRow(
            new Panel($"Target: [gold1]{weeklyTarget:C2}/wk[/]").BorderColor(Palette.Border).Expand(),
            new Panel($"Score: [cyan]{weeksMet}/8 Weeks Met[/]").BorderColor(Palette.Border).Expand()
        );

        AnsiConsole.Write(Align.Center(summaryGrid));
        
        AnsiConsole.Write(new Panel(table)
            .Header(" Weekly Savings Consistency Report ")
            .BorderColor(Palette.Brand));

        AnsiConsole.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }
}

