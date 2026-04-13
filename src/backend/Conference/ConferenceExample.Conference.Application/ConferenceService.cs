using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Application.GetConferenceSessions;
using ConferenceExample.Conference.Application.RenameConference;

namespace ConferenceExample.Conference.Application;

public class ConferenceService(
    ICreateConferenceCommandHandler createConferenceCommandHandler,
    IRenameConferenceCommandHandler renameConferenceCommandHandler,
    IGetConferenceSessionsQueryHandler getConferenceSessionsQueryHandler
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

    public async Task<IReadOnlyList<SessionDto>> GetSessions(Guid conferenceId)
    {
        var query = new GetConferenceSessionsQuery(conferenceId);
        return await getConferenceSessionsQueryHandler.Handle(query);
    }
}
