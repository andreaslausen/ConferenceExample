using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ConferenceExample.Conference.Application.UnitTests;

public class GetAllConferencesQueryHandlerTests
{
    private static ConferenceReadModel CreateSummary(string name, string city, string country) =>
        new(
            Guid.CreateVersion7(),
            name,
            DateTimeOffset.UtcNow.AddDays(30),
            DateTimeOffset.UtcNow.AddDays(32),
            city,
            city,
            "10115",
            country,
            "Draft"
        );

    [Fact]
    public async Task Handle_WithMultipleConferences_ReturnsAllConferences()
    {
        // Arrange
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var summaries = new List<ConferenceReadModel>
        {
            CreateSummary("DotNet Conf", "Berlin", "Germany"),
            CreateSummary("Java Summit", "Munich", "Germany"),
            CreateSummary("Python Con", "Hamburg", "Germany"),
        };

        repository.GetAll().Returns(summaries);
        var handler = new GetAllConferencesQueryHandler(repository);
        var query = new GetAllConferencesQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, c => c.Name == "DotNet Conf");
        Assert.Contains(result, c => c.Name == "Java Summit");
        Assert.Contains(result, c => c.Name == "Python Con");
    }

    [Fact]
    public async Task Handle_WithNoConferences_ReturnsEmptyList()
    {
        // Arrange
        var repository = Substitute.For<IConferenceReadModelRepository>();
        repository.GetAll().Returns(new List<ConferenceReadModel>());
        var handler = new GetAllConferencesQueryHandler(repository);
        var query = new GetAllConferencesQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ValidQuery_ReturnsCorrectData()
    {
        // Arrange
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var summary = CreateSummary("DotNet Conf", "Berlin", "Germany");
        repository.GetAll().Returns(new List<ConferenceReadModel> { summary });
        var handler = new GetAllConferencesQueryHandler(repository);
        var query = new GetAllConferencesQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.Single(result);
        var dto = result[0];
        Assert.Equal(summary.Id, dto.Id);
        Assert.Equal("DotNet Conf", dto.Name);
        Assert.Equal(summary.Start, dto.StartDate);
        Assert.Equal(summary.End, dto.EndDate);
        Assert.Equal("Berlin", dto.City);
        Assert.Equal("Berlin", dto.State);
        Assert.Equal("10115", dto.PostalCode);
        Assert.Equal("Germany", dto.Country);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceReadModelRepository>();
        repository.GetAll().Throws(new InvalidOperationException("Database error"));
        var handler = new GetAllConferencesQueryHandler(repository);
        var query = new GetAllConferencesQuery();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_ConferencesWithDifferentLocations_ReturnsAllLocations()
    {
        // Arrange
        var repository = Substitute.For<IConferenceReadModelRepository>();
        var summaries = new List<ConferenceReadModel>
        {
            CreateSummary("Conf 1", "Berlin", "Germany"),
            CreateSummary("Conf 2", "New York", "USA"),
            CreateSummary("Conf 3", "Tokyo", "Japan"),
        };

        repository.GetAll().Returns(summaries);
        var handler = new GetAllConferencesQueryHandler(repository);
        var query = new GetAllConferencesQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Contains(result, c => c.City == "Berlin" && c.Country == "Germany");
        Assert.Contains(result, c => c.City == "New York" && c.Country == "USA");
        Assert.Contains(result, c => c.City == "Tokyo" && c.Country == "Japan");
    }
}
