using Xunit;
using SavingsTracker;
using SavingsTracker.Data;
using System.IO;

namespace SavingsTracker.Tests;

public class UserRepositoryTests : IDisposable
{
    private const string TestUser = "test_unit_user";

    public UserRepositoryTests() => Cleanup();
    public void Dispose() => Cleanup();

    private void Cleanup()
    {
        if (File.Exists($"{TestUser}_goal.txt")) File.Delete($"{TestUser}_goal.txt");
    }

    [Fact]
    public void SaveAndLoadGoal_ShouldPreserveContributionHistory()
    {

        var goal = new Goal("Home", 50000, "5 years", DateTime.Now);
        var history = new List<Contribution> { new(100.50, DateTime.Now) };
        double currentSavings = 100.50;

        
        UserRepository.SaveGoal(TestUser, goal, currentSavings, history);
        var (loadedGoal, loadedSavings, loadedHistory) = UserRepository.LoadGoal(TestUser);

        // Assert: 
        Assert.NotNull(loadedGoal);
        Assert.Equal("Home", loadedGoal.Name);
        Assert.Equal(currentSavings, loadedSavings);
        Assert.Single(loadedHistory);
        Assert.Equal(100.50, loadedHistory[0].Amount);
    }
}
