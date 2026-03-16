using System.Text.RegularExpressions;
using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class GoalUI
{
    public static Goal? PromptForGoal()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Create New Savings Goal[/]").Centered());

            var name = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Name of new savings goal:");
            var amount = AnsiConsole.Prompt(new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Amount for goal: $")
                .Validate(amt => amt > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Amount must be > 0[/]")));
            var timeframe = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Time frame (e.g., '1 year', '8 months'): ");

            var tempGoal = new Goal(name, amount, timeframe, DateTime.Now);

            // Show the review panel =============
            RenderReviewPanel(tempGoal);

            if (AnsiConsole.Confirm("Is this information correct?")) return tempGoal;
            if (!AnsiConsole.Confirm("Try again?")) return null;
        }
    }

    private static void RenderReviewPanel(Goal goal)
    {
        var startCal = new Calendar(goal.CreatedAt).AddCalendarEvent(goal.CreatedAt)
            .HighlightStyle(new Style(Color.Black, Palette.Brand, Decoration.Bold));

        var endCal = new Calendar(goal.EndDate).AddCalendarEvent(goal.EndDate)
            .HighlightStyle(new Style(Color.Black, Palette.Accent, Decoration.Bold));

        var calGrid = new Grid().AddColumns(2).AddRow(
            new Panel(startCal).Header(" Start ").BorderColor(Palette.Border), 
            new Panel(endCal).Header(" Finish ").BorderColor(Palette.Border));

        var table = new Table().BorderColor(Palette.Border).Expand().AddColumn("Info").AddColumn("Details");
        table.AddRow("Goal Name", goal.Name)
             .AddRow("Target Amount", $"{goal.TargetAmount:C2}")
             .AddRow("Start Date", goal.CreatedAt.ToString("MMMM dd, yyyy"))
             .AddRow("End Date", goal.EndDate.ToString("MMMM dd, yyyy"));

        AnsiConsole.Write(new Panel(new Rows(new Padder(calGrid, new Padding(0, 0, 0, 1)), table))
            .Header($" [bold {Palette.Brand.ToMarkup()}]Review Your Goal[/] ")
            .BorderColor(Palette.Brand).Padding(2, 1, 2, 1));
    }
}
