using ConferenceExample.Talk.Persistence.ReadModels;
using MongoDB.Driver;
using NSubstitute;

namespace ConferenceExample.Talk.Persistence.UnitTests;

public class MongoDbTalkReadModelRepositoryTests
{
    private readonly IMongoDatabase _mockDatabase;
    private readonly IMongoCollection<TalkReadModel> _mockCollection;

    public MongoDbTalkReadModelRepositoryTests()
    {
        _mockDatabase = Substitute.For<IMongoDatabase>();
        _mockCollection = Substitute.For<IMongoCollection<TalkReadModel>>();
        _mockDatabase.GetCollection<TalkReadModel>("talk_readmodels").Returns(_mockCollection);
    }

    [Fact]
    public void Constructor_CreatesCollection()
    {
        // Act
        _ = new MongoDbTalkReadModelRepository(_mockDatabase);

        // Assert
        _mockDatabase.Received(1).GetCollection<TalkReadModel>("talk_readmodels");
    }

    [Fact]
    public async Task GetById_ExistingTalk_ReturnsTalkReadModel()
    {
        // Arrange
        var talkId = Guid.NewGuid();
        var expectedTalk = new TalkReadModel
        {
            Id = talkId.ToString(),
            Title = "Test Talk",
            Abstract = "Test Abstract",
            SpeakerId = Guid.NewGuid().ToString(),
            TalkTypeId = Guid.NewGuid().ToString(),
            ConferenceId = Guid.NewGuid().ToString(),
            Tags = new List<string> { "dotnet" },
            Status = "Submitted",
            Version = 1,
        };

        var mockCursor = Substitute.For<IAsyncCursor<TalkReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(new[] { expectedTalk });

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<TalkReadModel>>(),
                Arg.Any<FindOptions<TalkReadModel, TalkReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

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

        var mockCursor = Substitute.For<IAsyncCursor<TalkReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(Array.Empty<TalkReadModel>());

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<TalkReadModel>>(),
                Arg.Any<FindOptions<TalkReadModel, TalkReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

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
        var expectedTalks = new List<TalkReadModel>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Talk 1",
                ConferenceId = conferenceId.ToString(),
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Talk 2",
                ConferenceId = conferenceId.ToString(),
            },
        };

        var mockCursor = Substitute.For<IAsyncCursor<TalkReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(expectedTalks);

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<TalkReadModel>>(),
                Arg.Any<FindOptions<TalkReadModel, TalkReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetByConferenceId(conferenceId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, talk => Assert.Equal(conferenceId.ToString(), talk.ConferenceId));
    }

    [Fact]
    public async Task GetByConferenceId_NoTalks_ReturnsEmptyList()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();

        var mockCursor = Substitute.For<IAsyncCursor<TalkReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(Array.Empty<TalkReadModel>());

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<TalkReadModel>>(),
                Arg.Any<FindOptions<TalkReadModel, TalkReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetByConferenceId(conferenceId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetBySpeakerId_ExistingTalks_ReturnsTalkList()
    {
        // Arrange
        var speakerId = Guid.NewGuid();
        var expectedTalks = new List<TalkReadModel>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Talk 1",
                SpeakerId = speakerId.ToString(),
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Talk 2",
                SpeakerId = speakerId.ToString(),
            },
        };

        var mockCursor = Substitute.For<IAsyncCursor<TalkReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(expectedTalks);

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<TalkReadModel>>(),
                Arg.Any<FindOptions<TalkReadModel, TalkReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetBySpeakerId(speakerId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, talk => Assert.Equal(speakerId.ToString(), talk.SpeakerId));
    }

    [Fact]
    public async Task GetBySpeakerId_NoTalks_ReturnsEmptyList()
    {
        // Arrange
        var speakerId = Guid.NewGuid();

        var mockCursor = Substitute.For<IAsyncCursor<TalkReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(Array.Empty<TalkReadModel>());

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<TalkReadModel>>(),
                Arg.Any<FindOptions<TalkReadModel, TalkReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetBySpeakerId(speakerId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Save_NewTalkReadModel_InsertsIntoCollection()
    {
        // Arrange
        var talkReadModel = new TalkReadModel
        {
            Id = Guid.NewGuid().ToString(),
            Title = "New Talk",
            Abstract = "New Abstract",
            SpeakerId = Guid.NewGuid().ToString(),
            TalkTypeId = Guid.NewGuid().ToString(),
            ConferenceId = Guid.NewGuid().ToString(),
            Tags = new List<string> { "dotnet" },
            Status = "Submitted",
            Version = 1,
        };

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

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
        var talkReadModel = new TalkReadModel
        {
            Id = Guid.NewGuid().ToString(),
            Title = "Updated Talk",
            Abstract = "Updated Abstract",
            SpeakerId = Guid.NewGuid().ToString(),
            TalkTypeId = Guid.NewGuid().ToString(),
            ConferenceId = Guid.NewGuid().ToString(),
            Tags = new List<string> { "dotnet", "testing" },
            Status = "Submitted",
            Version = 2,
        };

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

        // Act
        await repository.Update(talkReadModel);

        // Assert
        await _mockCollection
            .Received(1)
            .ReplaceOneAsync(
                Arg.Any<FilterDefinition<TalkReadModel>>(),
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

        var repository = new MongoDbTalkReadModelRepository(_mockDatabase);

        // Act
        await repository.Delete(talkId);

        // Assert
        await _mockCollection
            .Received(1)
            .DeleteOneAsync(
                Arg.Any<FilterDefinition<TalkReadModel>>(),
                Arg.Any<CancellationToken>()
            );
    }
}
