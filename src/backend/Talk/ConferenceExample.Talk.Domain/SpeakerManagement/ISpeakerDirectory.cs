namespace ConferenceExample.Talk.Domain.SpeakerManagement;

/// <summary>
/// Resolves the signed-in account to the speaker profile that owns its talks, replicated from the
/// Speaker BC's profile events.
///
/// This is an identity index, not a read model: it answers "whose talks are these?", never "what
/// does that speaker's profile say". Command handlers may use it to establish ownership before
/// loading the talk from the event store — which is why it is deliberately not named
/// <c>*ReadModelRepository</c> and is not covered by the CQRS rule that keeps command handlers off
/// projections (see <c>EventSourcingRules</c> in the architecture tests).
/// </summary>
public interface ISpeakerDirectory
{
    Task<SpeakerId?> FindSpeakerIdByUserId(UserId userId);
}
