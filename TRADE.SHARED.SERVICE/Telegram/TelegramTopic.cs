namespace TRADE.SHARED.SERVICE
{
    public sealed class TelegramTopic
    {
        public long GroupId { get; set; }
        public int ThreadId { get; set; }           // message_thread_id
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
