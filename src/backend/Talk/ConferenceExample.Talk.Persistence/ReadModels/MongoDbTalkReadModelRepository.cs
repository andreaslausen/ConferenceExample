using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class MongoDbTalkReadModelRepository : ITalkDocumentRepository, ITalkReadModelRepository
{
    private readonly IMongoCollection<TalkDocument> _collection;

    public MongoDbTalkReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<TalkDocument>("talk_readmodels");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var conferenceIndexKeys = Builders<TalkDocument>.IndexKeys.Ascending(t => t.ConferenceId);
        var conferenceIndexModel = new CreateIndexModel<TalkDocument>(
            conferenceIndexKeys,
            new CreateIndexOptions { Name = "idx_conferenceId" }
        );

        var speakerIndexKeys = Builders<TalkDocument>.IndexKeys.Ascending(t => t.SpeakerId);
        var speakerIndexModel = new CreateIndexModel<TalkDocument>(
            speakerIndexKeys,
            new CreateIndexOptions { Name = "idx_speakerId" }
        );

        _collection.Indexes.CreateMany(new[] { conferenceIndexModel, speakerIndexModel });
    }

    public async Task<TalkDocument?> GetById(Guid talkId)
    {
        var filter = Builders<TalkDocument>.Filter.Eq(t => t.Id, talkId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<TalkDocument>> GetByConferenceId(Guid conferenceId)
    {
        var filter = Builders<TalkDocument>.Filter.Eq(t => t.ConferenceId, conferenceId.ToString());
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<IReadOnlyList<TalkDocument>> GetBySpeakerId(Guid speakerId)
    {
        var filter = Builders<TalkDocument>.Filter.Eq(t => t.SpeakerId, speakerId.ToString());
        return await _collection.Find(filter).ToListAsync();
    }

    async Task<TalkReadModel?> ITalkReadModelRepository.GetById(TalkId talkId)
    {
        var filter = Builders<TalkDocument>.Filter.Eq(t => t.Id, talkId.Value.Value.ToString());
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        if (document is null)
            return null;

        return new TalkReadModel(
            document.Id.ToGuid(),
            document.Title,
            document.Abstract,
            document.ConferenceId.ToGuid(),
            document.Status,
            document.Tags,
            document.SpeakerId.ToGuid(),
            $"{document.SpeakerFirstName} {document.SpeakerLastName}".Trim()
        );
    }

    async Task<IReadOnlyList<TalkReadModel>> ITalkReadModelRepository.GetBySpeakerId(
        SpeakerId speakerId
    )
    {
        var filter = Builders<TalkDocument>.Filter.Eq(
            t => t.SpeakerId,
            speakerId.Value.Value.ToString()
        );
        var documents = await _collection.Find(filter).ToListAsync();

        return documents
            .Select(d => new TalkReadModel(
                d.Id.ToGuid(),
                d.Title,
                d.Abstract,
                d.ConferenceId.ToGuid(),
                d.Status,
                d.Tags,
                d.SpeakerId.ToGuid(),
                $"{d.SpeakerFirstName} {d.SpeakerLastName}".Trim()
            ))
            .ToList();
    }

    public async Task Save(TalkDocument talkDocument)
    {
        await _collection.InsertOneAsync(talkDocument);
    }

    public async Task Update(TalkDocument talkDocument)
    {
        var filter = Builders<TalkDocument>.Filter.And(
            Builders<TalkDocument>.Filter.Eq(t => t.Id, talkDocument.Id),
            Builders<TalkDocument>.Filter.Lt(t => t.Version, talkDocument.Version)
        );

        _ = await _collection.ReplaceOneAsync(filter, talkDocument);
    }

    public async Task Delete(Guid talkId)
    {
        var filter = Builders<TalkDocument>.Filter.Eq(t => t.Id, talkId.ToString());
        await _collection.DeleteOneAsync(filter);
    }
}
