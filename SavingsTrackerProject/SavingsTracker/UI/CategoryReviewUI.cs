using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class CategoryReviewUI
{
    public static void ReviewCategorySpending(User user, Action renderHeader)
    {
        // REQUIREMENT: Get input from user instead of waiting if no transactions are available
        if (user.DailyTransactions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]! No transactions recorded yet.[/]");
            if (AnsiConsole.Confirm("Would you like to log your first transaction now?"))
            {
                // Pivot directly to the logging UI
                TransactionUI.LogDailyTransaction(user, renderHeader);
                
                // After logging, if they still have no transactions (cancelled), return
                if (user.DailyTransactions.Count == 0) return;
            }
            else
            {
                return; // Return to Step 2 (Menu)
            }
        }

        bool viewingCategories = true;
        while (viewingCategories)
        {
            AnsiConsole.Clear();
            renderHeader();

            // 1. Get unique categories manually
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

            // 2. Filter and calculate
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

            // 3. Display Table
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

            // Corrected Footer assignment (IRenderable)
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
