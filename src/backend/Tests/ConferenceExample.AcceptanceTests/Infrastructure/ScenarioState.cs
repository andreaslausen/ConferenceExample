using System.Net.Http.Headers;

namespace ConferenceExample.AcceptanceTests.Infrastructure;

/// <summary>
/// The handful of facts a scenario builds up as its Given steps run — who is signed in, which
/// conference and talk the following steps are about. Reqnroll creates one instance per scenario,
/// so binding classes for different bounded contexts can share it by taking it as a constructor
/// parameter instead of one step class reaching into another.
/// </summary>
public class ScenarioState(HttpClient httpClient)
{
    public string OrganizerToken { get; set; } = string.Empty;
    public string SpeakerToken { get; set; } = string.Empty;
    public string OtherSpeakerToken { get; set; } = string.Empty;

    public Guid ConferenceId { get; set; }
    public Guid TalkTypeId { get; set; }
    public Guid TalkId { get; set; }

    /// <summary>The most recent response, for steps that assert on the status code.</summary>
    public HttpResponseMessage LastResponse { get; set; } = null!;

    public void SignInAsOrganizer() => SetBearerToken(OrganizerToken);

    public void SignInAsSpeaker() => SetBearerToken(SpeakerToken);

    public void SignInAsOtherSpeaker() => SetBearerToken(OtherSpeakerToken);

    public void SignOut() => httpClient.DefaultRequestHeaders.Authorization = null;

    private void SetBearerToken(string token) =>
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            token
        );
}
