namespace SavingsTracker.Data;

/// <summary>
/// Represents a user profile within the SavingsTracker system.
/// Holds credentials, the current savings status, and all historical financial activity.
/// </summary>
/// <param name="Username">The unique identifier for the user's account.</param>
/// <param name="Password">The hashed or plain-text password for authentication.</param>
public record User(string Username, string Password)
{
    /// <summary>
    /// The specific savings goal the user is currently working toward. 
    /// Can be null if no goal is set.
    /// </summary>
    public Goal? ActiveGoal {get; set; }

    /// <summary>
    /// The total liquid balance available in the user's savings.
    /// </summary
    public double CurrentSavings {get; set;}

    /// <summary>
    /// A history of specific deposits or additions made toward savings goals.
    /// </summary>
    public List<Contribution> Contributions {get; set;} = new();

    /// <summary>
    /// A chronological log of all daily income and expense activities.
    /// </summary>
    public List<Transaction> DailyTransactions {get; set;} = new();

}
