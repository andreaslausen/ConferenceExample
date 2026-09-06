using ConferenceExample.Talk.Domain.SharedKernel.ValueObjects.Ids;

namespace ConferenceExample.Talk.Domain.TalkManagement;

/// <summary>
/// Minimal Conference representation in the Talk BC — confirms a conference referenced by
/// ConferenceId actually exists in the Conference BC. Whether it's accepting talk submissions
/// is Conference's own invariant, enforced there (see Conference.SubmitTalk in the Conference BC),
/// not re-derived here.
/// </summary>
public class Conference
{
    public ConferenceId Id { get; private set; } = null!;

    private Conference() { }

    public static Conference FromEvents(ConferenceId id)
    {
        return new Conference { Id = id };
    }
}
