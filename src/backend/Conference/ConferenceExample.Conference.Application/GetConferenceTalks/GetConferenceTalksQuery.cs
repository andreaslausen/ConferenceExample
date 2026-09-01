using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;

namespace ConferenceExample.Conference.Application.GetConferenceTalks;

public record GetConferenceTalksQuery(Guid ConferenceId, PageRequest PageRequest);
