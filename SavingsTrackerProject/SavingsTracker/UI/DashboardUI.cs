using Spectre.Console;
using Spectre.Console.Rendering;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

/// <summary>
/// The central hub of the application. Displays current goal status and navigates to all sub-menus.
/// </summary
public static class DashboardUI
{
    /// <summary>
    /// Runs the main dashboard loop, refreshing the UI after every operation until the user logs out.
    /// </summary>
    /// <param name="user">The currently logged-in user.</param>
    /// <param name="renderHeader">Action to draw the application's top branding.</param>
    /// <param name="monitorProgress">Action to trigger the progress monitoring view.</param>
    public static void Show(User user, Action renderHeader, Action<User> monitorProgress)
    {
        bool inDashboard = true;

        while (inDashboard)
        {
            AnsiConsole.Clear();
            renderHeader();

           
            SavingsPromptsUI.CheckScheduledTime(user);

            // Alert stays until manually cleared
            SavingsPromptsUI.RenderActiveNotice();

            if (user.ActiveGoal != null)
            {
                RenderGoalProgress(user);
            }
            else
            {
                AnsiConsole.MarkupLine($"[yellow]! No active goal found. Select 'Create a new savings goal' to begin.[/]");
            }

            var choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
                .Title($"[{Palette.TextDim.ToMarkup()}]SELECT OPERATION[/]")
                .HighlightStyle(new Style(Palette.SelectionFg, Palette.SelectionBg, Decoration.Bold))
                .AddChoices(DashboardMenu.Logout)
                .AddChoiceGroup($"[{Palette.Accent.ToMarkup()}]SAVINGS GOALS[/]", DashboardMenu.GoalOptions)
                .AddChoiceGroup($"[{Palette.Accent.ToMarkup()}]TRANSACTIONS[/]", DashboardMenu.TransactionOptions)
                .AddChoiceGroup($"[{Palette.Accent.ToMarkup()}]ANALYSIS[/]")
                .AddChoices(DashboardMenu.AnalysisOptions));

            if (choice == DashboardMenu.Logout) inDashboard = false;
            else if (choice == "Create a new savings goal") HandleCreateGoal(user);
            else if (choice == "Log a contribution") ContributionUI.LogContribution(user, renderHeader);
            else if (choice == "Monitor progress toward the goal") monitorProgress(user);
            else if (choice == "Predict milestones") PredictionUI.ShowPrediction(user, renderHeader);
            else if (choice == "Log daily transactions") TransactionUI.LogDailyTransaction(user, renderHeader);
            else if (choice == "View organized spending") ViewOrganizedUI.ViewOrganizedSpending(user, renderHeader);
            else if (choice == "Review specific category spending") CategoryReviewUI.ReviewCategorySpending(user, renderHeader);
            else if (choice == "Identify spending triggers") TriggerUI.IdentifyTriggers(user, renderHeader);
            else if (choice == "Track weekly savings consistency") TrackWeeklySavingsConsistencyUI.Show(user, renderHeader);
            else if (choice == "Analyze spending proportions") AnalyzeProportionsUI.Show(user, renderHeader);
            else if (choice == "Evaluate purchase impact") EvaluateImpactUI.Show(user, renderHeader);
            else if (choice == "Review monthly financial performance") MonthlyPerformanceUI.Show(user, renderHeader);
            else if (choice == "Receive savings prompts") SavingsPromptsUI.Show(user, renderHeader);
            else HandleUnimplemented(choice);
        }
    }

    /// <summary>
    /// Renders a visual breakdown of the user's progress toward their target amount.
    /// </summary>    
    private static void RenderGoalProgress(User user)
    {
        var goal = user.ActiveGoal!;
        double current = user.CurrentSavings;
        double target = goal.TargetAmount;
        double remaining = Math.Max(0, target - current);
        double percentComplete = target > 0 ? current / target : 0;

        var breakdown = new BreakdownChart().Width(60)
            .AddItem("Saved: $", current, Palette.StatusBar)
            .AddItem("Remaining: $", remaining, Palette.TextDim)
            .AddItem("Percent Remaining (%): ", Math.Round((1 - percentComplete) * 100, 2), Palette.TextDim);

        var summaryTable = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        summaryTable.AddColumn("[grey]Goal Progress[/]");
        summaryTable.AddColumn("[grey]Value[/]");
        summaryTable.AddRow("Target Goal", $"{target:C2}");
        summaryTable.AddRow("Current Balance", $"[{Palette.Brand.ToMarkup()}]{current:C2}[/]");
        summaryTable.AddRow("Amount Left", $"[red]{remaining:C2}[/]");
        summaryTable.AddRow("Percentage Complete: ", $"[blue]{percentComplete:P2}[/]");

        AnsiConsole.Write(new Panel(new Rows(
            new Text($"Goal: {goal.Name}", new Style(Palette.Brand, decoration: Decoration.Bold)),
            new Text($"Target End Date: {goal.EndDate:MMMM dd, yyyy}", new Style(Palette.TextDim)),
            new Rule().RuleStyle(Palette.Border.ToMarkup()),
            new Padder(breakdown, new Padding(0, 1, 0, 1)), 
            summaryTable
        ))
        .Header($" Status for {user.Username} ")
        .BorderColor(Palette.Border).Padding(2, 1, 2, 1));
    }

    /// <summary>
    /// Helper to initiate the goal creation flow and persist the results.
    /// </summary>
    private static void HandleCreateGoal(User user)
    {
        var newGoal = GoalUI.PromptForGoal(user);
        if (newGoal != null)
        {
            user.ActiveGoal = newGoal;
            UserRepository.SaveGoal(user.Username, newGoal, user.CurrentSavings, user.Contributions);
        }
    }

    /// <summary>
    /// Catch-all for features that are in the menu but lack implementation.
    /// </summary>
    private static void HandleUnimplemented(string choice)
    {
        AnsiConsole.Write(new Rule().RuleStyle(Palette.Border));
        AnsiConsole.MarkupLine($"[{Palette.Brand.ToMarkup()}]Not yet implemented:[/] {choice}... Press any key.");
        Console.ReadKey(true);
    }
}


