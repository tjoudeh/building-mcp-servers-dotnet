using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using BitOfTech.Expenses.Agent;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

// Models reply in Markdown with characters the default Windows console cannot render.
Console.OutputEncoding = System.Text.Encoding.UTF8;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

var mcpEndpoint = configuration["Mcp:Endpoint"]!;
var foundryEndpoint = configuration["Foundry:Endpoint"]!;
var deploymentName = configuration["Foundry:Deployment"]!;

// The SDK can run the whole OAuth flow itself, but not against Entra: Entra's v2.0 metadata
// omits code_challenge_methods_supported, and an MCP client must refuse a server that does not
// advertise PKCE. So we sign in ourselves and hand the transport a bearer token.
var credential = new InteractiveBrowserCredential(new InteractiveBrowserCredentialOptions
{
    TenantId = configuration["Entra:TenantId"],
    ClientId = configuration["Entra:ClientId"],
    RedirectUri = new Uri(configuration["Entra:RedirectUri"]!)
});

var accessToken = await credential.GetTokenAsync(
    new TokenRequestContext([configuration["Entra:Scope"]!]));

await using var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri(mcpEndpoint),
    Name = "bitoftech-expenses",
    AdditionalHeaders = new Dictionary<string, string>
    {
        ["Authorization"] = $"Bearer {accessToken.Token}"
    }
});

await using var mcpClient = await McpClient.CreateAsync(transport);

var tools = await mcpClient.ListToolsAsync();

Console.WriteLine($"Connected to {mcpClient.ServerInfo?.Name} at {mcpEndpoint}");
Console.WriteLine($"{tools.Count} tools: {string.Join(", ", tools.Select(t => t.Name))}");

IChatClient chatClient = new AzureOpenAIClient(
     new Uri(foundryEndpoint), 
     new DefaultAzureCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient();

var useAgentFramework = args.Contains("--agent");

Console.WriteLine(useAgentFramework
    ? "Mode: Microsoft.Agents.AI"
    : "Mode: hand written tool loop");
Console.WriteLine("Type a question, or an empty line to quit.");
Console.WriteLine();

if (useAgentFramework)
{
    // Tools that change something. Everything else runs without asking.
    string[] needsApproval =
    [
        "create_expense_report",
        "submit_expense_report",
        "approve_expense_report",
        "notify_employee"
    ];

    List<AITool> agentTools =
    [
        .. tools.Select(tool => needsApproval.Contains(tool.Name)
            ? new ApprovalRequiredAIFunction(tool)
            : (AITool)tool)
    ];

    AIAgent agent = chatClient
        .AsAIAgent(
            instructions: AgentInstructions.System,
            name: "expenses",
            tools: agentTools)
        .AsBuilder()
        .UseToolApproval(new ToolApprovalAgentOptions())
        .Build();

    var session = await agent.CreateSessionAsync();

    while (ReadInput() is { } input)
    {
        var response = await agent.RunAsync(new ChatMessage(ChatRole.User, input), session);

        // Answering is itself a turn, and one answer can surface the next request, so this
        // has to loop rather than check once.
        while (PendingApprovals(response) is { Count: > 0 } requests)
        {
            response = await agent.RunAsync(
                new ChatMessage(ChatRole.User, AnswerApprovals(requests)), session);
        }

        Console.WriteLine(response.Text);
        Console.WriteLine();
    }
}
else
{
    var loop = new ToolLoop(chatClient, [.. tools]);

    while (ReadInput() is { } input)
    {
        await loop.RunTurnAsync(input);
        Console.WriteLine();
    }
}

static string? ReadInput()
{
    Console.Write("> ");
    var line = Console.ReadLine();
    return string.IsNullOrWhiteSpace(line) ? null : line;
}

static List<ToolApprovalRequestContent> PendingApprovals(AgentResponse response) =>
    [.. response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>()];

static List<AIContent> AnswerApprovals(List<ToolApprovalRequestContent> requests)
{
    List<AIContent> answers = [];

    foreach (var request in requests)
    {
        var call = request.ToolCall as FunctionCallContent;

        Console.WriteLine($"  {call?.Name} wants to run");

        if (call?.Arguments is { } arguments)
        {
            foreach (var (name, value) in arguments)
            {
                Console.WriteLine($"    {name}: {value}");
            }
        }

        Console.Write("  allow it? (y)es, (n)o, (a)lways for this tool ");

        answers.Add(Console.ReadLine()?.Trim().ToLowerInvariant() switch
        {
            // Records a standing rule on the session, so the same tool stops asking.
            "a" => request.CreateAlwaysApproveToolResponse(),
            "y" => request.CreateResponse(approved: true),
            _ => request.CreateResponse(approved: false, "the user declined")
        });
    }

    return answers;
}
