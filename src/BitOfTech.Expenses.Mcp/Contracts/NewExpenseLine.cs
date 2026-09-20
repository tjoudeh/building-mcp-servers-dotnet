namespace BitOfTech.Expenses.Mcp.Contracts;

/// <summary>A line item supplied when creating a report. Has no id, the database assigns one.</summary>
public record NewExpenseLine(
    string Description,
    string Category,
    decimal Amount,
    DateOnly IncurredOn);
