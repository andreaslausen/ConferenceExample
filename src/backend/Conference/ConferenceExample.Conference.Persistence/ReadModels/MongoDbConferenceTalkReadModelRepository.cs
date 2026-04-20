using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.Extensions;
using ConferenceExample.Conference.Domain.TalkManagement;
using MongoDB.Driver;

namespace ConferenceExample.Conference.Persistence.ReadModels;

public class MongoDbConferenceTalkReadModelRepository
    : IConferenceTalkDocumentRepository,
        IConferenceTalkReadModelRepository
{
    private readonly IMongoCollection<ConferenceTalkDocument> _collection;

    public MongoDbConferenceTalkReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<ConferenceTalkDocument>("conference_talk_readmodels");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var conferenceIndexKeys = Builders<ConferenceTalkDocument>.IndexKeys.Ascending(t =>
            t.ConferenceId
        );
        var conferenceIndexModel = new CreateIndexModel<ConferenceTalkDocument>(
            conferenceIndexKeys,
            new CreateIndexOptions { Name = "idx_conferenceId" }
        );

        _collection.Indexes.CreateOne(conferenceIndexModel);
    }

    public async Task<ConferenceTalkDocument?> GetById(Guid talkId)
    {
        var filter = Builders<ConferenceTalkDocument>.Filter.Eq(t => t.Id, talkId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<ConferenceTalkDocument>> GetByConferenceId(Guid conferenceId)
    {
        var filter = Builders<ConferenceTalkDocument>.Filter.Eq(
            t => t.ConferenceId,
            conferenceId.ToString()
        );
        return await _collection.Find(filter).ToListAsync();
    }

    async Task<
        IReadOnlyList<ConferenceTalkReadModel>
    > IConferenceTalkReadModelRepository.GetByConferenceId(ConferenceId conferenceId)
    {
        var filter = Builders<ConferenceTalkDocument>.Filter.Eq(
            t => t.ConferenceId,
            conferenceId.Value.Value.ToString()
        );
        var documents = await _collection.Find(filter).ToListAsync();

        return documents
            .Select(d => new ConferenceTalkReadModel(
                d.Id.ToGuid(),
                d.Title,
                d.Abstract,
                d.SpeakerId.ToGuid(),
                d.Status,
                d.Tags,
                d.TalkTypeId.ToGuid()
            ))
            .ToList();
    }

    public async Task Save(ConferenceTalkDocument talkDocument)
    {
        await _collection.InsertOneAsync(talkDocument);
    }

    public async Task Update(ConferenceTalkDocument talkDocument)
    {
        var filter = Builders<ConferenceTalkDocument>.Filter.And(
            Builders<ConferenceTalkDocument>.Filter.Eq(t => t.Id, talkDocument.Id),
            Builders<ConferenceTalkDocument>.Filter.Lt(t => t.Version, talkDocument.Version)
        );

        _ = await _collection.ReplaceOneAsync(filter, talkDocument);
    }

    public async Task Delete(Guid talkId)
    {
        var filter = Builders<ConferenceTalkDocument>.Filter.Eq(t => t.Id, talkId.ToString());
        await _collection.DeleteOneAsync(filter);
    }
}
