using System.IO;
using System.Collections.Generic;

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

    public static void SaveGoal(string username, Goal goal, double currentSavings)
    {
       
        string content = $"{goal.Name}|{goal.TargetAmount}|{goal.TimeFrame}|{goal.CreatedAt:O}|{goal.EndDate:O}|{currentSavings}";
        File.WriteAllText($"{username}_goal.txt", content);
    }

    public static (Goal? goal, double savings) LoadGoal(string username)
    {
        string path = $"{username}_goal.txt";

        if (!File.Exists(path)) return (null, 0);

        var p = File.ReadAllText(path).Split('|');

        if (p.Length == 6)
        {
            var goal = new Goal(p[0], double.Parse(p[1]), p[2], DateTime.Parse(p[3]), DateTime.Parse(p[4]));

            return (goal, double.Parse(p[5]));
        }

        return(null, 0);
        //return p.Length == 5 ? new Goal(p[0], double.Parse(p[1]), p[2], DateTime.Parse(p[3]), DateTime.Parse(p[4])) : null;
    }

}


