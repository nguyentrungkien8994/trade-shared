namespace Shared.OpenAI;

public sealed class OpenAiParserOptions
{
    public string ApiKey { get; init; } = default!;
    public string Model { get; init; } = "gpt-4.1-mini";
    public double Temperature { get; init; } = 0.0;
    public int MaxTokens { get; init; } = 256;
}
