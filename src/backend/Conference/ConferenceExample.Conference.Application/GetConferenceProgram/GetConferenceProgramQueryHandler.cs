using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;

namespace ConferenceExample.Conference.Application.GetConferenceProgram;

public class GetConferenceProgramQueryHandler(
    IConferenceTalkReadModelRepository talkReadModelRepository,
    IConferenceRepository conferenceRepository
) : IGetConferenceProgramQueryHandler
{
    public async Task<IReadOnlyList<GetConferenceProgramDto>> Handle(
        GetConferenceProgramQuery query
    )
    {
        // Validate conference exists
        var conferenceId = new ConferenceId(new GuidV7(query.ConferenceId));
        await conferenceRepository.GetById(conferenceId);

        var talks = await talkReadModelRepository.GetByConferenceId(conferenceId);

        return talks
            .Where(t => t.SlotStart.HasValue && t.SlotEnd.HasValue)
            .Select(t => new GetConferenceProgramDto(
                t.Id,
                t.Title,
                $"{t.SpeakerFirstName} {t.SpeakerLastName}".Trim(),
                t.SlotStart,
                t.SlotEnd,
                t.RoomId,
                t.RoomName
            ))
            .ToList();
    }
}
