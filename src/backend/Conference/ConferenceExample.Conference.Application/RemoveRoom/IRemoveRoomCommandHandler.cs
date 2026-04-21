namespace ConferenceExample.Conference.Application.RemoveRoom;

public interface IRemoveRoomCommandHandler
{
    Task Handle(RemoveRoomCommand command);
}
