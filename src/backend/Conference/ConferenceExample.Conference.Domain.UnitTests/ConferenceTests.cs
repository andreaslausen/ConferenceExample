namespace ConferenceExample.Conference.Domain.UnitTests;

using ConferenceExample.Conference.Domain.Events;
using ConferenceExample.Conference.Domain.ValueObjects;
using ConferenceExample.Conference.Domain.ValueObjects.Ids;
using Xunit;

public class ConferenceTests
{
    [Fact]
    public void Create_ValidParameters_InitializesProperties()
    {
        // Arrange & Act
        var conference = CreateValidConference();

        // Assert
        Assert.NotNull(conference.Id);
        Assert.Equal("Test Conference", conference.Name.Value);
        Assert.Equal("Test Venue", conference.Location.Name.Value);
        Assert.Empty(conference.Sessions);
    }

    [Fact]
    public void Create_RaisesConferenceCreatedEvent()
    {
        // Act
        var conference = CreateValidConference();

        // Assert
        var events = conference.GetUncommittedEvents();
        var createdEvent = Assert.Single(events);
        Assert.IsType<ConferenceCreatedEvent>(createdEvent);
    }

    [Fact]
    public void Rename_ValidName_UpdatesName()
    {
        // Arrange
        var conference = CreateValidConference();
        var newName = new Text("Renamed Conference");

        // Act
        conference.Rename(newName);

        // Assert
        Assert.Equal(newName, conference.Name);
    }

    [Fact]
    public void Rename_RaisesConferenceRenamedEvent()
    {
        // Arrange
        var conference = CreateValidConference();

        // Act
        conference.Rename(new Text("Renamed Conference"));

        // Assert
        var events = conference.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<ConferenceRenamedEvent>(events[1]);
    }

    [Fact]
    public void SubmitSession_AddsSessionWithSubmittedStatus()
    {
        // Arrange
        var conference = CreateValidConference();
        var sessionId = new SessionId(GuidV7.NewGuid());

        // Act
        conference.SubmitSession(sessionId);

        // Assert
        var session = Assert.Single(conference.Sessions);
        Assert.Equal(sessionId, session.Id);
        Assert.Equal(SessionStatus.Submitted, session.Status);
    }

    [Fact]
    public void SubmitSession_RaisesSessionSubmittedToConferenceEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var sessionId = new SessionId(GuidV7.NewGuid());

        // Act
        conference.SubmitSession(sessionId);

        // Assert
        var events = conference.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<SessionSubmittedToConferenceEvent>(events[1]);
    }

    [Fact]
    public void AcceptSession_ChangesStatusToAccepted()
    {
        // Arrange
        var conference = CreateValidConference();
        var sessionId = new SessionId(GuidV7.NewGuid());
        conference.SubmitSession(sessionId);

        // Act
        conference.AcceptSession(sessionId);

        // Assert
        Assert.Equal(SessionStatus.Accepted, conference.Sessions[0].Status);
    }

    [Fact]
    public void RejectSession_ChangesStatusToRejected()
    {
        // Arrange
        var conference = CreateValidConference();
        var sessionId = new SessionId(GuidV7.NewGuid());
        conference.SubmitSession(sessionId);

        // Act
        conference.RejectSession(sessionId);

        // Assert
        Assert.Equal(SessionStatus.Rejected, conference.Sessions[0].Status);
    }

    [Fact]
    public void ScheduleSession_SetsSlot()
    {
        // Arrange
        var conference = CreateValidConference();
        var sessionId = new SessionId(GuidV7.NewGuid());
        conference.SubmitSession(sessionId);
        conference.AcceptSession(sessionId);
        var slot = new Time(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2));

        // Act
        conference.ScheduleSession(sessionId, slot);

        // Assert
        Assert.Equal(slot, conference.Sessions[0].Slot);
    }

    [Fact]
    public void AssignSessionToRoom_SetsRoom()
    {
        // Arrange
        var conference = CreateValidConference();
        var sessionId = new SessionId(GuidV7.NewGuid());
        conference.SubmitSession(sessionId);
        conference.AcceptSession(sessionId);
        var room = new Entities.Room(new RoomId(GuidV7.NewGuid()), new Text("Room A"));

        // Act
        conference.AssignSessionToRoom(sessionId, room);

        // Assert
        Assert.NotNull(conference.Sessions[0].Room);
        Assert.Equal(room.Id, conference.Sessions[0].Room!.Id);
        Assert.Equal(room.Name, conference.Sessions[0].Room!.Name);
    }

    [Fact]
    public void ReplayEvents_RestoresState()
    {
        // Arrange
        var conference = CreateValidConference();
        var sessionId = new SessionId(GuidV7.NewGuid());
        conference.SubmitSession(sessionId);
        conference.AcceptSession(sessionId);
        conference.Rename(new Text("Replayed Conference"));
        var events = conference.GetUncommittedEvents().ToList();

        // Act
        var replayedConference = Domain.Conference.LoadFromHistory(events);

        // Assert
        Assert.Equal(conference.Id, replayedConference.Id);
        Assert.Equal(new Text("Replayed Conference"), replayedConference.Name);
        Assert.Equal(conference.Location, replayedConference.Location);
        Assert.Equal(conference.ConferenceTime, replayedConference.ConferenceTime);
        var session = Assert.Single(replayedConference.Sessions);
        Assert.Equal(sessionId, session.Id);
        Assert.Equal(SessionStatus.Accepted, session.Status);
        Assert.Equal(3, replayedConference.Version);
    }

    private static Domain.Conference CreateValidConference()
    {
        var id = new ConferenceId(GuidV7.NewGuid());
        var name = new Text("Test Conference");
        var time = new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32));
        var location = new Location(
            new Text("Test Venue"),
            new Address("123 Main St", "Springfield", "IL", "62701", "US")
        );

        return Domain.Conference.Create(id, name, time, location);
    }
}
