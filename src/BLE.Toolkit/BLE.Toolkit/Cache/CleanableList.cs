using System.Collections;

namespace BLE.Toolkit.Cache;

public class CleanableList<T> : IEnumerable<CachedProxy<T>>
    where T : notnull
{
    private readonly Dictionary<T, Entry<T>> _items;
    private readonly TimeSpan _timeout;
    private readonly Func<DateTime> _utcNow;
    private readonly CachePopStrategy<T>? _popStrategy;

    public CleanableList(int timeout, Func<DateTime>? utcNow = null, IEqualityComparer<T>? comparer = null)
        : this(TimeSpan.FromSeconds(timeout), utcNow, comparer)
    {
    }

    public CleanableList(TimeSpan timeout, Func<DateTime>? utcNow = null, IEqualityComparer<T>? comparer = null)
        : this(timeout, popStrategy: null, utcNow, comparer)
    {
    }

    public CleanableList(int timeout, CachePopStrategy<T> popStrategy, Func<DateTime>? utcNow = null,
        IEqualityComparer<T>? comparer = null)
        : this(TimeSpan.FromSeconds(timeout), popStrategy, utcNow, comparer)
    {
    }

    public CleanableList(TimeSpan timeout, CachePopStrategy<T>? popStrategy, Func<DateTime>? utcNow = null,
        IEqualityComparer<T>? comparer = null)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        _timeout = timeout;
        _popStrategy = popStrategy;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
        _items = new Dictionary<T, Entry<T>>(comparer);
    }

    public bool Add(T item)
    {
        PurgeExpired();
        if (item is null) return false;

        var expireAt = _utcNow().Add(_timeout);
        if (_items.TryGetValue(item, out var existing))
        {
            existing.ExpireAtUtc = expireAt;
            return false;
        }

        _items[item] = new Entry<T>(item, expireAt);
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
