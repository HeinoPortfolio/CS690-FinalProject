namespace SavingsTracker;

public record User(string Username, string Password)
{
    public Goal? ActiveGoal {get; set; } 
}
