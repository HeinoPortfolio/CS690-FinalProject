namespace SavingsTracker.Data;

/// <summary>
/// Represents a specific progress point (e.g., "50%") toward a savings goal.
/// </summary>
/// <param name="Percentage">The label for the milestone (e.g., "25%", "Halfway").</param>
/// <param name="ProjectedDate">The estimated or actual date this milestone is reached.</param>
/// <param name="IsAchieved">Indicates if the user's current savings have met or exceeded this point.</param>
public record MilestoneDetail(
    string Percentage, 
    DateTime ProjectedDate, 
    bool IsAchieved
);
