using SavingsTracker.Data;

namespace SavingsTracker.Logic;

/// <summary>
/// Provides logic to analyze a user's progress and project future savings requirements.
/// </summary>
public static class SavingsPredictor
{
    /// <summary>
    /// Analyzes the active goal to determine the required savings rate and milestone dates.
    /// </summary>
    /// <param name="user">The user profile containing the active goal and current balance.</param>
    /// <returns>A prediction summary or null if no active goal exists.</returns>
    public static MilestonePrediction? CalculateRequiredRate(User user)
    {
        if (user.ActiveGoal == null) return null;

        var goal = user.ActiveGoal;
        double current = user.CurrentSavings;
        double target = goal.TargetAmount;
        double gap = target - current;
        TimeSpan timeLeft = goal.EndDate - DateTime.Now;

        // Determine the time unit based on the original goal timeframe
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

        unitsLeft = timeLeft.TotalDays <= 0 ? 0 : Math.Max(unitsLeft, 0.1);

        // Milestone Projections (25%, 50%, 75%, 90%)
        var milestones = new List<MilestoneDetail>();
        double[] targetPercents = { 0.25, 0.50, 0.75, 0.90 };
        
        // Rate needed from today to hit the goal exactly on the EndDate
        double dailyRateNeeded = (timeLeft.TotalDays > 0) ? gap / timeLeft.TotalDays : 0;

        foreach (var p in targetPercents)
        {
            double targetVal = target * p;
            bool achieved = current >= targetVal;
            
            DateTime projectedDate;
            if (achieved)
            {
                projectedDate = DateTime.Now;
            }
            else
            {
                double amountNeeded = targetVal - current;
                double daysToTarget = dailyRateNeeded > 0 ? amountNeeded / dailyRateNeeded : 0;
                projectedDate = DateTime.Now.AddDays(daysToTarget);
            }

            milestones.Add(new MilestoneDetail($"{(int)(p * 100)}%", projectedDate, achieved));
        }

        // Calculate total progress percentage (0-100 range)
        double completion = Math.Clamp((current / target) * 100, 0, 100);

        return new MilestonePrediction(
            Math.Round(gap / (unitsLeft > 0 ? unitsLeft : 1), 2),
            Math.Round(gap, 2),
            Math.Round(unitsLeft, 1),
            unit,
            milestones,
            Math.Round(completion, 1)
        );
    }
}
