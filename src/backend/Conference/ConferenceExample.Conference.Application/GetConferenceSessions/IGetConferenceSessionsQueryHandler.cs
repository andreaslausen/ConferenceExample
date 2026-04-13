namespace ConferenceExample.Conference.Application.GetConferenceSessions;

public interface IGetConferenceSessionsQueryHandler
{
    Task<IReadOnlyList<SessionDto>> Handle(GetConferenceSessionsQuery query);
}
