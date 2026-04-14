using ConferenceExample.Authentication;
using ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Application.CreateConference;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using AuthGuidV7 = ConferenceExample.Authentication.SharedKernel.ValueObjects.Ids.GuidV7;
using ConferenceGuidV7 = ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids.GuidV7;

namespace ConferenceExample.Conference.Application.UnitTests;

public class CreateConferenceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesConference()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetCurrentUserId().Returns(new UserId(AuthGuidV7.NewGuid()));
        var handler = new CreateConferenceCommandHandler(repository, currentUserService);
        var command = CreateValidCommand();

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Start, result.Start);
        Assert.Equal(command.End, result.End);
        Assert.Equal(command.LocationName, result.LocationName);
        await repository.Received(1).Save(Arg.Any<Domain.ConferenceManagement.Conference>());
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetCurrentUserId().Returns(new UserId(AuthGuidV7.NewGuid()));
        var handler = new CreateConferenceCommandHandler(repository, currentUserService);
        var command = CreateValidCommand();

        repository
            .Save(Arg.Any<Domain.ConferenceManagement.Conference>())
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetCurrentUserId().Returns(new UserId(AuthGuidV7.NewGuid()));
        var handler = new CreateConferenceCommandHandler(repository, currentUserService);
        var command = new CreateConferenceCommand(
            "",
            DateTimeOffset.UtcNow.AddDays(30),
            DateTimeOffset.UtcNow.AddDays(32),
            "Convention Center",
            "Main Street 1",
            "Berlin",
            "Berlin",
            "10115",
            "Germany"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_EmptyLocationName_ThrowsArgumentException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.GetCurrentUserId().Returns(new UserId(AuthGuidV7.NewGuid()));
        var handler = new CreateConferenceCommandHandler(repository, currentUserService);
        var command = new CreateConferenceCommand(
            "DotNet Conf",
            DateTimeOffset.UtcNow.AddDays(30),
            DateTimeOffset.UtcNow.AddDays(32),
            "",
            "Main Street 1",
            "Berlin",
            "Berlin",
            "10115",
            "Germany"
        );

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command));
    }

    private static CreateConferenceCommand CreateValidCommand() =>
        new(
            "DotNet Conf",
            new DateTimeOffset(2026, 9, 1, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 9, 3, 18, 0, 0, TimeSpan.Zero),
            "Convention Center",
            "Main Street 1",
            "Berlin",
            "Berlin",
            "10115",
            "Germany"
        );
}
