using System.Text.RegularExpressions;
using Spectre.Console;

namespace SavingsTracker;

public class Goal
{
    public string Name { get; set; }
    public double TargetAmount { get; set; }
    public string TimeFrame { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime EndDate { get; set; }

    public Goal(string name, double targetAmount, string timeFrame, DateTime createdAt)
    {
        Name = name;
        TargetAmount = targetAmount;
        TimeFrame = timeFrame;
        CreatedAt = createdAt;
        EndDate = CalculateEndDate(createdAt, timeFrame);
    }

    public Goal(string name, double targetAmount, string timeFrame, DateTime createdAt, DateTime endDate)
    {
        Name = name;
        TargetAmount = targetAmount;
        TimeFrame = timeFrame;
        CreatedAt = createdAt;
        EndDate = endDate;
    }

    private static DateTime CalculateEndDate(DateTime start, string timeframe)
    {
        var match = Regex.Match(timeframe.ToLower(), @"(\d+)\s*(day|month|year)");
        if (match.Success)
        {
            int value = int.Parse(match.Groups[1].Value);
            string unit = match.Groups[2].Value;

            if (unit.StartsWith("day")) return start.AddDays(value);

            if (unit.StartsWith("year")) return start.AddYears(value);

            return start.AddMonths(value);
        }
        var fallbackMatch = Regex.Match(timeframe, @"\d+");
        return fallbackMatch.Success ? start.AddMonths(int.Parse(fallbackMatch.Value)) : start;
    }

    public static Goal? Create()
    {
        while (true)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Create New Savings Goal[/]").Centered());

            var name = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Name of new savings goal:");
            var amount = AnsiConsole.Prompt(new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Amount for goal:")
                .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Amount must be > 0[/]")));
            var timeframe = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Time frame (e.g., '1 year', '8 months'):");

            var tempGoal = new Goal(name, amount, timeframe, DateTime.Now);

            var startCal = new Calendar(tempGoal.CreatedAt).AddCalendarEvent(tempGoal.CreatedAt)
                .HighlightStyle(new Style(Color.Black, Palette.Brand, Decoration.Bold));

            var endCal = new Calendar(tempGoal.EndDate).AddCalendarEvent(tempGoal.EndDate)
                .HighlightStyle(new Style(Color.Black, Palette.Accent, Decoration.Bold));

            var calGrid = new Grid().AddColumns(2).AddRow(new Panel(startCal)
                .Header(" Start ").BorderColor(Palette.Border), new Panel(endCal).Header(" Finish ").BorderColor(Palette.Border));

            var table = new Table().BorderColor(Palette.Border).Expand().AddColumn("Information")
                .AddColumn("Details");
            table.AddRow("Goal Name", name)
                .AddRow("Target Amount", $"{amount:C2}")
                .AddRow("Start Date", tempGoal.CreatedAt.ToString("MMMM dd, yyyy"))
                .AddRow("Target End Date", tempGoal.EndDate.ToString("MMMM dd, yyyy"));

            AnsiConsole.Write(new Panel(new Rows(new Padder(calGrid, new Padding(0, 0, 0, 1)), table))
                .Header($" [bold {Palette.Brand.ToMarkup()}]Review Your Goal[/] ")
                .BorderColor(Palette.Brand).Padding(2, 1, 2, 1));

            if (AnsiConsole.Confirm("Is this information correct?")) return tempGoal;
            
            if (!AnsiConsole.Confirm("Try again?")) return null;
        }
    }
}

