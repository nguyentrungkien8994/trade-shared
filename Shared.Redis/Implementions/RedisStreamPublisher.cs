using Shared.Redis;
using StackExchange.Redis;

namespace Shared.Redis.Implementions;

public class RedisStreamPublisher : IRedisStreamPublisher
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IRedisSerializer _serializer;

    public RedisStreamPublisher(
        IConnectionMultiplexer connection,
        IRedisSerializer serializer)
    {
        _connection = connection;
        _serializer = serializer;
    }

    private IDatabase Db => _connection.GetDatabase();

    public async Task<string> AddAsync<T>(
        string stream,
        T message,
        int? maxLen = null)
    {
        var payload = _serializer.Serialize(message);

        var entries = new NameValueEntry[]
        {
            new("data", payload)
        };

        if (maxLen.HasValue)
        {
            return await Db.StreamAddAsync(
                stream,
                entries,
                maxLength: maxLen,
                useApproximateMaxLength: true);
        }

        return await Db.StreamAddAsync(stream, entries);
    }
}
