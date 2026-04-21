namespace ConferenceExample.Conference.Application.GetConferenceRooms;

public interface IGetConferenceRoomsQueryHandler
{
    Task<IReadOnlyList<GetConferenceRoomsDto>> Handle(GetConferenceRoomsQuery query);
}
