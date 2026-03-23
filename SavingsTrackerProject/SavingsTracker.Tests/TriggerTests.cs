using Xunit;
using SavingsTracker.Data;
using SavingsTracker.UI;
using System.IO;

namespace SavingsTracker.Tests;

public class TriggerTests : IDisposable
{
    private const string TestTriggerFile = "unit_test_triggers.txt";

    public TriggerTests() => Cleanup();
    public void Dispose() => Cleanup();

    private void Cleanup()
    {
        if (File.Exists(TestTriggerFile)) File.Delete(TestTriggerFile);
    }

    [Fact]
    public void SaveAndLoadTriggeredTransactions_ShouldPreserveBehaviorTags()
    {
        
        var originalDate = new DateTime(2024, 5, 1, 10, 0, 0);
        var triggers = new List<TriggeredTransaction>
        {
            new TriggeredTransaction("Late Night Pizza", 25.50, "Boredom", originalDate)
        };

        
        var line = $"{triggers[0].Name}|{triggers[0].Amount}|{triggers[0].Tag}|{triggers[0].OriginalDate:O}";
        File.WriteAllLines(TestTriggerFile, new[] { line });

        // Simulate the loading logic from TriggerUI
        var loaded = new List<TriggeredTransaction>();
        foreach (var fileLine in File.ReadAllLines(TestTriggerFile))
        {
            var p = fileLine.Split('|');
            if (p.Length == 4)
                loaded.Add(new TriggeredTransaction(p[0], double.Parse(p[1]), p[2], DateTime.Parse(p[3])));
        }

        // Assert
        Assert.Single(loaded);
        Assert.Equal("Boredom", loaded[0].Tag);
        Assert.Equal(25.50, loaded[0].Amount);
        Assert.Equal(originalDate, loaded[0].OriginalDate);
    }
}
