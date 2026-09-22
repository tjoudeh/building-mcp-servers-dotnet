using Azure.AI.OpenAI;
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

await using var transport = new HttpClientTransport(new HttpClientTransportOptions
{
    Endpoint = new Uri(mcpEndpoint),
    Name = "bitoftech-expenses"
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
    AIAgent agent = chatClient.AsAIAgent(
        instructions: AgentInstructions.System,
        name: "expenses",
        tools: [.. tools]);

    var session = await agent.CreateSessionAsync();

    while (ReadInput() is { } input)
    {
        var response = await agent.RunAsync(new ChatMessage(ChatRole.User, input), session);
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
