using Newtonsoft.Json;
using StackExchange.Redis;

namespace Common.Services.Caching;

public class CacheService : ICacheService
{
    private readonly IDatabase _db;
    public CacheService(IConnectionMultiplexer connectionMultiplxer)
    {
        _db = connectionMultiplxer.GetDatabase();
    }

    public async Task<T> GetData<T>(string key)
    {
        RedisValue result = new();
        try
        {
            result = await _db.StringGetAsync($"{key}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return result.HasValue ? JsonConvert.DeserializeObject<T>(result) : default;
    }

    public async Task<bool> RemoveData(string key)
    {
        bool _isKeyExist = _db.KeyExists($"{key}");
        if (_isKeyExist == true)
        {
            return await _db.KeyDeleteAsync($"{key}");
        }
        return false;
    }

    public async Task<bool> SetData<T>(string key, T value, int ttl)
    {
        TimeSpan expiryTime = TimeSpan.FromSeconds(ttl); // ttl.DateTime.Subtract(DateTime.UtcNow);
        bool isSet = false;
        try
        {
            isSet = await _db.StringSetAsync($"{key}", JsonConvert.SerializeObject(value), expiryTime);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return isSet;
    } 
}
