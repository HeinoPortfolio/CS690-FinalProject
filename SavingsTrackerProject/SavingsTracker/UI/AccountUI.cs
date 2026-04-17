using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

/// <summary>
/// Handles account-related user interface operations using Spectre.Console.
/// </summary>
public static class AccountUI
{
    /// <summary>
    /// Displays a registration flow to create a new user account and save it to the database.
    /// </summary>
    /// <param name="userDatabase">The in-memory list of existing users for validation.</param>
    public static void CreateAccount(List<User> userDatabase)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Account Registration[/]").LeftJustified());

        var username = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Username:");
        var password = AnsiConsole.Prompt(new TextPrompt<string>($"[{Palette.Accent.ToMarkup()}]>[/] Password:").Secret());

        // Review Table to verify information before saving
        var table = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        table.AddColumn(new TableColumn("Field").Centered());
        table.AddColumn(new TableColumn("Value").LeftAligned());
        table.AddRow("Username", username);
        table.AddRow("Password", "[grey]********[/]");

        AnsiConsole.Write(new Panel(table).Header("Review Your Information").BorderColor(Palette.Brand));

        if (AnsiConsole.Confirm("\nIs this correct?"))
        {
            if (userDatabase.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[red]![/] Error: Username already exists.");
                Console.ReadKey(true);
            
                CreateAccount(userDatabase);
            }
            else
            {
                var newUser = new User(username, password);
                userDatabase.Add(newUser);
                UserRepository.SaveUser(newUser);
                AnsiConsole.MarkupLine($"[bold {Palette.Brand.ToMarkup()}]✓[/] Account saved successfully.");
                Thread.Sleep(1000);
            }
        }
        else 
        { 
            CreateAccount(userDatabase); 
        }
    }
}
