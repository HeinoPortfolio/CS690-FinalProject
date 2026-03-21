using Spectre.Console;
using Spectre.Console.Rendering;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class ProgressUI
{
    public static void RenderStats(User user)
    {
        if (user.ActiveGoal == null) return;

        var goal = user.ActiveGoal;
        double current = user.CurrentSavings;
        double target = goal.TargetAmount > 0 ? goal.TargetAmount : 1; 
        double remaining = Math.Max(0, target - current);
        
        var chart = new BreakdownChart()
            .FullSize()
            .AddItem("Saved: $", current, Palette.StatusBar)
            .AddItem("Remaining: $", remaining, Palette.TextDim);

        var infoTable = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        infoTable.AddColumn("[grey]Timeline & Totals[/]");
        infoTable.AddColumn(new TableColumn("[grey]Value[/]").RightAligned());
        infoTable.AddRow("Goal Start Date", goal.CreatedAt.ToString("MMMM dd, yyyy"));
        infoTable.AddRow("Target End Date", goal.EndDate.ToString("MMMM dd, yyyy"));
        infoTable.AddEmptyRow();
        infoTable.AddRow("Target Goal Amount", $"{target:C2}");
        infoTable.AddRow("Total Amount Saved", $"[green]{current:C2}[/]");

        var historyList = new List<IRenderable>();
        if (user.Contributions.Any())
        {
            foreach (var cont in user.Contributions)
                historyList.Add(new Text($" • {cont.Amount:C2} logged on {cont.Date:MMMM dd, yyyy}", new Style(Palette.TextDim)));
        }
        else
        {
            historyList.Add(new Text(" No contributions logged yet.", new Style(Palette.TextDim)));
        }

        var mainLayout = new Panel(new Rows(
            new Text($"Progress Analysis: {goal.Name}", new Style(Palette.Brand, decoration: Decoration.Bold)),
            new Padder(chart, new Padding(0, 1, 0, 1)),
            infoTable,
            new Panel(new Rows(historyList)).Header(" Contribution History ").BorderColor(Palette.Border)
        ))
        .BorderColor(Palette.Border)
        .Padding(2, 1, 2, 1);

        AnsiConsole.Write(mainLayout);
    }
}
