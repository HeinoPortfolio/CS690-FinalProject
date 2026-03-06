namespace SavingsTracker;

public record User(string Username, string Password)
{
    public Goal? ActiveGoal {get; set; }
    public double CurrentSavings {get; set;}

    public List<Contribution> Contributions {get; set;} = new();

}
