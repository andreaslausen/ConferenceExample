using System.Text.Json;
using MongoDB.Driver;

namespace ConferenceExample.EventStore;

public class MongoDbEventBus : IEventBus, IDisposable
{
    private readonly IMongoCollection<StoredEvent> _eventsCollection;
    private readonly Dictionary<string, List<Func<StoredEvent, Task>>> _subscriptions = [];
    private readonly Lock _lock = new();
    private Task? _changeStreamTask;
    private CancellationTokenSource? _cancellationTokenSource;

    public MongoDbEventBus(IMongoDatabase database)
    {
        _eventsCollection = database.GetCollection<StoredEvent>("events");
    }

    public void Subscribe(string eventType, Func<StoredEvent, Task> handler)
    {
        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers = [];
                _subscriptions[eventType] = handlers;
            }

            handlers.Add(handler);
        }

        // Start change stream monitoring if not already started
        EnsureChangeStreamStarted();
    }

    private void EnsureChangeStreamStarted()
    {
        if (_changeStreamTask != null)
        {
            return;
        }

        lock (_lock)
        {
            if (_changeStreamTask != null)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _changeStreamTask = Task.Run(() => MonitorChangeStream(_cancellationTokenSource.Token));
        }
    }

    private async Task MonitorChangeStream(CancellationToken cancellationToken)
    {
        // Change Stream pipeline - watch for inserts only
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<StoredEvent>>().Match(
            change => change.OperationType == ChangeStreamOperationType.Insert
        );

        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
        };

        try
        {
            // Note: Change Streams require MongoDB to be running as a replica set
            // For local development, this might not be available
            using var cursor = await _eventsCollection.WatchAsync(
                pipeline,
                options,
                cancellationToken
            );

            await cursor.ForEachAsync(
                async change =>
                {
                    if (change.FullDocument != null)
                    {
                        await NotifySubscribersAsync(change.FullDocument);
                    }
                },
                cancellationToken
            );
        }
        catch (MongoCommandException ex) when (ex.CodeName == "NotImplemented")
        {
            // Change Streams not supported (standalone MongoDB)
            // Fall back to polling or just use immediate notification from Publish
            Console.WriteLine(
                "MongoDB Change Streams not available (requires replica set). Using immediate notification only."
            );
        }
        catch (OperationCanceledException)
        {
            // Expected when disposing
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in MongoDB Change Stream: {ex.Message}");
        }
    }

    private async Task NotifySubscribersAsync(StoredEvent storedEvent)
    {
        List<Func<StoredEvent, Task>> handlers;

        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(storedEvent.EventType, out var registered))
            {
                return;
            }

            handlers = [.. registered];
        }

        // Execute all handlers in parallel (safe with fat events + optimistic locking)
        var tasks = handlers.Select(handler => ExecuteHandlerAsync(handler, storedEvent));
        await Task.WhenAll(tasks);
    }

    private static async Task ExecuteHandlerAsync(
        Func<StoredEvent, Task> handler,
        StoredEvent storedEvent
    )
    {
        try
        {
            await handler(storedEvent);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in event handler for {storedEvent.EventType}: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Cancel();
        _changeStreamTask?.Wait(TimeSpan.FromSeconds(5));
        _cancellationTokenSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
