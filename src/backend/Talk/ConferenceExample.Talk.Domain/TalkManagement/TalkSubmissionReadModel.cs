namespace ConferenceExample.Talk.Domain.TalkManagement;

public record TalkSubmissionReadModel(
    Guid TalkId,
    Guid ConferenceId,
    Guid TalkTypeId,
    string Status,
    string? Reason,
    DateTimeOffset SubmittedAt,
    string Title,
    string Abstract,
    IReadOnlyList<string> Tags
);
