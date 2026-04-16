namespace ConferenceExample.Conference.Application.DefineTalkType;

public interface IDefineTalkTypeCommandHandler
{
    Task<TalkTypeDefinedDto> Handle(DefineTalkTypeCommand command);
}
