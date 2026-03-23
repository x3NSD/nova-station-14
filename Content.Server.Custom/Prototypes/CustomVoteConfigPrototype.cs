using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Custom.Prototypes;

[DataDefinition]
public sealed partial class CustomVoteConfig
{
    [DataField]
    public VoteConfig StationTypeVote = new();

    [DataField]
    public VoteConfig MapVote = new();

    [DataField]
    public VoteConfig PresetVote = new();

    public sealed class VoteConfig
    {
        [DataField]
        public bool Enabled = true;

        [DataField]
        public float DelayAfterRoundEnd;

        [DataField]
        public float DelayAfterStationType;

        [DataField]
        public float DelayAfterMap;

        [DataField]
        public bool DisplayVotes = true;

        [DataField]
        public List<string> Options = new();

        [DataField]
        public int ExcludeRecent;
    }
}