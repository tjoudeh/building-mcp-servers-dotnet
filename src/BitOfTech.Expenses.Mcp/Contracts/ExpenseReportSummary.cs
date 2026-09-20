using BitOfTech.Expenses.Mcp.Models;

namespace BitOfTech.Expenses.Mcp.Contracts;

/// <summary>One line per report. Deliberately has no line items on it.</summary>
public record ExpenseReportSummary(
    int Id,
    string Title,
    string EmployeeName,
    ExpenseReportStatus Status,
    string Currency,
    decimal TotalAmount,
    DateOnly? SubmittedOn,
    int LineCount);
