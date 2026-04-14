using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace ConferenceExample.Conference.Application.UnitTests;

public class ConferenceServiceTests
{
    [Fact]
    public async Task CreateConference_ValidDto_CallsCommandHandler()
    {
        // Arrange
        var createCommandHandler = Substitute.For<ICreateConferenceCommandHandler>();
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var acceptTalkCommandHandler = Substitute.For<IAcceptTalkCommandHandler>();
        var rejectTalkCommandHandler = Substitute.For<IRejectTalkCommandHandler>();
        var scheduleTalkCommandHandler = Substitute.For<IScheduleTalkCommandHandler>();
        var assignTalkToRoomCommandHandler = Substitute.For<IAssignTalkToRoomCommandHandler>();
        var service = new ConferenceService(
            createCommandHandler,
            renameCommandHandler,
            queryHandler,
            acceptTalkCommandHandler,
            rejectTalkCommandHandler,
            scheduleTalkCommandHandler,
            assignTalkToRoomCommandHandler
        );

        // Act
        await service.CreateConference(CreateDto());

        // Assert
        await createCommandHandler.Received(1).Handle(Arg.Any<CreateConferenceCommand>());
    }

    [Fact]
    public async Task CreateConference_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        var createCommandHandler = Substitute.For<ICreateConferenceCommandHandler>();
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var acceptTalkCommandHandler = Substitute.For<IAcceptTalkCommandHandler>();
        var rejectTalkCommandHandler = Substitute.For<IRejectTalkCommandHandler>();
        var scheduleTalkCommandHandler = Substitute.For<IScheduleTalkCommandHandler>();
        var assignTalkToRoomCommandHandler = Substitute.For<IAssignTalkToRoomCommandHandler>();
        var service = new ConferenceService(
            createCommandHandler,
            renameCommandHandler,
            queryHandler,
            acceptTalkCommandHandler,
            rejectTalkCommandHandler,
            scheduleTalkCommandHandler,
            assignTalkToRoomCommandHandler
        );

        createCommandHandler
            .Handle(Arg.Any<CreateConferenceCommand>())
            .Throws(new InvalidOperationException("Repository error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateConference(CreateDto())
        );
    }

    [Fact]
    public async Task RenameConference_ValidDto_CallsCommandHandler()
    {
        // Arrange
        var createCommandHandler = Substitute.For<ICreateConferenceCommandHandler>();
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var acceptTalkCommandHandler = Substitute.For<IAcceptTalkCommandHandler>();
        var rejectTalkCommandHandler = Substitute.For<IRejectTalkCommandHandler>();
        var scheduleTalkCommandHandler = Substitute.For<IScheduleTalkCommandHandler>();
        var assignTalkToRoomCommandHandler = Substitute.For<IAssignTalkToRoomCommandHandler>();
        var service = new ConferenceService(
            createCommandHandler,
            renameCommandHandler,
            queryHandler,
            acceptTalkCommandHandler,
            rejectTalkCommandHandler,
            scheduleTalkCommandHandler,
            assignTalkToRoomCommandHandler
        );

        // Act
        await service.RenameConference(
            Guid.NewGuid(),
            new RenameConferenceDto { Name = "New Name" }
        );

        // Assert
        await renameCommandHandler.Received(1).Handle(Arg.Any<RenameConferenceCommand>());
    }

    [Fact]
    public async Task RenameConference_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        var createCommandHandler = Substitute.For<ICreateConferenceCommandHandler>();
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var acceptTalkCommandHandler = Substitute.For<IAcceptTalkCommandHandler>();
        var rejectTalkCommandHandler = Substitute.For<IRejectTalkCommandHandler>();
        var scheduleTalkCommandHandler = Substitute.For<IScheduleTalkCommandHandler>();
        var assignTalkToRoomCommandHandler = Substitute.For<IAssignTalkToRoomCommandHandler>();
        var service = new ConferenceService(
            createCommandHandler,
            renameCommandHandler,
            queryHandler,
            acceptTalkCommandHandler,
            rejectTalkCommandHandler,
            scheduleTalkCommandHandler,
            assignTalkToRoomCommandHandler
        );

        renameCommandHandler
            .Handle(Arg.Any<RenameConferenceCommand>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RenameConference(Guid.NewGuid(), new RenameConferenceDto { Name = "New Name" })
        );
    }

    [Fact]
    public async Task GetSessions_ValidConferenceId_CallsQueryHandler()
    {
        // Arrange
        var createCommandHandler = Substitute.For<ICreateConferenceCommandHandler>();
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var acceptTalkCommandHandler = Substitute.For<IAcceptTalkCommandHandler>();
        var rejectTalkCommandHandler = Substitute.For<IRejectTalkCommandHandler>();
        var scheduleTalkCommandHandler = Substitute.For<IScheduleTalkCommandHandler>();
        var assignTalkToRoomCommandHandler = Substitute.For<IAssignTalkToRoomCommandHandler>();
        var service = new ConferenceService(
            createCommandHandler,
            renameCommandHandler,
            queryHandler,
            acceptTalkCommandHandler,
            rejectTalkCommandHandler,
            scheduleTalkCommandHandler,
            assignTalkToRoomCommandHandler
        );

        var conferenceId = Guid.NewGuid();
        var expectedSessions = new List<GetConferenceSessionDto>
        {
            new(
                Guid.NewGuid(),
                "Accepted",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddHours(1),
                Guid.NewGuid(),
                "Room A"
            ),
        };

        queryHandler.Handle(Arg.Any<GetConferenceSessionsQuery>()).Returns(expectedSessions);

        // Act
        var result = await service.GetSessions(conferenceId);

        // Assert
        await queryHandler
            .Received(1)
            .Handle(Arg.Is<GetConferenceSessionsQuery>(q => q.ConferenceId == conferenceId));
        Assert.Equal(expectedSessions, result);
    }

    [Fact]
    public async Task GetSessions_QueryHandlerThrowsException_PropagatesException()
    {
        // Arrange
        var createCommandHandler = Substitute.For<ICreateConferenceCommandHandler>();
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var acceptTalkCommandHandler = Substitute.For<IAcceptTalkCommandHandler>();
        var rejectTalkCommandHandler = Substitute.For<IRejectTalkCommandHandler>();
        var scheduleTalkCommandHandler = Substitute.For<IScheduleTalkCommandHandler>();
        var assignTalkToRoomCommandHandler = Substitute.For<IAssignTalkToRoomCommandHandler>();
        var service = new ConferenceService(
            createCommandHandler,
            renameCommandHandler,
            queryHandler,
            acceptTalkCommandHandler,
            rejectTalkCommandHandler,
            scheduleTalkCommandHandler,
            assignTalkToRoomCommandHandler
        );

        queryHandler
            .Handle(Arg.Any<GetConferenceSessionsQuery>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetSessions(Guid.NewGuid())
        );
    }

    private static CreateConferenceDto CreateDto() =>
        new()
        {
            Name = "DotNet Conf",
            Start = new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 9, 3, 18, 0, 0, TimeSpan.Zero),
            LocationName = "Convention Center",
            Street = "Main Street 1",
            City = "Berlin",
            State = "Berlin",
            PostalCode = "10115",
            Country = "Germany",
        };
}
