namespace ConferenceExample.Speaker.Domain.SpeakerManagement;

public record SpeakerReadModel(
    Guid Id,
    Guid UserId,
    string FirstName,
    string LastName,
    string Biography
);
