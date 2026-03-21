using SavingsTracker.Data;

namespace SavingsTracker.Logic;

public static class SavingsPredictor
{
    public record MilestonePrediction(double RequiredRate, double RemainingAmount, double TimeUnitsLeft, string Unit);

    public static MilestonePrediction? CalculateRequiredRate(User user)
    {
        if (user.ActiveGoal == null || user.CurrentSavings >= user.ActiveGoal.TargetAmount)
            return null;

        var goal = user.ActiveGoal;
        double gap = goal.TargetAmount - user.CurrentSavings;
        TimeSpan timeLeft = goal.EndDate - DateTime.Now;

        if (timeLeft.TotalDays <= 0) 
            return new MilestonePrediction(gap, gap, 0, "overdue");

        string unit = "month";
        double unitsLeft = timeLeft.TotalDays / 30.44; 

        if (goal.TimeFrame.Contains("day", StringComparison.OrdinalIgnoreCase))
        {
            unit = "day";
            unitsLeft = timeLeft.TotalDays;
        }
        else if (goal.TimeFrame.Contains("week", StringComparison.OrdinalIgnoreCase))
        {
            unit = "week";
            unitsLeft = timeLeft.TotalDays / 7;
        }
        else if (goal.TimeFrame.Contains("year", StringComparison.OrdinalIgnoreCase))
        {
            unit = "year";
            unitsLeft = timeLeft.TotalDays / 365.25;
        }

        unitsLeft = Math.Max(unitsLeft, 0.1); 
        
        return new MilestonePrediction(
            Math.Round(gap / unitsLeft, 2), 
            Math.Round(gap, 2), 
            Math.Round(unitsLeft, 1), 
            unit);
    }
}
