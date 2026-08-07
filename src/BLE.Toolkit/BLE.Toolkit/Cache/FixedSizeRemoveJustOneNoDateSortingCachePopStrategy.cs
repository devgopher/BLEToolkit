namespace BLE.Toolkit.Cache;

/// <summary>
/// Removes only one last element in an array w/o date sorting
/// </summary>
/// <param name="cache"></param>
/// <param name="size"></param>
/// <typeparam name="T"></typeparam>
public class FixedSizeRemoveJustOneNoDateSortingCachePopStrategy<T>(HashSet<Entry<T>> cache, int size)
    : FixedSizeCachePopStrategy<T>(cache, size, true, false)
    where T : notnull;