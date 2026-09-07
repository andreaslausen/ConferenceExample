using ConferenceExample.Talk.Domain.SharedKernel.Extensions;
using ConferenceExample.Talk.Domain.TalkManagement;
using MongoDB.Driver;

namespace ConferenceExample.Talk.Persistence.ReadModels;

public class MongoDbTalkSubmissionReadModelRepository
    : ITalkSubmissionDocumentRepository,
        ITalkSubmissionReadModelRepository
{
    private readonly IMongoCollection<TalkSubmissionDocument> _collection;

    public MongoDbTalkSubmissionReadModelRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<TalkSubmissionDocument>("talk_submission_readmodels");
        CreateIndexes();
    }

    private void CreateIndexes()
    {
        var talkIndexKeys = Builders<TalkSubmissionDocument>.IndexKeys.Ascending(s => s.TalkId);
        var talkIndexModel = new CreateIndexModel<TalkSubmissionDocument>(
            talkIndexKeys,
            new CreateIndexOptions { Name = "idx_talkId" }
        );

        var speakerIndexKeys = Builders<TalkSubmissionDocument>.IndexKeys.Ascending(s =>
            s.SpeakerId
        );
        var speakerIndexModel = new CreateIndexModel<TalkSubmissionDocument>(
            speakerIndexKeys,
            new CreateIndexOptions { Name = "idx_speakerId" }
        );

        _collection.Indexes.CreateMany([talkIndexModel, speakerIndexModel]);
    }

    public async Task<TalkSubmissionDocument?> Get(Guid talkId, Guid conferenceId)
    {
        var filter = Builders<TalkSubmissionDocument>.Filter.Eq(
            s => s.Id,
            TalkSubmissionDocument.BuildId(talkId, conferenceId)
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    async Task<
        IReadOnlyList<TalkSubmissionReadModel>
    > ITalkSubmissionReadModelRepository.GetByTalkId(TalkId talkId)
    {
        var filter = Builders<TalkSubmissionDocument>.Filter.Eq(
            s => s.TalkId,
            talkId.Value.Value.ToString()
        );

        var documents = await _collection.Find(filter).SortBy(s => s.SubmittedAt).ToListAsync();

        return documents
            .Select(d => new TalkSubmissionReadModel(
                d.TalkId.ToGuid(),
                d.ConferenceId.ToGuid(),
                d.TalkTypeId.ToGuid(),
                d.Status,
                d.Reason,
                d.SubmittedAt,
                d.Title,
                d.Abstract,
                d.Tags
            ))
            .ToList();
    }

    public async Task Save(TalkSubmissionDocument document)
    {
        await _collection.InsertOneAsync(document);
    }

    public async Task Update(TalkSubmissionDocument document)
    {
        var filter = Builders<TalkSubmissionDocument>.Filter.Eq(s => s.Id, document.Id);
        _ = await _collection.ReplaceOneAsync(filter, document);
    }
}
