namespace ConferenceExample.Talk.Domain.TalkManagement;

/// <summary>
/// Value object containing essential conference information needed for talk submission validation.
/// This is a lightweight DTO replicated from the Conference BC to the Talk BC.
/// Does not have any infrastructure dependencies (e.g., MongoDB).
/// </summary>
public record ConferenceInfo(ConferenceId Id, string Name, string Status);
