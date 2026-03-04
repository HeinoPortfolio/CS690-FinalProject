namespace SavingsTracker;

public static class DashboardMenu
{
    public static readonly string[] GoalOptions = { 
        "Create a new savings goal", "Log a contribution", 
        "Monitor progress toward the goal", "Predict milestones" 
    };

    public static readonly string[] TransactionOptions = { 
        "Log daily transactions", "View organized spending", 
        "Review specific category spending", "Identify spending triggers" 
    };

    public static readonly string[] AnalysisOptions = { 
        "Track weekly savings consistency", "Analyze spending proportions", 
        "Evaluate purchase impact", "Review monthly financial performance", 
        "Receive savings prompts" 
    };

    public const string Logout = "Logout";
}
