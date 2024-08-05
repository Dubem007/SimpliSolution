using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Common.Services.Caching;

public static class Extensions
{
    public static void AddCachingService(this WebApplicationBuilder builder)
    {
        var redisConfig = Environment.GetEnvironmentVariable(builder.Configuration.GetConnectionString("RedisCon") ?? string.Empty, EnvironmentVariableTarget.Process)
            ?? builder.Configuration.GetValue<string>("ConnectionStrings:RedisCon");

        builder.Services.AddSingleton<ICacheService>((svcProvider) =>
        {
            var cxnMultiplexer = ConnectionMultiplexer.Connect(redisConfig);
            return new CacheService(cxnMultiplexer);
        });

        builder.Services.AddStackExchangeRedisCache((options) =>
        {
            options.Configuration = redisConfig;
        });
    }
}
