namespace ConferenceExample.Conference.Application.GetConferenceById;

public record GetConferenceByIdDto(
    Guid Id,
    string Name,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string LocationName,
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country,
    Guid OrganizerId
);
