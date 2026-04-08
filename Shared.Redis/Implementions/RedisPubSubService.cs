using Shared.Redis;
using StackExchange.Redis;

public class RedisPubSubService : IRedisPubSubService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IRedisSerializer _serializer;

    public RedisPubSubService(
        IConnectionMultiplexer connection,
        IRedisSerializer serializer)
    {
        _connection = connection;
        _serializer = serializer;
    }

    public async Task PublishAsync<T>(string channel, T message)
    {
        var sub = _connection.GetSubscriber();
        var payload = _serializer.Serialize(message);

        await sub.PublishAsync(channel, payload);
    }

    public async Task SubscribeAsync<T>(
        string channel,
        Func<T, Task> handler)
    {
        var sub = _connection.GetSubscriber();

        await sub.SubscribeAsync(channel, async (_, value) =>
        {
            var msg = _serializer.Deserialize<T>(value!);
            if (msg != null)
                await handler(msg);
        });
    }
}