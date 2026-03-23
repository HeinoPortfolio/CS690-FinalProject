using Xunit;
using SavingsTracker.Data;

namespace SavingsTracker.Tests;

public class GoalTests
{
    [Theory]
    [InlineData("30 days", 30)]
    [InlineData("2 weeks", 14)]
    [InlineData("6 months", 182)] 
    [InlineData("1 year", 365)]
    [InlineData("12MONTHS", 365)] 
    public void CalculateEndDate_ShouldHandleVariousFormats(string timeframe, int expectedMinDays)
    {
        
        var start = new DateTime(2024, 1, 1);
        
        
        var goal = new Goal("Test Goal", 1000, timeframe, start);
        var totalDays = (goal.EndDate - start).TotalDays;

        
        Assert.True(totalDays >= expectedMinDays);
    }
}
