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
           
            var barChart = new BarChart()
                .Width(60)
                .Label($"[white]Goal Progress: {prediction.OverallPercentage}%[/]")
                .CenterLabel()
                .WithMaxValue(100)
                .AddItem("Progress", prediction.OverallPercentage, GetProgressColor(prediction.OverallPercentage));

            AnsiConsole.Write(new Panel(barChart).Header(" Current Standing ").BorderColor(Color.Grey));
            AnsiConsole.WriteLine();

        
            var mainTable = new Table().Border(TableBorder.Rounded).Expand();
            mainTable.AddColumn("[grey]Metric[/]");
            mainTable.AddColumn("[grey]Value[/]");

            mainTable.AddRow("Remaining Amount", $"[red]{prediction.RemainingAmount:C2}[/]");
            mainTable.AddRow("Time Remaining", $"{prediction.TimeUnitsLeft} {prediction.Unit}(s)");
            mainTable.AddRow("New Target Rate", $"[bold yellow]{prediction.RequiredRate:C2}[/] per {prediction.Unit}");

            AnsiConsole.Write(new Panel(mainTable)
                .Header("[bold cyan] Re-calculated Savings Rate [/]")
                .Padding(1, 1));

            
            var milestoneTable = new Table().Border(TableBorder.Minimal).Expand();
            milestoneTable.AddColumn("Milestone");
            milestoneTable.AddColumn("Projected Date");

            foreach (var m in prediction.Milestones)
            {
                string label = m.Percentage == "90%" ? $"[gold1]{m.Percentage} (Final Stretch!)[/]" : m.Percentage;
                string status = m.IsAchieved 
                    ? "[green]✓ Achieved[/]" 
                    : $"[white]{m.ProjectedDate:MMM dd, yyyy}[/]";
                
                milestoneTable.AddRow(label, status);
            }

            AnsiConsole.Write(new Panel(milestoneTable)
                .Header("[bold blue] Projected Milestones [/]")
                .Padding(1, 0));

            AnsiConsole.MarkupLine($"\n[grey]Final Deadline:[/] {user.ActiveGoal!.EndDate:MMMM dd, yyyy}");
        }

        AnsiConsole.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }

  
    private static Color GetProgressColor(double percentage) => percentage switch
    {
        < 35 => Color.Red,
        < 75 => Color.Yellow,
        _ => Color.Green
    };
}

