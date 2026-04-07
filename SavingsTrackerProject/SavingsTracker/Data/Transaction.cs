namespace SavingsTracker.Data;

/// <summary>
/// Represents an individual financial movement, such as an expense or an income entry.
/// </summary>
/// <param name="Name">A descriptive name or label for the transaction (e.g., "Grocery Store").</param>
/// <param name="Amount">The monetary value (positive for income, negative for expenses).</param>
/// <param name="Category">The classification for the transaction (e.g., "Food", "Salary", "Rent").</param>
/// <param name="Date">The specific date and time the transaction occurred.</param>
public record Transaction(string Name, double Amount, string Category, DateTime Date);
