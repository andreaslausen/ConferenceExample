namespace ConferenceExample.Persistence.Model;

public class Conference
{
    public required  long Id { get; init; }
    public required string Name { get; init; }
    public required DateTimeOffset Start { get; init; }
    public required DateTimeOffset End { get; init; }
    public required string LocationName { get; init; }
    public required string LocationStreet { get; init; }
    public required string LocationCity { get; init; }
    public required string LocationState { get; init; }
    public required string LocationPostalCode { get; init; }
    public required string LocationCountry { get; init; }
}