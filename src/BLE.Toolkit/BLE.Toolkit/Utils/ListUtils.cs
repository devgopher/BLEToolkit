namespace BLE.Toolkit.Utils;

public static class ListUtils
{
    public static void AddRange<T>(this IList<T> list, IEnumerable<T>? collection)
    {
        if (collection == null)
            return;
        
        foreach (var item in collection)
            list.Add(item);
    }
}