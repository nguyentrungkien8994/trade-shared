
namespace Shared.Redis;

public interface IRedisStreamService
{
    Task<string> AddAsync<T>(
        string stream,
        T message,
        int? maxLen = null);

    Task CreateConsumerGroupAsync(
        string stream,
        string group);

    Task<IReadOnlyList<RedisStreamMessage<T>>> ReadGroupAsync<T>(
        string stream,
        string group,
        string consumer,
        int count = 10,
        int blockMs = 5000);

    Task AckAsync(
        string stream,
        string group,
        params string[] messageIds);

    Task<IReadOnlyList<RedisStreamMessage<T>>> ReadPendingAsync<T>(
        string stream,
        string group,
        string consumer,
        int count = 10);

    Task ClaimAsync(
        string stream,
        string group,
        string consumer,
        long minIdleTime,
        params string[] messageIds);
}