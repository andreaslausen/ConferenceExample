using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;
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
        var speakerIndexKeys = Builders<TalkDocument>.IndexKeys.Ascending(t => t.SpeakerId);
        var speakerIndexModel = new CreateIndexModel<TalkDocument>(
            speakerIndexKeys,
            new CreateIndexOptions { Name = "idx_speakerId" }
        );

        _collection.Indexes.CreateOne(speakerIndexModel);
    }

    public async Task<TalkDocument?> GetById(Guid talkId)
    {
        var filter = Builders<TalkDocument>.Filter.Eq(t => t.Id, talkId.ToString());
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    async Task<TalkReadModel?> ITalkReadModelRepository.GetById(TalkId talkId)
    {
        var filter = Builders<TalkDocument>.Filter.Eq(t => t.Id, talkId.Value.Value.ToString());
        var document = await _collection.Find(filter).FirstOrDefaultAsync();

        return document is null ? null : ToReadModel(document);
    }

    async Task<(
        IReadOnlyList<TalkReadModel> Items,
        int TotalCount
    )> ITalkReadModelRepository.GetBySpeakerId(SpeakerId speakerId, PageRequest pageRequest)
    {
        var filter = Builders<TalkDocument>.Filter.Eq(
            t => t.SpeakerId,
            speakerId.Value.Value.ToString()
        );

        var countTask = _collection.CountDocumentsAsync(filter);
        var documentsTask = _collection
            .Find(filter)
            .Skip(pageRequest.Skip)
            .Limit(pageRequest.PageSize)
            .ToListAsync();
        await Task.WhenAll(countTask, documentsTask);

        var items = documentsTask.Result.Select(ToReadModel).ToList();

        return (items, (int)countTask.Result);
    }

    private static TalkReadModel ToReadModel(TalkDocument document) =>
        new(
            document.Id.ToGuid(),
            document.Title,
            document.Abstract,
            document.SpeakerId.ToGuid(),
            document.Tags,
            document.SubmissionCount
        );

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
