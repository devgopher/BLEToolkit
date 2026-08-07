namespace BLE.Toolkit.Cache;

/// <summary>
///     Remove expired items
/// </summary>
/// <param name="cache"></param>
public class RemoveExpiredPopStrategy<T>(HashSet<Entry<T>> cache) : CachePopStrategy<T>(cache)
    where T : notnull
{
    public override void Do()
    {
        var expired = cache.Where(c => c != null! && c.ExpireAtUtc >= DateTime.UtcNow);

        foreach (var entry in expired)
            cache.Remove(entry);
    }
}