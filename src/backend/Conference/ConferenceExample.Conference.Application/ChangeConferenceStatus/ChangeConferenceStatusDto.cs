using ConferenceExample.Conference.Domain.ConferenceManagement;

namespace ConferenceExample.Conference.Application.ChangeConferenceStatus;

public class ChangeConferenceStatusDto
{
    public required ConferenceStatus Status { get; init; }
}
