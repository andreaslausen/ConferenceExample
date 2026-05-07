using ConferenceExample.EventStore;
using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;
using Reqnroll;
using Xunit;
using TalkEntity = ConferenceExample.Talk.Domain.TalkManagement.Talk;

namespace ConferenceExample.Talk.AcceptanceTests.StepDefinitions;

[Binding]
public class TalkSubmissionSteps(
    ITalkService talkService,
    ITalkRepository talkRepository,
    ITalkEventStore eventStore
)
{
    private Guid _conferenceId;
    private TalkEntity? _submittedTalk;

    [Given("a conference exists")]
    public async Task GivenAConferenceExists()
    {
        _conferenceId = Guid.CreateVersion7();

        var conferenceCreatedEvent = new StoredEvent(
            Guid.CreateVersion7(),
            _conferenceId,
            "ConferenceCreatedEvent",
            $$"""
            {
                "AggregateId": "{{_conferenceId}}",
                "OccurredAt": "{{DateTimeOffset.UtcNow:O}}",
                "Version": 0,
                "Name": "Test Conference",
                "Start": "{{DateTimeOffset.UtcNow.AddMonths(1):O}}",
                "End": "{{DateTimeOffset.UtcNow.AddMonths(1).AddDays(2):O}}",
                "LocationName": "Test Location",
                "Street": "123 Test St",
                "City": "Test City",
                "State": "Test State",
                "PostalCode": "12345",
                "Country": "Test Country",
                "OrganizerId": "{{Guid.CreateVersion7()}}",
                "Status": "CallForSpeakers"
            }
            """,
            DateTimeOffset.UtcNow,
            1
        );

        await eventStore.AppendEvents(_conferenceId, [conferenceCreatedEvent], 0);
    }

    [When("a speaker submits a talk titled {string} with abstract {string}")]
    public async Task WhenASpeakerSubmitsATalk(string title, string @abstract)
    {
        var talkId = await talkService.SubmitTalk(
            new SubmitTalkDto
            {
                Title = title,
                Abstract = @abstract,
                ConferenceId = _conferenceId,
                Tags = [],
                TalkTypeId = Guid.CreateVersion7(),
            }
        );
        _submittedTalk = await talkRepository.GetById(new TalkId(new GuidV7(talkId)));
    }

    [When(
        "a speaker submits a talk titled {string} with abstract {string} tagged {string} and {string}"
    )]
    public async Task WhenASpeakerSubmitsATalkWithTags(
        string title,
        string @abstract,
        string tag1,
        string tag2
    )
    {
        var talkId = await talkService.SubmitTalk(
            new SubmitTalkDto
            {
                Title = title,
                Abstract = @abstract,
                ConferenceId = _conferenceId,
                Tags = [tag1, tag2],
                TalkTypeId = Guid.CreateVersion7(),
            }
        );
        _submittedTalk = await talkRepository.GetById(new TalkId(new GuidV7(talkId)));
    }

    [Then("the talk is stored with status Submitted")]
    public void ThenTheTalkIsStoredWithStatusSubmitted()
    {
        Assert.NotNull(_submittedTalk);
        Assert.Equal(TalkStatus.Submitted, _submittedTalk.Status);
    }

    [Then("the talk has the tag {string}")]
    public void ThenTheTalkHasTheTag(string expectedTag)
    {
        Assert.NotNull(_submittedTalk);
        Assert.Contains(_submittedTalk.Tags, t => t.Tag == expectedTag);
    }
}
