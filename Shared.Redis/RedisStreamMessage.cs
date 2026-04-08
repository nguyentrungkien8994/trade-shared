

namespace Shared.Redis;

public class RedisStreamMessage<T>
{
    public string Id { get; set; } = default!;
    public T Data { get; set; } = default!;
}
