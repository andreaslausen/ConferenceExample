using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

namespace ConferenceExample.Conference.Application.UnitTests;

public class GetConferenceSessionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidQuery_ReturnsSessions()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new GetConferenceSessionsQueryHandler(repository);
        var conference = CreateConferenceWithTalks();
        var query = new GetConferenceSessionsQuery(conference.Id.Value);

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Submitted", result[0].Status);
    }

    [Fact]
    public async Task Handle_ConferenceWithNoTalks_ReturnsEmptyList()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new GetConferenceSessionsQueryHandler(repository);
        var conference = CreateValidConference();
        var query = new GetConferenceSessionsQuery(conference.Id.Value);

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        // Act
        var result = await handler.Handle(query);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_ConferenceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new GetConferenceSessionsQueryHandler(repository);
        var query = new GetConferenceSessionsQuery(GuidV7.NewGuid());

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
        var handler = new GetConferenceSessionsQueryHandler(repository);
        var invalidGuid = Guid.NewGuid(); // Not a GuidV7
        var query = new GetConferenceSessionsQuery(invalidGuid);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(query));
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new GetConferenceSessionsQueryHandler(repository);
        var query = new GetConferenceSessionsQuery(GuidV7.NewGuid());

        repository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(query));
    }

    private static ConferenceAggregate CreateValidConference()
    {
        var id = new ConferenceId(GuidV7.NewGuid());
        var name = new Text("Test Conference");
        var time = new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32));
        var location = new Location(
            new Text("Test Venue"),
            new Address("123 Main St", "Springfield", "IL", "62701", "US")
        );

        return ConferenceAggregate.Create(id, name, time, location);
    }

    private static ConferenceAggregate CreateConferenceWithTalks()
    {
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        return conference;
    }
}
