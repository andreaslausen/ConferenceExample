using ConferenceExample.Conference.Application.Dtos;

namespace ConferenceExample.Conference.Application.Queries;

public interface IGetConferenceSessionsQueryHandler
{
    Task<IReadOnlyList<SessionDto>> Handle(GetConferenceSessionsQuery query);
}
