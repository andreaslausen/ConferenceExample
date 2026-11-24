using ConferenceExample.Talk.Domain.TalkManagement;

namespace ConferenceExample.Talk.Domain.UnitTests;

public class ReadModelTests
{
    [Fact]
    public void TalkReadModel_Constructor_InitializesProperties()
    {
        var id = Guid.CreateVersion7();
        var speakerId = Guid.CreateVersion7();
        var tags = new List<string> { "tag1" };

        var readModel = new TalkReadModel(id, "Title", "Abstract", speakerId, tags, 2);

        Assert.Equal(id, readModel.Id);
        Assert.Equal("Title", readModel.Title);
        Assert.Equal("Abstract", readModel.Abstract);
        Assert.Equal(speakerId, readModel.SpeakerId);
        Assert.Equal(tags, readModel.Tags);
        Assert.Equal(2, readModel.SubmissionCount);
    }

    [Fact]
    public void TalkReadModel_Equality_SameValues_ReturnsTrue()
    {
        var id = Guid.CreateVersion7();
        var speakerId = Guid.CreateVersion7();
        var tags = new List<string> { "tag1" };

        Assert.Equal(
            new TalkReadModel(id, "Title", "Abstract", speakerId, tags, 1),
            new TalkReadModel(id, "Title", "Abstract", speakerId, tags, 1)
        );
    }

    [Fact]
    public void TalkSubmissionReadModel_Constructor_InitializesProperties()
    {
        var talkId = Guid.CreateVersion7();
        var conferenceId = Guid.CreateVersion7();
        var talkTypeId = Guid.CreateVersion7();
        var submittedAt = DateTimeOffset.UtcNow;
        var tags = new List<string> { "tag1" };

        var readModel = new TalkSubmissionReadModel(
            talkId,
            conferenceId,
            talkTypeId,
            nameof(SubmissionStatus.Accepted),
            null,
            submittedAt,
            "Title",
            "Abstract",
            tags
        );

        Assert.Equal(talkId, readModel.TalkId);
        Assert.Equal(conferenceId, readModel.ConferenceId);
        Assert.Equal(talkTypeId, readModel.TalkTypeId);
        Assert.Equal("Accepted", readModel.Status);
        Assert.Null(readModel.Reason);
        Assert.Equal(submittedAt, readModel.SubmittedAt);
        Assert.Equal("Title", readModel.Title);
        Assert.Equal("Abstract", readModel.Abstract);
        Assert.Equal(tags, readModel.Tags);
    }

    [Fact]
    public void TalkSubmissionReadModel_CarriesAReasonWhenTheSubmissionFailed()
    {
        var readModel = new TalkSubmissionReadModel(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            nameof(SubmissionStatus.Failed),
            "Conference is not accepting talk submissions.",
            DateTimeOffset.UtcNow,
            "Title",
            "Abstract",
            []
        );

        Assert.Equal("Failed", readModel.Status);
        Assert.Equal("Conference is not accepting talk submissions.", readModel.Reason);
    }
}
