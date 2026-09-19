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
    [Description("Searches expense reports by employee name and status. Returns every matching report with its line items.")]
    public IReadOnlyList<ExpenseReport> SearchExpenseReports(
        [Description("Part of an employee name, matched case insensitively. Omit to search across all employees.")]
        string? employeeName = null,
        [Description("Only return reports in this status. Omit to return reports in any status.")]
        ExpenseReportStatus? status = null)
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

        if (status is not null)
        {
            results = results.Where(r => r.Status == status);
        }

        return results.OrderByDescending(r => r.SubmittedOn).ToList();
    }

    [McpServerTool(Name = "get_expense_report")]
    [Description("Gets a single expense report with its line items, by report id.")]
    public ExpenseReport GetExpenseReport(
        [Description("The expense report id, for example 1001.")] int reportId)
    {
        // McpException reaches the model. Other exception types are replaced with a
        // generic message, so the model never learns what it did wrong.
        return store.Reports.FirstOrDefault(r => r.Id == reportId)
            ?? throw new McpException($"No expense report found with id {reportId}.");
    }
}
