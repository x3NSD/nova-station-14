using Content.Server.GameTicking;
using Content.Server.Voting.Managers;
using Content.Shared.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;

namespace Content.Server.Custom.Systems;

<summary>
</summary>
public sealed class AutoVoteSystem : EntitySystem
{
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    private bool _hasStartedVotes = false;

    public override void Initialize()
    {
        base.Initialize();

        _playerManager.PlayerCountChanged += OnPlayerCountChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _playerManager.PlayerCountChanged -= OnPlayerCountChanged;
    }

    private void OnPlayerCountChanged(object? sender, PlayerCountChangedEventArgs e)
    {
        if (_hasStartedVotes || e.NewCount < 1)
            return;

        if (_gameTicker.RunLevel != GameRunLevel.PreRoundLobby)
            return;

        StartAutoVotes();
        _hasStartedVotes = true;
    }

    private void StartAutoVotes()
    {
        if (_cfg.GetCVar(CCVars.VotePresetEnabled))
        {
            _voteManager.CreatePresetVote(null);
        }

        if (_cfg.GetCVar(CCVars.VoteMapEnabled))
        {
            _voteManager.CreateMapVote(null);
        }
    }
}