namespace SavingsTracker.Data;

/// <summary>
/// Represents a single financial contribution to a savings goal.
/// Using a record provides built-in value-based equality and immutability.
/// </summary>
/// <param name="Amount">The monetary value of the contribution.</param>
/// <param name="Date">The date and time when the contribution was made.</param>

public record Contribution (double Amount, DateTime Date);
