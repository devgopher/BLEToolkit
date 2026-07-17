using System.Collections.Concurrent;
using System.Text;
using BLE.Toolkit.Sample.Load.Models;

namespace BLE.Toolkit.Sample.Load.Services;

public sealed class ReceivedMessageStore
{
    private readonly ConcurrentQueue<ReceivedMessageDto> _messages = new();
    private int _totalCount;

    public int TotalCount => _totalCount;

    public void Add(byte[] data)
    {
        if (data.Length == 0)
            return;

        var index = Interlocked.Increment(ref _totalCount);
        var text = Encoding.UTF8.GetString(data);
        _messages.Enqueue(new ReceivedMessageDto(index, text, DateTimeOffset.UtcNow));
    }

    public ReceiverMessagesResponse GetMessages(int skip, int take)
    {
        var all = _messages.ToArray();
        var slice = all
            .Skip(skip)
            .Take(take)
            .ToArray();

        return new ReceiverMessagesResponse(_totalCount, slice);
    }

    public void Clear()
    {
        while (_messages.TryDequeue(out _))
        {
        }

        Interlocked.Exchange(ref _totalCount, 0);
    }
}
