using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Shared.Kafka;

public sealed class KafkaConsumer : IKafkaConsumer, IDisposable
{
    private readonly IConsumer<string, string> _consumer;

    public KafkaConsumer(IOptions<KafkaOptions> options)
    {
        var opt = options.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = opt.BootstrapServers,
            GroupId = opt.GroupId,
            AutoOffsetReset = opt.AutoOffsetReset,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    public async Task ConsumeAsync(
        string topic,
        Func<string, string, Task> handler,
        CancellationToken cancellationToken)
    {
        _consumer.Subscribe(topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(cancellationToken);

                await handler(result.Message.Key, result.Message.Value);

                _consumer.Commit(result);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Dispose()
    {
        _consumer.Close();
        _consumer.Dispose();
    }
}
