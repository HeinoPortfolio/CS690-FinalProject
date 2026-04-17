namespace SavingsTracker;

/// <summary>
/// Defines the centralized menu structure and navigation options for the application.
/// Provides categorized string arrays used to populate the main dashboard's selection prompts.
/// </summary
public static class DashboardMenu
{
    /// <summary>
    /// Options related to long-term savings objectives and tracking direct progress.
    /// </summary>
    public static readonly string[] GoalOptions = { 
        "Create a new savings goal", "Log a contribution", 
        "Monitor progress toward the goal", "Predict milestones" 
    };

    /// <summary>
    /// Options for day-to-day expense tracking and categorization.
    /// </summary>
    public static readonly string[] TransactionOptions = { 
        "Log daily transactions", "View organized spending", 
        "Review specific category spending", "Identify spending triggers" 
    };

    /// <summary>
    /// Options for advanced data visualization, behavioral analysis, and performance metrics.
    /// </summary>
    public static readonly string[] AnalysisOptions = { 
        "Track weekly savings consistency", "Analyze spending proportions", 
        "Evaluate purchase impact", "Review monthly financial performance", 
        "Receive savings prompts" 
    };

    /// <summary>
    /// Standardized string for exiting the user session.
    /// </summary>
    public const string Logout = "Logout";
}
