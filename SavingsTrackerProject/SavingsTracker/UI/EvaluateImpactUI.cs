
using Spectre.Console;
using SavingsTracker.Data;
using SavingsTracker.Logic;

namespace SavingsTracker.UI;

public static class EvaluateImpactUI
{
    private static readonly string[] SpecifiedCategories = { 
        "Housing", 
        "Utilities", 
        "Groceries", 
        "Transportation", 
        "Dining Out", 
        "Entertainment", 
        "Healthcare", 
        "Insurance",
        "Shopping",
        "Other"
    };

    public static void Show(User user, Action renderHeader)
    {
        AnsiConsole.Clear();
        renderHeader();

        if (user.ActiveGoal == null)
        {
            AnsiConsole.MarkupLine("[yellow]! An active goal is required to evaluate purchase impact.[/]");
            AnsiConsole.WriteLine("\nPress any key to return...");
            Console.ReadKey(true);
            return;
        }

        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Evaluate Purchase Impact[/]").Centered());

        var amount = AnsiConsole.Prompt(
            new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Potential Purchase Amount: $")
                .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Amount must be > 0[/]")));

        var selectedCategory = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{Palette.TextDim.ToMarkup()}]SELECT CATEGORY FOR THIS PURCHASE[/]")
                .PageSize(10)
                .HighlightStyle(new Style(Palette.SelectionFg, Palette.SelectionBg, Decoration.Bold))
                .AddChoices(SpecifiedCategories));

        // Calculate impact based on current required daily rate
        TimeSpan timeLeft = user.ActiveGoal.EndDate - DateTime.Now;
        double currentBalance = user.CurrentSavings;
        double target = user.ActiveGoal.TargetAmount;
        double currentGap = target - currentBalance;

        // Daily rate needed to hit the goal exactly on the original EndDate
        double dailyRateNeeded = (timeLeft.TotalDays > 0) ? currentGap / timeLeft.TotalDays : 0;

        // If they spend 'amount', the gap increases, requiring more time at the same daily rate.
        double daysDelay = dailyRateNeeded > 0 ? amount / dailyRateNeeded : 0;
        DateTime newProjectedDate = user.ActiveGoal.EndDate.AddDays(daysDelay);

        RenderImpactReport(amount, selectedCategory, daysDelay, newProjectedDate, user.ActiveGoal.EndDate);

        AnsiConsole.WriteLine("\nPress any key to return...");
        Console.ReadKey(true);
    }

    private static void RenderImpactReport(double amount, string category, double daysDelay, DateTime newDate, DateTime originalDate)
    {
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        table.AddColumn("[grey]Metric[/]");
        table.AddColumn("[grey]Result[/]");

        table.AddRow("Potential Purchase", $"[red]{amount:C2}[/]");
        table.AddRow("Selected Category", $"[blue]{category}[/]");
        table.AddRow("Time Setback", $"[bold yellow]{daysDelay:F1} extra days[/] of saving");
        table.AddRow("Original Goal Date", $"{originalDate:MMMM dd, yyyy}");
        table.AddRow("New Projected Date", $"[bold cyan]{newDate:MMMM dd, yyyy}[/]");

        string impactMessage = daysDelay > 7 
            ? "[bold red]Significant Impact:[/] This purchase will delay your goal by over a week." 
            : "[bold blue]Notice:[/] This is a minor delay, but consistency is key to your goal.";

        AnsiConsole.Write(new Panel(new Rows(
            table,
            new Padder(new Markup(impactMessage), new Padding(1, 1, 1, 0))
        ))
        .Header($" Impact Analysis: {category} ")
        .BorderColor(Palette.Brand));
    }
}
