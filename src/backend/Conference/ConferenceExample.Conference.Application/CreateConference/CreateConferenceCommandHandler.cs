using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

namespace ConferenceExample.Conference.Application.CreateConference;

public class CreateConferenceCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : ICreateConferenceCommandHandler
{
    public async Task<ConferenceCreatedDto> Handle(CreateConferenceCommand command)
    {
        // Get the current authenticated organizer's ID
        var currentUserId = currentUserService.GetCurrentUserId();

        // Convert Authentication.GuidV7 to Conference.Domain.GuidV7
        var organizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        var conference = ConferenceAggregate.Create(
            new ConferenceId(GuidV7.NewGuid()),
            new Text(command.Name),
            new Time(command.Start, command.End),
            new Location(
                new Text(command.LocationName),
                new Address(
                    command.Street,
                    command.City,
                    command.State,
                    command.PostalCode,
                    command.Country
                )
            ),
            organizerId
        );

        await conferenceRepository.Save(conference);

        return new ConferenceCreatedDto(
            conference.Id.Value,
            conference.Name.Value,
            conference.ConferenceTime.Start,
            conference.ConferenceTime.End,
            conference.Location.Name.Value
        );
    }
}
