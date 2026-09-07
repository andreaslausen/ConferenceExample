namespace ConferenceExample.Talk.Domain.UnitTests;

using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SharedKernel;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.SpeakerManagement;
using ConferenceExample.Talk.Domain.TalkManagement;
using ConferenceExample.Talk.Domain.TalkManagement.Events;
using Xunit;
using TalkAggregate = ConferenceExample.Talk.Domain.TalkManagement.Talk;

public class TalkTests
{
    [Fact]
    public void Create_ValidParameters_InitializesProperties()
    {
        // Arrange
        var id = new TalkId(GuidV7.NewGuid());
        var title = new TalkTitle("Test Title");
        var @abstract = new Abstract("Test Abstract");
        var tags = new List<TalkTag> { new("Tag1"), new("Tag2") };
        var speakerId = new SpeakerId(GuidV7.NewGuid());

        // Act
        var talk = TalkAggregate.Create(id, title, @abstract, tags, speakerId);

        // Assert
        Assert.Equal(id, talk.Id);
        Assert.Equal(title, talk.Title);
        Assert.Equal(@abstract, talk.Abstract);
        Assert.Equal(speakerId, talk.SpeakerId);
        Assert.Equal(tags, talk.Tags);
        Assert.False(talk.IsDeleted);
    }

    [Fact]
    public void Create_HasNoConferenceAttached()
    {
        // A talk exists on its own; submitting it somewhere is a separate, later decision.
        var talk = CreateTalk();

        Assert.Empty(talk.Submissions);
    }

    [Fact]
    public void Create_NullTags_ThrowsArgumentNullException()
    {
        // Arrange
        var id = new TalkId(GuidV7.NewGuid());
        var title = new TalkTitle("Test Title");
        var @abstract = new Abstract("Test Abstract");
        var speakerId = new SpeakerId(GuidV7.NewGuid());

        // Act & Assert - the guard names the parameter; LINQ further down would blame "source"
        var exception = Assert.Throws<ArgumentNullException>(() =>
            TalkAggregate.Create(id, title, @abstract, null!, speakerId)
        );
        Assert.Equal("tags", exception.ParamName);
    }

    [Fact]
    public void Create_RaisesTalkCreatedEvent()
    {
        // Arrange
        var id = new TalkId(GuidV7.NewGuid());
        var speakerId = new SpeakerId(GuidV7.NewGuid());

        // Act
        var talk = TalkAggregate.Create(
            id,
            new TalkTitle("Test Title"),
            new Abstract("Test Abstract"),
            [new TalkTag("Tag1")],
            speakerId
        );

        // Assert
        var @event = Assert.Single(talk.GetUncommittedEvents());
        var created = Assert.IsType<TalkCreatedEvent>(@event);
        Assert.Equal(id.Value.Value, created.AggregateId);
        Assert.Equal("Test Title", created.Title);
        Assert.Equal("Test Abstract", created.Abstract);
        Assert.Equal(speakerId.Value.Value, created.SpeakerId);
        Assert.Equal(new List<string> { "Tag1" }, created.Tags);
    }

    [Fact]
    public void EditTitle_ChangesTitleAndRaisesEvent()
    {
        // Arrange
        var talk = CreateTalk();
        talk.ClearUncommittedEvents();

        // Act
        talk.EditTitle(new TalkTitle("New Title"));

        // Assert
        Assert.Equal(new TalkTitle("New Title"), talk.Title);
        var edited = Assert.IsType<TalkTitleEditedEvent>(
            Assert.Single(talk.GetUncommittedEvents())
        );
        Assert.Equal(talk.Id.Value.Value, edited.AggregateId);
        Assert.Equal("New Title", edited.Title);
    }

    [Fact]
    public void EditAbstract_ChangesAbstractAndRaisesEvent()
    {
        // Arrange
        var talk = CreateTalk();
        talk.ClearUncommittedEvents();

        // Act
        talk.EditAbstract(new Abstract("New Abstract"));

        // Assert
        Assert.Equal(new Abstract("New Abstract"), talk.Abstract);
        var edited = Assert.IsType<TalkAbstractEditedEvent>(
            Assert.Single(talk.GetUncommittedEvents())
        );
        Assert.Equal(talk.Id.Value.Value, edited.AggregateId);
        Assert.Equal("New Abstract", edited.Abstract);
    }

    [Fact]
    public void AddTag_AppendsTagAndRaisesEvent()
    {
        // Arrange
        var talk = CreateTalk();
        talk.ClearUncommittedEvents();

        // Act
        talk.AddTag(new TalkTag("Extra"));

        // Assert
        Assert.Contains(new TalkTag("Extra"), talk.Tags);
        var added = Assert.IsType<TalkTagAddedEvent>(Assert.Single(talk.GetUncommittedEvents()));
        Assert.Equal(talk.Id.Value.Value, added.AggregateId);
        Assert.Equal("Extra", added.Tag);
    }

    [Fact]
    public void RemoveTag_DropsTagAndRaisesEvent()
    {
        // Arrange
        var talk = CreateTalk();
        talk.ClearUncommittedEvents();

        // Act
        talk.RemoveTag(new TalkTag("Tag1"));

        // Assert
        Assert.DoesNotContain(new TalkTag("Tag1"), talk.Tags);
        var removed = Assert.IsType<TalkTagRemovedEvent>(
            Assert.Single(talk.GetUncommittedEvents())
        );
        Assert.Equal(talk.Id.Value.Value, removed.AggregateId);
        Assert.Equal("Tag1", removed.Tag);
    }

    [Fact]
    public void Delete_MarksTalkDeletedAndRaisesEvent()
    {
        // Arrange
        var talk = CreateTalk();
        talk.ClearUncommittedEvents();

        // Act
        talk.Delete();

        // Assert
        Assert.True(talk.IsDeleted);
        var deleted = Assert.IsType<TalkDeletedEvent>(Assert.Single(talk.GetUncommittedEvents()));
        Assert.Equal(talk.Id.Value.Value, deleted.AggregateId);
    }

    [Fact]
    public void Delete_KeepsSubmissionsAlreadyMade()
    {
        // Conferences own the submissions they received; deleting the talk must not erase them.
        var talk = CreateTalk();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        talk.SubmitToConference(conferenceId, new TalkTypeId(GuidV7.NewGuid()));

        talk.Delete();

        Assert.Single(talk.Submissions);
        Assert.Equal(conferenceId, talk.Submissions[0].ConferenceId);
    }

    [Fact]
    public void Delete_Twice_ThrowsDomainException()
    {
        // Arrange
        var talk = CreateTalk();
        talk.Delete();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(talk.Delete);
        Assert.Contains("has been deleted", exception.Message);
    }

    [Theory]
    [MemberData(nameof(MutatingOperations))]
    public void MutatingOperation_OnDeletedTalk_ThrowsDomainException(
        string _,
        Action<TalkAggregate> operation
    )
    {
        // Arrange
        var talk = CreateTalk();
        talk.Delete();

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => operation(talk));
        Assert.Contains("has been deleted", exception.Message);
    }

    public static TheoryData<string, Action<TalkAggregate>> MutatingOperations =>
        new()
        {
            { "EditTitle", talk => talk.EditTitle(new TalkTitle("New Title")) },
            { "EditAbstract", talk => talk.EditAbstract(new Abstract("New Abstract")) },
            { "AddTag", talk => talk.AddTag(new TalkTag("Extra")) },
            { "RemoveTag", talk => talk.RemoveTag(new TalkTag("Tag1")) },
            {
                "SubmitToConference",
                talk =>
                    talk.SubmitToConference(
                        new ConferenceId(GuidV7.NewGuid()),
                        new TalkTypeId(GuidV7.NewGuid())
                    )
            },
        };

    [Fact]
    public void SubmitToConference_RecordsSubmissionAndRaisesEvent()
    {
        // Arrange
        var talk = CreateTalk();
        talk.ClearUncommittedEvents();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());

        // Act
        talk.SubmitToConference(conferenceId, talkTypeId);

        // Assert
        var submission = Assert.Single(talk.Submissions);
        Assert.Equal(conferenceId, submission.ConferenceId);
        Assert.Equal(talkTypeId, submission.TalkTypeId);

        var submitted = Assert.IsType<TalkSubmittedToConferenceEvent>(
            Assert.Single(talk.GetUncommittedEvents())
        );
        Assert.Equal(talk.Id.Value.Value, submitted.AggregateId);
        Assert.Equal(conferenceId.Value.Value, submitted.ConferenceId);
        Assert.Equal(talkTypeId.Value.Value, submitted.TalkTypeId);
        Assert.Equal(talk.SpeakerId.Value.Value, submitted.SpeakerId);
        Assert.Equal("Test Title", submitted.Title);
        Assert.Equal("Test Abstract", submitted.Abstract);
        Assert.Equal(new List<string> { "Tag1", "Tag2" }, submitted.Tags);
    }

    [Fact]
    public void SubmitToConference_SnapshotsTheContentAtSubmissionTime()
    {
        // Arrange
        var talk = CreateTalk();
        talk.SubmitToConference(
            new ConferenceId(GuidV7.NewGuid()),
            new TalkTypeId(GuidV7.NewGuid())
        );

        // Act: the speaker keeps working on the talk after submitting it
        talk.EditTitle(new TalkTitle("Rewritten Title"));
        talk.EditAbstract(new Abstract("Rewritten Abstract"));
        talk.AddTag(new TalkTag("Later"));

        // Assert: the submission still shows what was actually submitted
        var submission = Assert.Single(talk.Submissions);
        Assert.Equal(new TalkTitle("Test Title"), submission.Title);
        Assert.Equal(new Abstract("Test Abstract"), submission.Abstract);
        Assert.Equal(new List<TalkTag> { new("Tag1"), new("Tag2") }, submission.Tags);
    }

    [Fact]
    public void SubmitToConference_TwoDifferentConferences_RecordsBothSubmissions()
    {
        // Arrange
        var talk = CreateTalk();
        var firstConference = new ConferenceId(GuidV7.NewGuid());
        var secondConference = new ConferenceId(GuidV7.NewGuid());

        // Act
        talk.SubmitToConference(firstConference, new TalkTypeId(GuidV7.NewGuid()));
        talk.SubmitToConference(secondConference, new TalkTypeId(GuidV7.NewGuid()));

        // Assert
        Assert.Equal(2, talk.Submissions.Count);
        Assert.Equal(firstConference, talk.Submissions[0].ConferenceId);
        Assert.Equal(secondConference, talk.Submissions[1].ConferenceId);
    }

    [Fact]
    public void SubmitToConference_SameConferenceTwice_ThrowsDomainException()
    {
        // Arrange
        var talk = CreateTalk();
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        talk.SubmitToConference(conferenceId, new TalkTypeId(GuidV7.NewGuid()));

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            talk.SubmitToConference(conferenceId, new TalkTypeId(GuidV7.NewGuid()))
        );
        Assert.Contains("already been submitted", exception.Message);
    }

    [Fact]
    public void LoadFromHistory_ReplaysAllEventsAndCountsVersion()
    {
        // Arrange
        var talkId = GuidV7.NewGuid();
        var speakerId = GuidV7.NewGuid();
        var conferenceId = GuidV7.NewGuid();
        var talkTypeId = GuidV7.NewGuid();
        var submittedAt = DateTimeOffset.UtcNow;

        var events = new List<IDomainEvent>
        {
            new TalkCreatedEvent(
                talkId,
                DateTimeOffset.UtcNow,
                "Original Title",
                "Original Abstract",
                speakerId,
                ["Tag1"]
            ),
            new TalkSubmittedToConferenceEvent(
                talkId,
                submittedAt,
                conferenceId,
                talkTypeId,
                speakerId,
                "Original Title",
                "Original Abstract",
                ["Tag1"]
            ),
            new TalkTitleEditedEvent(talkId, DateTimeOffset.UtcNow, "Edited Title"),
            new TalkAbstractEditedEvent(talkId, DateTimeOffset.UtcNow, "Edited Abstract"),
            new TalkTagAddedEvent(talkId, DateTimeOffset.UtcNow, "Tag2"),
            new TalkTagRemovedEvent(talkId, DateTimeOffset.UtcNow, "Tag1"),
        };

        // Act
        var talk = TalkAggregate.LoadFromHistory(events);

        // Assert
        Assert.Equal(new TalkId(talkId), talk.Id);
        Assert.Equal(new TalkTitle("Edited Title"), talk.Title);
        Assert.Equal(new Abstract("Edited Abstract"), talk.Abstract);
        Assert.Equal(new SpeakerId(speakerId), talk.SpeakerId);
        Assert.Equal(new List<TalkTag> { new("Tag2") }, talk.Tags);
        Assert.False(talk.IsDeleted);
        Assert.Equal(5, talk.Version);
        Assert.Empty(talk.GetUncommittedEvents());

        var submission = Assert.Single(talk.Submissions);
        Assert.Equal(new ConferenceId(conferenceId), submission.ConferenceId);
        Assert.Equal(new TalkTypeId(talkTypeId), submission.TalkTypeId);
        Assert.Equal(submittedAt, submission.SubmittedAt);
        Assert.Equal(new TalkTitle("Original Title"), submission.Title);
        Assert.Equal(new Abstract("Original Abstract"), submission.Abstract);
        Assert.Equal(new List<TalkTag> { new("Tag1") }, submission.Tags);
    }

    [Fact]
    public void LoadFromHistory_WithDeletion_MarksTalkDeleted()
    {
        // Arrange
        var talkId = GuidV7.NewGuid();
        var events = new List<IDomainEvent>
        {
            new TalkCreatedEvent(
                talkId,
                DateTimeOffset.UtcNow,
                "Title",
                "Abstract",
                GuidV7.NewGuid(),
                []
            ),
            new TalkDeletedEvent(talkId, DateTimeOffset.UtcNow),
        };

        // Act
        var talk = TalkAggregate.LoadFromHistory(events);

        // Assert
        Assert.True(talk.IsDeleted);
    }

    [Fact]
    public void LoadFromHistory_UnknownEvent_IsIgnored()
    {
        // Arrange
        var talkId = GuidV7.NewGuid();
        var events = new List<IDomainEvent>
        {
            new TalkCreatedEvent(
                talkId,
                DateTimeOffset.UtcNow,
                "Title",
                "Abstract",
                GuidV7.NewGuid(),
                []
            ),
            new UnknownEvent(talkId, DateTimeOffset.UtcNow),
        };

        // Act
        var talk = TalkAggregate.LoadFromHistory(events);

        // Assert
        Assert.Equal(new TalkTitle("Title"), talk.Title);
    }

    private static TalkAggregate CreateTalk() =>
        TalkAggregate.Create(
            new TalkId(GuidV7.NewGuid()),
            new TalkTitle("Test Title"),
            new Abstract("Test Abstract"),
            [new TalkTag("Tag1"), new TalkTag("Tag2")],
            new SpeakerId(GuidV7.NewGuid())
        );

    private record UnknownEvent(Guid AggregateId, DateTimeOffset OccurredAt) : IDomainEvent;
}
