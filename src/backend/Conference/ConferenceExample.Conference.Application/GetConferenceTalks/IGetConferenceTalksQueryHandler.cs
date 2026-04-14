namespace ConferenceExample.Conference.Application.GetConferenceTalks;

public interface IGetConferenceTalksQueryHandler
{
    Task<IReadOnlyList<GetConferenceTalksDto>> Handle(GetConferenceTalksQuery query);
}
