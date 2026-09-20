using System.ComponentModel;
using BitOfTech.Expenses.Mcp.Contracts;
using BitOfTech.Expenses.Mcp.Data;
using BitOfTech.Expenses.Mcp.Models;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BitOfTech.Expenses.Mcp.Tools;

[McpServerToolType]
public class ExpenseTools(ExpensesDbContext db)
{
    [McpServerTool(Name = "search_expense_reports")]
    [Description("Searches expense reports by employee and by where they are in the approval workflow. Returns a summary of each match without the line items. Call get_expense_report when you need the lines.")]
    public async Task<IReadOnlyList<ExpenseReportSummary>> SearchExpenseReportsAsync(
        [Description("Part of an employee name, matched case insensitively. Omit to search across all employees.")]
        string? employeeName = null,
        [Description("Where the report sits in the approval workflow right now. Only use this when the question is about the current state. A report that was approved or rejected was also submitted at some point, so do not filter on Submitted to answer questions about what somebody has submitted.")]
        ExpenseReportStatus? currentStatus = null,
        CancellationToken cancellationToken = default)
    {
        var query =
            from report in db.Reports.AsNoTracking()
            join employee in db.Employees on report.EmployeeId equals employee.Id
            select new { Report = report, Employee = employee };

        if (!string.IsNullOrWhiteSpace(employeeName))
        {
            query = query.Where(x => x.Employee.Name.Contains(employeeName));
        }

        if (currentStatus is not null)
        {
            query = query.Where(x => x.Report.Status == currentStatus);
        }

        return await query
            .OrderByDescending(x => x.Report.SubmittedOn)
            .Select(x => new ExpenseReportSummary(
                x.Report.Id,
                x.Report.Title,
                x.Employee.Name,
                x.Report.Status,
                x.Report.Currency,
                x.Report.TotalAmount,
                x.Report.SubmittedOn,
                x.Report.Lines.Count))
            .ToListAsync(cancellationToken);
    }

    [McpServerTool(Name = "get_expense_report")]
    [Description("Gets one expense report with its line items, by id. Call search_expense_reports first if you do not already have an id.")]
    public async Task<ExpenseReportDetail> GetExpenseReportAsync(
        [Description("The expense report id, for example 1001.")] int reportId,
        CancellationToken cancellationToken = default)
    {
        // McpException reaches the model. Other exception types are replaced with a
        // generic message, so the model never learns what it did wrong.
        return await db.ReportDetail(reportId).FirstOrDefaultAsync(cancellationToken)
            ?? throw new McpException(
                $"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");
    }

    [McpServerTool(Name = "submit_expense_report")]
    [Description("Submits a draft expense report for approval. Only works on a report that is still a draft, and it cannot be undone.")]
    public async Task<ExpenseReportDetail> SubmitExpenseReportAsync(
        [Description("The id of the draft expense report to submit.")] int reportId,
        CancellationToken cancellationToken = default)
    {
        var report = await db.Reports.FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken)
            ?? throw new McpException(
                $"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");

        if (report.Status is not ExpenseReportStatus.Draft)
        {
            throw new McpException(
                $"Report {reportId} is {report.Status}, so it has already been submitted and cannot be submitted again.");
        }

        report.Status = ExpenseReportStatus.Submitted;
        report.SubmittedOn = DateOnly.FromDateTime(DateTime.UtcNow);

        await db.SaveChangesAsync(cancellationToken);

        return await db.ReportDetail(reportId).FirstAsync(cancellationToken);
    }
}
