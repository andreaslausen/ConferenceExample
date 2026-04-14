using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.RenameConference;

public class RenameConferenceCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IRenameConferenceCommandHandler
{
    public async Task Handle(RenameConferenceCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.Id))
        );

        // Check ownership: Only the organizer who created the conference can rename it
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to rename conference {conference.Id.Value}. Only the organizer who created the conference can rename it."
            );
        }

        conference.Rename(new Text(command.Name));
        await conferenceRepository.Save(conference);
    }
}
