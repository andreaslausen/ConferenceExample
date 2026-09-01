namespace ConferenceExample.Conference.Application.GetMyConferences;

public interface IGetMyConferencesQueryHandler
{
    Task<(IReadOnlyList<GetMyConferencesDto> Items, int TotalCount)> Handle(
        GetMyConferencesQuery query
    );
}
