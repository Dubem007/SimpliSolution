namespace Common.Services.Caching;

public class CacheKey
{
    public static string GetTokenKey(Guid userId)
    {
        return $"UserToken:User_{userId}";
    }

    public static string GetAppKey(string key, string appName)
    {
        return $"{appName}:{key}";
    }
}
