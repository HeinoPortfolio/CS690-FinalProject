using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text;

// New Classes =====================================
using SavingsTracker.Data;
using SavingsTracker.UI;

namespace SavingsTracker;

public class Program
{
    private static List<User> _userDatabase = new();
    private static readonly string[] menuItems = { "Create an Account", "Login", "Quit the application" };

    public static async Task Main(string[] args)
    {
        
        Console.OutputEncoding = Encoding.UTF8;

        Console.InputEncoding = Encoding.UTF8;
        
        // Load existing users from the text file at startup
        _userDatabase = UserRepository.LoadUsers();

        int selectedIndex = 0;

        bool isRunning = true;

        while (isRunning)
        {
            AnsiConsole.Clear();
            Header(); 

            // Live display for the main selection dashboard
            await AnsiConsole.Live(GetMainContainer(selectedIndex)).StartAsync(async ctx =>
                {
                    ctx.Refresh();

                    while (true)
                    {
                        var key = Console.ReadKey(true).Key;

                        if (key == ConsoleKey.UpArrow)
                            selectedIndex = (selectedIndex == 0) ? menuItems.Length - 1 : selectedIndex - 1;
                        else if (key == ConsoleKey.DownArrow)
                            selectedIndex = (selectedIndex == menuItems.Length - 1) ? 0 : selectedIndex + 1;
                        else if (key == ConsoleKey.Enter) break;

                        ctx.UpdateTarget(GetMainContainer(selectedIndex));
                    }
                });


            string choice = menuItems[selectedIndex];

            if (choice == "Create an Account")
            {
                AccountUI.CreateAccount(_userDatabase);

            } 
            else if (choice == "Login") Login();
            else isRunning = false;
        }

        AnsiConsole.MarkupLine("[bold blue]Goodbye!.[/]");
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

            var (goal, savings, history) = UserRepository.LoadGoal(user.Username);

            user.ActiveGoal = goal;
            user.CurrentSavings = savings;
            user.Contributions = history;

            DashboardUI.Show(user, Header, MonitorProgress);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]![/] Invalid credentials.");
            Thread.Sleep(1500);
            Login();
        }
    }

    private static void MonitorProgress(User user)
    {
        if (user.ActiveGoal == null) return;

        AnsiConsole.Clear();
        Header();

        // Call the new UI class
        ProgressUI.RenderStats(user);

        AnsiConsole.WriteLine();
        // Use a manual wait to prevent the "reverting" issue
        AnsiConsole.MarkupLine($"[{Palette.Brand.ToMarkup()}]Analysis Complete.[/] Press [yellow]ENTER[/] to return...");
        Console.ReadLine();
       
    }

    private static void Header()
    {
        AnsiConsole.Write(new Rule($"[bold {Palette.Brand.ToMarkup()}] SAVINGS TRACKER [/]").RuleStyle(Palette.Border).Centered());
        AnsiConsole.Write(new Text(" v1.0 ", new Style(Palette.TextDim)).Centered());
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

