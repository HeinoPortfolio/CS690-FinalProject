using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;


/// <summary>
/// Provides a detailed drill-down view of transactions filtered by their category.
/// </summary>
public static class CategoryReviewUI
{
    /// <summary>
    /// Displays an interactive menu for selecting a category and viewing its associated transaction history.
    /// </summary>
    /// <param name="user">The user whose transactions are being reviewed.</param>
    /// <param name="renderHeader">A delegate to draw the standard application header.</param>
    public static void ReviewCategorySpending(User user, Action renderHeader)
    {
    
        if (user.DailyTransactions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]! No transactions recorded yet.[/]");
            if (AnsiConsole.Confirm("Would you like to log your first transaction now?"))
            {
        
                TransactionUI.LogDailyTransaction(user, renderHeader);
                
                if (user.DailyTransactions.Count == 0) return;
            }
            else
            {
                return; 
            }
        }

        bool viewingCategories = true;
        while (viewingCategories)
        {
            AnsiConsole.Clear();
            renderHeader();

            List<string> categoryChoices = new List<string>();
            foreach (var t in user.DailyTransactions)
            {
                bool exists = false;
                foreach (var c in categoryChoices) { if (c == t.Category) exists = true; }
                if (!exists) categoryChoices.Add(t.Category);
            }

            var selectedCategory = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{Palette.TextDim.ToMarkup()}]SELECT CATEGORY TO REVIEW[/]")
                    .HighlightStyle(new Style(Palette.SelectionFg, Palette.SelectionBg, Decoration.Bold))
                    .AddChoices(categoryChoices));

            List<Transaction> results = new List<Transaction>();
            double total = 0;
            foreach (var t in user.DailyTransactions)
            {
                if (t.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(t);
                    total += t.Amount;
                }
            }

            AnsiConsole.Clear();
            renderHeader();
            var table = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
            table.AddColumn("[grey]Date[/]");
            table.AddColumn("[grey]Description[/]");
            table.AddColumn("[grey]Amount[/]").RightAligned();

            foreach (var t in results)
            {
                table.AddRow(t.Date.ToString("MMM dd, yyyy"), t.Name, $"[green]{t.Amount:C2}[/]");
            }

           
            table.Columns[1].Footer = new Markup("[bold white]Total Spent:[/]");
            table.Columns[2].Footer = new Markup($"[bold white]{total:C2}[/]");

            AnsiConsole.Write(new Panel(table).Header($" {selectedCategory} ").BorderColor(Palette.Brand));

            AnsiConsole.WriteLine();
            if (!AnsiConsole.Confirm($"[{Palette.TextDim.ToMarkup()}]Review another category?[/]"))
            {
                viewingCategories = false;
            }
        }
    }
}
