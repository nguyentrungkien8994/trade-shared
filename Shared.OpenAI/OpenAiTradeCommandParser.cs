using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Shared.OpenAI;

public sealed class OpenAiTradeCommandParser : ITradeCommandParser
{
    private readonly OpenAiParserOptions _options;
    private readonly ChatClient _client;
    private readonly OpenAIClient _openAIClient;

    public OpenAiTradeCommandParser(IOptions<OpenAiParserOptions> options)
    {
        _options = options.Value;

        _client = new ChatClient(
            model: _options.Model,
            apiKey: _options.ApiKey);
        _openAIClient = new OpenAIClient(_options.ApiKey);
    }

    public async Task<ParseTradeResult> ParseAsync(
        string rawMessage,
        CancellationToken cancellationToken = default)
    {
        var systemPrompt =
        """
        You are a trading signal parser.

        Extract ONE trade instruction from a SINGLE raw message.

        IMPORTANT PRINCIPLES:
        - Do NOT guess randomly.
        - Use PRICE RELATION rules to determine side when keywords are missing.
        - Prefer DEFAULT behaviors over returning null.

        --------------------------------------------------
        MARKET RULES (STRICT):

        - Market = SPOT ONLY IF the message explicitly contains:
            • the word "spot"
            • or an explicit spot marker such as "[S]", "(S)", "🟢S"

        - Otherwise:
            • market MUST be FUTURES
        --------------------------------------------------

        ENTRY RULES (MANDATORY):

        - If the message contains ANY of the following:
            • a price range written as "X-Y"
            • "between X-Y"
            • "from X to Y"
            • "from X/Y"
            • TWO numeric prices written next to each other (e.g. "0.077 0.075")

          THEN:
            • entryRange MUST be { "min": number, "max": number }
            • entry MUST be null

        - If the message contains ONLY ONE entry price:
            • entry MUST be that number
            • entryRange MUST be null
        --------------------------------------------------

        STOP LOSS / TAKE PROFIT RULES:

        - stopLoss is extracted from "sl" or "stop" if present.
        - takeProfit is extracted ONLY if explicitly mentioned.
        --------------------------------------------------

        SIDE DETERMINATION RULES (CRITICAL):

        1. If message explicitly contains:
            • "buy limit" or "limit buy"   → side = BUY_LIMIT
            • "sell limit" or "limit sell" → side = SELL_LIMIT
            • "buy" or "long"              → side = BUY
            • "sell" or "short"            → side = SELL

        2. If NO explicit side keywords exist, determine side USING PRICE RELATION:

           a) When entryRange exists:
              - If stopLoss < entryRange.min → side = BUY_LIMIT
              - If stopLoss > entryRange.max → side = SELL_LIMIT

           b) When entry (single price) exists:
              - If stopLoss < entry        → side = BUY
              - If stopLoss > entry        → side = SELL

           c) When takeProfit exists (and stopLoss is missing):
              - If takeProfit > entry or entryRange.max → side = BUY / BUY_LIMIT
              - If takeProfit < entry or entryRange.min → side = SELL / SELL_LIMIT

        3. If side still cannot be determined:
            • DEFAULT side = BUY_LIMIT
        --------------------------------------------------

        OUTPUT RULES:

        - Return ONLY valid JSON
        - NO explanation
        - NO extra fields
        --------------------------------------------------

        JSON SCHEMA:

        {
          "symbol": string,
          "market": "SPOT" | "FUTURES",
          "side": "BUY" | "SELL" | "BUY_LIMIT" | "SELL_LIMIT",
          "entry": number | null,
          "entryRange": { "min": number, "max": number } | null,
          "stopLoss": number | null,
          "takeProfit": number | null
        }
        """;
        var completion = await _client.CompleteChatAsync(
            new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(rawMessage)
            },
            new ChatCompletionOptions
            {
                Temperature = (float)_options.Temperature,
                MaxOutputTokenCount = _options.MaxTokens
            },
            cancellationToken);

        var content = completion.Value.Content[0].Text;

        if (string.IsNullOrWhiteSpace(content))
            return ParseTradeResult.Fail("Empty response from OpenAI");

        try
        {
            var command = JsonSerializer.Deserialize<TradeCommand>(
                content,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            if (command == null || string.IsNullOrWhiteSpace(command.Symbol))
                return ParseTradeResult.Fail("No trade command detected");

            return ParseTradeResult.Success(command);
        }
        catch (Exception ex)
        {
            return ParseTradeResult.Fail($"Parse error: {ex.Message}");
        }
    }

    public async Task<ParseTradeResult> ParseImageAsync(string base64Image, CancellationToken cancellationToken = default)
    {
        try
        {
            var systemPrompt =
       """
        You are a professional crypto trading assistant.

        Your task:
        - Analyze the trading signal shown in the image.
        - Extract trading information if present.

        OUTPUT RULES:

        - Return ONLY valid JSON
        - No markdown, no code fences, no commentary, no extra characters before/after.
        - NO explanation
        - NO extra fields
        --------------------------------------------------

        JSON SCHEMA:

        {
          "symbol": string,
          "market": "SPOT" | "FUTURES",
          "side": "BUY" | "SELL" | "BUY_LIMIT" | "SELL_LIMIT",
          "entry": number | null,
          "entryRange": { "min": number, "max": number } | null,
          "stopLoss": number | null,
          "takeProfit": number | null
        }
        """;
            //using var http = new HttpClient();
            //http.DefaultRequestHeaders.Authorization =
            //    new AuthenticationHeaderValue("Bearer",
            //        _options.ApiKey);

            //var body = new
            //{
            //    model = _options.Model,
            //    input = new[]
            //    {
            //        new
            //        {
            //            role = "user",
            //            content = new object[]
            //            {
            //                new { type = "input_text", text = systemPrompt },
            //                new { type = "input_image", image_base64 = base64Image }
            //            }
            //        }
            //    }
            //};
            //var res = await http.PostAsync(
            //    "https://api.openai.com/v1/responses",
            //    new StringContent(
            //        JsonSerializer.Serialize(body),
            //        Encoding.UTF8,
            //        "application/json")
            //);
            //string content = await res.Content.ReadAsStringAsync();    
            byte[] imageBytes = Convert.FromBase64String(base64Image);

            var completion = await _client.CompleteChatAsync(
            new ChatMessage[]
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(ChatMessageContentPart.CreateImagePart(
                    imageBytes: BinaryData.FromBytes(imageBytes),
                    imageBytesMediaType: "image/jpg"
                ))
            },
            new ChatCompletionOptions
            {
                Temperature = (float)_options.Temperature,
                MaxOutputTokenCount = _options.MaxTokens
            },
            cancellationToken);

            var content = completion.Value.Content[0].Text;

            if (string.IsNullOrWhiteSpace(content))
                return ParseTradeResult.Fail("Empty response from OpenAI");

            try
            {
                var command = JsonSerializer.Deserialize<TradeCommand>(
                    content,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (command == null || string.IsNullOrWhiteSpace(command.Symbol))
                    return ParseTradeResult.Fail("No trade command detected");

                return ParseTradeResult.Success(command);
            }
            catch (Exception ex)
            {
                return ParseTradeResult.Fail($"Parse error: {ex.Message}");
            }
        }
        catch (Exception)
        {

            throw;
        }
    }
}
