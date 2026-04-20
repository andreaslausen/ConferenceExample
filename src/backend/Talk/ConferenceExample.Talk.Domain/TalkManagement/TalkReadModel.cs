namespace ConferenceExample.Talk.Domain.TalkManagement;

public record TalkReadModel(
    Guid Id,
    string Title,
    string Abstract,
    Guid ConferenceId,
    string Status,
    IReadOnlyList<string> Tags
);
