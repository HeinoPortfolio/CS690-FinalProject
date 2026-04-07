using System.IO;
using System.Collections.Generic;
using SavingsTracker.Data;


namespace SavingsTracker;


/// <summary>
/// Provides data persistence services for the application, handling the saving and loading 
/// of user credentials, savings goals, contribution history, and daily transactions.
/// </summary>
public static class UserRepository
{
    private const string FilePath = "users.txt";

    /// <summary>
    /// Reads the master user list from the file system.
    /// </summary>
    /// <returns>A list of registered User objects.</returns>
    public static List<User> LoadUsers()
    {
        var users = new List<User>();

        if(!File.Exists(FilePath)) return users;

        foreach(var line in File.ReadAllLines(FilePath))
        {
            var parts = line.Split(':');

            if(parts.Length == 2)
            {
                users.Add(new User(parts[0], parts[1]));
            }
        }
        return users;
    }

    /// <summary>
    /// Appends a new user's credentials to the master user file.
    /// </summary>
    /// <param name="user">The user object containing username and password to persist.</param>
    public static void SaveUser(User user)
    {
        File.AppendAllLines(FilePath, new [] {$"{user.Username}:{user.Password}"});
    }

    /// <summary>
    /// Persists a user's active savings goal and full contribution history to a unique text file.
    /// Uses a combination of pipe (|), semicolon (;), and underscore (_) delimiters.
    /// </summary>
    /// <param name="username">The username used to identify the file.</param>
    /// <param name="goal">The Goal object defining targets and dates.</param>
    /// <param name="currentSavings">The current total saved amount.</param>
    /// <param name="history">The list of individual contributions made.</param>
    public static void SaveGoal(string username, Goal goal, double currentSavings, List<Contribution> history)
    {
       
        string historyData = string.Join(";", history.Select(contri => $"{contri.Amount}_{contri.Date.ToString("O")}"));


        string content = $"{goal.Name}|{goal.TargetAmount}|{goal.TimeFrame}|{goal.CreatedAt.ToString("O")}|{goal.EndDate.ToString("O")}|{currentSavings}|{historyData}";

        File.WriteAllText($"{username}_goal.txt", content);
    }

    /// <summary>
    /// Retrieves a user's goal data and contribution history from their specific data file.
    /// </summary>
    /// <param name="username">The username identifying which file to read.</param>
    /// <returns>A tuple containing the Goal object (if any), total savings, and history list.</returns>
    public static (Goal? goal, double savings, List<Contribution> history) LoadGoal(string username)
    {
        string path = $"{username}_goal.txt";

        var history = new List<Contribution>();

        if (!File.Exists(path)) return (null, 0, history);

        var infile = File.ReadAllText(path).Split('|');

        if (infile.Length >= 6)
        {
            var goal = new Goal(
                infile[0], 
                double.Parse(infile[1].Trim()),
                infile[2].Trim(),
                DateTime.Parse(infile[3]),
                DateTime.Parse(infile[4]));

           double savings = double.Parse(infile[5].Trim());

           if(infile.Length >= 7 && !string.IsNullOrWhiteSpace(infile[6]))
            {
                var entries = infile[6].Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach(var entry in entries)
                {
                    var parts = entry.Split('_');

                    if(parts.Length == 2)
                    {
                        history.Add(new Contribution(
                            double.Parse(parts[0])
                            , DateTime.Parse(parts[1])
                        ));
                    }
                }
            }

            return(goal, savings, history);
        }

        return(null, 0, history);
        
    }

    /// <summary>
    /// Serializes and saves the user's daily transaction list to a dedicated file.
    /// </summary>
    /// <param name="username">The username used to generate the filename.</param>
    /// <param name="transactions">The collection of daily spending records to save.</param>
    public static void SaveTransactions(string username, List<Transaction> transactions)
    {
        // Uses ~ as a separator to avoid conflicts with names
        string content = string.Join(";", transactions.Select(t => 
            $"{t.Name}~{t.Amount}~{t.Category}~{t.Date:O}"));

        File.WriteAllText($"{username}_transactions.txt", content);
    }

    /// <summary>
    /// Loads and deserializes daily transactions from the user's transaction file.
    /// </summary>
    /// <param name="username">The username identifying the source file.</param>
    /// <returns>A list of Transaction objects retrieved from storage.</returns>
    public static List<Transaction> LoadTransactions(string username)
    {
        string path = $"{username}_transactions.txt";
        var list = new List<Transaction>();
        if (!File.Exists(path)) return list;

        var entries = File.ReadAllText(path).Split(';', StringSplitOptions.RemoveEmptyEntries);
    
        foreach (var entry in entries)
        {
            var p = entry.Split('~');
            if (p.Length == 4)
            {
                list.Add(new Transaction(p[0], double.Parse(p[1]), p[2], DateTime.Parse(p[3])));
            }
        }
        return list;
    }

}