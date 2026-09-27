using System.ComponentModel;
using BitOfTech.Expenses.Mcp.Data;
using BitOfTech.Expenses.Mcp.Models;
using BitOfTech.Expenses.Mcp.Services;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BitOfTech.Expenses.Mcp.Tools;

[McpServerToolType]
public class NotificationTools(ExpensesDbContext db, EmailSender emailSender)
{
    [McpServerTool(Name = "notify_employee")]
    [Description("Emails the employee who owns an expense report to tell them it has been approved. Only works on a report that is already Approved. The message text is fixed, so there is nothing to write and no wording to ask the user for.")]
    public async Task<string> NotifyEmployeeAsync(
        [Description("The id of the approved expense report.")] int reportId,
        CancellationToken cancellationToken = default)
    {
        var target = await (
            from report in db.Reports.AsNoTracking()
            join employee in db.Employees on report.EmployeeId equals employee.Id
            where report.Id == reportId
            select new { report.Title, report.Status, employee.Name, employee.Email })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new McpException(
                $"No expense report found with id {reportId}. Call search_expense_reports to find valid ids.");

        if (target.Status is not ExpenseReportStatus.Approved)
        {
            throw new McpException(
                $"Report {reportId} is {target.Status}. Only an approved report can be notified about.");
        }

        // The body is built here, not by the model. A tool that sends whatever text it is handed
        // is a tool that will eventually send something you did not write.
        await emailSender.SendAsync(
            target.Email,
            "Your expense report has been approved",
            $"<p>Hello {target.Name},</p><p>Your expense report <strong>{target.Title}</strong> has been approved.</p>",
            cancellationToken);

        return $"Emailed {target.Name} at {target.Email} about report {reportId}.";
    }
}
