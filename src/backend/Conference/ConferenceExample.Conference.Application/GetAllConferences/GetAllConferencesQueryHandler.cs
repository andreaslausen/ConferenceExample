using ConferenceExample.Conference.Domain.ConferenceManagement;

namespace ConferenceExample.Conference.Application.GetAllConferences;

public class GetAllConferencesQueryHandler(
    IConferenceReadModelRepository conferenceReadModelRepository
) : IGetAllConferencesQueryHandler
{
    public async Task<(IReadOnlyList<GetAllConferencesDto> Items, int TotalCount)> Handle(
        GetAllConferencesQuery query
    )
    {
        var (conferences, totalCount) = await conferenceReadModelRepository.GetAll(
            query.PageRequest
        );

        var items = conferences
            .Where(c => c.Status != ConferenceStatus.Draft.ToString())
            .Select(c => new GetAllConferencesDto(
                c.Id,
                c.Name,
                c.Start,
                c.End,
                c.City,
                c.State,
                c.PostalCode,
                c.Country,
                c.Status
            ))
            .ToList();

        return (items, totalCount);
    }
}
