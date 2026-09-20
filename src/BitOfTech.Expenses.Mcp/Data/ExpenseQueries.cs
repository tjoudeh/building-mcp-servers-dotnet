using BitOfTech.Expenses.Mcp.Contracts;
using Microsoft.EntityFrameworkCore;

namespace BitOfTech.Expenses.Mcp.Data;

// The tool, the resource and the prompt all need a report in the same shape.
public static class ExpenseQueries
{
    public static IQueryable<ExpenseReportDetail> ReportDetail(this ExpensesDbContext db, int reportId) =>
        from report in db.Reports.AsNoTracking()
        join employee in db.Employees on report.EmployeeId equals employee.Id
        where report.Id == reportId
        select new ExpenseReportDetail(
            report.Id,
            report.Title,
            employee.Name,
            employee.Email,
            report.Status,
            report.Currency,
            report.TotalAmount,
            report.SubmittedOn,
            report.Lines
                .OrderBy(l => l.IncurredOn)
                .Select(l => new ExpenseLineDetail(l.Description, l.Category, l.Amount, l.IncurredOn))
                .ToList());
}
