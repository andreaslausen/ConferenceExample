using ConferenceExample.Authentication;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.DefineTalkType;

public class DefineTalkTypeCommandHandler(
    IConferenceRepository conferenceRepository,
    ICurrentUserService currentUserService
) : IDefineTalkTypeCommandHandler
{
    public async Task<TalkTypeDefinedDto> Handle(DefineTalkTypeCommand command)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(command.ConferenceId))
        );

        // Check ownership: Only the organizer who created the conference can define talk types
        var currentUserId = currentUserService.GetCurrentUserId();
        var currentOrganizerId = new OrganizerId(new GuidV7(currentUserId.Value.Value));

        if (conference.OrganizerId.Value != currentOrganizerId.Value)
        {
            throw new UnauthorizedAccessException(
                $"User {currentUserId.Value} is not authorized to define talk types for conference {conference.Id.Value}. Only the organizer who created the conference can define talk types."
            );
        }

        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        conference.DefineTalkType(talkTypeId, new Text(command.Name));

        await conferenceRepository.Save(conference);

        return new TalkTypeDefinedDto(talkTypeId.Value, command.Name);
    }
}
