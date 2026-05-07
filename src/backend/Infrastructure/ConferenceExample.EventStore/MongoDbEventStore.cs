using MongoDB.Driver;

namespace ConferenceExample.EventStore;

public abstract class MongoDbEventStore : IEventStore
{
    private readonly IMongoCollection<StoredEvent> _eventsCollection;

    public MongoDbEventStore(IMongoDatabase database, string collectionName)
    {
        _eventsCollection = database.GetCollection<StoredEvent>(collectionName);
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var aggregateIndexKeys = Builders<StoredEvent>
            .IndexKeys.Ascending(e => e.AggregateId)
            .Ascending(e => e.Version);

        var aggregateIndexModel = new CreateIndexModel<StoredEvent>(
            aggregateIndexKeys,
            new CreateIndexOptions { Name = "idx_aggregate_version" }
        );

        var versionIndexKeys = Builders<StoredEvent>.IndexKeys.Ascending(e => e.Version);
        var versionIndexModel = new CreateIndexModel<StoredEvent>(
            versionIndexKeys,
            new CreateIndexOptions { Name = "idx_version" }
        );

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

        using var session = await _eventsCollection.Database.Client.StartSessionAsync();

        try
        {
            await session.WithTransactionAsync(
                async (sessionHandle, cancellationToken) =>
                {
                    var currentVersion = await GetCurrentVersion(aggregateId, sessionHandle);

                    if (currentVersion != expectedVersion)
                    {
                        throw new ConcurrencyException(
                            $"Expected version {expectedVersion} but found {currentVersion} for aggregate {aggregateId}."
                        );
                    }

                    await _eventsCollection.InsertManyAsync(
                        sessionHandle,
                        eventsList,
                        cancellationToken: cancellationToken
                    );

                    return Task.CompletedTask;
                }
            );
        }
        catch (MongoWriteException ex)
            when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
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
