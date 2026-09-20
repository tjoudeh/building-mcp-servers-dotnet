using BitOfTech.Expenses.Mcp.Models;

namespace BitOfTech.Expenses.Mcp.Contracts;

public record ExpenseReportDetail(
    int Id,
    string Title,
    string EmployeeName,
    string EmployeeEmail,
    ExpenseReportStatus Status,
    string Currency,
    decimal TotalAmount,
    DateOnly? SubmittedOn,
    IReadOnlyList<ExpenseLineDetail> Lines);

public record ExpenseLineDetail(
    string Description,
    string Category,
    decimal Amount,
    DateOnly IncurredOn);
