using SavingsTracker.Data;

public record MilestonePrediction(
    double RequiredRate, 
    double RemainingAmount, 
    double TimeUnitsLeft, 
    string Unit, 
    List<MilestoneDetail> Milestones,
    double OverallPercentage
);
