using ConferenceExample.Conference.Persistence.ReadModels;
using MongoDB.Driver;
using NSubstitute;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class MongoDbConferenceTalkReadModelRepositoryTests
{
    private readonly IMongoDatabase _mockDatabase;
    private readonly IMongoCollection<ConferenceTalkDocument> _mockCollection;
    private readonly IMongoIndexManager<ConferenceTalkDocument> _mockIndexManager;

    public MongoDbConferenceTalkReadModelRepositoryTests()
    {
        _mockDatabase = Substitute.For<IMongoDatabase>();
        _mockCollection = Substitute.For<IMongoCollection<ConferenceTalkDocument>>();
        _mockIndexManager = Substitute.For<IMongoIndexManager<ConferenceTalkDocument>>();

        _mockCollection.Indexes.Returns(_mockIndexManager);
        _mockDatabase
            .GetCollection<ConferenceTalkDocument>("conference_talk_readmodels")
            .Returns(_mockCollection);
    }

    [Fact]
    public void Constructor_CreatesCollectionAndIndexes()
    {
        // Act
        _ = new MongoDbConferenceTalkReadModelRepository(_mockDatabase);

        // Assert
        _mockDatabase
            .Received(1)
            .GetCollection<ConferenceTalkDocument>("conference_talk_readmodels");
        _mockIndexManager
            .Received(1)
            .CreateOne(Arg.Any<CreateIndexModel<ConferenceTalkDocument>>());
    }

    [Fact]
    public async Task GetById_ExistingTalk_ReturnsConferenceTalkDocument()
    {
        // Arrange
        var talkId = Guid.NewGuid();
        var expectedTalk = new ConferenceTalkDocument
        {
            Id = talkId.ToString(),
            ConferenceId = Guid.NewGuid().ToString(),
            Title = "Test Talk",
            Abstract = "Test Abstract",
            SpeakerId = Guid.NewGuid().ToString(),
            TalkTypeId = Guid.NewGuid().ToString(),
            Tags = new List<string> { "dotnet", "testing" },
            Status = "Submitted",
            Version = 1,
        };

        var mockCursor = Substitute.For<IAsyncCursor<ConferenceTalkDocument>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(new[] { expectedTalk });

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<ConferenceTalkDocument>>(),
                Arg.Any<FindOptions<ConferenceTalkDocument, ConferenceTalkDocument>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbConferenceTalkReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetById(talkId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedTalk.Id, result.Id);
        Assert.Equal(expectedTalk.Title, result.Title);
    }

    [Fact]
    public async Task GetById_NonExistingTalk_ReturnsNull()
    {
        // Arrange
        var talkId = Guid.NewGuid();

        var mockCursor = Substitute.For<IAsyncCursor<ConferenceTalkDocument>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(Array.Empty<ConferenceTalkDocument>());

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<ConferenceTalkDocument>>(),
                Arg.Any<FindOptions<ConferenceTalkDocument, ConferenceTalkDocument>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbConferenceTalkReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetById(talkId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByConferenceId_ExistingTalks_ReturnsTalkList()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var expectedTalks = new List<ConferenceTalkDocument>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                ConferenceId = conferenceId.ToString(),
                Title = "Talk 1",
                Status = "Submitted",
                Version = 1,
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                ConferenceId = conferenceId.ToString(),
                Title = "Talk 2",
                Status = "Accepted",
                Version = 1,
            },
        };

        var mockCursor = Substitute.For<IAsyncCursor<ConferenceTalkDocument>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(expectedTalks);

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<ConferenceTalkDocument>>(),
                Arg.Any<FindOptions<ConferenceTalkDocument, ConferenceTalkDocument>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbConferenceTalkReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetByConferenceId(conferenceId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Talk 1");
        Assert.Contains(result, t => t.Title == "Talk 2");
    }

    [Fact]
    public async Task GetByConferenceId_NoTalks_ReturnsEmptyList()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();

        var mockCursor = Substitute.For<IAsyncCursor<ConferenceTalkDocument>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(Array.Empty<ConferenceTalkDocument>());

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<ConferenceTalkDocument>>(),
                Arg.Any<FindOptions<ConferenceTalkDocument, ConferenceTalkDocument>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbConferenceTalkReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetByConferenceId(conferenceId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Save_NewTalkReadModel_InsertsIntoCollection()
    {
        // Arrange
        var talkReadModel = new ConferenceTalkDocument
        {
            Id = Guid.NewGuid().ToString(),
            ConferenceId = Guid.NewGuid().ToString(),
            Title = "New Talk",
            Abstract = "New Abstract",
            SpeakerId = Guid.NewGuid().ToString(),
            TalkTypeId = Guid.NewGuid().ToString(),
            Tags = ["dotnet"],
            Status = "Submitted",
            Version = 1,
        };

        var repository = new MongoDbConferenceTalkReadModelRepository(_mockDatabase);

        // Act
        await repository.Save(talkReadModel);

        // Assert
        await _mockCollection
            .Received(1)
            .InsertOneAsync(
                talkReadModel,
                Arg.Any<InsertOneOptions>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Update_ExistingTalkReadModel_ReplacesInCollection()
    {
        // Arrange
        var talkReadModel = new ConferenceTalkDocument
        {
            Id = Guid.NewGuid().ToString(),
            ConferenceId = Guid.NewGuid().ToString(),
            Title = "Updated Talk",
            Abstract = "Updated Abstract",
            SpeakerId = Guid.NewGuid().ToString(),
            TalkTypeId = Guid.NewGuid().ToString(),
            Tags = ["dotnet", "testing"],
            Status = "Accepted",
            Version = 2,
        };

        var repository = new MongoDbConferenceTalkReadModelRepository(_mockDatabase);

        // Act
        await repository.Update(talkReadModel);

        // Assert
        await _mockCollection
            .Received(1)
            .ReplaceOneAsync(
                Arg.Any<FilterDefinition<ConferenceTalkDocument>>(),
                talkReadModel,
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Delete_ExistingTalk_DeletesFromCollection()
    {
        // Arrange
        var talkId = Guid.NewGuid();

        var repository = new MongoDbConferenceTalkReadModelRepository(_mockDatabase);

        // Act
        await repository.Delete(talkId);

        // Assert
        await _mockCollection
            .Received(1)
            .DeleteOneAsync(
                Arg.Any<FilterDefinition<ConferenceTalkDocument>>(),
                Arg.Any<CancellationToken>()
            );
    }
}
