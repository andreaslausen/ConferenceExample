namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.Events;
using ConferenceExample.Talk.Domain.ValueObjects;
using ConferenceExample.Talk.Domain.ValueObjects.Ids;
using Xunit;

public class TalkTests
{
    [Fact]
    public void Submit_ValidParameters_InitializesProperties()
    {
        // Arrange
        var id = new TalkId(GuidV7.NewGuid());
        var title = new TalkTitle("Test Title");
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var tags = new List<TalkTag> { new("Tag1"), new("Tag2") };
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Act
        var talk = Entities.Talk.Submit(
            id,
            title,
            speakerId,
            tags,
            talkTypeId,
            @abstract,
            conferenceId
        );

        // Assert
        Assert.Equal(id, talk.Id);
        Assert.Equal(title, talk.Title);
        Assert.Equal(speakerId, talk.SpeakerId);
        Assert.Equal(talkTypeId, talk.TalkTypeId);
        Assert.Equal(@abstract, talk.Abstract);
        Assert.Equal(conferenceId, talk.ConferenceId);
        Assert.Equal(TalkStatus.Submitted, talk.Status);
        Assert.Equal(tags, talk.Tags);
    }

    [Fact]
    public void Submit_NullTags_ThrowsArgumentNullException()
    {
        // Arrange
        var id = new TalkId(GuidV7.NewGuid());
        var title = new TalkTitle("Test Title");
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Entities.Talk.Submit(id, title, speakerId, null!, talkTypeId, @abstract, conferenceId)
        );
    }

    [Fact]
    public void Submit_RaisesTalkSubmittedEvent()
    {
        // Act
        var talk = CreateValidTalk();

        // Assert
        var events = talk.GetUncommittedEvents();
        var submittedEvent = Assert.Single(events);
        Assert.IsType<TalkSubmittedEvent>(submittedEvent);
    }

    [Fact]
    public void EditTitle_ValidTitle_UpdatesTitle()
    {
        // Arrange
        var talk = CreateValidTalk();
        var newTitle = new TalkTitle("Updated Title");

        // Act
        talk.EditTitle(newTitle);

        // Assert
        Assert.Equal(newTitle, talk.Title);
    }

    [Fact]
    public void EditTitle_RaisesTalkTitleEditedEvent()
    {
        // Arrange
        var talk = CreateValidTalk();

        // Act
        talk.EditTitle(new TalkTitle("Updated Title"));

        // Assert
        var events = talk.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<TalkTitleEditedEvent>(events[1]);
    }

    [Fact]
    public void EditAbstract_ValidAbstract_UpdatesAbstract()
    {
        // Arrange
        var talk = CreateValidTalk();
        var newAbstract = new Abstract("Updated Abstract");

        // Act
        talk.EditAbstract(newAbstract);

        // Assert
        Assert.Equal(newAbstract, talk.Abstract);
    }

    [Fact]
    public void AddTag_ValidTag_AddsTagToTalk()
    {
        // Arrange
        var talk = CreateValidTalk();
        var newTag = new TalkTag("NewTag");

        // Act
        talk.AddTag(newTag);

        // Assert
        Assert.Contains(newTag, talk.Tags);
    }

    [Fact]
    public void RemoveTag_ValidTag_RemovesTagFromTalk()
    {
        // Arrange
        var talk = CreateValidTalk();
        var tagToRemove = talk.Tags[0];

        // Act
        talk.RemoveTag(tagToRemove);

        // Assert
        Assert.DoesNotContain(tagToRemove, talk.Tags);
    }

    [Fact]
    public void EditAbstract_RaisesTalkAbstractEditedEvent()
    {
        // Arrange
        var talk = CreateValidTalk();

        // Act
        talk.EditAbstract(new Abstract("Updated Abstract"));

        // Assert
        var events = talk.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<TalkAbstractEditedEvent>(events[1]);
    }

    [Fact]
    public void AddTag_RaisesTalkTagAddedEvent()
    {
        // Arrange
        var talk = CreateValidTalk();

        // Act
        talk.AddTag(new TalkTag("NewTag"));

        // Assert
        var events = talk.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<TalkTagAddedEvent>(events[1]);
    }

    [Fact]
    public void RemoveTag_RaisesTalkTagRemovedEvent()
    {
        // Arrange
        var talk = CreateValidTalk();
        var tagToRemove = talk.Tags[0];

        // Act
        talk.RemoveTag(tagToRemove);

        // Assert
        var events = talk.GetUncommittedEvents();
        Assert.Equal(2, events.Count);
        Assert.IsType<TalkTagRemovedEvent>(events[1]);
    }

    [Fact]
    public void ReplayEvents_RestoresState()
    {
        // Arrange
        var talk = CreateValidTalk();
        talk.EditTitle(new TalkTitle("Replayed Title"));
        var events = talk.GetUncommittedEvents().ToList();

        // Act
        var replayedTalk = Entities.Talk.LoadFromHistory(events);

        // Assert
        Assert.Equal(talk.Id, replayedTalk.Id);
        Assert.Equal(new TalkTitle("Replayed Title"), replayedTalk.Title);
        Assert.Equal(talk.SpeakerId, replayedTalk.SpeakerId);
        Assert.Equal(talk.Tags, replayedTalk.Tags);
        Assert.Equal(1, replayedTalk.Version);
    }

    private static Entities.Talk CreateValidTalk()
    {
        var id = new TalkId(GuidV7.NewGuid());
        var title = new TalkTitle("Test Title");
        var speakerId = new SpeakerId(GuidV7.NewGuid());
        var tags = new List<TalkTag> { new("Tag1"), new("Tag2") };
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var @abstract = new Abstract("Test Abstract");
        var conferenceId = new ConferenceId(GuidV7.NewGuid());

        return Entities.Talk.Submit(
            id,
            title,
            speakerId,
            tags,
            talkTypeId,
            @abstract,
            conferenceId
        );
    }
}
