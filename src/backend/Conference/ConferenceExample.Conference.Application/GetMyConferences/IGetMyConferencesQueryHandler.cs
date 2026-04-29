namespace ConferenceExample.Conference.Application.GetMyConferences;

public interface IGetMyConferencesQueryHandler
{
    Task<IReadOnlyList<GetMyConferencesDto>> Handle(GetMyConferencesQuery query);
}
