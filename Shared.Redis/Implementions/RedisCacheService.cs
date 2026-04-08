using Shared.Redis;
using StackExchange.Redis;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _db;
    private readonly IRedisSerializer _serializer;
    private readonly string _prefix;

    public RedisCacheService(
        IConnectionMultiplexer connection,
        IRedisSerializer serializer,
        RedisOptions options)
    {
        _db = connection.GetDatabase();
        _serializer = serializer;
        _prefix = options.InstanceName;
    }

    private string BuildKey(string key) => $"{_prefix}{key}";

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(BuildKey(key));
        if (value.IsNullOrEmpty) return default;

        return _serializer.Deserialize<T>(value!);
    }

    public async Task<bool> SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null)
    {
        var data = _serializer.Serialize(value);
        return await _db.StringSetAsync(BuildKey(key), data, expiry, false);
    }

    public async Task<bool> RemoveAsync(string key)
        => await _db.KeyDeleteAsync(BuildKey(key));

    public async Task<bool> ExistsAsync(string key)
        => await _db.KeyExistsAsync(BuildKey(key));
}