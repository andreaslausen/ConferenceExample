namespace ConferenceExample.Conference.Application.GetAllConferences;

public interface IGetAllConferencesQueryHandler
{
    Task<(IReadOnlyList<GetAllConferencesDto> Items, int TotalCount)> Handle(
        GetAllConferencesQuery query
    );
}
