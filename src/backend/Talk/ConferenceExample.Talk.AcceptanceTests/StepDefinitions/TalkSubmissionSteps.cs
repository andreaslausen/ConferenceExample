using ConferenceExample.Talk.Application;
using ConferenceExample.Talk.Application.SubmitTalk;
using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;
using ConferenceExample.Talk.Domain.TalkManagement;
using Reqnroll;
using Xunit;
using TalkEntity = ConferenceExample.Talk.Domain.TalkManagement.Talk;

namespace ConferenceExample.Talk.AcceptanceTests.StepDefinitions;

[Binding]
public class TalkSubmissionSteps(ITalkService talkService, ITalkRepository talkRepository)
{
    private Guid _conferenceId;
    private IReadOnlyList<TalkEntity> _talks = [];

    [Given("a conference exists")]
    public void GivenAConferenceExists()
    {
        _conferenceId = Guid.CreateVersion7();
    }

    [When("a speaker submits a talk titled {string} with abstract {string}")]
    public async Task WhenASpeakerSubmitsATalk(string title, string @abstract)
    {
        await talkService.SubmitTalk(
            new SubmitTalkDto
            {
                Title = title,
                Abstract = @abstract,
                ConferenceId = _conferenceId,
                Tags = [],
                TalkTypeId = Guid.CreateVersion7(),
            }
        );
        _talks = await talkRepository.GetTalks(new ConferenceId(_conferenceId));
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
        await talkService.SubmitTalk(
            new SubmitTalkDto
            {
                Title = title,
                Abstract = @abstract,
                ConferenceId = _conferenceId,
                Tags = [tag1, tag2],
                TalkTypeId = Guid.CreateVersion7(),
            }
        );
        _talks = await talkRepository.GetTalks(new ConferenceId(_conferenceId));
    }

    [Then("the talk is stored with status Submitted")]
    public void ThenTheTalkIsStoredWithStatusSubmitted()
    {
        Assert.Single(_talks);
        Assert.Equal(TalkStatus.Submitted, _talks[0].Status);
    }

    [Then("the talk has the tag {string}")]
    public void ThenTheTalkHasTheTag(string expectedTag)
    {
        Assert.Contains(_talks[0].Tags, t => t.Tag == expectedTag);
    }
}
