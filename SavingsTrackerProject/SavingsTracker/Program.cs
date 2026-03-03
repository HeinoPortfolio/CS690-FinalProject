using Spectre.Console;
using Spectre.Console.Rendering;

namespace SavingsTracker;

public class Program
{
    private static List<User> _userDatabase = new();
    private static readonly string[] menuItems = { "Create an Account", "Login", "Quit the application" };

    public static async Task Main(string[] args)
    {
        // Load existing users from the text file at startup
        _userDatabase = UserRepository.LoadUsers();

        int selectedIndex = 0;
        bool isRunning = true;

        while (isRunning)
        {
            AnsiConsole.Clear();
            Header(); 

            // Live display for the main selection dashboard
            await AnsiConsole.Live(GetMainContainer(selectedIndex))
                .StartAsync(async ctx =>
                {
                    ctx.Refresh();
                    while (true)
                    {
                        var key = Console.ReadKey(true).Key;
                        if (key == ConsoleKey.UpArrow)
                            selectedIndex = (selectedIndex == 0) ? menuItems.Length - 1 : selectedIndex - 1;
                        else if (key == ConsoleKey.DownArrow)
                            selectedIndex = (selectedIndex == menuItems.Length - 1) ? 0 : selectedIndex + 1;
                        else if (key == ConsoleKey.Enter)
                            break;

                        ctx.UpdateTarget(GetMainContainer(selectedIndex));
                    }
                });

            string choice = menuItems[selectedIndex];

            if (choice == "Create an Account") CreateAccount();
            else if (choice == "Login") Login();
            else isRunning = false;
        }

        AnsiConsole.MarkupLine("[bold red]Application Terminated.[/]");
    }

    private static void CreateAccount()
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
            if (_userDatabase.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[red]![/] Error: Username already exists.");
                Console.ReadKey(true);
            }
            else
            {
                var newUser = new User(username, password);
                _userDatabase.Add(newUser);
                UserRepository.SaveUser(newUser);
                AnsiConsole.MarkupLine($"[bold {Palette.Brand.ToMarkup()}]✓[/] Account saved successfully.");
                Thread.Sleep(1000);
            }
        }
        else { CreateAccount(); }
    }

    private static void Login()
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Secure Login[/]").LeftJustified());

        var username = AnsiConsole.Ask<string>("Username:");
        var password = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

        var user = _userDatabase.FirstOrDefault(u => u.Username == username && u.Password == password);

        if (user != null)
        {
            ShowFinancialDashboard(user.Username);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]![/] Invalid credentials.");
            Thread.Sleep(1500);
            Login();
        }
    }

    private static void ShowFinancialDashboard(string username)
    {
        AnsiConsole.Clear();
        Header();

        double target = 500.00;
        double current = 325.50;
        double remaining = target - current;

        // Visual Progress: BreakdownChart for linear visualization
        var breakdown = new BreakdownChart()
            .Width(60)
            .AddItem("Saved", current, Palette.Brand)
            .AddItem("Remaining", remaining, Palette.TextDim);

        var summaryTable = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
        summaryTable.AddColumn("[grey]Metric[/]");
        summaryTable.AddColumn("[grey]Value[/]");
        summaryTable.AddRow("Target Goal", $"{target:C2}");
        summaryTable.AddRow("Current Balance", $"[{Palette.Brand.ToMarkup()}]{current:C2}[/]");
        summaryTable.AddRow("Amount Left", $"[red]{remaining:C2}[/]");

        AnsiConsole.Write(new Panel(new Rows(
            new Text($"Progress for {username}", new Style(Palette.Brand)),
            new Padder(breakdown, new Padding(0, 1, 0, 1)),
            summaryTable
        )).Header(new PanelHeader(" Goal Progress ", Justify.Center))
          .BorderColor(Palette.Border).Padding(2, 1, 2, 1));

        AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("\n[blue]Select Operation:[/]")
            .AddChoices(["Log Deposit", "Logout"]));
    }

    private static void Header()
    {
        AnsiConsole.Write(new Rule($"[bold {Palette.Brand.ToMarkup()}] SAVINGS TRACKER [/]").RuleStyle(Palette.Border).Centered());
        AnsiConsole.Write(new Text("  v1.0 Enterprise Edition", new Style(Palette.TextDim)).Centered());
        AnsiConsole.WriteLine();
    }

    private static IRenderable GetMainContainer(int index)
    {
        var leftPanel = new Panel(new Rows(
                new Text("↑ ↓   Browse options", new Style(Palette.TextDim)),
                new Text("Enter Confirm selection", new Style(Palette.TextDim))
            ))
            .Header(" Instructions ").BorderColor(Palette.Border).Expand();

        var menuRows = menuItems.Select((item, i) => 
            new Text($"{(i == index ? "> " : "  ")}{item}", 
            (i == index) ? new Style(Palette.SelectionFg, Palette.SelectionBg) : new Style(Palette.TextDim))).ToList();

        var rightPanel = new Panel(new Rows(menuRows)).Header(" Menu Choice ").BorderColor(Palette.Border).Expand();

        var grid = new Grid().AddColumn(new GridColumn().Width(30)).AddColumn(new GridColumn().Width(35));
        grid.AddRow(leftPanel, rightPanel);

        return new Panel(grid)
            .Header(new PanelHeader("[bold grey74] Dashboard [/]", Justify.Center))
            .Border(BoxBorder.Double).BorderColor(Palette.Border).Expand();
    }
}

