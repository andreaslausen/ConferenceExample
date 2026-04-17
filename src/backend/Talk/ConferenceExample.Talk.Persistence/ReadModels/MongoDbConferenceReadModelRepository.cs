using ConferenceExample.Talk.Domain.TalkManagement;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

/// <summary>
/// MongoDB implementation of Conference Info repository for the Talk bounded context.
/// Stores denormalized conference data replicated from Conference BC in the 'talk_conference_readmodels' collection.
/// Converts between MongoDB documents (ConferenceReadModel) and domain objects (ConferenceInfo).
/// </summary>
public class MongoDbConferenceReadModelRepository : IConferenceInfoRepository
{
    private readonly IMongoCollection<ConferenceReadModel> _collection;

    public MongoDbConferenceReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConferenceReadModel>("talk_conference_readmodels");
    }

    public async Task<ConferenceInfo?> GetById(ConferenceId conferenceId)
    {
        var filter = Builders<ConferenceReadModel>.Filter.Eq(
            c => c.Id,
            conferenceId.Value.Value.ToString()
        );
        var readModel = await _collection.Find(filter).FirstOrDefaultAsync();
        return readModel?.ToConferenceInfo();
    }

    // Internal methods used by event handlers - not part of domain interface
    internal async Task<ConferenceReadModel?> GetReadModelById(Guid conferenceId)
    {
        var filter = Builders<ConferenceReadModel>.Filter.Eq(c => c.Id, conferenceId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    internal async Task Save(ConferenceReadModel conferenceReadModel)
    {
        await _collection.InsertOneAsync(conferenceReadModel);
    }

    internal async Task Update(ConferenceReadModel conferenceReadModel)
    {
        // Optimistic locking: Only update if the event version is newer than the current read model version
        var filter = Builders<ConferenceReadModel>.Filter.And(
            Builders<ConferenceReadModel>.Filter.Eq(c => c.Id, conferenceReadModel.Id),
            Builders<ConferenceReadModel>.Filter.Lt(c => c.Version, conferenceReadModel.Version)
        );

        _ = await _collection.ReplaceOneAsync(filter, conferenceReadModel);

        // If ModifiedCount is 0, the read model already has a newer or equal version
        // This is expected with out-of-order events and can be safely ignored
    }

    internal async Task Delete(Guid conferenceId)
    {
        var filter = Builders<ConferenceReadModel>.Filter.Eq(c => c.Id, conferenceId.ToString());
        await _collection.DeleteOneAsync(filter);
    }
}
