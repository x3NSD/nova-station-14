using Content.Shared.Implants;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Server.Jobs;

/// <summary>
/// Adds implants on spawn to the entity
/// </summary>
[UsedImplicitly]
[DataDefinition]
public sealed partial class AddImplantSpecial : JobSpecial
{
    [DataField]
    public HashSet<EntProtoId> Implants { get; private set; } = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var implantSystem = entMan.System<SharedSubdermalImplantSystem>();
        implantSystem.AddImplants(mob, Implants);
    }
}
