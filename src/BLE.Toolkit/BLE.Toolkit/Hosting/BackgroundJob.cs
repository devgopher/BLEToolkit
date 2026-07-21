namespace BLE.Toolkit.Hosting;

/// <summary>
/// Runs a long-running action on a background thread with explicit start and stop.
/// </summary>
public sealed class BackgroundJob
{
    private CancellationTokenSource? _cts;
    private Task? _task;

    public bool IsRunning => _task is { IsCompleted: false };

    public void Start(Func<CancellationToken, Task> work, CancellationToken cancellationToken)
    {
        if (IsRunning)
            throw new InvalidOperationException("Background job is already running.");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _task = Task.Run(() => work(_cts.Token), _cts.Token);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_cts is null || _task is null)
            return;

        await _cts.CancelAsync();

        try
        {
            await _task.WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
        }

        _cts.Dispose();
        _cts = null;
        _task = null;
    }
}
