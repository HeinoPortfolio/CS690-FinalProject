using System.IO;
using System.Collections.Generic;

namespace SavingsTracker;

public static class UserRepository
{
    private const string FilePath = "user.txt";

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
}


