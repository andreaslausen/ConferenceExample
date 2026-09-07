using ConferenceExample.Talk.Domain.ConferenceManagement;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class TalkSubmissionTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var submittedAt = DateTimeOffset.UtcNow;
        var title = new TalkTitle("Title");
        var @abstract = new Abstract("Abstract");
        var tags = new List<TalkTag> { new("tag1") };

        // Act
        var submission = new TalkSubmission(
            conferenceId,
            talkTypeId,
            submittedAt,
            title,
            @abstract,
            tags
        );

        // Assert
        Assert.Equal(conferenceId, submission.ConferenceId);
        Assert.Equal(talkTypeId, submission.TalkTypeId);
        Assert.Equal(submittedAt, submission.SubmittedAt);
        Assert.Equal(title, submission.Title);
        Assert.Equal(@abstract, submission.Abstract);
        Assert.Equal(tags, submission.Tags);
    }

    [Fact]
    public void Equality_SameValues_ReturnsTrue()
    {
        // Arrange
        var conferenceId = new ConferenceId(GuidV7.NewGuid());
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var submittedAt = DateTimeOffset.UtcNow;
        var tags = new List<TalkTag> { new("tag1") };

        // Act & Assert
        Assert.Equal(
            new TalkSubmission(
                conferenceId,
                talkTypeId,
                submittedAt,
                new TalkTitle("Title"),
                new Abstract("Abstract"),
                tags
            ),
            new TalkSubmission(
                conferenceId,
                talkTypeId,
                submittedAt,
                new TalkTitle("Title"),
                new Abstract("Abstract"),
                tags
            )
        );
    }

    [Fact]
    public void Equality_DifferentConference_ReturnsFalse()
    {
        // Arrange
        var talkTypeId = new TalkTypeId(GuidV7.NewGuid());
        var submittedAt = DateTimeOffset.UtcNow;

        // Act & Assert
        Assert.NotEqual(
            new TalkSubmission(
                new ConferenceId(GuidV7.NewGuid()),
                talkTypeId,
                submittedAt,
                new TalkTitle("Title"),
                new Abstract("Abstract"),
                []
            ),
            new TalkSubmission(
                new ConferenceId(GuidV7.NewGuid()),
                talkTypeId,
                submittedAt,
                new TalkTitle("Title"),
                new Abstract("Abstract"),
                []
            )
        );
    }
}
