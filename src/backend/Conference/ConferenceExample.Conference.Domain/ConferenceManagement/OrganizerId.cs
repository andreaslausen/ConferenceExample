namespace ConferenceExample.Conference.Domain.ConferenceManagement;

// The organizer's identity comes from an external identity provider (Keycloak), whose
// subject ids are plain UUIDs, not the app-minted GuidV7 that other identifiers use.
public record OrganizerId(Guid Value);
