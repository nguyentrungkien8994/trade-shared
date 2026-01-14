namespace Shared.Kafka;

public interface IKafkaConsumer
{
    Task ConsumeAsync(
    string topic,
    Func<string, string, Task> handler,
    CancellationToken cancellationToken=default);
}
