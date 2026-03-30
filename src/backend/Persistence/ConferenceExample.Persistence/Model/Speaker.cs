namespace ConferenceExample.Persistence.Model;

public class Speaker
{
    public required long Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Biography { get; init; }
}