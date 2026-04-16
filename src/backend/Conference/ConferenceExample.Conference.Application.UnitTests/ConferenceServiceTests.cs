using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.GetConferenceTalkTypes;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RemoveTalkType;
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
        var service = CreateConferenceService(createCommandHandler: createCommandHandler);

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
        var service = CreateConferenceService(createCommandHandler: createCommandHandler);

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
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var service = CreateConferenceService(renameCommandHandler: renameCommandHandler);

        // Act
        await service.RenameConference(
            Guid.CreateVersion7(),
            new RenameConferenceDto { Name = "New Name" }
        );

        // Assert
        await renameCommandHandler.Received(1).Handle(Arg.Any<RenameConferenceCommand>());
    }

    [Fact]
    public async Task RenameConference_HandlerThrowsException_PropagatesException()
    {
        // Arrange
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var service = CreateConferenceService(renameCommandHandler: renameCommandHandler);

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
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var service = CreateConferenceService(queryHandler: queryHandler);

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
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var service = CreateConferenceService(queryHandler: queryHandler);

        queryHandler
            .Handle(Arg.Any<GetConferenceSessionsQuery>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.GetSessions(Guid.NewGuid())
        );
    }

    [Fact]
    public async Task ChangeConferenceStatus_ValidDto_CallsCommandHandler()
    {
        // Arrange
        var changeStatusHandler = Substitute.For<IChangeConferenceStatusCommandHandler>();
        var service = CreateConferenceService(
            changeConferenceStatusCommandHandler: changeStatusHandler
        );

        // Act
        await service.ChangeConferenceStatus(
            Guid.NewGuid(),
            new ChangeConferenceStatusDto
            {
                Status = Domain.ConferenceManagement.ConferenceStatus.CallForSpeakers,
            }
        );

        // Assert
        await changeStatusHandler.Received(1).Handle(Arg.Any<ChangeConferenceStatusCommand>());
    }

    [Fact]
    public async Task GetAllConferences_CallsQueryHandler()
    {
        // Arrange
        var getAllHandler = Substitute.For<IGetAllConferencesQueryHandler>();
        var service = CreateConferenceService(getAllConferencesQueryHandler: getAllHandler);

        // Act
        await service.GetAllConferences();

        // Assert
        await getAllHandler.Received(1).Handle(Arg.Any<GetAllConferencesQuery>());
    }

    [Fact]
    public async Task GetConferenceById_ValidId_CallsQueryHandler()
    {
        // Arrange
        var getByIdHandler = Substitute.For<IGetConferenceByIdQueryHandler>();
        var service = CreateConferenceService(getConferenceByIdQueryHandler: getByIdHandler);
        var conferenceId = Guid.NewGuid();

        // Act
        await service.GetConferenceById(conferenceId);

        // Assert
        await getByIdHandler.Received(1).Handle(Arg.Any<GetConferenceByIdQuery>());
    }

    [Fact]
    public async Task GetConferenceTalks_ValidId_CallsQueryHandler()
    {
        // Arrange
        var getTalksHandler = Substitute.For<IGetConferenceTalksQueryHandler>();
        var service = CreateConferenceService(getConferenceTalksQueryHandler: getTalksHandler);
        var conferenceId = Guid.NewGuid();

        // Act
        await service.GetConferenceTalks(conferenceId);

        // Assert
        await getTalksHandler.Received(1).Handle(Arg.Any<GetConferenceTalksQuery>());
    }

    [Fact]
    public async Task AcceptTalk_ValidIds_CallsCommandHandler()
    {
        // Arrange
        var acceptHandler = Substitute.For<IAcceptTalkCommandHandler>();
        var service = CreateConferenceService(acceptTalkCommandHandler: acceptHandler);
        var conferenceId = Guid.NewGuid();
        var talkId = Guid.NewGuid();

        // Act
        await service.AcceptTalk(conferenceId, talkId);

        // Assert
        await acceptHandler.Received(1).Handle(Arg.Any<AcceptTalkCommand>());
    }

    [Fact]
    public async Task RejectTalk_ValidIds_CallsCommandHandler()
    {
        // Arrange
        var rejectHandler = Substitute.For<IRejectTalkCommandHandler>();
        var service = CreateConferenceService(rejectTalkCommandHandler: rejectHandler);
        var conferenceId = Guid.NewGuid();
        var talkId = Guid.NewGuid();

        // Act
        await service.RejectTalk(conferenceId, talkId);

        // Assert
        await rejectHandler.Received(1).Handle(Arg.Any<RejectTalkCommand>());
    }

    [Fact]
    public async Task ScheduleTalk_ValidDto_CallsCommandHandler()
    {
        // Arrange
        var scheduleHandler = Substitute.For<IScheduleTalkCommandHandler>();
        var service = CreateConferenceService(scheduleTalkCommandHandler: scheduleHandler);
        var conferenceId = Guid.NewGuid();
        var talkId = Guid.NewGuid();
        var dto = new ScheduleTalkDto
        {
            Start = DateTimeOffset.UtcNow,
            End = DateTimeOffset.UtcNow.AddHours(1),
        };

        // Act
        await service.ScheduleTalk(conferenceId, talkId, dto);

        // Assert
        await scheduleHandler.Received(1).Handle(Arg.Any<ScheduleTalkCommand>());
    }

    [Fact]
    public async Task AssignTalkToRoom_ValidDto_CallsCommandHandler()
    {
        // Arrange
        var assignHandler = Substitute.For<IAssignTalkToRoomCommandHandler>();
        var service = CreateConferenceService(assignTalkToRoomCommandHandler: assignHandler);
        var conferenceId = Guid.NewGuid();
        var talkId = Guid.NewGuid();
        var dto = new AssignTalkToRoomDto { RoomId = Guid.NewGuid(), RoomName = "Room A" };

        // Act
        await service.AssignTalkToRoom(conferenceId, talkId, dto);

        // Assert
        await assignHandler.Received(1).Handle(Arg.Any<AssignTalkToRoomCommand>());
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

    private static ConferenceService CreateConferenceService(
        ICreateConferenceCommandHandler? createCommandHandler = null,
        IRenameConferenceCommandHandler? renameCommandHandler = null,
        IChangeConferenceStatusCommandHandler? changeConferenceStatusCommandHandler = null,
        IGetAllConferencesQueryHandler? getAllConferencesQueryHandler = null,
        IGetConferenceByIdQueryHandler? getConferenceByIdQueryHandler = null,
        IGetConferenceSessionsQueryHandler? queryHandler = null,
        IGetConferenceTalksQueryHandler? getConferenceTalksQueryHandler = null,
        IAcceptTalkCommandHandler? acceptTalkCommandHandler = null,
        IRejectTalkCommandHandler? rejectTalkCommandHandler = null,
        IScheduleTalkCommandHandler? scheduleTalkCommandHandler = null,
        IAssignTalkToRoomCommandHandler? assignTalkToRoomCommandHandler = null,
        IDefineTalkTypeCommandHandler? defineTalkTypeCommandHandler = null,
        IRemoveTalkTypeCommandHandler? removeTalkTypeCommandHandler = null,
        IGetConferenceTalkTypesQueryHandler? getConferenceTalkTypesQueryHandler = null
    )
    {
        return new ConferenceService(
            createCommandHandler ?? Substitute.For<ICreateConferenceCommandHandler>(),
            renameCommandHandler ?? Substitute.For<IRenameConferenceCommandHandler>(),
            changeConferenceStatusCommandHandler
                ?? Substitute.For<IChangeConferenceStatusCommandHandler>(),
            getAllConferencesQueryHandler ?? Substitute.For<IGetAllConferencesQueryHandler>(),
            getConferenceByIdQueryHandler ?? Substitute.For<IGetConferenceByIdQueryHandler>(),
            queryHandler ?? Substitute.For<IGetConferenceSessionsQueryHandler>(),
            getConferenceTalksQueryHandler ?? Substitute.For<IGetConferenceTalksQueryHandler>(),
            acceptTalkCommandHandler ?? Substitute.For<IAcceptTalkCommandHandler>(),
            rejectTalkCommandHandler ?? Substitute.For<IRejectTalkCommandHandler>(),
            scheduleTalkCommandHandler ?? Substitute.For<IScheduleTalkCommandHandler>(),
            assignTalkToRoomCommandHandler ?? Substitute.For<IAssignTalkToRoomCommandHandler>(),
            defineTalkTypeCommandHandler ?? Substitute.For<IDefineTalkTypeCommandHandler>(),
            removeTalkTypeCommandHandler ?? Substitute.For<IRemoveTalkTypeCommandHandler>(),
            getConferenceTalkTypesQueryHandler
                ?? Substitute.For<IGetConferenceTalkTypesQueryHandler>()
        );
    }
}
