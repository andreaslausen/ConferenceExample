using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;
using ConferenceGuidV7 = ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Conference.Application.UnitTests;

public class GetAllConferencesQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithMultipleConferences_ReturnsAllConferences()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conferences = new List<ConferenceAggregate>
        {
            CreateValidConference("DotNet Conf", "Berlin", "Germany"),
            CreateValidConference("Java Summit", "Munich", "Germany"),
            CreateValidConference("Python Con", "Hamburg", "Germany"),
        };

        repository.GetAll().Returns(conferences);
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
        var repository = Substitute.For<IConferenceRepository>();
        repository.GetAll().Returns(new List<ConferenceAggregate>());
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
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateValidConference("DotNet Conf", "Berlin", "Germany");
        repository.GetAll().Returns(new List<ConferenceAggregate> { conference });
        var handler = new GetAllConferencesQueryHandler(repository);
        var query = new GetAllConferencesQuery();

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.Single(result);
        var dto = result[0];
        Assert.Equal(conference.Id.Value.Value, dto.Id);
        Assert.Equal("DotNet Conf", dto.Name);
        Assert.Equal(conference.ConferenceTime.Start, dto.StartDate);
        Assert.Equal(conference.ConferenceTime.End, dto.EndDate);
        Assert.Equal("Berlin", dto.City);
        Assert.Equal("Berlin", dto.State);
        Assert.Equal("10115", dto.PostalCode);
        Assert.Equal("Germany", dto.Country);
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
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
        var repository = Substitute.For<IConferenceRepository>();
        var conferences = new List<ConferenceAggregate>
        {
            CreateValidConference("Conf 1", "Berlin", "Germany"),
            CreateValidConference("Conf 2", "New York", "USA"),
            CreateValidConference("Conf 3", "Tokyo", "Japan"),
        };

        repository.GetAll().Returns(conferences);
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

    private static ConferenceAggregate CreateValidConference(
        string name,
        string city,
        string country
    )
    {
        var id = new ConferenceId(ConferenceGuidV7.NewGuid());
        var conferenceName = new Text(name);
        var time = new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32));
        var location = new Location(
            new Text("Test Venue"),
            new Address("123 Main St", city, city, "10115", country)
        );

        return ConferenceAggregate.Create(
            id,
            conferenceName,
            time,
            location,
            new OrganizerId(ConferenceGuidV7.NewGuid())
        );
    }
}
