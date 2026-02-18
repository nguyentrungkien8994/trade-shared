namespace Shared.OpenAI;

public interface ITradeCommandParser
{
    Task<ParseTradeResult> ParseAsync(
        string rawMessage,
        CancellationToken cancellationToken = default);
    Task<ParseTradeResult> ParseImageAsync(
        string base64Image,
        CancellationToken cancellationToken = default);
}
