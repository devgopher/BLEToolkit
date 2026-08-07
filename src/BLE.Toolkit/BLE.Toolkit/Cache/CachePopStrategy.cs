namespace BLE.Toolkit.Cache;

/// <summary>
///     Cache cleaning strategy
/// </summary>
/// <param name="cache"></param>
public abstract class CachePopStrategy<T>(HashSet<Entry<T>> cache)
    where T : notnull
{
    public abstract void Do();
}