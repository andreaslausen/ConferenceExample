namespace ConferenceExample.Conference.Application.GetAllConferences;

public interface IGetAllConferencesQueryHandler
{
    Task<IReadOnlyList<GetAllConferencesDto>> Handle(GetAllConferencesQuery query);
}
