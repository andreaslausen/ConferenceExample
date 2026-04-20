using ConferenceExample.Conference.Domain.ConferenceManagement;

namespace ConferenceExample.Conference.Application.GetAllConferences;

public class GetAllConferencesQueryHandler(
    IConferenceReadModelRepository conferenceReadModelRepository
) : IGetAllConferencesQueryHandler
{
    public async Task<IReadOnlyList<GetAllConferencesDto>> Handle(GetAllConferencesQuery query)
    {
        var conferences = await conferenceReadModelRepository.GetAll();

        return conferences
            .Select(c => new GetAllConferencesDto(
                c.Id,
                c.Name,
                c.Start,
                c.End,
                c.City,
                c.State,
                c.PostalCode,
                c.Country
            ))
            .ToList();
    }
}
