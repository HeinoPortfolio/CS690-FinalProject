namespace SavingsTracker;

using Spectre.Console;


class Program
{
    static void Main(string[] args)
    {
        // 1. Define your selection prompt
        var prompt = new SelectionPrompt<string>()
            .Title("What is your [green]favorite fruit[/]?")
            .AddChoices(new[] { "Apple", "Banana", "Orange", "Mango" });

        // 2. Wrap the result or content in a Panel to add a border
        string fruit = AnsiConsole.Prompt(prompt);

        var resultPanel = new Panel($"You selected: [yellow]{fruit}[/]")
            .Header("[bold] Selection Result [/]")
            .Border(BoxBorder.Rounded)
            .BorderColor(Color.Green)
            .Padding(1, 1, 1, 1); // Adds internal spacing

        AnsiConsole.Write(resultPanel);
    }
}








