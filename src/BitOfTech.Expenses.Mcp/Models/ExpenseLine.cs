namespace BitOfTech.Expenses.Mcp.Models;

public class ExpenseLine
{
    public int Id { get; set; }

    public int ExpenseReportId { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateOnly IncurredOn { get; set; }
}
