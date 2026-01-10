using Confluent.Kafka;

namespace TRADE.SHARED.SERVICE
{
    public class KafkaOptions
    {
        public string BootstrapServers { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;

        public AutoOffsetReset AutoOffsetReset { get; set; } = AutoOffsetReset.Earliest;
    }
}
