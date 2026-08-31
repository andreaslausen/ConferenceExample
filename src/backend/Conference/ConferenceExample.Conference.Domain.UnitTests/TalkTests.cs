namespace ConferenceExample.Conference.Domain.UnitTests;

using ConferenceExample.Conference.Domain.RoomManagement;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects;
using ConferenceExample.Conference.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Conference.Domain.TalkManagement;
using Xunit;

public class TalkTests
{
    [Fact]
    public void Constructor_ReadModelParameters_InitializesProperties()
    {
        // Arrange
        var id = new TalkId(GuidV7.NewGuid());
        var title = new Text("Talk Title");
        var abstractText = new Text("Talk Abstract");
        var speakerId = GuidV7.NewGuid();
        var talkTypeId = GuidV7.NewGuid();
        var tags = new List<string> { "Tag1", "Tag2" };
        var slot = new Time(
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(1).AddHours(1)
        );
        var room = new Room(new RoomId(GuidV7.NewGuid()), new Text("Room A"));

        // Act
        var talk = new Talk(
            id,
            title,
            abstractText,
            speakerId,
            "Jane",
            "Doe",
            "Speaker biography",
            talkTypeId,
            tags,
            TalkStatus.Accepted,
            slot,
            room
        );

        // Assert
        Assert.Equal(id, talk.Id);
        Assert.Equal(title, talk.Title);
        Assert.Equal(abstractText, talk.Abstract);
        Assert.Equal(speakerId, talk.SpeakerId);
        Assert.Equal("Jane", talk.SpeakerFirstName);
        Assert.Equal("Doe", talk.SpeakerLastName);
        Assert.Equal("Speaker biography", talk.SpeakerBiography);
        Assert.Equal(talkTypeId, talk.TalkTypeId);
        Assert.Equal(tags, talk.Tags);
        Assert.Equal(TalkStatus.Accepted, talk.Status);
        Assert.Equal(slot, talk.Slot);
        Assert.Equal(room, talk.Room);
    }
}
