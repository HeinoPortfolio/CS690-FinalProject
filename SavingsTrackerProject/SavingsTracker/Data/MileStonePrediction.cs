using SavingsTracker.Data;

/// <summary>
/// Contains calculated projections and progress milestones for a savings goal.
/// Used to display progress bars or "how-to-get-there" logic.
/// </summary>
/// <param name="RequiredRate">The amount needed per unit (e.g., $50/month) to finish on time.</param>
/// <param name="RemainingAmount">The dollar amount still needed to reach the target.</param>
/// <param name="TimeUnitsLeft">The numeric count of units (days, months, etc.) remaining until the deadline.</param>
/// <param name="Unit">The text label for the time unit (e.g., "Months" or "Weeks").</param>
/// <param name="Milestones">A breakdown of specific progress points (e.g., 25%, 50%, 75%).</param>
/// <param name="OverallPercentage">The total percentage of the goal completed so far (0.0 to 100.0+).</param>
public record MilestonePrediction(
    double RequiredRate, 
    double RemainingAmount, 
    double TimeUnitsLeft, 
    string Unit, 
    List<MilestoneDetail> Milestones,
    double OverallPercentage
);
