using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

namespace ConferenceExample.EventStore;

/// <summary>
/// In-memory event bus that guarantees FIFO order and is safe for concurrent publishers.
/// Events are deduplicated by <see cref="StoredEvent.Id"/> so cross-context replication
/// (Conference→Talk event store) does not cause double dispatch when both stores publish.
/// Limitation: events are not durable across process restarts and not visible to other instances.
/// Suitable for the demo; replace with RabbitMQ/Kafka in production.
/// </summary>
public class InMemoryEventBus : IEventBus, IHostedService, IAsyncDisposable
{
    private readonly Channel<StoredEvent> _channel = Channel.CreateUnbounded<StoredEvent>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false }
    );
    private readonly Dictionary<string, List<Func<StoredEvent, Task>>> _subscriptions = [];
    private readonly Lock _subscriptionLock = new();
    private readonly HashSet<Guid> _publishedIds = [];
    private readonly Lock _publishedLock = new();
    private CancellationTokenSource? _cts;
    private Task? _processingTask;

    public void Subscribe(string eventType, Func<StoredEvent, Task> handler)
    {
        lock (_subscriptionLock)
        {
            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers = [];
                _subscriptions[eventType] = handlers;
            }

            handlers.Add(handler);
        }
    }

    public void Publish(StoredEvent storedEvent)
    {
        lock (_publishedLock)
        {
            if (!_publishedIds.Add(storedEvent.Id))
            {
                return;
            }
        }

        if (!_channel.Writer.TryWrite(storedEvent))
        {
            throw new InvalidOperationException(
                "Event bus is closed and can no longer accept events."
            );
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _processingTask = Task.Run(() => ProcessAsync(_cts.Token), CancellationToken.None);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _channel.Writer.TryComplete();

        if (_cts is not null)
        {
            await _cts.CancelAsync();
        }

        if (_processingTask is not null)
        {
            try
            {
                await _processingTask.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) { }
        }
    }

    private async Task ProcessAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (
                var storedEvent in _channel.Reader.ReadAllAsync(cancellationToken)
            )
            {
                List<Func<StoredEvent, Task>> handlers;
                lock (_subscriptionLock)
                {
                    if (!_subscriptions.TryGetValue(storedEvent.EventType, out var registered))
                    {
                        continue;
                    }

                    handlers = [.. registered];
                }

                foreach (var handler in handlers)
                {
                    try
                    {
                        await handler(storedEvent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"Error in event handler for {storedEvent.EventType}: {ex.Message}"
                        );
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    public async ValueTask DisposeAsync()
    {
        _channel.Writer.TryComplete();

        if (_cts is { IsCancellationRequested: false })
        {
            await _cts.CancelAsync();
        }

        if (_processingTask is not null)
        {
            try
            {
                await _processingTask;
            }
            catch (OperationCanceledException) { }
        }

        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
