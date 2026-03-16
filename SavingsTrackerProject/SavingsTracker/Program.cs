using Spectre.Console;
using Spectre.Console.Rendering;
using System.Text;

// New Classes
using SavingsTracker.Data;

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

            if (choice == "Create an Account") CreateAccount();
            else if (choice == "Login") Login();
            else isRunning = false;
        }

        AnsiConsole.MarkupLine("[bold blue]Goodbye!.[/]");
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

            var (goal, savings, history) = UserRepository.LoadGoal(user.Username);
            user.ActiveGoal = goal;
            user.CurrentSavings = savings;

            user.Contributions = history;

            ShowFinancialDashboard(user);
        }
        else
        {
            AnsiConsole.MarkupLine("[red]![/] Invalid credentials.");
            Thread.Sleep(1500);
            Login();
        }
    }

    private static void LogContribution(User user)
    {
         if (user.ActiveGoal == null) return;
         
        AnsiConsole.Clear(); Header();
        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Log Contribution[/]").Centered());
        
        var amount = AnsiConsole.Prompt(new TextPrompt<double>($"[{Palette.Accent.ToMarkup()}]>[/] Amount to contribute: $")
            .Validate(n => n > 0 ? ValidationResult.Success() : ValidationResult.Error("[red]Must be > 0[/]")));

        user.CurrentSavings += amount;
        user.Contributions.Add(new Contribution(amount, DateTime.Now));

        UserRepository.SaveGoal(user.Username, user.ActiveGoal
            , user.CurrentSavings
            , user.Contributions);
    
        if (user.CurrentSavings >= user.ActiveGoal.TargetAmount)
        {
            AnsiConsole.Clear();
            Header();

            var goldPanel = new Panel(Align.Center(
                new Rows(
                    new Text("★ SAVINGS GOAL ACHIEVED ★", new Style(Color.Gold1, decoration: Decoration.Bold)),
                    new Text("\n"),
                    new Text($"GOLD STATUS REACHED!", new Style(Color.Yellow, decoration: Decoration.Bold)),
                    new Text($"You've successfully saved {user.CurrentSavings:C2} for '{user.ActiveGoal.Name}'", new Style(Palette.TextDim)),
                    new Text("\n[ Press any key to continue ]", new Style(Color.Grey))
                ), 
                VerticalAlignment.Middle))
                .BorderColor(Color.Gold1)
                .Border(BoxBorder.Double)
                .Padding(2, 2)
                .Expand();

            AnsiConsole.Write(goldPanel);
            Console.ReadKey(true);

            if (AnsiConsole.Confirm("[red]Goal reached![/] Would you like to [bold red]delete[/] this goal and all contribution history now?"))
            {
                
                user.ActiveGoal = null;
                user.CurrentSavings = 0;
                user.Contributions.Clear();

                string path = $"{user.Username}_goal.txt";
                if (File.Exists(path))
                {
                    try 
                    {
                        File.Delete(path);
                        AnsiConsole.MarkupLine("[grey]✓ Success: Goal file permanently removed.[/]");
                    }
                    catch (IOException ex)
                    {
                        AnsiConsole.MarkupLine($"[red]! Error deleting file: {ex.Message}[/]");
                    }
                }
    
                 Thread.Sleep(1500);
            }
        }
        else 
        {
            
            AnsiConsole.MarkupLine($"[green]✓[/] {amount:C2} added! New Total: [bold]{user.CurrentSavings:C2}[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);

        }
    }

    private static void MonitorProgress(User user)
    {
        if (user.ActiveGoal == null) return;
        if (user.Contributions == null) user.Contributions = new List<Contribution>();

        AnsiConsole.Clear();
        Header();

        var goal = user.ActiveGoal;
        double current = user.CurrentSavings;
    
        double target = goal.TargetAmount > 0 ? goal.TargetAmount : 1; 
        double remaining = Math.Max(0, target - current);
        
        var chart = new BreakdownChart()
            .FullSize()
            .AddItem("Saved: $", current, Palette.StatusBar)
            .AddItem("Remaining: $", remaining, Palette.TextDim);

        var infoTable = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
            infoTable.AddColumn("[grey]Timeline & Totals[/]");
            infoTable.AddColumn(new TableColumn("[grey]Value[/]").RightAligned());
            infoTable.AddRow("Goal Start Date", goal.CreatedAt.ToString("MMMM dd, yyyy") ?? "N/A");
            infoTable.AddRow("Target End Date", goal.EndDate.ToString("MMMM dd, yyyy") ?? "N/A");
            infoTable.AddEmptyRow();
            infoTable.AddRow("Target Goal Amount", $"{target:C2}");
            infoTable.AddRow("Total Amount Saved", $"[green]{current:C2}[/]");

        var historyList = new List<IRenderable>();
        
        if (user.Contributions.Any())
        {
            foreach (var cont in user.Contributions)
            {
                historyList.Add(new Text($" • {cont.Amount:C2} logged on {cont.Date:MMMM dd, yyyy}", new Style(Palette.TextDim)));
            }
        }
        else
        {
            historyList.Add(new Text(" No contributions logged yet.", new Style(Palette.TextDim)));
        }

        var mainLayout = new Panel(new Rows(
            new Text($"Progress Analysis: {goal.Name}", new Style(Palette.Brand, decoration: Decoration.Bold)),
            new Padder(chart, new Padding(0, 1, 0, 1)),
            infoTable,
            new Panel(new Rows(historyList)).Header(" Contribution History ").BorderColor(Palette.Border)
        ))
        .BorderColor(Palette.Border)
        .Padding(2, 1, 2, 1);

        AnsiConsole.Write(mainLayout);

        AnsiConsole.WriteLine();
        // Use a manual wait to prevent the "reverting" issue
        AnsiConsole.MarkupLine($"[{Palette.Brand.ToMarkup()}]Analysis Complete.[/] Press [yellow]ENTER[/] to return...");
        Console.ReadLine(); 
    }


    private static void ShowFinancialDashboard(User user)
    {

        bool inDashboard = true;

        while (inDashboard)
        {
            AnsiConsole.Clear();
            Header();

            if (user.ActiveGoal != null)
            {
                var goal = user.ActiveGoal;
                double current = user.CurrentSavings;


                double remaining = goal.TargetAmount - current;  
                double target = goal.TargetAmount;
                double percentComplete = current / target;

                // New
                Panel ? achievementPanel = null;

                if (current >= target &&  target> 0)
                {
                    achievementPanel = new Panel(new Text("★ SAVINGS GOAL ACHIEVED ★"
                        , new Style(Color.Gold1, decoration: Decoration.Bold)).Centered())
                        .BorderColor(Color.Gold1)
                        .Border(BoxBorder.Double);
                }


                var breakdown = new BreakdownChart().Width(60)
                    .AddItem("Saved: $", current, Palette.StatusBar) 
                    .AddItem("Remaining: $", Math.Max(0, goal.TargetAmount - current), Palette.TextDim)
                    .AddItem("Percent Remaining (%): ", Math.Round((1 - percentComplete) * 100, 2), Palette.TextDim);


                var summaryTable = new Table().Border(TableBorder.Rounded).BorderColor(Palette.Border).Expand();
                    summaryTable.AddColumn("[grey]Goal Progress[/]");
                    summaryTable.AddColumn("[grey]Value[/]");
                    summaryTable.AddRow("Target Goal", $"{target:C2}");
                    summaryTable.AddRow("Current Balance", $"[{Palette.Brand.ToMarkup()}]{current:C2}[/]");
                    summaryTable.AddRow("Amount Left", $"[red]{remaining:C2}[/]");
                    summaryTable.AddRow("Percentage Complete: ", $"[blue]{percentComplete:P2}[/]");

              
                AnsiConsole.Write(new Panel(new Rows(
                    new Text($"Goal: {goal.Name}", new Style(Palette.Brand, decoration: Decoration.Bold)),
                    achievementPanel ?? (IRenderable)new Text(""),
                    new Text($"Target End Date: {goal.EndDate:MMMM dd, yyyy}", new Style(Palette.TextDim)),
                    new Rule().RuleStyle(Palette.Border.ToMarkup()),
                    new Padder(breakdown, new Padding(0, 1, 0, 1)), summaryTable
                    )).Header($" Progress for {user.Username} ")
                    
                    .BorderColor(Palette.Border).Padding(2, 1, 2, 1));
            }
            else AnsiConsole.MarkupLine($"[yellow]! No active goal found. Select 'Create a new savings goal' to begin.[/]");

            var choice = AnsiConsole.Prompt( new SelectionPrompt<string>().Title($"[{Palette.TextDim.ToMarkup()}]SELECT OPERATION[/]")
                .HighlightStyle(new Style(Palette.SelectionFg, Palette.SelectionBg, Decoration.Bold))
                .AddChoices(DashboardMenu.Logout) 
                .AddChoiceGroup($"[{Palette.Accent.ToMarkup()}]SAVINGS GOALS[/]", DashboardMenu.GoalOptions)
                .AddChoiceGroup($"[{Palette.Accent.ToMarkup()}]TRANSACTIONS[/]", DashboardMenu.TransactionOptions)
                .AddChoiceGroup($"[{Palette.Accent.ToMarkup()}]ANALYSIS[/]")
                .AddChoices(DashboardMenu.AnalysisOptions)); 


            if (choice == DashboardMenu.Logout) inDashboard = false;
            else if (choice == "Create a new savings goal")
            {
                bool proceedWithCreation = true;

                if (user.ActiveGoal != null)
                {
                    AnsiConsole.MarkupLine($"[yellow]![/] A goal [bold]'{user.ActiveGoal.Name}'[/] already exists.");

                    var summaryTable = new Table().Border(TableBorder.None).HideHeaders().AddColumn("Data");
                        summaryTable.AddRow($"[red]•[/] Goal: [bold]{user.ActiveGoal.Name}[/]");
                        summaryTable.AddRow($"[red]•[/] Saved: {user.CurrentSavings:C2}");
                        summaryTable.AddRow($"[red]•[/] Contribution History: {user.Contributions.Count} contribution(s)");

                     AnsiConsole.Write(new Panel(summaryTable)
                        .Header("[bold red] DATA DELETION SUMMARY [/]")
                        .BorderColor(Color.Red)
                        .Padding(1, 1));

                    if (AnsiConsole
                        .Confirm("Creating a new goal will [red]delete all current progress and contributions[/]. Proceed?"))
                    {
                        user.ActiveGoal = null;
                        user.CurrentSavings = 0;
                        user.Contributions.Clear();

                        string path = $"{user.Username}_goal.txt";
                        if (File.Exists(path)) File.Delete(path);

                        AnsiConsole.MarkupLine("[grey]Existing goal and history cleared.[/]");
                    }
                    else
                    {
                        proceedWithCreation = false;
                    }
                }

                if (proceedWithCreation)
                {
                    var newGoal = Goal.Create();
                    if (newGoal != null)
                    {
                        user.ActiveGoal = newGoal;
                        user.CurrentSavings = 0;

                        // Save fresh (this overwrites or creates the new file)
                        UserRepository.SaveGoal(user.Username, newGoal, 0, user.Contributions); 

                    }   
                }
            }
            else if (choice == "Log a contribution") LogContribution(user);
            else if (choice == "Monitor progress toward the goal")
            {
                MonitorProgress(user);
            }
            else
            {
                AnsiConsole.Write(new Rule().RuleStyle(Palette.Border));
                AnsiConsole.MarkupLine($"[{Palette.Brand.ToMarkup()}]Not yet implemented:[/] Will not be executing {choice}... Hit a key to be taken to main menu!");
                Console.ReadKey(true);   
            }
        }
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

