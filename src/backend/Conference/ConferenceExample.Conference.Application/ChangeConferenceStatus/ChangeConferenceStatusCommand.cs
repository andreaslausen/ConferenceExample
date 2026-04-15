using ConferenceExample.Conference.Domain.ConferenceManagement;

namespace ConferenceExample.Conference.Application.ChangeConferenceStatus;

public record ChangeConferenceStatusCommand(Guid ConferenceId, ConferenceStatus NewStatus);
