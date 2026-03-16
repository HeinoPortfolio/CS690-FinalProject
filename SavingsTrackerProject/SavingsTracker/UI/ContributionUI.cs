using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class ContributionUI
{
    public static void LogContribution(User user, Action renderHeader)
    {
        if (user.ActiveGoal == null) return;

        AnsiConsole.Clear(); 
        renderHeader();
        
        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Log Contribution[/]").Centered());

        var amount = AnsiConsole.Prompt(new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Amount to contribute: $")
            .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be > 0[/]")));

        // Update Model
        user.CurrentSavings += amount;
        user.Contributions.Add(new Contribution(amount, DateTime.Now));

        // Persist Data
        UserRepository.SaveGoal(user.Username, user.ActiveGoal, user.CurrentSavings, user.Contributions);

        if (user.CurrentSavings >= user.ActiveGoal.TargetAmount)
        {
            RenderSuccessScreen(user, renderHeader);
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]✓[/] {amount:C2} added! New Total: [bold]{user.CurrentSavings:C2}[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
        }
    }

    private static void RenderSuccessScreen(User user, Action renderHeader)
    {
        AnsiConsole.Clear();
        renderHeader();

        var goldPanel = new Panel(Align.Center(
            new Rows(
                new Text("★ SAVINGS GOAL ACHIEVED ★", new Style(Color.Gold1, decoration: Decoration.Bold)),
                new Text("\n"),
                new Text($"GOLD STATUS REACHED!", new Style(Color.Yellow, decoration: Decoration.Bold)),
                new Text($"You've successfully saved {user.CurrentSavings:C2} for '{user.ActiveGoal.Name}'", new Style(Palette.TextDim)),
                new Text("\n[ Press any key to continue ]", new Style(Color.Grey))
            ), 
            VerticalAlignment.Middle))
            .BorderColor(Color.Gold1)
            .Border(BoxBorder.Double)
            .Padding(2, 2)
            .Expand();

        AnsiConsole.Write(goldPanel);
        Console.ReadKey(true);

        if (AnsiConsole.Confirm("[red]Goal reached![/] Would you like to [bold red]delete[/] this goal and all contribution history now?"))
        {
            user.ActiveGoal = null;
            user.CurrentSavings = 0;
            user.Contributions.Clear();

            string path = $"{user.Username}_goal.txt";
            if (File.Exists(path))
            {
                try
                {
                    File.Delete(path);
                    AnsiConsole.MarkupLine("[grey]✓ Success: Goal file permanently removed.[/]");
                }
                catch (IOException ex)
                {
                    AnsiConsole.MarkupLine($"[red]! Error deleting file: {ex.Message}[/]");
                }
            }
            Thread.Sleep(1500);
        }
    }
}
