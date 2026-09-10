using ConferenceExample.Speaker.Domain.SharedKernel;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Speaker.Domain.SpeakerManagement;

namespace ConferenceExample.Speaker.Application.CreateSpeakerProfile;

public class CreateSpeakerProfileCommandHandler(
    ISpeakerRepository speakerRepository,
    ISpeakerLookup speakerLookup,
    ICurrentUserService currentUserService
) : ICreateSpeakerProfileCommandHandler
{
    public async Task<SpeakerProfileCreatedDto> Handle(CreateSpeakerProfileCommand command)
    {
        var userId = new UserId(new GuidV7(currentUserService.GetCurrentUserId()));

        // The profile gets its own identity rather than reusing the account id, so the same
        // account can later hold other role profiles alongside this one.
        if (await speakerLookup.FindSpeakerIdByUserId(userId) is not null)
        {
            throw new DomainException("A speaker profile already exists for this account.");
        }

        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var speaker = Domain.SpeakerManagement.Speaker.Create(
            speakerId,
            userId,
            new Name(command.FirstName, command.LastName),
            new SpeakerBiography(command.Biography)
        );

        await speakerRepository.Save(speaker);

        return new SpeakerProfileCreatedDto(speakerId.Value.Value);
    }
}
