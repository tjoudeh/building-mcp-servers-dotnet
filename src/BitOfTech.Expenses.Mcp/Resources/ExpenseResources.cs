using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using BitOfTech.Expenses.Mcp.Data;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BitOfTech.Expenses.Mcp.Resources;

[McpServerResourceType]
public class ExpenseResources(ExpenseStore store)
{
    private static readonly Lazy<string> PolicyText = new(() =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", "policy.md")));

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [McpServerResource(UriTemplate = "expenses://policy", Name = "Expense policy", MimeType = "text/markdown")]
    [Description("The BitOfTech expense policy. Attach this when you need to judge whether a report follows company rules.")]
    public static string GetExpensePolicy() => PolicyText.Value;

    [McpServerResource(UriTemplate = "expenses://reports/{reportId}", Name = "Expense report", MimeType = "application/json")]
    [Description("A single expense report as JSON, addressed by its id. Attach this when you already know which report you want to talk about.")]
    public TextResourceContents GetExpenseReport(int reportId)
    {
        var report = store.Reports.FirstOrDefault(r => r.Id == reportId)
            ?? throw new McpException($"No expense report found with id {reportId}.");

        return new TextResourceContents
        {
            Uri = $"expenses://reports/{reportId}",
            MimeType = "application/json",
            Text = JsonSerializer.Serialize(report, JsonOptions)
        };
    }
}
