namespace ConferenceExample.Conference.Application.GetConferenceTalks;

public interface IGetConferenceTalksQueryHandler
{
    Task<(IReadOnlyList<GetConferenceTalksDto> Items, int TotalCount)> Handle(
        GetConferenceTalksQuery query
    );
}
