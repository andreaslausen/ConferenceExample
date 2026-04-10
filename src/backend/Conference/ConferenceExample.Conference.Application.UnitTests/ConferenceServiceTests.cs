using ConferenceExample.Conference.Application.Commands;
using ConferenceExample.Conference.Application.Queries;
using NSubstitute;

namespace ConferenceExample.Conference.Application.UnitTests;

public class ConferenceServiceTests
{
    [Fact]
    public async Task CreateConference_ValidDto_CallsCommandHandler()
    {
        // Arrange
        var createCommandHandler = Substitute.For<ICreateConferenceCommandHandler>();
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var service = new ConferenceService(
            createCommandHandler,
            renameCommandHandler,
            queryHandler
        );

        // Act
        await service.CreateConference(CreateDto());

        // Assert
        await createCommandHandler.Received(1).Handle(Arg.Any<CreateConferenceCommand>());
    }

    [Fact]
    public async Task RenameConference_ValidDto_CallsCommandHandler()
    {
        // Arrange
        var createCommandHandler = Substitute.For<ICreateConferenceCommandHandler>();
        var renameCommandHandler = Substitute.For<IRenameConferenceCommandHandler>();
        var queryHandler = Substitute.For<IGetConferenceSessionsQueryHandler>();
        var service = new ConferenceService(
            createCommandHandler,
            renameCommandHandler,
            queryHandler
        );

        // Act
        await service.RenameConference(
            Guid.NewGuid(),
            new RenameConferenceDto { Name = "New Name" }
        );

        // Assert
        await renameCommandHandler.Received(1).Handle(Arg.Any<RenameConferenceCommand>());
    }

    private static CreateConferenceDto CreateDto() =>
        new()
        {
            Name = "DotNet Conf",
            Start = new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero),
            End = new DateTimeOffset(2026, 9, 3, 18, 0, 0, TimeSpan.Zero),
            LocationName = "Convention Center",
            Street = "Main Street 1",
            City = "Berlin",
            State = "Berlin",
            PostalCode = "10115",
            Country = "Germany",
        };
}
