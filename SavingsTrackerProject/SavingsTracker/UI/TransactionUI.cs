using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;
public static class TransactionUI
{
     private static readonly string[] ExpenseCategories = 
    { 
        "Housing", 
        "Utilities", 
        "Groceries", 
        "Transportation", 
        "Dining Out", 
        "Entertainment", 
        "Healthcare", 
        "Insurance",
        "Shopping",
        "Other" 
    };

    public static void LogDailyTransaction(User user, Action renderHeader)
    {
        bool isConfirmed = false;

        while (!isConfirmed)
        {
            AnsiConsole.Clear();
            renderHeader();
            AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Log Daily Transaction[/]").Centered());

            var name = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Name of transaction:");

            var amount = AnsiConsole.Prompt(
                new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Amount: $")
                .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Amount must be > 0[/]")));

            var category = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{Palette.TextDim.ToMarkup()}]Select a category:[/]")
                    .PageSize(10)
                    .AddChoices(ExpenseCategories));

            // Summary Table for Review
            var table = new Table().BorderColor(Palette.Border).Expand();
            table.AddColumn("Field").AddColumn("Value");
            table.AddRow("Transaction", name);
            table.AddRow("Amount", $"[green]{amount:C2}[/]");
            table.AddRow("Category", $"[blue]{category}[/]");

            AnsiConsole.Write(new Panel(table)
                .Header(" [bold]Review Transaction Details[/] ")
                .BorderColor(Palette.Brand).Padding(1, 1));

            if (AnsiConsole.Confirm("Is this information correct?"))
            {
                var newTransaction = new Transaction(name, amount, category, DateTime.Now);
                user.DailyTransactions.Add(newTransaction);

                UserRepository.SaveTransactions(user.Username, user.DailyTransactions);

                AnsiConsole.MarkupLine("[bold green]✓[/] Transaction recorded successfully!");
                isConfirmed = true;
                Thread.Sleep(1500);
            }
            else
            {
            
                AnsiConsole.MarkupLine("[yellow]![/] Restarting transaction entry...");
                Thread.Sleep(1000);
            }
        }

        
    }
}
