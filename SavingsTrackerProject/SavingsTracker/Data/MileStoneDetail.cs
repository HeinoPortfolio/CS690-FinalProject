namespace SavingsTracker.Data;

public record MilestoneDetail(
    string Percentage, 
    DateTime ProjectedDate, 
    bool IsAchieved
);
