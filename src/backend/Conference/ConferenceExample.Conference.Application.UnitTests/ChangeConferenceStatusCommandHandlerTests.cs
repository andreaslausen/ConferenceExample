using ConferenceExample.Authentication;
using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using AuthGuidV7 = ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids.GuidV7;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;
using ConferenceGuidV7 = ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Conference.Application.UnitTests;

public class ChangeConferenceStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_ChangesConferenceStatus()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateValidConference();
        var currentUserService = CreateMockCurrentUserService(conference.OrganizerId);
        var handler = new ChangeConferenceStatusCommandHandler(repository, currentUserService);
        var command = new ChangeConferenceStatusCommand(
            conference.Id.Value,
            ConferenceStatus.CallForSpeakers
        );

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
            .Save(Arg.Is<ConferenceAggregate>(c => c.Status == ConferenceStatus.CallForSpeakers));
    }

    [Fact]
    public async Task Handle_MultipleStatusChanges_UpdatesStatusCorrectly()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateValidConference();
        var currentUserService = CreateMockCurrentUserService(conference.OrganizerId);
        var handler = new ChangeConferenceStatusCommandHandler(repository, currentUserService);

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        // Act
        await handler.Handle(
            new ChangeConferenceStatusCommand(conference.Id.Value, ConferenceStatus.CallForSpeakers)
        );
        await handler.Handle(
            new ChangeConferenceStatusCommand(
                conference.Id.Value,
                ConferenceStatus.CallForSpeakersClosed
            )
        );
        await handler.Handle(
            new ChangeConferenceStatusCommand(
                conference.Id.Value,
                ConferenceStatus.ProgramPublished
            )
        );

        // Assert
        await repository.Received(3).Save(Arg.Any<ConferenceAggregate>());
        Assert.Equal(ConferenceStatus.ProgramPublished, conference.Status);
    }

    [Fact]
    public async Task Handle_ConferenceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = CreateMockCurrentUserService();
        var handler = new ChangeConferenceStatusCommandHandler(repository, currentUserService);
        var command = new ChangeConferenceStatusCommand(
            ConferenceGuidV7.NewGuid(),
            ConferenceStatus.CallForSpeakers
        );

        repository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_InvalidGuidV7_ThrowsArgumentException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = CreateMockCurrentUserService();
        var handler = new ChangeConferenceStatusCommandHandler(repository, currentUserService);
        var invalidGuid = Guid.NewGuid(); // Not a GuidV7
        var command = new ChangeConferenceStatusCommand(
            invalidGuid,
            ConferenceStatus.CallForSpeakers
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateValidConference();
        var differentUserId = ConferenceGuidV7.NewGuid();
        var currentUserService = CreateMockCurrentUserService(new OrganizerId(differentUserId));
        var handler = new ChangeConferenceStatusCommandHandler(repository, currentUserService);
        var command = new ChangeConferenceStatusCommand(
            conference.Id.Value,
            ConferenceStatus.CallForSpeakers
        );

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var conference = CreateValidConference();
        var currentUserService = CreateMockCurrentUserService(conference.OrganizerId);
        var handler = new ChangeConferenceStatusCommandHandler(repository, currentUserService);
        var command = new ChangeConferenceStatusCommand(
            conference.Id.Value,
            ConferenceStatus.CallForSpeakers
        );

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
}
