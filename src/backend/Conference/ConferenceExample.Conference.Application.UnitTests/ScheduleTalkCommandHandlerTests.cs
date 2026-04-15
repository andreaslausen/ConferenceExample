using ConferenceExample.Authentication;
using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Application.ScheduleTalk;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using AuthGuidV7 = ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids.GuidV7;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;
using ConferenceGuidV7 = ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Conference.Application.UnitTests;

public class ScheduleTalkCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_SchedulesTalk()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateConferenceWithTalk();
        var currentUserService = CreateMockCurrentUserService(conference.OrganizerId);
        var handler = new ScheduleTalkCommandHandler(repository, currentUserService);
        var talkId = conference.Talks[0].Id.Value;
        var start = DateTimeOffset.UtcNow.AddDays(30);
        var end = start.AddHours(1);
        var command = new ScheduleTalkCommand(conference.Id.Value, talkId, start, end);

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        // Act
        await handler.Handle(command);

        // Assert
        await repository
            .Received(1)
            .GetById(
                Arg.Is<ConferenceId>(id => id.Value == (ConferenceGuidV7)command.ConferenceId)
            );
        await repository
            .Received(1)
            .Save(
                Arg.Is<ConferenceAggregate>(c =>
                    c.Talks[0].Slot != null
                    && c.Talks[0].Slot!.Start == start
                    && c.Talks[0].Slot!.End == end
                )
            );
    }

    [Fact]
    public async Task Handle_ConferenceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = CreateMockCurrentUserService();
        var handler = new ScheduleTalkCommandHandler(repository, currentUserService);
        var start = DateTimeOffset.UtcNow.AddDays(30);
        var end = start.AddHours(1);
        var command = new ScheduleTalkCommand(
            ConferenceGuidV7.NewGuid(),
            ConferenceGuidV7.NewGuid(),
            start,
            end
        );

        repository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_InvalidConferenceGuidV7_ThrowsArgumentException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = CreateMockCurrentUserService();
        var handler = new ScheduleTalkCommandHandler(repository, currentUserService);
        var invalidGuid = Guid.NewGuid(); // Not a GuidV7
        var start = DateTimeOffset.UtcNow.AddDays(30);
        var end = start.AddHours(1);
        var command = new ScheduleTalkCommand(invalidGuid, ConferenceGuidV7.NewGuid(), start, end);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateConferenceWithTalk();
        var differentUserId = ConferenceGuidV7.NewGuid();
        var currentUserService = CreateMockCurrentUserService(new OrganizerId(differentUserId));
        var handler = new ScheduleTalkCommandHandler(repository, currentUserService);
        var talkId = conference.Talks[0].Id.Value;
        var start = DateTimeOffset.UtcNow.AddDays(30);
        var end = start.AddHours(1);
        var command = new ScheduleTalkCommand(conference.Id.Value, talkId, start, end);

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateConferenceWithTalk();
        var currentUserService = CreateMockCurrentUserService(conference.OrganizerId);
        var handler = new ScheduleTalkCommandHandler(repository, currentUserService);
        var talkId = conference.Talks[0].Id.Value;
        var start = DateTimeOffset.UtcNow.AddDays(30);
        var end = start.AddHours(1);
        var command = new ScheduleTalkCommand(conference.Id.Value, talkId, start, end);

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);
        repository
            .Save(Arg.Any<ConferenceAggregate>())
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
    }

    private static ICurrentUserService CreateMockCurrentUserService(OrganizerId? organizerId = null)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        var userId = organizerId?.Value.Value ?? ConferenceGuidV7.NewGuid();
        currentUserService.GetCurrentUserId().Returns(new UserId(new AuthGuidV7(userId)));
        return currentUserService;
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

    private static ConferenceAggregate CreateConferenceWithTalk()
    {
        var conference = CreateValidConference();
        var talkId = new TalkId(ConferenceGuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        return conference;
    }
}
