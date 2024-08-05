namespace Common.Services.Caching;

public interface ICacheService
{
    /// <summary>
    /// Gets data from the cache store as type of T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<T> GetData<T>(string key);

    /// <summary>
    /// Persists data inthe cache store
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value">Value of object to be stored in cache</param>
    /// <param name="ttl">Time to Live in seconds</param>
    /// <returns></returns>
    Task<bool> SetData<T>(string key, T value, int ttl = 300);

    /// <summary>
    /// Remove data from the cache store
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<bool> RemoveData(string key);
}
