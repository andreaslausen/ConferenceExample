using ConferenceExample.Session.Domain.Repositories;
using ConferenceExample.Session.Domain.ValueObjects;
using ConferenceExample.Session.Domain.ValueObjects.Ids;

namespace ConferenceExample.Session.Application.Commands;

public class SubmitSessionCommandHandler(ISessionRepository sessionRepository)
    : ISubmitSessionCommandHandler
{
    public async Task Handle(SubmitSessionCommand command)
    {
        var session = Domain.Entities.Session.Submit(
            new SessionId(GuidV7.NewGuid()),
            new SessionTitle(command.Title),
            new SpeakerId(command.SpeakerId),
            command.Tags.Select(t => new SessionTag(t)),
            new SessionTypeId(command.SessionTypeId),
            new Abstract(command.Abstract),
            new ConferenceId(command.ConferenceId)
        );

        await sessionRepository.Save(session);
    }
}
