using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using BitOfTech.Expenses.Mcp.Data;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BitOfTech.Expenses.Mcp.Prompts;

[McpServerPromptType]
public class ExpenseReviewPrompt(ExpenseStore store)
{
    private static readonly Lazy<string> PolicyText = new(() =>
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Resources", "policy.md")));

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    [McpServerPrompt(Name = "review_expense_report")]
    [Description("Reviews one expense report against the BitOfTech expense policy and lists anything that breaks the rules.")]
    public IEnumerable<PromptMessage> ReviewExpenseReport(
        [Description("The expense report id to review, for example 1003.")] int reportId)
    {
        var report = store.Reports.FirstOrDefault(r => r.Id == reportId)
            ?? throw new McpException($"No expense report found with id {reportId}.");

        return
        [
            new PromptMessage
            {
                Role = Role.User,
                Content = new TextContentBlock
                {
                    Text = """
                        You are reviewing an expense report for BitOfTech.

                        Work through the report line by line against the policy below. For every
                        breach, quote the rule number, the line it applies to and the amount. If a
                        line is fine, do not mention it. Finish with a single recommendation of
                        approve, reject, or return for more information.

                        Amounts are in the currency shown on the report. Where a limit is stated in
                        GBP and the report is in another currency, say that the conversion needs
                        checking rather than guessing a rate.
                        """
                }
            },
            new PromptMessage
            {
                Role = Role.User,
                Content = new EmbeddedResourceBlock
                {
                    Resource = new TextResourceContents
                    {
                        Uri = "expenses://policy",
                        MimeType = "text/markdown",
                        Text = PolicyText.Value
                    }
                }
            },
            new PromptMessage
            {
                Role = Role.User,
                Content = new EmbeddedResourceBlock
                {
                    Resource = new TextResourceContents
                    {
                        Uri = $"expenses://reports/{reportId}",
                        MimeType = "application/json",
                        Text = JsonSerializer.Serialize(report, JsonOptions)
                    }
                }
            }
        ];
    }
}
