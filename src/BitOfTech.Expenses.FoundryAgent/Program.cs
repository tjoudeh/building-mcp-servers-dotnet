using System.ClientModel;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Responses;

// Models reply in Markdown with characters the default Windows console cannot render.
Console.OutputEncoding = System.Text.Encoding.UTF8;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var projectEndpoint = configuration["Foundry:ProjectEndpoint"]!;
var agentName = configuration["Foundry:Agent"]!;

// There is no MCP client here. The agent in Azure owns the tool loop, so this application
// only ever sends a sentence and reads the reply.
AIProjectClient projectClient = new(
    endpoint: new Uri(projectEndpoint),
    tokenProvider: new DefaultAzureCredential());

ProjectResponsesClient responsesClient =
    projectClient.ProjectOpenAIClient.GetProjectResponsesClientForAgent(agentName);

Console.WriteLine($"Connected to {agentName} at {projectEndpoint}");
Console.WriteLine("The tool loop runs in Azure. Type a question, or an empty line to quit.");
Console.WriteLine();

string? previousResponseId = null;

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input))
    {
        break;
    }

    CreateResponseOptions options = new();
    options.InputItems.Add(ResponseItem.CreateUserMessageItem(input));
    if (previousResponseId is not null)
    {
        options.PreviousResponseId = previousResponseId;
    }

    try
    {
        ResponseResult response = await responsesClient.CreateResponseAsync(options);
        previousResponseId = response.Id;

        // The agent pauses instead of finishing when it needs something from us. Each pause
        // arrives as an output item, and we answer it by continuing from this response.
        while (true)
        {
            var follow = HandlePauses(response);
            if (follow is null)
            {
                break;
            }

            follow.PreviousResponseId = response.Id;
            response = await responsesClient.CreateResponseAsync(follow);
            previousResponseId = response.Id;
        }

        Console.WriteLine(response.GetOutputText());
    }
    catch (ClientResultException failure)
    {
        // A tool that refuses fails the whole request here, rather than returning text the
        // model can explain. Our own agent in Part 6 sees the same refusal as content.
        Console.WriteLine(Readable(failure));
    }

    Console.WriteLine();
}

// Returns options to continue the conversation, or null when the agent is not waiting on us.
static CreateResponseOptions? HandlePauses(ResponseResult response)
{
    CreateResponseOptions? follow = null;

    foreach (ResponseItem item in response.OutputItems)
    {
        if (item is McpToolCallApprovalRequestItem approval)
        {
            Console.WriteLine($"  {approval.ServerLabel} wants to call {approval.ToolName}");
            Console.Write("  allow it? (y/N) ");
            var answer = Console.ReadLine();
            var approved = string.Equals(answer, "y", StringComparison.OrdinalIgnoreCase);
            follow ??= new CreateResponseOptions();
            follow.InputItems.Add(
                ResponseItem.CreateMcpApprovalResponseItem(approval.Id, approved));
        }
        // Consent is a Foundry concept rather than an OpenAI one, so it arrives wrapped.
        else if (item.AsAgentResponseItem() is OAuthConsentRequestResponseItem consent)
        {
            Console.WriteLine($"  sign in to continue: {consent.ConsentLink}");
            Console.Write("  press Enter once you have finished signing in ");
            Console.ReadLine();
            follow ??= new CreateResponseOptions();
        }
    }

    return follow;
}

// Pulls the tool's own message out of the HTTP failure and drops the wrapping.
static string Readable(ClientResultException failure)
{
    var text = failure.Message;

    var guide = text.IndexOf("Troubleshooting guide:", StringComparison.Ordinal);
    if (guide > 0)
    {
        text = text[..guide];
    }

    // An HTTP status line, then a blank line, then what the tool actually said.
    var parts = text.Split(["\r\n\r\n", "\n\n"], 2, StringSplitOptions.None);
    return parts[^1].Trim();
}
