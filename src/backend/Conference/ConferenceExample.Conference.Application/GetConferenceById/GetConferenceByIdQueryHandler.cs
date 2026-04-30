using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Conference.Application.GetConferenceById;

public class GetConferenceByIdQueryHandler(IConferenceRepository conferenceRepository)
    : IGetConferenceByIdQueryHandler
{
    public async Task<GetConferenceByIdDto> Handle(GetConferenceByIdQuery query)
    {
        var conference = await conferenceRepository.GetById(
            new ConferenceId(new GuidV7(query.ConferenceId))
        );

        var talkTypesCount = conference.TalkTypes.Count;
        var talksCount = conference.Talks.Count;
        var acceptedTalksCount = conference.Talks.Count(t =>
            t.Status == Domain.TalkManagement.TalkStatus.Accepted
        );
        var unscheduledAcceptedTalksCount = conference.Talks.Count(t =>
            t.Status == Domain.TalkManagement.TalkStatus.Accepted
            && (t.Slot == null || t.Room == null)
        );

        return new GetConferenceByIdDto
        {
            Id = conference.Id.Value.Value,
            Name = conference.Name.Value,
            StartDate = conference.ConferenceTime.Start,
            EndDate = conference.ConferenceTime.End,
            LocationName = conference.Location.Name.Value,
            Street = conference.Location.Address.Street,
            City = conference.Location.Address.City,
            State = conference.Location.Address.State,
            PostalCode = conference.Location.Address.PostalCode,
            Country = conference.Location.Address.Country,
            OrganizerId = conference.OrganizerId.Value.Value,
            Status = conference.Status.ToString(),
            TalkTypesCount = talkTypesCount,
            TalksCount = talksCount,
            AcceptedTalksCount = acceptedTalksCount,
            UnscheduledAcceptedTalksCount = unscheduledAcceptedTalksCount,
        };
    }
}
