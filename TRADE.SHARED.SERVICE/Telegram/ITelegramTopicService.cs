namespace TRADE.SHARED.SERVICE
{
    public interface ITelegramTopicService
    {
        Task<CreateResult?> CreateTopicAsync(string topicName, CancellationToken ct = default);
        Task SendMessageAsync(int topicId, string message, CancellationToken ct = default);
    }
}
