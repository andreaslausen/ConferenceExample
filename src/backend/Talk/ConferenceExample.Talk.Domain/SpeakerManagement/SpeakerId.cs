namespace ConferenceExample.Talk.Domain.SpeakerManagement;

// The speaker's identity comes from an external identity provider (Keycloak), whose
// subject ids are plain UUIDs, not the app-minted GuidV7 that other identifiers use.
public record SpeakerId(Guid Value);
