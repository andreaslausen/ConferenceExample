using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.RemoveTalkType;

public class RemoveTalkTypeCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IRemoveTalkTypeCommandHandler
{
    public async Task Handle(RemoveTalkTypeCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can remove talk types
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to remove talk types for conference {conference.Id.Value}. Only the organizer who created the conference can remove talk types."
            );
        }

        conference.RemoveTalkType(new TalkTypeId(new GuidV7(command.TalkTypeId)));

        await conferenceRepository.Save(conference);
    }
}
