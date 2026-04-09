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
        Assert.Empty(conference.Talks);
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
    public void SubmitTalk_AddsTalkWithSubmittedStatus()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());

        // Act
        conference.SubmitTalk(talkId);

        // Assert
        var talk = Assert.Single(conference.Talks);
        Assert.Equal(talkId, talk.Id);
        Assert.Equal(TalkStatus.Submitted, talk.Status);
    }

    [Fact]
    public void SubmitTalk_RaisesTalkSubmittedToConferenceEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());

        // Act
        conference.SubmitTalk(talkId);

        // Assert
        var events = conference.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<TalkSubmittedToConferenceEvent>(events[1]);
    }

    [Fact]
    public void AcceptTalk_ChangesStatusToAccepted()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);

        // Act
        conference.AcceptTalk(talkId);

        // Assert
        Assert.Equal(TalkStatus.Accepted, conference.Talks[0].Status);
    }

    [Fact]
    public void RejectTalk_ChangesStatusToRejected()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);

        // Act
        conference.RejectTalk(talkId);

        // Assert
        Assert.Equal(TalkStatus.Rejected, conference.Talks[0].Status);
    }

    [Fact]
    public void ScheduleTalk_SetsSlot()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        var slot = new Time(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2));

        // Act
        conference.ScheduleTalk(talkId, slot);

        // Assert
        Assert.Equal(slot, conference.Talks[0].Slot);
    }

    [Fact]
    public void AssignTalkToRoom_SetsRoom()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        var room = new Entities.Room(new RoomId(GuidV7.NewGuid()), new Text("Room A"));

        // Act
        conference.AssignTalkToRoom(talkId, room);

        // Assert
        Assert.NotNull(conference.Talks[0].Room);
        Assert.Equal(room.Id, conference.Talks[0].Room!.Id);
        Assert.Equal(room.Name, conference.Talks[0].Room!.Name);
    }

    [Fact]
    public void AcceptTalk_RaisesTalkAcceptedEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.ClearUncommittedEvents();

        // Act
        conference.AcceptTalk(talkId);

        // Assert
        var events = conference.GetUncommittedEvents();
        var single = Assert.Single(events);
        Assert.IsType<TalkAcceptedEvent>(single);
    }

    [Fact]
    public void RejectTalk_RaisesTalkRejectedEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.ClearUncommittedEvents();

        // Act
        conference.RejectTalk(talkId);

        // Assert
        var events = conference.GetUncommittedEvents();
        var single = Assert.Single(events);
        Assert.IsType<TalkRejectedEvent>(single);
    }

    [Fact]
    public void ScheduleTalk_RaisesTalkScheduledEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        conference.ClearUncommittedEvents();
        var slot = new Time(DateTimeOffset.UtcNow.AddHours(1), DateTimeOffset.UtcNow.AddHours(2));

        // Act
        conference.ScheduleTalk(talkId, slot);

        // Assert
        var events = conference.GetUncommittedEvents();
        var single = Assert.Single(events);
        Assert.IsType<TalkScheduledEvent>(single);
    }

    [Fact]
    public void AssignTalkToRoom_RaisesTalkAssignedToRoomEvent()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        conference.ClearUncommittedEvents();
        var room = new Entities.Room(new RoomId(GuidV7.NewGuid()), new Text("Room A"));

        // Act
        conference.AssignTalkToRoom(talkId, room);

        // Assert
        var events = conference.GetUncommittedEvents();
        var single = Assert.Single(events);
        Assert.IsType<TalkAssignedToRoomEvent>(single);
    }

    [Fact]
    public void ReplayEvents_RestoresState()
    {
        // Arrange
        var conference = CreateValidConference();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId);
        conference.AcceptTalk(talkId);
        conference.Rename(new Text("Replayed Conference"));
        var events = conference.GetUncommittedEvents().ToList();

        // Act
        var replayedConference = Domain.Conference.LoadFromHistory(events);

        // Assert
        Assert.Equal(conference.Id, replayedConference.Id);
        Assert.Equal(new Text("Replayed Conference"), replayedConference.Name);
        Assert.Equal(conference.Location, replayedConference.Location);
        Assert.Equal(conference.ConferenceTime, replayedConference.ConferenceTime);
        var talk = Assert.Single(replayedConference.Talks);
        Assert.Equal(talkId, talk.Id);
        Assert.Equal(TalkStatus.Accepted, talk.Status);
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
