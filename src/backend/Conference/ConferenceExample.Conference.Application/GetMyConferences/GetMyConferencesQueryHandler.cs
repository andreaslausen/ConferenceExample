using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetMyConferences;

public class GetMyConferencesQueryHandler(
    IConferenceReadModelRepository conferenceReadModelRepository,
    ICurrentUserService currentUserService
) : IGetMyConferencesQueryHandler
{
    public async Task<IReadOnlyList<GetMyConferencesDto>> Handle(GetMyConferencesQuery query)
    {
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId));

        var conferences = await conferenceReadModelRepository.GetByOrganizerId(currentOrganizerId);

        return conferences
            .Select(c => new GetMyConferencesDto(
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
    }
}
