
using StackExchange.Redis;
namespace Shared.Redis;
public class RedisStreamService : IRedisStreamService
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IRedisSerializer _serializer;

    public RedisStreamService(
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

    public async Task CreateConsumerGroupAsync(
        string stream,
        string group)
    {
        try
        {
            await Db.StreamCreateConsumerGroupAsync(
                stream,
                group,
                "$",
                createStream: true);
        }
        catch (RedisServerException ex)
            when (ex.Message.Contains("BUSYGROUP"))
        {
            // group đã tồn tại → ignore
        }
    }

    public async Task<IReadOnlyList<RedisStreamMessage<T>>> ReadGroupAsync<T>(
        string stream,
        string group,
        string consumer,
        int count = 10,
        int blockMs = 5000)
    {
        var entries = await Db.StreamReadGroupAsync(
            stream,
            group,
            consumer,
            ">",
            count);

        return entries.Select(Map<T>).ToList();
    }

    public async Task AckAsync(
        string stream,
        string group,
        params string[] messageIds)
    {
        if (messageIds.Length == 0) return;

        await Db.StreamAcknowledgeAsync(
            stream,
            group,
            messageIds.Select(id => (RedisValue)id).ToArray());
    }

    public async Task<IReadOnlyList<RedisStreamMessage<T>>> ReadPendingAsync<T>(
        string stream,
        string group,
        string consumer,
        int count = 10)
    {
        var entries = await Db.StreamReadGroupAsync(
            stream,
            group,
            consumer,
            "0",
            count);

        return entries.Select(Map<T>).ToList();
    }

    public async Task ClaimAsync(
        string stream,
        string group,
        string consumer,
        long minIdleTime,
        params string[] messageIds)
    {
        if (messageIds.Length == 0) return;

        await Db.StreamClaimAsync(
            stream,
            group,
            consumer,
            minIdleTime,

            messageIds.Select(id => (RedisValue)id).ToArray());
    }

    private RedisStreamMessage<T> Map<T>(StreamEntry entry)
    {
        var value = entry.Values.FirstOrDefault(v => v.Name == "data").Value;

        return new RedisStreamMessage<T>
        {
            Id = entry.Id,
            Data = _serializer.Deserialize<T>(value!)!
        };
    }
}
