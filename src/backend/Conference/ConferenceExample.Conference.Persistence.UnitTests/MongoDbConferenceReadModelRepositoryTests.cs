using ConferenceExample.Conference.Persistence.ReadModels;
using MongoDB.Driver;
using NSubstitute;

namespace ConferenceExample.Conference.Persistence.UnitTests;

public class MongoDbConferenceReadModelRepositoryTests
{
    private readonly IMongoDatabase _mockDatabase;
    private readonly IMongoCollection<ConferenceReadModel> _mockCollection;

    public MongoDbConferenceReadModelRepositoryTests()
    {
        _mockDatabase = Substitute.For<IMongoDatabase>();
        _mockCollection = Substitute.For<IMongoCollection<ConferenceReadModel>>();
        _mockDatabase
            .GetCollection<ConferenceReadModel>("conference_readmodels")
            .Returns(_mockCollection);
    }

    [Fact]
    public void Constructor_CreatesCollection()
    {
        // Act
        _ = new MongoDbConferenceReadModelRepository(_mockDatabase);

        // Assert
        _mockDatabase.Received(1).GetCollection<ConferenceReadModel>("conference_readmodels");
    }

    [Fact]
    public async Task GetById_ExistingConference_ReturnsConferenceReadModel()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();
        var expectedConference = new ConferenceReadModel
        {
            Id = conferenceId.ToString(),
            Name = "Test Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow.AddDays(2),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = Guid.NewGuid().ToString(),
            Status = "Draft",
            Version = 1,
        };

        var mockCursor = Substitute.For<IAsyncCursor<ConferenceReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(new[] { expectedConference });

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<ConferenceReadModel>>(),
                Arg.Any<FindOptions<ConferenceReadModel, ConferenceReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbConferenceReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetById(conferenceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedConference.Id, result.Id);
        Assert.Equal(expectedConference.Name, result.Name);
    }

    [Fact]
    public async Task GetById_NonExistingConference_ReturnsNull()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();

        var mockCursor = Substitute.For<IAsyncCursor<ConferenceReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(Array.Empty<ConferenceReadModel>());

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<ConferenceReadModel>>(),
                Arg.Any<FindOptions<ConferenceReadModel, ConferenceReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbConferenceReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetById(conferenceId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAll_ExistingConferences_ReturnsConferenceList()
    {
        // Arrange
        var expectedConferences = new List<ConferenceReadModel>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Conference 1",
                Status = "Draft",
                Version = 1,
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Conference 2",
                Status = "Published",
                Version = 1,
            },
        };

        var mockCursor = Substitute.For<IAsyncCursor<ConferenceReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(expectedConferences);

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<ConferenceReadModel>>(),
                Arg.Any<FindOptions<ConferenceReadModel, ConferenceReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbConferenceReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, c => c.Name == "Conference 1");
        Assert.Contains(result, c => c.Name == "Conference 2");
    }

    [Fact]
    public async Task GetAll_NoConferences_ReturnsEmptyList()
    {
        // Arrange
        var mockCursor = Substitute.For<IAsyncCursor<ConferenceReadModel>>();
        mockCursor.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCursor.Current.Returns(Array.Empty<ConferenceReadModel>());

        _mockCollection
            .FindAsync(
                Arg.Any<FilterDefinition<ConferenceReadModel>>(),
                Arg.Any<FindOptions<ConferenceReadModel, ConferenceReadModel>>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(mockCursor);

        var repository = new MongoDbConferenceReadModelRepository(_mockDatabase);

        // Act
        var result = await repository.GetAll();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Save_NewConferenceReadModel_InsertsIntoCollection()
    {
        // Arrange
        var conferenceReadModel = new ConferenceReadModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "New Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow.AddDays(2),
            LocationName = "Convention Center",
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "USA",
            OrganizerId = Guid.NewGuid().ToString(),
            Status = "Draft",
            Version = 1,
        };

        var repository = new MongoDbConferenceReadModelRepository(_mockDatabase);

        // Act
        await repository.Save(conferenceReadModel);

        // Assert
        await _mockCollection
            .Received(1)
            .InsertOneAsync(
                conferenceReadModel,
                Arg.Any<InsertOneOptions>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Update_ExistingConferenceReadModel_ReplacesInCollection()
    {
        // Arrange
        var conferenceReadModel = new ConferenceReadModel
        {
            Id = Guid.NewGuid().ToString(),
            Name = "Updated Conference",
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow.AddDays(3),
            LocationName = "New Location",
            Street = "456 Oak St",
            City = "Metropolis",
            State = "NY",
            PostalCode = "10001",
            Country = "USA",
            OrganizerId = Guid.NewGuid().ToString(),
            Status = "Published",
            Version = 2,
        };

        var repository = new MongoDbConferenceReadModelRepository(_mockDatabase);

        // Act
        await repository.Update(conferenceReadModel);

        // Assert
        await _mockCollection
            .Received(1)
            .ReplaceOneAsync(
                Arg.Any<FilterDefinition<ConferenceReadModel>>(),
                conferenceReadModel,
                Arg.Any<ReplaceOptions>(),
                Arg.Any<CancellationToken>()
            );
    }

    [Fact]
    public async Task Delete_ExistingConference_DeletesFromCollection()
    {
        // Arrange
        var conferenceId = Guid.NewGuid();

        var repository = new MongoDbConferenceReadModelRepository(_mockDatabase);

        // Act
        await repository.Delete(conferenceId);

        // Assert
        await _mockCollection
            .Received(1)
            .DeleteOneAsync(
                Arg.Any<FilterDefinition<ConferenceReadModel>>(),
                Arg.Any<CancellationToken>()
            );
    }
}
