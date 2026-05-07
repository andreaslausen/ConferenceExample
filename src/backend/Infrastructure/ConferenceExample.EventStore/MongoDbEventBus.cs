using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ConferenceExample.EventStore;

public class MongoDbEventBus : IEventBus, IHostedService, IDisposable
{
    private readonly IMongoDatabase _database;
    private readonly IReadOnlyCollection<string> _collectionNames;
    private readonly Dictionary<string, List<Func<StoredEvent, Task>>> _subscriptions = [];
    private readonly Lock _lock = new();
    private CancellationTokenSource? _cts;
    private Task? _processingTask;

    public MongoDbEventBus(IMongoDatabase database, IReadOnlyCollection<string> collectionNames)
    {
        _database = database;
        _collectionNames = collectionNames;
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
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var watchedCollections = _collectionNames;
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<BsonDocument>>().Match(
            change =>
                change.OperationType == ChangeStreamOperationType.Insert
                && watchedCollections.Contains(change.CollectionNamespace.CollectionName)
        );

        var options = new ChangeStreamOptions
        {
            FullDocument = ChangeStreamFullDocumentOption.UpdateLookup,
        };

        try
        {
            // Watch at database level — survives individual collection drops (e.g. between tests)
            var cursor = await _database.WatchAsync(pipeline, options, cancellationToken);
            _processingTask = Task.Run(() => ProcessEventsAsync(cursor, _cts.Token), _cts.Token);
        }
        catch (MongoCommandException ex) when (ex.CodeName == "NotImplemented")
        {
            // Change Streams require a replica set — not available in standalone MongoDB
            Console.WriteLine(
                "MongoDB Change Streams not available (requires replica set). Event handlers will not run."
            );
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        if (_processingTask != null)
        {
            try
            {
                await _processingTask.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException) { }
        }
    }

    private async Task ProcessEventsAsync(
        IAsyncCursor<ChangeStreamDocument<BsonDocument>> cursor,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await cursor.ForEachAsync(
                async change =>
                {
                    if (change.FullDocument == null)
                        return;

                    try
                    {
                        var storedEvent = BsonSerializer.Deserialize<StoredEvent>(
                            change.FullDocument
                        );
                        await NotifySubscribersAsync(storedEvent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing change stream event: {ex.Message}");
                    }
                },
                cancellationToken
            );
        }
        catch (OperationCanceledException) { }
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
        if (_cts is { IsCancellationRequested: false })
        {
            _cts.Cancel();
        }

        _processingTask?.Wait(TimeSpan.FromSeconds(5));
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
