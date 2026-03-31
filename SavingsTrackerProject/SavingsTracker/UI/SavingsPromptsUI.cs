


using Spectre.Console;
using SavingsTracker.Data;

namespace SavingsTracker.UI;

public static class SavingsPromptsUI
{
    private static bool _notificationsEnabled = true;
    private static TimeSpan? _scheduledTime = null;
    private static DateTime? _lastTriggeredDate = null;
    
    public static string? ActiveNotice = null;

    public static void CheckScheduledTime(User user)
    {
        if (!_notificationsEnabled || !_scheduledTime.HasValue) return;

        string nowTime = DateTime.Now.ToString("h:mm tt");
        string scheduledTime = DateTime.Today.Add(_scheduledTime.Value).ToString("h:mm tt");

        if (nowTime == scheduledTime)
        {
            if (_lastTriggeredDate == null || _lastTriggeredDate.Value.Date != DateTime.Today.Date)
            {
                _lastTriggeredDate = DateTime.Today;
                ActiveNotice = $"⏰ Time to transfer funds for: {user.ActiveGoal?.Name ?? "your goal"}";
            }
        }
    }

    public static void Show(User user, Action renderHeader)
    {
        AnsiConsole.Clear();
        renderHeader();
        
        RenderActiveNotice();

        AnsiConsole.Write(new Rule($"[{Palette.Brand.ToMarkup()}]Savings Prompts[/]").Centered());

        var status = _notificationsEnabled ? "[green]ENABLED[/]" : "[red]SUSPENDED[/]";
        AnsiConsole.MarkupLine($"Current Status: {status}");
        
        if (_scheduledTime.HasValue) 
        {
            var displayTime = DateTime.Today.Add(_scheduledTime.Value).ToString("h:mm tt");
            AnsiConsole.MarkupLine($"Scheduled for: [yellow]{displayTime}[/] daily");
        }
        else
        {
            AnsiConsole.MarkupLine("[grey]No reminder scheduled.[/]");
        }

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("\nManage Reminders")
                .HighlightStyle(new Style(foreground: Palette.SelectionFg, background: Palette.SelectionBg, decoration: Decoration.Bold))
                .AddChoices("Set/Change Reminder Time", "Toggle Enable/Suspend", "Clear Active Notice", "TEST: Trigger Now", "Return to Menu"));

        if (choice == "Set/Change Reminder Time")
        {
            var timeInput = AnsiConsole.Prompt(
                new TextPrompt<string>("Enter time (e.g., [yellow]7:45 AM[/] or [yellow]10:00 PM[/]):")
                    .Validate(input => 
                    {
                        return DateTime.TryParse(input, out _) 
                            ? ValidationResult.Success() 
                            : ValidationResult.Error("[red]Invalid format. Please use: 7:45 AM[/]");
                    }));

            if (DateTime.TryParse(timeInput, out DateTime parsedTime))
            {
                _scheduledTime = parsedTime.TimeOfDay;
                _notificationsEnabled = true;
                AnsiConsole.MarkupLine($"[green]✓ Reminder set for {parsedTime:h:mm tt}[/]");
                Thread.Sleep(1200);
            }
            
            Show(user, renderHeader); 
        }
        else if (choice == "Toggle Enable/Suspend")
        {
            _notificationsEnabled = !_notificationsEnabled;
            Show(user, renderHeader);
        }
        else if (choice == "Clear Active Notice")
        {
            ActiveNotice = null;
            Show(user, renderHeader);
        }
        else if (choice == "TEST: Trigger Now")
        {
            ActiveNotice = $"⏰ Reminder: It's time to save for {user.ActiveGoal?.Name ?? "your goal"}!";
            Show(user, renderHeader);
        }
    }

    public static void RenderActiveNotice()
    {
        if (string.IsNullOrEmpty(ActiveNotice)) return;

        // FIX: Using API-based styling to prevent the word 'grey' from appearing as literal text
        var noticeText = new Text(ActiveNotice, new Style(foreground: Color.Yellow, decoration: Decoration.Bold));
        var instructions = new Text(
            "\nTo dismiss: Go to 'Receive savings prompts' > 'Clear Active Notice'", 
            new Style(foreground: Color.Grey, decoration: Decoration.Italic));

        var panel = new Panel(new Rows(noticeText, instructions))
            .Header(" [bold red]SAVINGS ALERT[/] ")
            .BorderColor(Color.Yellow)
            .Padding(1, 1)
            .Expand();

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }
}





