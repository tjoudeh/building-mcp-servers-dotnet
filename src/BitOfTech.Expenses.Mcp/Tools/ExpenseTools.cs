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
    [Description("Searches expense reports by employee. Returns a summary of each match without the line items. The word submitted in a question is not a status filter: every report that is now Approved or Rejected was submitted earlier too. To answer what somebody submitted or filed, pass employeeName only and leave currentStatus empty. Call get_expense_report when you need the lines.")]
    public async Task<IReadOnlyList<ExpenseReportSummary>> SearchExpenseReportsAsync(
        [Description("Part of an employee name, matched case insensitively. Omit to search across all employees.")]
        string? employeeName = null,
        [Description("Filters to reports sitting in this exact state right now. Omit it unless the user asked for one specific state.")]
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

    [McpServerTool(Name = "create_expense_report")]
    [Description("Creates a new expense report as a draft, together with its line items. A report cannot be edited after it is created. If the user did not give a title, or gave a date without a year, ask them for it before calling this tool rather than choosing a value yourself. The report is not submitted, so tell the user the new report id and ask whether they want it submitted, then call submit_expense_report.")]
    public async Task<ExpenseReportDetail> CreateExpenseReportAsync(
        [Description("Part of the name of the employee the report belongs to. Must match exactly one employee.")]
        string employeeName,
        [Description("A short title for the report, for example Berlin customer workshop.")]
        string title,
        [Description("Three letter ISO 4217 code the expenses were paid in, for example EUR.")]
        string currency,
        [Description("The line items on the report. At least one is required.")]
        IReadOnlyList<NewExpenseLine> lines,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new McpException("The report needs a title. Ask the user what the expenses were for.");
        }

        if (lines is null or { Count: 0 })
        {
            throw new McpException("An expense report needs at least one line item. Ask the user what they are claiming for.");
        }

        if (lines.Any(line => line.Amount <= 0))
        {
            throw new McpException("Every line item must have an amount greater than zero.");
        }

        if (currency is null || currency.Trim().Length != 3)
        {
            throw new McpException("Currency must be a three letter ISO 4217 code, for example EUR.");
        }

        var matches = await db.Employees
            .Where(e => e.Name.Contains(employeeName))
            .Select(e => new { e.Id, e.Name })
            .ToListAsync(cancellationToken);

        if (matches.Count == 0)
        {
            throw new McpException($"No employee matches '{employeeName}'. Ask the user for the full name.");
        }

        if (matches.Count > 1)
        {
            var names = string.Join(", ", matches.Select(m => m.Name));
            throw new McpException($"'{employeeName}' matches more than one employee: {names}. Ask the user which one they mean.");
        }

        var report = new ExpenseReport
        {
            EmployeeId = matches[0].Id,
            Title = title.Trim(),
            Currency = currency.Trim().ToUpperInvariant(),
            Status = ExpenseReportStatus.Draft,
            SubmittedOn = null,
            TotalAmount = lines.Sum(line => line.Amount),
            Lines = [.. lines.Select(line => new ExpenseLine
            {
                Description = line.Description.Trim(),
                Category = line.Category.Trim(),
                Amount = line.Amount,
                IncurredOn = line.IncurredOn
            })]
        };

        db.Reports.Add(report);
        await db.SaveChangesAsync(cancellationToken);

        return await db.ReportDetail(report.Id).FirstAsync(cancellationToken);
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
