namespace BitOfTech.Expenses.Mcp.Contracts;

public record CurrencyConversion(
    decimal Amount,
    string From,
    string To,
    decimal Rate,
    /// <summary>The date the rate actually applies to, which may differ from the date asked for.</summary>
    DateOnly RateDate,
    decimal ConvertedAmount);
