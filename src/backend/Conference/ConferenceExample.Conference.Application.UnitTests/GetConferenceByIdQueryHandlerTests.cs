using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;
using ConferenceGuidV7 = ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Conference.Application.UnitTests;

public class GetConferenceByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_ReturnsConference()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateValidConference();
        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new GetConferenceByIdQueryHandler(repository);
        var query = new GetConferenceByIdQuery(conference.Id.Value);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(conference.Id.Value.Value, result.Id);
        Assert.Equal(conference.Name.Value, result.Name);
        Assert.Equal(conference.ConferenceTime.Start, result.StartDate);
        Assert.Equal(conference.ConferenceTime.End, result.EndDate);
        Assert.Equal(conference.Location.Name.Value, result.LocationName);
        Assert.Equal(conference.Location.Address.Street, result.Street);
        Assert.Equal(conference.Location.Address.City, result.City);
        Assert.Equal(conference.Location.Address.State, result.State);
        Assert.Equal(conference.Location.Address.PostalCode, result.PostalCode);
        Assert.Equal(conference.Location.Address.Country, result.Country);
        Assert.Equal(conference.OrganizerId.Value.Value, result.OrganizerId);
    }

    [Fact]
    public async Task Handle_ConferenceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new GetConferenceByIdQueryHandler(repository);
        var query = new GetConferenceByIdQuery(ConferenceGuidV7.NewGuid());

        repository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_InvalidGuidV7_ThrowsArgumentException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new GetConferenceByIdQueryHandler(repository);
        var invalidGuid = Guid.NewGuid(); // Not a GuidV7
        var query = new GetConferenceByIdQuery(invalidGuid);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new GetConferenceByIdQueryHandler(repository);
        var query = new GetConferenceByIdQuery(ConferenceGuidV7.NewGuid());

        repository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_ValidQuery_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateValidConference();
        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new GetConferenceByIdQueryHandler(repository);
        var query = new GetConferenceByIdQuery(conference.Id.Value);

        // Act
        await handler.Handle(query);

        // Assert
        await repository
            .Received(1)
            .GetById(Arg.Is<ConferenceId>(id => id.Value == (ConferenceGuidV7)query.ConferenceId));
    }

    [Fact]
    public async Task Handle_ConferenceWithAllFields_MapsAllFieldsCorrectly()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var organizerId = ConferenceGuidV7.NewGuid();
        var id = new ConferenceId(ConferenceGuidV7.NewGuid());
        var name = new Text("DotNet Conf 2026");
        var start = new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero);
        var end = new DateTimeOffset(2026, 9, 3, 18, 0, 0, TimeSpan.Zero);
        var time = new Time(start, end);
        var location = new Location(
            new Text("Berlin Convention Center"),
            new Address("Messedamm 22", "Berlin", "Berlin", "14055", "Germany")
        );
        var conference = ConferenceAggregate.Create(
            id,
            name,
            time,
            location,
            new OrganizerId(organizerId)
        );

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        var handler = new GetConferenceByIdQueryHandler(repository);
        var query = new GetConferenceByIdQuery(conference.Id.Value);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.Equal(id.Value.Value, result.Id);
        Assert.Equal("DotNet Conf 2026", result.Name);
        Assert.Equal(start, result.StartDate);
        Assert.Equal(end, result.EndDate);
        Assert.Equal("Berlin Convention Center", result.LocationName);
        Assert.Equal("Messedamm 22", result.Street);
        Assert.Equal("Berlin", result.City);
        Assert.Equal("Berlin", result.State);
        Assert.Equal("14055", result.PostalCode);
        Assert.Equal("Germany", result.Country);
        Assert.Equal((Guid)organizerId, result.OrganizerId);
    }

    private static ConferenceAggregate CreateValidConference()
    {
        var id = new ConferenceId(ConferenceGuidV7.NewGuid());
        var name = new Text("Test Conference");
        var time = new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32));
        var location = new Location(
            new Text("Test Venue"),
            new Address("123 Main St", "Springfield", "IL", "62701", "US")
        );

        return ConferenceAggregate.Create(
            id,
            name,
            time,
            location,
            new OrganizerId(ConferenceGuidV7.NewGuid())
        );
    }
}
