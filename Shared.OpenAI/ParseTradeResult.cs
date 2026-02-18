namespace Shared.OpenAI;

public sealed class ParseTradeResult
{
    public bool IsSuccess { get; init; }
    public TradeCommand? Command { get; init; }
    public string? Error { get; init; }

    public static ParseTradeResult Success(TradeCommand cmd)
        => new() { IsSuccess = true, Command = cmd };

    public static ParseTradeResult Fail(string error)
        => new() { IsSuccess = false, Error = error };
}
