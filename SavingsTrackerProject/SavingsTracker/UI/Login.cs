using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class LoginUI
{
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
