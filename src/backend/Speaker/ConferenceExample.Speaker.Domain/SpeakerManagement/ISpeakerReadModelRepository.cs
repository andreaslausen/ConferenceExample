namespace ConferenceExample.Speaker.Domain.SpeakerManagement;

/// <summary>
/// Query API for the speaker projection, returning the domain-shaped <see cref="SpeakerReadModel"/>.
/// Used by application query handlers (e.g. <c>GetSpeakerByIdQueryHandler</c>,
/// <c>GetMyProfileQueryHandler</c>) to serve reads.
///
/// Command handlers must not depend on this — see <see cref="ISpeakerLookup"/> for the identity
/// index they use instead, and the <c>EventSourcingRules</c> architecture test that enforces this
/// separation.
/// </summary>
public interface ISpeakerReadModelRepository
{
    Task<SpeakerReadModel?> GetById(SpeakerId speakerId);

    Task<SpeakerReadModel?> GetByUserId(UserId userId);
}
