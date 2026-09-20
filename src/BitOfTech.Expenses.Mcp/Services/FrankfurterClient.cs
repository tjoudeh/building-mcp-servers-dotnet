using System.Net.Http.Json;
using ModelContextProtocol;

namespace BitOfTech.Expenses.Mcp.Services;

public class FrankfurterClient(HttpClient http)
{
    public async Task<FrankfurterRate> GetRateAsync(
        string from,
        string to,
        DateOnly? onDate,
        CancellationToken cancellationToken)
    {
        var path = $"v2/rate/{Uri.EscapeDataString(from.Trim().ToLowerInvariant())}/{Uri.EscapeDataString(to.Trim().ToLowerInvariant())}";

        if (onDate is not null)
        {
            path += $"?date={onDate:yyyy-MM-dd}";
        }

        using var response = await http.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Frankfurter returns a readable reason, for example "invalid currency: ZZZ".
            var error = await response.Content.ReadFromJsonAsync<FrankfurterError>(cancellationToken);

            throw new McpException(error?.Message is { Length: > 0 } message
                ? $"Exchange rate lookup failed: {message}"
                : $"Exchange rate lookup failed with status {(int)response.StatusCode}.");
        }

        return await response.Content.ReadFromJsonAsync<FrankfurterRate>(cancellationToken)
            ?? throw new McpException("The exchange rate service returned an empty response.");
    }
}

public record FrankfurterRate(DateOnly Date, string Base, string Quote, decimal Rate);

public record FrankfurterError(int Status, string Message);
