using Microsoft.Extensions.DependencyInjection;
using Shared.Redis;
using Shared.Redis.Implementions;
using StackExchange.Redis;

public static class Extensions
{
    public static IServiceCollection AddRedis(
    this IServiceCollection services,
    Action<RedisOptions> configure)
    {
        var options = new RedisOptions();
        configure(options);

        services.AddSingleton(options);

        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(options.ConnectionString));

        services.AddSingleton<IRedisSerializer, SystemTextJsonSerializer>();

        services.AddSingleton<IRedisCacheService, RedisCacheService>();
        services.AddSingleton<IRedisPubSubService, RedisPubSubService>();

        // 🔥 thêm stream
        services.AddSingleton<IRedisStreamService, RedisStreamService>();
        services.AddSingleton<IRedisStreamPublisher, RedisStreamPublisher>();

        return services;
    }
}