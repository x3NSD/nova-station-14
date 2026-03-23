using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Server.Custom.Prototypes;
using Robust.Server.Player;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.ContentPack;

namespace Content.Server.Custom.Systems;

public sealed class AutoAdminSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly IResourceManager _resourceManager = default!;

    private CustomAdminConfig? _config;

    public override void Initialize()
    {
        base.Initialize();
        LoadConfig();
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    private void LoadConfig()
    {
        var path = new ResPath("/Prototypes/!NS14/admin_config.yml");
        if (_resourceManager.TryContentFileRead(path, out var stream))
        {
            _config = _serialization.Deserialize<CustomAdminConfig>(stream);
        }
        else
        {
            Log.Error("Failed to load admin config");
            _config = new CustomAdminConfig();
        }
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Connected)
            _ = AssignRolesAsync(e.Session);
    }

    private async Task AssignRolesAsync(ICommonSession session)
    {
        if (_config.UserIdMapping.TryGetValue(session.UserId.ToString(), out var flags))
        {
            await AssignAdminRole(session, flags, "UserId");
            return;
        }

        var discordRole = await GetDiscordRole(session);
        if (!string.IsNullOrEmpty(discordRole) && _config.DiscordRoleMapping.TryGetValue(discordRole, out flags))
            await AssignAdminRole(session, flags, $"Discord: {discordRole}");
    }

    private async Task<string?> GetDiscordRole(ICommonSession session) => null;

    private async Task AssignAdminRole(ICommonSession session, AdminFlags flags, string reason)
    {
        var existingData = _adminManager.GetAdminData(session);
        if (existingData?.Flags.HasFlag(flags) == true) return;

        if (flags.HasFlag(AdminFlags.Host))
        {
            var hasHost = _playerManager.Sessions.Any(s =>
            {
                var data = _adminManager.GetAdminData(s);
                return data?.Flags.HasFlag(AdminFlags.Host) == true;
            });
            if (hasHost) return;
        }

        var adminData = new AdminData { Flags = flags, Title = GetRoleTitle(flags), Active = true };

        if (existingData != null)
            await _dbManager.UpdateAdmin(session.UserId, adminData);
        else
            await _dbManager.AddAdmin(session.UserId, adminData, null);

        _adminManager.ReloadAdmin(session);
        Log.Info($"Assigned {GetRoleTitle(flags)} to {session.Name} ({reason})");
    }

    private string GetRoleTitle(AdminFlags flags)
    {
        if (flags.HasFlag(AdminFlags.Host)) return "Основатель";
        if (flags.HasFlag(AdminFlags.ManageRoles)) return "Администратор";
        if (flags.HasFlag(AdminFlags.Admin)) return "Администратор";
        if (flags.HasFlag(AdminFlags.Moderator)) return "Модератор";
        if (flags.HasFlag(AdminFlags.Adminhelp)) return "Помощник";
        return "Персонал";
    }
}