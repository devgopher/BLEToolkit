using System.Collections;

namespace BLE.Toolkit.Cache;

public class ExpiredList<T> : IList<T>
{
    private sealed class Entry(T item, DateTime expireAtUtc)
    {
        public T Item = item;
        public DateTime ExpireAtUtc = expireAtUtc;
    }

    private readonly List<Entry> _items = new();
    private readonly TimeSpan _timeout;
    private readonly Func<DateTime> _utcNow;

    public ExpiredList(TimeSpan timeout, Func<DateTime>? utcNow = null)
    {
        if (timeout <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(timeout));
        _timeout = timeout;
        _utcNow = utcNow ?? (() => DateTime.UtcNow);
    }

    private void PurgeExpired()
    {
        var now = _utcNow();
        _items.RemoveAll(e => e.ExpireAtUtc <= now);
    }

    public IEnumerator<T> GetEnumerator()
    {
        PurgeExpired();
        // snapshot to avoid issues if caller enumerates while list changes
        var snapshot = new T[_items.Count];
        for (var i = 0; i < _items.Count; i++) snapshot[i] = _items[i].Item;
        return ((IEnumerable<T>)snapshot).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(T item)
    {
        PurgeExpired();
        _items.Add(new Entry(item, _utcNow().Add(_timeout)));
    }

    public void Clear()
    {
        PurgeExpired();
        _items.Clear();
    }

    public bool Contains(T item)
    {
        PurgeExpired();
        return _items.Exists(e => EqualityComparer<T>.Default.Equals(e.Item, item));
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (array is null) throw new ArgumentNullException(nameof(array));
        PurgeExpired();
        for (var i = 0; i < _items.Count; i++)
            array[arrayIndex + i] = _items[i].Item;
    }

    public bool Remove(T item)
    {
        PurgeExpired();
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < _items.Count; i++)
            if (comparer.Equals(_items[i].Item, item))
            {
                _items.RemoveAt(i);
                return true;
            }

        return false;
    }

    public int Count
    {
        get
        {
            PurgeExpired();
            return _items.Count;
        }
    }

    public bool IsReadOnly => false;

    public int IndexOf(T item)
    {
        PurgeExpired();
        var comparer = EqualityComparer<T>.Default;
        for (var i = 0; i < _items.Count; i++)
            if (comparer.Equals(_items[i].Item, item))
                return i;
        return -1;
    }

    public void Insert(int index, T item)
    {
        PurgeExpired();
        _items.Insert(index, new Entry(item, _utcNow().Add(_timeout)));
    }

    public void RemoveAt(int index)
    {
        PurgeExpired();
        _items.RemoveAt(index);
    }

    public T this[int index]
    {
        get
        {
            PurgeExpired();
            return _items[index].Item;
        }
        set
        {
            PurgeExpired();
            _items[index].Item = value;
            // reset timeout from this access, if you want "only on Add/Insert" remove this line:
            _items[index].ExpireAtUtc = _utcNow().Add(_timeout);
        }
    }
}