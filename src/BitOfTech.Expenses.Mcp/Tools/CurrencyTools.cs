using System.ComponentModel;
using BitOfTech.Expenses.Mcp.Contracts;
using BitOfTech.Expenses.Mcp.Services;
using ModelContextProtocol;
using ModelContextProtocol.Server;

namespace BitOfTech.Expenses.Mcp.Tools;

[McpServerToolType]
public class CurrencyTools(FrankfurterClient frankfurter)
{
    [McpServerTool(Name = "convert_currency")]
    [Description("Converts an amount between two currencies using published reference rates. Company policy converts at the rate for the date the expense was incurred, so pass that date rather than relying on today's rate.")]
    public async Task<CurrencyConversion> ConvertCurrencyAsync(
        [Description("The amount to convert, in the source currency.")]
        decimal amount,
        [Description("Three letter ISO 4217 code to convert from, for example EUR.")]
        string from,
        [Description("Three letter ISO 4217 code to convert to, for example GBP.")]
        string to,
        [Description("The date the expense was incurred, as yyyy-MM-dd. Omit only when the date is genuinely unknown, in which case the latest rate is used.")]
        DateOnly? onDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var rate = await frankfurter.GetRateAsync(from, to, onDate, cancellationToken);

            return new CurrencyConversion(
                amount,
                rate.Base,
                rate.Quote,
                rate.Rate,
                rate.Date,
                Math.Round(amount * rate.Rate, 2, MidpointRounding.AwayFromZero));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new McpException(
                "The exchange rate service did not respond in time. Tell the user the converted amount is unavailable rather than estimating a rate.");
        }
        catch (HttpRequestException ex)
        {
            throw new McpException(
                $"Could not reach the exchange rate service: {ex.Message}. Tell the user the converted amount is unavailable rather than estimating a rate.");
        }
    }
}
