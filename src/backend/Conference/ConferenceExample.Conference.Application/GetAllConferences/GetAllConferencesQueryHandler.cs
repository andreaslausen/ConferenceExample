using ConferenceExample.Conference.Domain.ConferenceManagement;

namespace ConferenceExample.Conference.Application.GetAllConferences;

public class GetAllConferencesQueryHandler(IConferenceRepository conferenceRepository)
    : IGetAllConferencesQueryHandler
{
    public async Task<IReadOnlyList<GetAllConferencesDto>> Handle(GetAllConferencesQuery query)
    {
        var conferences = await conferenceRepository.GetAll();

        return conferences
            .Select(conference => new GetAllConferencesDto(
                conference.Id.Value.Value,
                conference.Name.Value,
                conference.ConferenceTime.Start,
                conference.ConferenceTime.End,
                conference.Location.Address.City,
                conference.Location.Address.State,
                conference.Location.Address.PostalCode,
                conference.Location.Address.Country
            ))
            .ToList();
    }
}
