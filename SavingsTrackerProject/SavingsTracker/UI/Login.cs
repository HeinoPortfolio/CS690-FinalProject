using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

/// <summary>
/// Handles the authentication flow and data restoration for returning users.
/// </summary>
public static class LoginUI
{
    /// <summary>
    /// Authenticates a user against the provided database and loads their persisted goal and transaction data.
    /// </summary>
    /// <param name="userDatabase">The list of registered users to validate against.</param>
    /// <returns>The authenticated User object with loaded data, or null if login fails.</returns>
    public static User? Login(List<User> userDatabase)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Secure Login[/]").LeftJustified());

        var username = AnsiConsole.Ask<string>("Username:");
        var password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

        var user = userDatabase.FirstOrDefault(u => u.Username == username && u.Password == password);

        if (user != null)
        {
            
            var (goal, savings, history) = UserRepository.LoadGoal(user.Username);

            user.ActiveGoal = goal;
            user.CurrentSavings = savings;
            user.Contributions = history;

            user.DailyTransactions = UserRepository.LoadTransactions(user.Username);

            return user;
        }
        else
        {
            AnsiConsole.MarkupLine("[red]![/] Invalid credentials.");
            Thread.Sleep(1500);
            return null; 
        }
    }
}
