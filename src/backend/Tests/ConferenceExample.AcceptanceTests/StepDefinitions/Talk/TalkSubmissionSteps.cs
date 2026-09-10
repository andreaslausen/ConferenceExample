using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ConferenceExample.AcceptanceTests.Infrastructure;
using ConferenceExample.Talk.Application.GetTalkSubmissions;
using ConferenceExample.Talk.Application.SubmitTalkToConference;
using ConferenceExample.Talk.Persistence.ReadModels;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;
using Xunit;
using ConferenceTalkRepository = ConferenceExample.Conference.Persistence.ReadModels.IConferenceTalkDocumentRepository;

namespace ConferenceExample.AcceptanceTests.StepDefinitions.Talk;

/// <summary>
/// Submitting an existing talk to a conference. The conference decides asynchronously, so the
/// outcome steps poll for the status the speaker eventually sees.
/// </summary>
[Binding]
public class TalkSubmissionSteps(HttpClient httpClient, ScenarioState state)
{
    private static readonly JsonSerializerOptions ResponseJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    [When("the speaker submits the talk to the conference")]
    public async Task WhenTheSpeakerSubmitsTheTalkToTheConference()
    {
        await Submit(state.ConferenceId, state.TalkTypeId);
    }

    [When("the speaker submits the talk to a nonexistent conference")]
    public async Task WhenTheSpeakerSubmitsTheTalkToANonexistentConference()
    {
        state.ConferenceId = Guid.CreateVersion7();
        await Submit(state.ConferenceId, Guid.CreateVersion7());
    }

    [When("the speaker submits the talk with a talk type the conference does not offer")]
    public async Task WhenTheSpeakerSubmitsTheTalkWithAnUnknownTalkType()
    {
        await Submit(state.ConferenceId, Guid.CreateVersion7());
    }

    [When("the speaker submits the talk to the conference again")]
    public async Task WhenTheSpeakerSubmitsTheTalkToTheConferenceAgain()
    {
        await Submit(state.ConferenceId, state.TalkTypeId);
    }

    [Then("the submission is accepted with status {int}")]
    public void ThenTheSubmissionIsAcceptedWithStatus(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)state.LastResponse.StatusCode);
    }

    [Then("the submission is rejected with status {int}")]
    public void ThenTheSubmissionIsRejectedWithStatus(int expectedStatusCode)
    {
        Assert.Equal(expectedStatusCode, (int)state.LastResponse.StatusCode);
    }

    [Then("the submission eventually has status {word}")]
    public async Task ThenTheSubmissionEventuallyHasStatus(string expectedStatus)
    {
        var document = await WaitForSubmissionDocument(d => d.Status == expectedStatus);
        Assert.Equal(expectedStatus, document.Status);

        var submission = await WaitForSubmissionResponse(s => s.Status == expectedStatus);
        Assert.Equal(expectedStatus, submission.Status);
        Assert.Equal(state.ConferenceId, submission.ConferenceId);
    }

    [Then("the submission shows the conference name {string}")]
    public async Task ThenTheSubmissionShowsTheConferenceName(string expectedName)
    {
        var submission = await WaitForSubmissionResponse(s => s.ConferenceName == expectedName);
        Assert.Equal(expectedName, submission.ConferenceName);
    }

    [Then("the submission still shows the title {string}")]
    public async Task ThenTheSubmissionStillShowsTheTitle(string expectedTitle)
    {
        // The submission is a snapshot: editing the talk afterwards must not rewrite it.
        var document = await WaitForSubmissionDocument(_ => true);
        Assert.Equal(expectedTitle, document.Title);

        var submission = await WaitForSubmissionResponse(_ => true);
        Assert.Equal(expectedTitle, submission.Title);
    }

    [Then("the conference has the talk with the speaker name {string}")]
    public async Task ThenTheConferenceHasTheTalkWithSpeakerName(string expectedSpeakerName)
    {
        var document = await Eventually.Succeeds(
            async () =>
            {
                using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
                var repository =
                    scope.ServiceProvider.GetRequiredService<ConferenceTalkRepository>();
                return await repository.Get(state.ConferenceId, state.TalkId);
            },
            $"Talk {state.TalkId} appearing in conference {state.ConferenceId}"
        );

        Assert.Equal(
            expectedSpeakerName,
            $"{document.SpeakerFirstName} {document.SpeakerLastName}".Trim()
        );

        state.SignInAsOrganizer();
        var talks = await Eventually.Succeeds(
            async () =>
            {
                var response = await httpClient.GetAsync(
                    $"/api/conferences/{state.ConferenceId}/talks"
                );
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                var body = await response.Content.ReadAsStringAsync();
                return body.Contains(expectedSpeakerName, StringComparison.Ordinal) ? body : null;
            },
            $"GET /api/conferences/{state.ConferenceId}/talks showing {expectedSpeakerName}"
        );
        Assert.Contains(expectedSpeakerName, talks, StringComparison.Ordinal);
    }

    [Then("the talk has {int} submission(s)")]
    public async Task ThenTheTalkHasSubmissions(int expectedCount)
    {
        var submissions = await Eventually.Succeeds(
            async () =>
            {
                state.SignInAsSpeaker();
                var response = await httpClient.GetAsync($"/api/talks/{state.TalkId}/submissions");
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                var body = await response.Content.ReadFromJsonAsync<List<GetTalkSubmissionsDto>>(
                    ResponseJsonOptions
                );
                return body?.Count == expectedCount ? body : null;
            },
            $"Talk {state.TalkId} having {expectedCount} submission(s)"
        );

        Assert.Equal(expectedCount, submissions.Count);
    }

    private async Task Submit(Guid conferenceId, Guid talkTypeId)
    {
        state.SignInAsSpeaker();

        state.LastResponse = await httpClient.PostAsJsonAsync(
            $"/api/talks/{state.TalkId}/submissions",
            new SubmitTalkToConferenceDto(conferenceId, talkTypeId)
        );
    }

    private Task<TalkSubmissionDocument> WaitForSubmissionDocument(
        Func<TalkSubmissionDocument, bool> matches
    ) =>
        Eventually.Succeeds(
            async () =>
            {
                using var scope = AcceptanceTestEnvironment.Factory.Services.CreateScope();
                var repository =
                    scope.ServiceProvider.GetRequiredService<ITalkSubmissionDocumentRepository>();
                var document = await repository.Get(state.TalkId, state.ConferenceId);
                return document is not null && matches(document) ? document : null;
            },
            $"Submission of talk {state.TalkId} to conference {state.ConferenceId} in the database"
        );

    private Task<GetTalkSubmissionsDto> WaitForSubmissionResponse(
        Func<GetTalkSubmissionsDto, bool> matches
    )
    {
        state.SignInAsSpeaker();

        return Eventually.Succeeds(
            async () =>
            {
                var response = await httpClient.GetAsync($"/api/talks/{state.TalkId}/submissions");
                if (response.StatusCode != HttpStatusCode.OK)
                    return null;

                var submissions = await response.Content.ReadFromJsonAsync<
                    List<GetTalkSubmissionsDto>
                >(ResponseJsonOptions);

                return submissions?.FirstOrDefault(s =>
                    s.ConferenceId == state.ConferenceId && matches(s)
                );
            },
            $"GET /api/talks/{state.TalkId}/submissions returning the expected submission"
        );
    }
}
