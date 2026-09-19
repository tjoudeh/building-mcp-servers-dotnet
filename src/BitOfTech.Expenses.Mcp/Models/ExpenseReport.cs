namespace BitOfTech.Expenses.Mcp.Models;

public enum ExpenseReportStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected
}

public class ExpenseReport
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string Title { get; set; } = string.Empty;

    public ExpenseReportStatus Status { get; set; }

    /// <summary>ISO 4217 code the report was filed in, for example EUR.</summary>
    public string Currency { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateOnly? SubmittedOn { get; set; }

    public List<ExpenseLine> Lines { get; set; } = [];
}
