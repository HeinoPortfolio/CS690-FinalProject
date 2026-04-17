using System.Text.RegularExpressions;
using Spectre.Console;

namespace SavingsTracker.Data;

/// <summary>
/// Represents a financial savings goal with a specific target amount and timeframe.
/// </summary>
public class Goal
{
    public string Name { get; set; }
    public double TargetAmount { get; set; }
    public string TimeFrame { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Initializes a new goal and automatically calculates the EndDate based on the provided timeframe string.
    /// </summary
    public Goal(string name, double targetAmount, string timeFrame, DateTime createdAt)
    {
        Name = name;
        TargetAmount = targetAmount;
        TimeFrame = timeFrame;
        CreatedAt = createdAt;

        // Calculates the deadline based on typed in timeframe (e.g., "6 months")
        EndDate = CalculateEndDate(createdAt, timeFrame);
    }

    /// <summary>
    /// Initializes a new goal with a pre-defined EndDate.
    /// </summary>
    public Goal(string name, double targetAmount, string timeFrame, DateTime createdAt, DateTime endDate)
    {
        Name = name;
        TargetAmount = targetAmount;
        TimeFrame = timeFrame;
        CreatedAt = createdAt;
        EndDate = endDate;
    }

    /// <summary>
    /// Parses the timeframe string using Regex to determine the goal's completion date.
    /// Supports formats like "12 days", "4 weeks", "3 months", or "1 year".
    /// </summary>
    /// <param name="start">The starting date of the goal.</param>
    /// <param name="timeframe">The descriptive string representing the duration.</param>
    /// <returns>A DateTime representing the calculated deadline.</returns>
    private static DateTime CalculateEndDate(DateTime start, string timeframe)
    {
        // Matches a number followed by a unit (day, week, month, year)
        var match = Regex.Match(timeframe.ToLower(), @"(\d+)\s*(day|week|month|year)");

        if (match.Success)
        {
            int value = int.Parse(match.Groups[1].Value);

            string unit = match.Groups[2].Value;

            if (unit.StartsWith("day")) return start.AddDays(value);

            if (unit.StartsWith("week")) return start.AddDays(value * 7);

            if (unit.StartsWith("year")) return start.AddYears(value);

             // Default to months if "month" is detected.
            return start.AddMonths(value);
        }

        // Fallback: If only a number is provided, assume the unit is months
        var fallbackMatch = Regex.Match(timeframe, @"\d+");

        return fallbackMatch.Success ? start.AddMonths(int.Parse(fallbackMatch.Value)) : start;
    }

}

