using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Shared.Administration;
using Content.Server.Custom.Prototypes;
using Robust.Server.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Custom.Systems;

public sealed class AutoAdminSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private CustomAdminConfigPrototype? _config;

    public override void Initialize()
    {
        base.Initialize();
        _config = _prototypeManager.Index<CustomAdminConfigPrototype>("CustomAdminConfig");
        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
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
        if (flags.HasFlag(AdminFlags.Host)) return "Host";
        if (flags.HasFlag(AdminFlags.ManageRoles)) return "Manager";
        if (flags.HasFlag(AdminFlags.Admin)) return "Admin";
        if (flags.HasFlag(AdminFlags.Moderator)) return "Moderator";
        if (flags.HasFlag(AdminFlags.Adminhelp)) return "Helper";
        return "Staff";
    }
}