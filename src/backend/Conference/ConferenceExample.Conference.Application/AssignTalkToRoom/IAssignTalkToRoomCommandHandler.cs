namespace ConferenceExample.Conference.Application.AssignTalkToRoom;

public interface IAssignTalkToRoomCommandHandler
{
    Task Handle(AssignTalkToRoomCommand command);
}
