using System.ComponentModel.DataAnnotations;

namespace ConferenceExample.Conference.Application.GetConferenceById;

public class GetConferenceByIdDto
{
    [Required]
    public required Guid Id { get; init; }

    [Required]
    public required string Name { get; init; }

    [Required]
    public required DateTimeOffset StartDate { get; init; }

    [Required]
    public required DateTimeOffset EndDate { get; init; }

    [Required]
    public required string LocationName { get; init; }

    [Required]
    public required string Street { get; init; }

    [Required]
    public required string City { get; init; }

    [Required]
    public required string State { get; init; }

    [Required]
    public required string PostalCode { get; init; }

    [Required]
    public required string Country { get; init; }

    [Required]
    public required Guid OrganizerId { get; init; }

    [Required]
    public required string Status { get; init; }

    [Required]
    public required int TalkTypesCount { get; init; }

    [Required]
    public required int TalksCount { get; init; }

    [Required]
    public required int AcceptedTalksCount { get; init; }

    [Required]
    public required int UnscheduledAcceptedTalksCount { get; init; }
}
