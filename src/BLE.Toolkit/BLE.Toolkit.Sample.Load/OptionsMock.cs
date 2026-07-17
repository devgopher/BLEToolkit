using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Sample.Load;

public class OptionsMock<T>(T value) : IOptionsMonitor<T>
{
    public T Get(string? name)
    {
        throw new NotImplementedException();
    }

    public IDisposable? OnChange(Action<T, string?> listener)
    {
        throw new NotImplementedException();
    }

    public T CurrentValue { get; } = value;
}