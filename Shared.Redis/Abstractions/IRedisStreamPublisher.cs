namespace Shared.Redis;

public interface IRedisStreamPublisher
{
    Task<string> AddAsync<T>(
    string stream,
    T message,
    int? maxLen = null);
}
