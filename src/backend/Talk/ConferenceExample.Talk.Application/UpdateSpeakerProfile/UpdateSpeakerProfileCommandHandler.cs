using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Application.UpdateSpeakerProfile;

public class UpdateSpeakerProfileCommandHandler(
    ISpeakerRepository speakerRepository,
    ICurrentUserService currentUserService
) : IUpdateSpeakerProfileCommandHandler
{
    public async Task Handle(UpdateSpeakerProfileCommand command)
    {
        var currentUserId = currentUserService.GetCurrentUserId();
        var speakerId = new SpeakerId(new GuidV7(currentUserId));

        var speaker = await speakerRepository.GetSpeaker(speakerId);

        if (speaker.Id != speakerId)
            throw new UnauthorizedAccessException("Speakers can only update their own profile.");

        speaker.UpdateProfile(
            new Name(command.FirstName, command.LastName),
            new SpeakerBiography(command.Biography)
        );

        await speakerRepository.Save(speaker);
    }
}
