namespace ConferenceExample.Conference.Application.AddRoom;

public interface IAddRoomCommandHandler
{
    Task<RoomAddedDto> Handle(AddRoomCommand command);
}
