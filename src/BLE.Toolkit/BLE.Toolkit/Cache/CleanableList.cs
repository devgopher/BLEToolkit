using System.Collections;

namespace BLE.Toolkit.Cache;

public class CleanableList<T> : IEnumerable<CachedProxy<T>>
    where T : notnull
{
    private readonly Dictionary<T, Entry<T>> _items;
    private readonly Func<DateTime> _utcNow;
    private readonly CachePopStrategy<T>? _popStrategy;

    public CleanableList(Func<DateTime>? utcNow = null, IEqualityComparer<T>? comparer = null)
    {
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
        _items = new Dictionary<T, Entry<T>>(comparer);
    }

    public CleanableList(CachePopStrategy<T> popStrategy, Func<DateTime>? utcNow = null,
        IEqualityComparer<T>? comparer = null)
        : this(utcNow, comparer)
    {
        _popStrategy = popStrategy;
    }

    public bool Add(T item)
    {
        PurgeExpired();
        if (item is null) return false;

        if (_items.TryGetValue(item, out var existing))
        {
            existing.ExpireAtUtc = default;
            return false;
        }

        _items[item] = new Entry<T>(item, default);
        return true;
    }

    public bool Remove(T item)
    {
        if (item is null) return false;
        return _items.Remove(item);
    }

    public void Clear() => _items.Clear();

    public bool Contains(T item)
    {
        if (item is null) return false;
        return _items.TryGetValue(item, out var entry) && !IsExpired(entry);
    }

    public int Count => _items.Values.Count(entry => !IsExpired(entry));

    public void CopyTo(CachedProxy<T>[] array, int arrayIndex)
    {
        ArgumentNullException.ThrowIfNull(array);
        var index = arrayIndex;
        foreach (var entry in _items.Values)
        {
            if (IsExpired(entry)) continue;
            array[index++] = entry.Proxy;
        }
    }

    public IEnumerator<CachedProxy<T>> GetEnumerator()
    {
        foreach (var entry in _items.Values)
        {
            if (!IsExpired(entry))
                yield return entry.Proxy;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private bool IsExpired(Entry<T> entry) =>
        entry.ExpireAtUtc != default && entry.ExpireAtUtc <= _utcNow();

    private void PurgeExpired()
    {
        _popStrategy?.Do();
    }
}
