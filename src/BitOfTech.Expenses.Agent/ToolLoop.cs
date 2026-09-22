using Microsoft.Extensions.AI;

namespace BitOfTech.Expenses.Agent;

// The tool calling loop, written out by hand so every step is visible.
// The numbered comments match the four steps described in the post.
public sealed class ToolLoop(IChatClient chatClient, IReadOnlyList<AITool> tools)
{
    // The system prompt goes in once. Everything after it is appended as the conversation grows.
    private readonly List<ChatMessage> _messages =
        [new ChatMessage(ChatRole.System, AgentInstructions.System)];

    // The MCP tools go in as they arrived, because McpClientTool is already an AITool.
    private readonly ChatOptions _options = new() { Tools = [.. tools] };

    public async Task RunTurnAsync(string userInput, CancellationToken cancellationToken = default)
    {
        _messages.Add(new ChatMessage(ChatRole.User, userInput));

        // One question can take several trips to the model, so keep going until it stops
        // asking for tools.
        while (true)
        {
            // Step 1. Send the whole conversation and the tool list.
            var response = await chatClient.GetResponseAsync(_messages, _options, cancellationToken);
            _messages.AddMessages(response);

            // Step 2. Read the reply. One reply can ask for several tools at once.
            var calls = response.Messages
                .SelectMany(message => message.Contents)
                .OfType<FunctionCallContent>()
                .ToList();

            // No tool call means the model is answering, which is the only way out of the loop.
            if (calls.Count == 0)
            {
                Console.WriteLine(response.Text);
                return;
            }

            var results = new List<AIContent>();

            foreach (var call in calls)
            {
                Console.WriteLine($"  -> {call.Name}({FormatArguments(call.Arguments)})");

                var tool = tools.OfType<AIFunction>().FirstOrDefault(t => t.Name == call.Name);

                // A model can ask for a tool that isn't there. Tell it so instead of throwing,
                // and it gets a chance to correct itself on the next trip.
                if (tool is null)
                {
                    results.Add(new FunctionResultContent(call.CallId, $"No tool named {call.Name}."));
                    continue;
                }

                // Step 3. Run the tool. This goes over HTTP to the MCP server.
                var arguments = new AIFunctionArguments(call.Arguments);
                var result = await tool.InvokeAsync(arguments, cancellationToken);

                Console.WriteLine($"  <- {Truncate(result?.ToString())}");

                // CallId is what ties this result back to the request the model made.
                results.Add(new FunctionResultContent(call.CallId, result));
            }

            // Step 4. Put the results into the conversation and go round again.
            _messages.Add(new ChatMessage(ChatRole.Tool, results));
        }
    }

    private static string FormatArguments(IDictionary<string, object?>? arguments) =>
        arguments is null or { Count: 0 }
            ? string.Empty
            : string.Join(", ", arguments.Select(a => $"{a.Key}: {a.Value}"));

    private static string Truncate(string? text, int max = 160) =>
        text is null ? "null" : text.Length <= max ? text : text[..max] + " ...";
}
