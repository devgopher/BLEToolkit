namespace BLE.Toolkit.Settings;

public class RateLimitingSettings
{
    public required bool Enabled { get; init; }
    public required RatePeriod RatePeriod { get; init; }
    public required ushort Limit { get; init; }
}