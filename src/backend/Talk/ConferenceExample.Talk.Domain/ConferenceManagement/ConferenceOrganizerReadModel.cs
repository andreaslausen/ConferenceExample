namespace ConferenceExample.Talk.Domain.ConferenceManagement;

/// <summary>
/// Minimal cross-BC projection of who organizes a conference, derived from the Conference
/// BC's ConferenceCreatedEvent. Lets the Talk BC authorize organizer access to talks without
/// depending on the Conference BC directly.
/// </summary>
public record ConferenceOrganizerReadModel(Guid ConferenceId, Guid OrganizerId);
