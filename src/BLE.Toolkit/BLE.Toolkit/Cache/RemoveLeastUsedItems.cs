namespace BLE.Toolkit.Cache;

/// <summary>
/// Removes the least used items during a statistics window
/// </summary>
/// <param name="cache"></param>
/// <param name="statWindow">Statistics window for each device</param>
/// <param name="percentile">Lowest percentile of usage statistics</param>
/// <typeparam name="T"></typeparam>
public class RemoveLeastUsedItems<T>(HashSet<Entry<T>> cache, TimeSpan statWindow, int percentile) : RemoveExpiredPopStrategy<T>(cache)
    where T : notnull
{
    public override void Do()
    {
        // ArgumentOutOfRangeException.ThrowIfEqual(statWindow, TimeSpan.Zero);
        //                   
        //                   base.Do();
        
        throw new NotImplementedException();
    }
}