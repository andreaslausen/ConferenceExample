namespace ConferenceExample.Conference.Application.GetConferenceSessions;

public interface IGetConferenceSessionsQueryHandler
{
    Task<IReadOnlyList<GetConferenceSessionDto>> Handle(GetConferenceSessionsQuery query);
}
