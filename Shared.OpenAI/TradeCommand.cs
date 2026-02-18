namespace Shared.OpenAI;

public sealed class TradeCommand
{
    public string Symbol { get; init; } = default!;
    public string Side { get; init; } = default!;     // BUY / SELL
    public decimal? Entry { get; init; }
    public EntryRange? EntryRange { get; init; }
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit { get; init; }
    public string? Market { get; init; }
}
public sealed class EntryRange
{
    public decimal Min { get; init; }
    public decimal Max { get; init; }
}