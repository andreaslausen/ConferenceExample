using ConferenceExample.Conference.Application.AcceptTalk;
using ConferenceExample.Conference.Application.AddRoom;
using ConferenceExample.Conference.Application.AssignTalkToRoom;
using ConferenceExample.Conference.Application.ChangeConferenceStatus;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.DefineTalkType;
using ConferenceExample.Conference.Application.GetAllConferences;
using ConferenceExample.Conference.Application.GetConferenceById;
using ConferenceExample.Conference.Application.GetConferenceProgram;
using ConferenceExample.Conference.Application.GetConferenceRooms;
using ConferenceExample.Conference.Application.GetConferenceSchedule;
using ConferenceExample.Conference.Application.GetConferenceTalks;
using ConferenceExample.Conference.Application.GetConferenceTalkTypes;
using ConferenceExample.Conference.Application.RejectTalk;
using ConferenceExample.Conference.Application.RemoveRoom;
using ConferenceExample.Conference.Application.RemoveTalkType;
using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Application.ScheduleTalk;
using ConferenceExample.Conference.Application.UpdateConferenceDetails;

namespace ConferenceExample.Conference.Application;

public class ConferenceService(
    ICreateConferenceCommandHandler createConferenceCommandHandler,
    IRenameConferenceCommandHandler renameConferenceCommandHandler,
    IUpdateConferenceDetailsCommandHandler updateConferenceDetailsCommandHandler,
    IChangeConferenceStatusCommandHandler changeConferenceStatusCommandHandler,
    IGetAllConferencesQueryHandler getAllConferencesQueryHandler,
    IGetConferenceByIdQueryHandler getConferenceByIdQueryHandler,
    IGetConferenceScheduleQueryHandler getConferenceScheduleQueryHandler,
    IGetConferenceTalksQueryHandler getConferenceTalksQueryHandler,
    IAcceptTalkCommandHandler acceptTalkCommandHandler,
    IRejectTalkCommandHandler rejectTalkCommandHandler,
    IScheduleTalkCommandHandler scheduleTalkCommandHandler,
    IAssignTalkToRoomCommandHandler assignTalkToRoomCommandHandler,
    IDefineTalkTypeCommandHandler defineTalkTypeCommandHandler,
    IRemoveTalkTypeCommandHandler removeTalkTypeCommandHandler,
    IGetConferenceTalkTypesQueryHandler getConferenceTalkTypesQueryHandler,
    IGetConferenceProgramQueryHandler getConferenceProgramQueryHandler,
    IAddRoomCommandHandler addRoomCommandHandler,
    IRemoveRoomCommandHandler removeRoomCommandHandler,
    IGetConferenceRoomsQueryHandler getConferenceRoomsQueryHandler
) : IConferenceService
{
    public async Task<ConferenceCreatedDto> CreateConference(CreateConferenceDto dto)
    {
        var command = new CreateConferenceCommand(
            dto.Name,
            dto.Start,
            dto.End,
            dto.LocationName,
            dto.Street,
            dto.City,
            dto.State,
            dto.PostalCode,
            dto.Country
        );

        return await createConferenceCommandHandler.Handle(command);
    }

    public async Task RenameConference(Guid id, RenameConferenceDto dto)
    {
        var command = new RenameConferenceCommand(id, dto.Name);
        await renameConferenceCommandHandler.Handle(command);
    }

    public async Task UpdateConferenceDetails(Guid id, UpdateConferenceDetailsDto dto)
    {
        var command = new UpdateConferenceDetailsCommand(
            id,
            dto.Name,
            dto.Start,
            dto.End,
            dto.LocationName,
            dto.Street,
            dto.City,
            dto.State,
            dto.PostalCode,
            dto.Country
        );
        await updateConferenceDetailsCommandHandler.Handle(command);
    }

    public async Task ChangeConferenceStatus(Guid id, ChangeConferenceStatusDto dto)
    {
        var command = new ChangeConferenceStatusCommand(id, dto.Status);
        await changeConferenceStatusCommandHandler.Handle(command);
    }

    public async Task<IReadOnlyList<GetAllConferencesDto>> GetAllConferences()
    {
        var query = new GetAllConferencesQuery();
        return await getAllConferencesQueryHandler.Handle(query);
    }

    public async Task<GetConferenceByIdDto> GetConferenceById(Guid conferenceId)
    {
        var query = new GetConferenceByIdQuery(conferenceId);
        return await getConferenceByIdQueryHandler.Handle(query);
    }

    public async Task<IReadOnlyList<GetConferenceScheduleDto>> GetConferenceSchedule(
        Guid conferenceId
    )
    {
        var query = new GetConferenceScheduleQuery(conferenceId);
        return await getConferenceScheduleQueryHandler.Handle(query);
    }

    public async Task<IReadOnlyList<GetConferenceTalksDto>> GetConferenceTalks(Guid conferenceId)
    {
        var query = new GetConferenceTalksQuery(conferenceId);
        return await getConferenceTalksQueryHandler.Handle(query);
    }

    public async Task AcceptTalk(Guid conferenceId, Guid talkId)
    {
        var command = new AcceptTalkCommand(conferenceId, talkId);
        await acceptTalkCommandHandler.Handle(command);
    }

    public async Task RejectTalk(Guid conferenceId, Guid talkId)
    {
        var command = new RejectTalkCommand(conferenceId, talkId);
        await rejectTalkCommandHandler.Handle(command);
    }

    public async Task ScheduleTalk(Guid conferenceId, Guid talkId, ScheduleTalkDto dto)
    {
        var command = new ScheduleTalkCommand(conferenceId, talkId, dto.Start, dto.End);
        await scheduleTalkCommandHandler.Handle(command);
    }

    public async Task AssignTalkToRoom(Guid conferenceId, Guid talkId, AssignTalkToRoomDto dto)
    {
        var command = new AssignTalkToRoomCommand(conferenceId, talkId, dto.RoomId);
        await assignTalkToRoomCommandHandler.Handle(command);
    }

    public async Task<TalkTypeDefinedDto> DefineTalkType(Guid conferenceId, DefineTalkTypeDto dto)
    {
        var command = new DefineTalkTypeCommand(conferenceId, dto.Name, dto.DurationInMinutes);
        return await defineTalkTypeCommandHandler.Handle(command);
    }

    public async Task RemoveTalkType(Guid conferenceId, Guid talkTypeId)
    {
        var command = new RemoveTalkTypeCommand(conferenceId, talkTypeId);
        await removeTalkTypeCommandHandler.Handle(command);
    }

    public async Task<IReadOnlyList<GetConferenceTalkTypesDto>> GetConferenceTalkTypes(
        Guid conferenceId
    )
    {
        var query = new GetConferenceTalkTypesQuery(conferenceId);
        return await getConferenceTalkTypesQueryHandler.Handle(query);
    }

    public async Task<IReadOnlyList<GetConferenceProgramDto>> GetConferenceProgram(
        Guid conferenceId
    )
    {
        var query = new GetConferenceProgramQuery(conferenceId);
        return await getConferenceProgramQueryHandler.Handle(query);
    }

    public async Task<RoomAddedDto> AddRoom(Guid conferenceId, AddRoomDto dto)
    {
        var command = new AddRoomCommand(conferenceId, dto.Name);
        return await addRoomCommandHandler.Handle(command);
    }

    public async Task RemoveRoom(Guid conferenceId, Guid roomId)
    {
        var command = new RemoveRoomCommand(conferenceId, roomId);
        await removeRoomCommandHandler.Handle(command);
    }

    public async Task<IReadOnlyList<GetConferenceRoomsDto>> GetConferenceRooms(Guid conferenceId)
    {
        var query = new GetConferenceRoomsQuery(conferenceId);
        return await getConferenceRoomsQueryHandler.Handle(query);
    }
}
