using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class TriggerUI
{
    private static List<string> CommonTriggers = new() 
    { 
        "Stress", "Boredom", "Social Pressure", "Impulse", "Reward" 
    };

    public static void IdentifyTriggers(User user, Action renderHeader)
    {
        var triggerFile = $"{user.Username}_triggers.txt";
        var triggeredTransactions = LoadTriggeredTransactions(triggerFile);

        bool inTriggerMenu = true;
        while (inTriggerMenu)
        {
            AnsiConsole.Clear();
            renderHeader();

            var mode = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[{Palette.TextDim.ToMarkup()}]IDENTIFY SPENDING TRIGGERS[/]")
                    .HighlightStyle(new Style(Palette.SelectionFg, Palette.SelectionBg, Decoration.Bold))
                    .AddChoices("Log New Triggered Transaction", "See Spending Reports (Tags)", "Return to Main Menu"));

            if (mode == "Return to Main Menu") break;

            if (mode == "Log New Triggered Transaction")
            {
                HandleLogTrigger(user, triggerFile, triggeredTransactions);
            }
            else if (mode == "See Spending Reports (Tags)")
            {
                ShowReport(triggeredTransactions);
            }
        }
    }

    private static void HandleLogTrigger(User user, string filePath, List<TriggeredTransaction> list)
    {
        string name = "";
        double amount = 0;
        DateTime transactionDate;

        var entryMode = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{Palette.TextDim.ToMarkup()}]CHOOSE ENTRY MODE[/]")
                .AddChoices("Select from History", "Manual Entry"));

        if (entryMode == "Select from History")
        {
            // FILTER: Hide transactions already linked to a trigger based on Name, Amount, and Exact Date
            var available = user.DailyTransactions.Where(dt => 
                !list.Any(tt => tt.Name == dt.Name && tt.Amount == dt.Amount && tt.OriginalDate == dt.Date)).ToList();

            if (!available.Any())
            {
                AnsiConsole.MarkupLine("[yellow]! All historical transactions have already been tagged.[/]");
                Thread.Sleep(2000);
                return;
            }

            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<Transaction>()
                    .Title("Select a transaction to tag:")
                    .PageSize(10)
                    .UseConverter(t => $"{t.Date:MMM dd} - {t.Name} ({t.Amount:C2})")
                    .AddChoices(available));
            
            name = selected.Name;
            amount = selected.Amount;
            transactionDate = selected.Date;
        }
        else
        {
            var manual = GetManualEntry();
            name = manual.name;
            amount = manual.amount;
            transactionDate = DateTime.Now;
        }

        var tag = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"[{Palette.TextDim.ToMarkup()}]Select or Create a Behavior Tag:[/]")
                .AddChoices(CommonTriggers)
                .AddChoices("Add New Tag..."));

        if (tag == "Add New Tag...")
        {
            tag = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Name of new behavior tag:");
            if (AnsiConsole.Confirm("Confirm this new behavior tag?"))
            {
                if (!CommonTriggers.Contains(tag)) CommonTriggers.Add(tag);
            }
            else return;
        }

        list.Add(new TriggeredTransaction(name, amount, tag, transactionDate));
        SaveTriggeredTransactions(filePath, list);
        
        AnsiConsole.MarkupLine("[bold green]✓[/] Behavior insight recorded and hidden from future history selection.");
        Thread.Sleep(1200);
    }

    private static (string name, double amount) GetManualEntry()
    {
        var n = AnsiConsole.Ask<string>($"[{Palette.Accent.ToMarkup()}]>[/] Transaction Name:");
        var a = AnsiConsole.Prompt(
            new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Amount: $")
            .Validate(v => v > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Amount must be > 0[/]")));
        return (n, a);
    }

    private static void ShowReport(List<TriggeredTransaction> transactions)
    {
        if (!transactions.Any())
        {
            AnsiConsole.MarkupLine("[yellow]! No triggered transactions recorded yet.[/]");
            Console.ReadKey(true);
            return;
        }

        double grandTotal = transactions.Sum(t => t.Amount);

        var table = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        table.AddColumn("[grey]Tag[/]");
        table.AddColumn("[grey]Date[/]"); 
        table.AddColumn("[grey]Description[/]");
        table.AddColumn(new TableColumn("[grey]Amount[/]").RightAligned());
        table.AddColumn(new TableColumn("[grey]Share %[/]").RightAligned());

        var grouped = transactions.GroupBy(t => t.Tag).OrderByDescending(g => g.Sum(x => x.Amount));

        foreach (var group in grouped)
        {
            double subTotal = 0;
            foreach (var t in group)
            {
                double itemPercent = (t.Amount / grandTotal) * 100;
                table.AddRow($"[bold yellow]{t.Tag}[/]", $"[blue]{t.OriginalDate:MMM dd}[/]", t.Name, $"{t.Amount:C2}", $"{itemPercent:F1}%");
                subTotal += t.Amount;
            }
            
            double subPercent = (subTotal / grandTotal) * 100;
            table.AddRow("[grey]Sub-total[/]", "", "", $"[grey]{subTotal:C2}[/]", $"[grey]{subPercent:F1}%[/]");
            table.AddEmptyRow(); 
        }

        table.Columns[3].Footer = new Markup($"[bold green]{grandTotal:C2}[/]");
        table.Columns[4].Footer = new Markup("[bold white]100%[/]");

        AnsiConsole.Write(new Panel(table).Header(" Behavior Spending Analysis ").BorderColor(Palette.Brand));
        AnsiConsole.MarkupLine("\nPress [yellow]ENTER[/] to return to menu...");
        Console.ReadLine();
    }

    private static void SaveTriggeredTransactions(string path, List<TriggeredTransaction> data)
    {
        var lines = data.Select(t => $"{t.Name}|{t.Amount}|{t.Tag}|{t.OriginalDate:O}");
        File.WriteAllLines(path, lines);
    }

    private static List<TriggeredTransaction> LoadTriggeredTransactions(string path)
    {
        var list = new List<TriggeredTransaction>();
        if (!File.Exists(path)) return list;

        foreach (var line in File.ReadAllLines(path))
        {
            var p = line.Split('|');
            if (p.Length == 4)
                list.Add(new TriggeredTransaction(p[0], double.Parse(p[1]), p[2], DateTime.Parse(p[3])));
        }
        return list;
    }
}
