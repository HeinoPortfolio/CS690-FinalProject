using Spectre.Console;
using SavingsTracker.Data;
using SavingsTracker.Logic;

namespace SavingsTracker.UI;

public static class PredictionUI
{
    public static void ShowPrediction(User user, Action renderHeader)
    {
        AnsiConsole.Clear();
        renderHeader();

        var prediction = SavingsPredictor.CalculateRequiredRate(user);

        if (prediction == null)
        {
            AnsiConsole.MarkupLine("[yellow]! No active goal or goal already achieved.[/]");
        }
        else
        {
            var table = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
            table.AddColumn("[grey]Metric[/]");
            table.AddColumn("[grey]Value[/]");

            table.AddRow("Remaining Amount", $"[red]{prediction.RemainingAmount:C2}[/]");
            table.AddRow("Time Remaining", $"{prediction.TimeUnitsLeft} {prediction.Unit}(s)");
            table.AddRow("New Target Rate", $"[bold yellow]{prediction.RequiredRate:C2}[/] per {prediction.Unit}");

            AnsiConsole.Write(new Panel(table)
                .Header($"[{Palette.Brand.ToMarkup()}]Re-calculated Savings Rate [/]")
                .Padding(1, 1));
            
            AnsiConsole.MarkupLine($"\n[grey]To hit your goal by {user.ActiveGoal!.EndDate:MMMM dd, yyyy}, save the rate above.[/]");
        }

        AnsiConsole.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }
}

