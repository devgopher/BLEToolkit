namespace BLE.Toolkit.Cache;

public sealed class CachedProxy<T>(T inner)
    where T : notnull
{
    public long UsedStat { get; private set; }
    public DateTime CreationTime { get; } = DateTime.UtcNow;
    
    public T Value
    {
        get
        {
            ++UsedStat;
            return inner;
        }
    }
}
