using Azure;
using Azure.Communication.Email;

namespace BitOfTech.Expenses.Mcp.Services;

public sealed class EmailSender(IConfiguration configuration)
{
    private readonly EmailClient _client = new(configuration["Email:ConnectionString"]!);
    private readonly string _sender = configuration["Email:SenderAddress"]!;

    public async Task SendAsync(
        string recipient,
        string subject,
        string html,
        CancellationToken cancellationToken)
    {
        // Completed waits for the service to accept the message, which is not the same as
        // delivery. Started returns an operation id to poll instead, for anything sending in bulk.
        await _client.SendAsync(
            WaitUntil.Completed,
            _sender,
            recipient,
            subject,
            html,
            cancellationToken: cancellationToken);
    }
}
