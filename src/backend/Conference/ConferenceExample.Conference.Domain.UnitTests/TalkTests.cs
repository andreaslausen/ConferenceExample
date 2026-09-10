using ConferenceExample.Conference.Domain.ConferenceManagement;
using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using ConferenceAggregate = ConferenceExample.Conference.Domain.ConferenceManagement.Conference;

namespace ConferenceExample.Conference.Domain.UnitTests;

/// <summary>
/// The conference's view of a submitted talk. It is only ever created and changed by the
/// Conference aggregate, so it is exercised through it rather than constructed directly.
/// </summary>
public class TalkTests
{
    [Fact]
    public void SubmittedTalk_StartsOutSubmittedWithoutSlotOrRoom()
    {
        // Arrange
        var (conference, talkTypeId) = CreateConferenceAcceptingSubmissions();
        var talkId = new TalkId(GuidV7.NewGuid());

        // Act
        conference.SubmitTalk(talkId, talkTypeId);

        // Assert
        var talk = Assert.Single(conference.Talks);
        Assert.Equal(talkId, talk.Id);
        Assert.Equal(talkTypeId, talk.TalkTypeId);
        Assert.Equal(TalkStatus.Submitted, talk.Status);
        Assert.Null(talk.Slot);
        Assert.Null(talk.Room);
    }

    [Fact]
    public void Accept_SetsStatusToAccepted()
    {
        // Arrange
        var (conference, talkTypeId) = CreateConferenceAcceptingSubmissions();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId, talkTypeId);

        // Act
        conference.AcceptTalk(talkId);

        // Assert
        Assert.Equal(TalkStatus.Accepted, Assert.Single(conference.Talks).Status);
    }

    [Fact]
    public void Reject_SetsStatusToRejected()
    {
        // Arrange
        var (conference, talkTypeId) = CreateConferenceAcceptingSubmissions();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId, talkTypeId);

        // Act
        conference.RejectTalk(talkId);

        // Assert
        Assert.Equal(TalkStatus.Rejected, Assert.Single(conference.Talks).Status);
    }

    [Fact]
    public void Schedule_SetsSlot()
    {
        // Arrange
        var (conference, talkTypeId) = CreateConferenceAcceptingSubmissions();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId, talkTypeId);
        var slot = new Time(
            DateTimeOffset.UtcNow.AddDays(30).AddHours(9),
            DateTimeOffset.UtcNow.AddDays(30).AddHours(10)
        );

        // Act
        conference.ScheduleTalk(talkId, slot);

        // Assert
        Assert.Equal(slot, Assert.Single(conference.Talks).Slot);
    }

    [Fact]
    public void AssignRoom_SetsRoom()
    {
        // Arrange
        var (conference, talkTypeId) = CreateConferenceAcceptingSubmissions();
        var talkId = new TalkId(GuidV7.NewGuid());
        conference.SubmitTalk(talkId, talkTypeId);
        var roomId = new RoomId(GuidV7.NewGuid());
        conference.AddRoom(roomId, new Text("Main Hall"));

        // Act
        conference.AssignTalkToRoom(talkId, new Room(roomId, new Text("Main Hall")));

        // Assert
        var room = Assert.Single(conference.Talks).Room;
        Assert.NotNull(room);
        Assert.Equal(roomId, room.Id);
        Assert.Equal(new Text("Main Hall"), room.Name);
    }

    private static (
        ConferenceAggregate Conference,
        TalkTypeId TalkTypeId
    ) CreateConferenceAcceptingSubmissions()
    {
        var conference = ConferenceAggregate.Create(
            new ConferenceId(GuidV7.NewGuid()),
            new Text("Test Conference"),
            new Time(DateTimeOffset.UtcNow.AddDays(30), DateTimeOffset.UtcNow.AddDays(32)),
            new Location(
                new Text("Test Venue"),
                new Address("Main Street 1", "Berlin", "Berlin", "10115", "Germany")
            ),
            new OrganizerId(GuidV7.NewGuid())
        );

        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        conference.DefineTalkType(talkTypeId, new Text("Talk"), 45);
        conference.ChangeStatus(ConferenceStatus.CallForSpeakers);

        return (conference, talkTypeId);
    }
}
