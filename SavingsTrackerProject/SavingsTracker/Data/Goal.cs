using System.Text.RegularExpressions;
using Spectre.Console;

namespace SavingsTracker.Data;

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
        var match = Regex.Match(timeframe.ToLower(), @"(\d+)\s*(day|week|month|year)");

        if (match.Success)
        {
            int value = int.Parse(match.Groups[1].Value);

            string unit = match.Groups[2].Value;

            if (unit.StartsWith("day")) return start.AddDays(value);

            if (unit.StartsWith("week")) return start.AddDays(value * 7);

            if (unit.StartsWith("year")) return start.AddYears(value);

            return start.AddMonths(value);
        }
        var fallbackMatch = Regex.Match(timeframe, @"\d+");
        return fallbackMatch.Success ? start.AddMonths(int.Parse(fallbackMatch.Value)) : start;
    }

}

