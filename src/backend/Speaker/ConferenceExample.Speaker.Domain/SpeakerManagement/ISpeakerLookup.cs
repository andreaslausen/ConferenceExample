namespace ConferenceExample.Speaker.Domain.SpeakerManagement;

/// <summary>
/// Resolves an account to the speaker profile it owns.
///
/// This is an identity index, not a read model: it answers "which aggregate am I talking about?",
/// never "what is that aggregate's state". Command handlers may use it to find the stream they
/// then load from the event store — which is why it is deliberately not named
/// <c>*ReadModelRepository</c> and is not covered by the CQRS rule that keeps command handlers off
/// projections (see <c>EventSourcingRules</c> in the architecture tests).
///
/// It is maintained by a projection and therefore eventually consistent: two profile creations for
/// the same account racing each other can both pass the duplicate check. Acceptable here because
/// profile creation is a rare, user-driven action.
/// </summary>
public interface ISpeakerLookup
{
    Task<SpeakerId?> FindSpeakerIdByUserId(UserId userId);
}
