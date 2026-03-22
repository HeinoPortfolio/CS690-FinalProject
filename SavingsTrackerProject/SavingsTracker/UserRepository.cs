using System.IO;
using System.Collections.Generic;
using SavingsTracker.Data;


namespace SavingsTracker;

public static class UserRepository
{
    private const string FilePath = "users.txt";

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

    public static void SaveUser(User user)
    {
        File.AppendAllLines(FilePath, new [] {$"{user.Username}:{user.Password}"});
    }

    public static void SaveGoal(string username, Goal goal, double currentSavings, List<Contribution> history)
    {
       
        string historyData = string.Join(";", history.Select(contri => $"{contri.Amount}_{contri.Date.ToString("O")}"));


        string content = $"{goal.Name}|{goal.TargetAmount}|{goal.TimeFrame}|{goal.CreatedAt.ToString("O")}|{goal.EndDate.ToString("O")}|{currentSavings}|{historyData}";

        File.WriteAllText($"{username}_goal.txt", content);
    }

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

    public static void SaveTransactions(string username, List<Transaction> transactions)
    {
        // Uses ~ as a separator to avoid conflicts with names
        string content = string.Join(";", transactions.Select(t => 
            $"{t.Name}~{t.Amount}~{t.Category}~{t.Date:O}"));

        File.WriteAllText($"{username}_transactions.txt", content);
    }

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