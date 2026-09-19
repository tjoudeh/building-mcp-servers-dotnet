using System.ComponentModel;
using BitOfTech.Expenses.Mcp.Data;
using BitOfTech.Expenses.Mcp.Models;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BitOfTech.Expenses.Mcp.Tools;

[McpServerToolType]
public class ExpenseTools(ExpenseStore store)
{
    [McpServerTool(Name = "search_expense_reports")]
    [Description("Searches expense reports by employee and by where they are in the approval workflow. Returns the matching reports with their line items.")]
    public IReadOnlyList<ExpenseReport> SearchExpenseReports(
        [Description("Part of an employee name, matched case insensitively. Omit to search across all employees.")]
        string? employeeName = null,
        [Description("Where the report sits in the approval workflow right now. Only use this when the question is about the current state. A report that was approved or rejected was also submitted at some point, so do not filter on Submitted to answer questions about what somebody has submitted.")]
        ExpenseReportStatus? currentStatus = null)
    {
        IEnumerable<ExpenseReport> results = store.Reports;

        if (!string.IsNullOrWhiteSpace(employeeName))
        {
            var matchingIds = store.Employees
                .Where(e => e.Name.Contains(employeeName, StringComparison.OrdinalIgnoreCase))
                .Select(e => e.Id)
                .ToHashSet();

            results = results.Where(r => matchingIds.Contains(r.EmployeeId));
        }

        if (currentStatus is not null)
        {
            results = results.Where(r => r.Status == currentStatus);
        }

        return results.OrderByDescending(r => r.SubmittedOn).ToList();
    }

    [McpServerTool(Name = "get_expense_report")]
    [Description("Gets one expense report with its line items, by id. Call search_expense_reports first if you do not already have an id.")]
    public ExpenseReport GetExpenseReport(
        [Description("The expense report id, for example 1001.")] int reportId)
    {
        // McpException reaches the model. Other exception types are replaced with a
        // generic message, so the model never learns what it did wrong.
        return store.Reports.FirstOrDefault(r => r.Id == reportId)
            ?? throw new McpException(
                $"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");
    }
}
