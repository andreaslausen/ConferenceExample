using ConferenceExample.Talk.Domain.SpeakerManagement;

namespace ConferenceExample.Talk.Application;

/// <summary>
/// Resolves the signed-in account to the speaker profile that owns its talks. Every talk use case
/// needs this, so it lives in one place rather than being repeated per handler.
/// </summary>
public interface ICurrentSpeakerProvider
{
    /// <summary>
    /// Throws <see cref="Domain.SharedKernel.NotFoundException"/> when the account has no speaker
    /// profile yet — talks belong to a speaker, so there is nothing to own them.
    /// </summary>
    Task<SpeakerId> GetCurrentSpeakerId();
}
