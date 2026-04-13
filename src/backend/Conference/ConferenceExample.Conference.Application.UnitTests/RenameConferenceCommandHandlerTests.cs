using ConferenceExample.Conference.Application.RenameConference;
using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

namespace ConferenceExample.Conference.Application.UnitTests;

public class RenameConferenceCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_RenamesConference()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new RenameConferenceCommandHandler(repository);
        var conference = CreateValidConference();
        var command = new RenameConferenceCommand(conference.Id.Value, "Renamed Conference");

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        // Act
        await handler.Handle(command);

        // Assert
        await repository
            .Received(1)
            .GetById(Arg.Is<ConferenceId>(id => id.Value == (GuidV7)command.Id));
        await repository
            .Received(1)
            .Save(Arg.Is<ConferenceAggregate>(c => c.Name.Value == "Renamed Conference"));
    }

    [Fact]
    public async Task Handle_ConferenceNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new RenameConferenceCommandHandler(repository);
        var command = new RenameConferenceCommand(GuidV7.NewGuid(), "Renamed Conference");

        repository
            .GetById(Arg.Any<ConferenceId>())
            .Throws(new InvalidOperationException("Conference not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_EmptyName_ThrowsArgumentException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new RenameConferenceCommandHandler(repository);
        var conference = CreateValidConference();
        var command = new RenameConferenceCommand(conference.Id.Value, "");

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_InvalidGuidV7_ThrowsArgumentException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new RenameConferenceCommandHandler(repository);
        var invalidGuid = Guid.NewGuid(); // Not a GuidV7
        var command = new RenameConferenceCommand(invalidGuid, "New Name");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        var repository = Substitute.For<IConferenceRepository>();
        var handler = new RenameConferenceCommandHandler(repository);
        var conference = CreateValidConference();
        var command = new RenameConferenceCommand(conference.Id.Value, "Renamed Conference");

        repository.GetById(Arg.Any<ConferenceId>()).Returns(conference);
        repository
            .Save(Arg.Any<ConferenceAggregate>())
            .Throws(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
    }

    private static ConferenceAggregate CreateValidConference()
    {
        var id = new ConferenceId(GuidV7.NewGuid());
        var name = new Text("Test Conference");
        var time = new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32));
        var location = new Location(
            new Text("Test Venue"),
            new Address("123 Main St", "Springfield", "IL", "62701", "US")
        );

        return ConferenceAggregate.Create(id, name, time, location);
    }
}
