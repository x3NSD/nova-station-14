using Content.Shared.Administration;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.Custom.Prototypes;

[DataDefinition]
public sealed partial class CustomAdminConfig
{
    [DataField]
    public Dictionary<string, AdminFlags> DiscordRoleMapping = new();

    [DataField]
    public Dictionary<string, AdminFlags> UserIdMapping = new();
}