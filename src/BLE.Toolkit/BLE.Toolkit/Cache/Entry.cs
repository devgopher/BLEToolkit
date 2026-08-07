namespace BLE.Toolkit.Cache;

public sealed class Entry<T>(T item, DateTime expireAtUtc)
    where T : notnull
{
    public DateTime ExpireAtUtc = expireAtUtc;
    public T Item = item;
    public CachedProxy<T> Proxy { get; } = new(item);
}
