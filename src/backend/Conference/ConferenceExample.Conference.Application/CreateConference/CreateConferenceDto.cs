namespace ConferenceExample.Conference.Application.CreateConference;

public class CreateConferenceDto
{
    public required string Name { get; init; }
    public DateTimeOffset Start { get; init; }
    public DateTimeOffset End { get; init; }
    public required string LocationName { get; init; }
    public required string Street { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string PostalCode { get; init; }
    public required string Country { get; init; }
}
