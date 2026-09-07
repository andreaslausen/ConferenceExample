using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Application.GetTalkSubmissions;

/// <summary>
/// Answers "where did I submit this talk, and what came of it?". Each entry shows the content as
/// it was submitted, plus the outcome the conference reported back.
/// </summary>
public class GetTalkSubmissionsQueryHandler(
    ITalkReadModelRepository talkReadModelRepository,
    ITalkSubmissionReadModelRepository submissionReadModelRepository,
    IConferenceDirectory conferenceDirectory,
    ICurrentSpeakerProvider currentSpeakerProvider
) : IGetTalkSubmissionsQueryHandler
{
    public async Task<IReadOnlyList<GetTalkSubmissionsDto>?> Handle(GetTalkSubmissionsQuery query)
    {
        var talkId = new TalkId(new GuidV7(query.TalkId));

        var talk = await talkReadModelRepository.GetById(talkId);

        if (talk is null)
            return null;

        var currentSpeakerId = await currentSpeakerProvider.GetCurrentSpeakerId();

        if (new SpeakerId(new GuidV7(talk.SpeakerId)) != currentSpeakerId)
            return null;

        var submissions = await submissionReadModelRepository.GetByTalkId(talkId);

        var conferenceNames = await conferenceDirectory.GetConferenceNames(
            submissions.Select(s => new ConferenceId(new GuidV7(s.ConferenceId))).ToList()
        );

        return submissions
            .Select(submission => new GetTalkSubmissionsDto(
                submission.ConferenceId,
                conferenceNames.TryGetValue(submission.ConferenceId, out var name)
                    ? name
                    : string.Empty,
                submission.TalkTypeId,
                submission.Status,
                submission.Reason,
                submission.SubmittedAt,
                submission.Title,
                submission.Abstract,
                submission.Tags.ToList()
            ))
            .ToList();
    }
}
