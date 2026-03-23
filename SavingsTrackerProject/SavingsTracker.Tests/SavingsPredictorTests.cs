using Xunit;
using SavingsTracker.Data;
using SavingsTracker.Logic;

namespace SavingsTracker.Tests;

public class SavingsPredictorTests
{
    [Fact]
    public void CalculateRequiredRate_ShouldReturnCorrectRemainingAmount()
    {
        var start = DateTime.Now;
        var end = start.AddDays(10);
        var goal = new Goal("Vacation Fund", 1000, "10 days", start, end);
        var user = new User("Tester", "pass123") 
        { 
            ActiveGoal = goal, 
            CurrentSavings = 200 
        };

        var result = SavingsPredictor.CalculateRequiredRate(user);

        Assert.NotNull(result);
        Assert.Equal(800, result.RemainingAmount); // Target - Current
        Assert.Equal(20, result.OverallPercentage); // (200/1000)*100
    }

    [Fact]
    public void CalculateRequiredRate_ShouldReturnNull_WhenNoActiveGoal()
    {
        // Arrange: User has no active goal
        var user = new User("EmptyUser", "pass") { ActiveGoal = null };

        // Act
        var result = SavingsPredictor.CalculateRequiredRate(user);

        // Assert
        Assert.Null(result);
    }
}
