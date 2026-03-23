using Content.Server.GameTicking;
using Content.Server.Voting.Managers;
using Content.Server.Custom.Prototypes;
using Content.Shared.Maps;
using Content.Shared.GameTicking;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.ContentPack;

namespace Content.Server.Custom.Systems;

public sealed class VoteSchedulerSystem : EntitySystem
{
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly IResourceManager _resourceManager = default!;

    private CustomVoteConfig? _config;
    private readonly HashSet<string> _recentMaps = new();
    private readonly HashSet<string> _recentPresets = new();
    private string? _selectedStationType;

    public override void Initialize()
    {
        base.Initialize();
        LoadConfig();
        SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }

    private void LoadConfig()
    {
        var path = new ResPath("/Prototypes/!NS14/vote_config.yml");
        if (_resourceManager.TryContentFileRead(path, out var stream))
        {
            _config = _serialization.Deserialize<CustomVoteConfig>(stream);
        }
        else
        {
            Log.Error("Failed to load vote config");
            _config = new CustomVoteConfig();
        }
    }

    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        if (!_config.StationTypeVote.Enabled) return;

        Timer.Spawn(TimeSpan.FromSeconds(_config.StationTypeVote.DelayAfterRoundEnd), () =>
        {
            StartStationTypeVote();
        });
    }

    private void StartStationTypeVote()
    {
        var options = _config.StationTypeVote.Options;
        if (options.Count == 0) return;

        _voteManager.CreateStandardVote(null, "Тип станции", options, StandardVoteType.Restart, _config.StationTypeVote.DisplayVotes, (winner) =>
        {
            _selectedStationType = winner;
            if (_config.MapVote.Enabled)
            {
                Timer.Spawn(TimeSpan.FromSeconds(_config.MapVote.DelayAfterStationType), () =>
                {
                    StartMapVote();
                });
            }
        });
    }

    private void StartMapVote()
    {
        var maps = _prototypeManager.EnumeratePrototypes<MapPrototype>()
            .Where(m => !_recentMaps.Contains(m.ID))
            .Select(m => GetLocalizedMapName(m.ID))
            .ToList();

        if (maps.Count == 0) return;

        _voteManager.CreateStandardVote(null, "Карта", maps, StandardVoteType.Map, _config.MapVote.DisplayVotes, (winner) =>
        {
            var originalId = GetMapIdFromLocalizedName(winner);
            _recentMaps.Add(originalId);
            if (_recentMaps.Count > _config.MapVote.ExcludeRecent)
                _recentMaps.Remove(_recentMaps.First());

            if (_config.PresetVote.Enabled)
            {
                Timer.Spawn(TimeSpan.FromSeconds(_config.PresetVote.DelayAfterMap), () =>
                {
                    StartPresetVote();
                });
            }
        });
    }

    private void StartPresetVote()
    {
        var presets = _prototypeManager.EnumeratePrototypes<GamePresetPrototype>()
            .Where(p => !_recentPresets.Contains(p.ID) && IsPresetForStationType(p.ID, _selectedStationType))
            .Select(p => GetLocalizedPresetName(p.ID))
            .ToList();

        if (presets.Count == 0) return;

        _voteManager.CreateStandardVote(null, "Режим игры", presets, StandardVoteType.Preset, _config.PresetVote.DisplayVotes, (winner) =>
        {
            var originalId = GetPresetIdFromLocalizedName(winner);
            _recentPresets.Add(originalId);
            if (_recentPresets.Count > _config.PresetVote.ExcludeRecent)
                _recentPresets.Remove(_recentPresets.First());
        });
    }

    private string GetLocalizedMapName(string mapId)
    {
        return mapId switch
        {
            "Saltern" => "Сольтерн",
            "Core" => "Ядро",
            "Meta" => "Мета",
            "Box" => "Коробка",
            _ => mapId
        };
    }

    private string GetMapIdFromLocalizedName(string localizedName)
    {
        return localizedName switch
        {
            "Сольтерн" => "Saltern",
            "Ядро" => "Core",
            "Мета" => "Meta",
            "Коробка" => "Box",
            _ => localizedName
        };
    }

    private string GetLocalizedPresetName(string presetId)
    {
        return presetId switch
        {
            "Traitor" => "Предатель",
            "Deathmatch" => "Смертельный матч",
            "Sandbox" => "Песочница",
            "Nukeops" => "Ядерные оперативники",
            _ => presetId
        };
    }

    private string GetPresetIdFromLocalizedName(string localizedName)
    {
        return localizedName switch
        {
            "Предатель" => "Traitor",
            "Смертельный матч" => "Deathmatch",
            "Песочница" => "Sandbox",
            "Ядерные оперативники" => "Nukeops",
            _ => localizedName
        };
    }

    private bool IsPresetForStationType(string presetId, string? stationType)
    {
        if (stationType == "Спокойный")
        {
            return presetId.Contains("Sandbox") || presetId.Contains("Extended");
        }
        else if (stationType == "РДМ")
        {
            return presetId.Contains("Traitor") || presetId.Contains("Deathmatch") || presetId.Contains("Nukeops");
        }
        return true;
    }
}