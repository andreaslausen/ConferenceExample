namespace ConferenceExample.Session.Application.Commands;

public interface ISubmitSessionCommandHandler
{
    Task Handle(SubmitSessionCommand command);
}
