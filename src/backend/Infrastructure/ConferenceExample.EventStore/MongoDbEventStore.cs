using MongoDB.Driver;

namespace ConferenceExample.EventStore;

public class MongoDbEventStore : IEventStore
{
    private readonly IMongoCollection<StoredEvent> _eventsCollection;

    public MongoDbEventStore(IMongoDatabase database)
    {
        _eventsCollection = database.GetCollection<StoredEvent>("events");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        // Compound index for efficient aggregate retrieval
        var aggregateIndexKeys = Builders<StoredEvent>
            .IndexKeys.Ascending(e => e.AggregateId)
            .Ascending(e => e.Version);

        var aggregateIndexModel = new CreateIndexModel<StoredEvent>(
            aggregateIndexKeys,
            new CreateIndexOptions { Name = "idx_aggregate_version" }
        );

        // Index for version ordering across all events
        var versionIndexKeys = Builders<StoredEvent>.IndexKeys.Ascending(e => e.Version);
        var versionIndexModel = new CreateIndexModel<StoredEvent>(
            versionIndexKeys,
            new CreateIndexOptions { Name = "idx_version" }
        );

        // Note: MongoDB automatically creates a unique index on _id (which maps to the Id property)
        // so we don't need to explicitly create one

        _eventsCollection.Indexes.CreateMany(new[] { aggregateIndexModel, versionIndexModel });
    }

    public async Task AppendEvents(
        Guid aggregateId,
        IEnumerable<StoredEvent> events,
        long expectedVersion
    )
    {
        var eventsList = events.ToList();
        if (eventsList.Count == 0)
        {
            return;
        }

        // Start a session for transaction support
        using var session = await _eventsCollection.Database.Client.StartSessionAsync();

        try
        {
            await session.WithTransactionAsync(
                async (sessionHandle, cancellationToken) =>
                {
                    // Check current version (optimistic concurrency control)
                    var currentVersion = await GetCurrentVersion(aggregateId, sessionHandle);

                    if (currentVersion != expectedVersion)
                    {
                        throw new ConcurrencyException(
                            $"Expected version {expectedVersion} but found {currentVersion} for aggregate {aggregateId}."
                        );
                    }

                    // Insert all events atomically
                    await _eventsCollection.InsertManyAsync(
                        sessionHandle,
                        eventsList,
                        cancellationToken: cancellationToken
                    );

                    return Task.CompletedTask;
                },
                cancellationToken: CancellationToken.None
            );
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // This should not happen in normal operation, but could indicate a retry
            throw new ConcurrencyException(
                $"Duplicate event detected for aggregate {aggregateId}. Possible concurrency conflict.",
                ex
            );
        }
    }

    public async Task<IReadOnlyList<StoredEvent>> GetEvents(Guid aggregateId)
    {
        var filter = Builders<StoredEvent>.Filter.Eq(e => e.AggregateId, aggregateId);
        var sort = Builders<StoredEvent>.Sort.Ascending(e => e.Version);

        var events = await _eventsCollection.Find(filter).Sort(sort).ToListAsync();

        return events;
    }

    public async Task<IReadOnlyList<StoredEvent>> GetAllEvents()
    {
        var sort = Builders<StoredEvent>.Sort.Ascending(e => e.Version);

        var events = await _eventsCollection
            .Find(FilterDefinition<StoredEvent>.Empty)
            .Sort(sort)
            .ToListAsync();

        return events;
    }

    private async Task<long> GetCurrentVersion(
        Guid aggregateId,
        IClientSessionHandle? session = null
    )
    {
        var filter = Builders<StoredEvent>.Filter.Eq(e => e.AggregateId, aggregateId);
        var sort = Builders<StoredEvent>.Sort.Descending(e => e.Version);

        var latestEvent =
            session != null
                ? await _eventsCollection
                    .Find(session, filter)
                    .Sort(sort)
                    .Limit(1)
                    .FirstOrDefaultAsync()
                : await _eventsCollection.Find(filter).Sort(sort).Limit(1).FirstOrDefaultAsync();

        return latestEvent?.Version ?? -1;
    }
}
