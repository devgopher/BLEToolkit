namespace BLE.Toolkit.Cache;

/// <summary>
///     Removes firstly added and outdated elements from an array
/// </summary>
/// <param name="cache"></param>
/// <param name="size"></param>
/// <typeparam name="T"></typeparam>
public class FixedSizeCachePopStrategy<T>(HashSet<Entry<T>> cache, int size, bool removeJustOne = false, bool sortByData = false)
    : RemoveExpiredPopStrategy<T>(cache)
    where T : notnull
{
    public override void Do()
    {
        ArgumentOutOfRangeException.ThrowIfNegative(size);

        base.Do();

        if (!removeJustOne)
        {
            var toRemove = sortByData ? cache.OrderByDescending(c => c.ExpireAtUtc).Take(cache.Count - size) : cache.Take(cache.Count - size);

            foreach (var entry in toRemove)
            {
                cache.Remove(entry);
            }
        }
        else if (cache.Count >= size)
        {
            var toRemove = sortByData ? cache.OrderByDescending(c => c.ExpireAtUtc).LastOrDefault() : cache.Take(cache.Count - size).LastOrDefault();
            
            if (toRemove != null)
                cache.Remove(toRemove);

        }
    }
}