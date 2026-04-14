using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetConferenceById;

public class GetConferenceByIdQueryHandler(IConferenceRepository conferenceRepository)
    : IGetConferenceByIdQueryHandler
{
    public async Task<GetConferenceByIdDto> Handle(GetConferenceByIdQuery query)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        return new GetConferenceByIdDto(
            conference.Id.Value.Value,
            conference.Name.Value,
            conference.ConferenceTime.Start,
            conference.ConferenceTime.End,
            conference.Location.Name.Value,
            conference.Location.Address.Street,
            conference.Location.Address.City,
            conference.Location.Address.State,
            conference.Location.Address.PostalCode,
            conference.Location.Address.Country,
            conference.OrganizerId.Value.Value
        );
    }
}
