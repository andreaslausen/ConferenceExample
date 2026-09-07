using ConferenceExample.Speaker.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Speaker.Domain.SpeakerManagement;

/// <summary>
/// The account a speaker profile belongs to. Deliberately distinct from <see cref="SpeakerId"/>:
/// one account may hold several roles (speaker, organizer, ...), so the account identity must not
/// double as the identity of any single role's profile.
/// </summary>
public record UserId(GuidV7 Value);
