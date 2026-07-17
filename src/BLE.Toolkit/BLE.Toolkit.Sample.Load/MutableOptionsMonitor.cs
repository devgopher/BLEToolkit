using Microsoft.Extensions.Options;

namespace BLE.Toolkit.Sample.Load;

public sealed class MutableOptionsMonitor<T> : IOptionsMonitor<T>
{
    private T _value;

    public MutableOptionsMonitor(T initial) => _value = initial;

    public T CurrentValue => _value;

    public void Set(T value) => _value = value;

    public T Get(string? name) => _value;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}
