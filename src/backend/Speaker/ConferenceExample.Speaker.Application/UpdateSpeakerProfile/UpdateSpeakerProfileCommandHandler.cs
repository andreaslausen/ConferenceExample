using ConferenceExample.Speaker.Domain.SharedKernel;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Speaker.Domain.SpeakerManagement;

namespace ConferenceExample.Speaker.Application.UpdateSpeakerProfile;

public class UpdateSpeakerProfileCommandHandler(
    ISpeakerRepository speakerRepository,
    ISpeakerLookup speakerLookup,
    ICurrentUserService currentUserService
) : IUpdateSpeakerProfileCommandHandler
{
    public async Task Handle(UpdateSpeakerProfileCommand command)
    {
        var userId = new UserId(new GuidV7(currentUserService.GetCurrentUserId()));

        var speakerId =
            await speakerLookup.FindSpeakerIdByUserId(userId)
            ?? throw new NotFoundException("No speaker profile exists for this account.");

        var speaker = await speakerRepository.GetById(speakerId);

        speaker.UpdateProfile(
            new Name(command.FirstName, command.LastName),
            new SpeakerBiography(command.Biography)
        );

        await speakerRepository.Save(speaker);
    }
}
