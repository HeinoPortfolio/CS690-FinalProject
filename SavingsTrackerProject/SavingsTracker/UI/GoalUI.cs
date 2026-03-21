using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class GoalUI
{
    public static Goal? PromptForGoal(User user)
    {
        if (user.ActiveGoal != null)
        {
            if (!ConfirmGoalDeletion(user)) return null; 
        }

        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Create New Savings Goal[/]").Centered());

            var name = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Name of new savings goal:");
            var amount = AnsiConsole.Prompt(new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Amount for goal: $")
                .Validate(amt => amt > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Amount must be > 0[/]")));
            var timeframe = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Time frame (e.g., '1 year', '8 months'): ");

            var tempGoal = new Goal(name, amount, timeframe, DateTime.Now);

            RenderReviewPanel(tempGoal);

            if (AnsiConsole.Confirm("Is this information correct?")) return tempGoal;
            if (!AnsiConsole.Confirm("Try again?")) return null;
        }
    }

    private static bool ConfirmGoalDeletion(User user)
    {
        
        AnsiConsole.MarkupLine($"[yellow]![/] A goal [bold]'{user.ActiveGoal!.Name}'[/] already exists.");

        var summaryTable = new Table().Border(TableBorder.None).HideHeaders().AddColumn("Data");
        summaryTable.AddRow($"[red]•[/] Goal: [bold]{user.ActiveGoal.Name}[/]");
        summaryTable.AddRow($"[red]•[/] Saved: [yellow]{user.CurrentSavings:C2}[/]");
        summaryTable.AddRow($"[red]•[/] History: {user.Contributions.Count} contribution(s)");

        // DISPLAY THE SUMMARY
        AnsiConsole.Write(new Panel(summaryTable)
            .Header("[bold red] DATA DELETION SUMMARY [/]")
            .BorderColor(Color.Red)
            .Padding(1, 1));

        if (AnsiConsole.Confirm("Creating a new goal will [red]delete all current progress[/]. Proceed?"))
        {
            user.ActiveGoal = null;
            user.CurrentSavings = 0;
            user.Contributions.Clear();

            string path = $"{user.Username}_goal.txt";
            if (File.Exists(path)) File.Delete(path);

            AnsiConsole.MarkupLine("[green]✔ Existing goal and history cleared.[/]");
            Thread.Sleep(1200); 
            return true;
        }

        return false;
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
