namespace SavingsTracker.Data;

public record TriggeredTransaction(
    string Name, 
    double Amount, 
    string Tag, 
    DateTime OriginalDate
);

