using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.UpdateConferenceDetails;

public class UpdateConferenceDetailsCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IUpdateConferenceDetailsCommandHandler
{
    public async Task Handle(UpdateConferenceDetailsCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.Id))
        );

        // Check ownership: Only the organizer who created the conference can update it
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId} is not authorized to update conference {conference.Id.Value}. Only the organizer who created the conference can update it."
            );
        }

        var start = command.Start ?? conference.ConferenceTime.Start;
        var end = command.End ?? conference.ConferenceTime.End;

        conference.UpdateDetails(
            new Text(command.Name),
            new Time(start, end),
            new Location(
                new Text(command.LocationName),
                new Address(
                    command.Street,
                    command.City,
                    command.State,
                    command.PostalCode,
                    command.Country
                )
            )
        );
        await conferenceRepository.Save(conference);
    }
}
