using Content.Server.Administration.Managers;
using Content.Server.EUI;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared.Administration;
using Content.Shared.GameTicking;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.Station;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.ManageRoles)]
    public sealed class SpawnRoleCommand : LocalizedEntityCommands
    {
        [Dependency] private readonly StationSpawningSystem _spawning = default!;
        [Dependency] private readonly StationSystem _stations = default!;
        [Dependency] private readonly GameTicker _gameTicker = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly MindSystem _mindSystem = default!;
        [Dependency] private readonly TransformSystem _transformSystem = default!;

        public override string Command => "spawnrole";
        public override string Description => "Spawn a player with a specific role";

        public override void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length < 2)
            {
                shell.WriteLine("Usage: spawnplayerjob <entityId> <jobId>");
                return;
            }

            if (!int.TryParse(args[0], out var entInt))
            {
                shell.WriteLine("Entity ID must be a number.");
                return;
            }

            var nent = new NetEntity(entInt);

            if (!EntityManager.TryGetEntity(nent, out var target))
            {
                shell.WriteLine("Invalid entity ID.");
                return;
            }

            var jobId = args[1];

            if (!_prototypeManager.TryIndex<JobPrototype>(jobId, out var jobPrototype))
            {
                shell.WriteLine($"Invalid job ID: {jobId}");
                return;
            }

            if (!EntityManager.TryGetComponent<ActorComponent>(target.Value, out var actorComponent))
            {
                shell.WriteLine("Target entity must have an ActorComponent (be a player).");
                return;
            }

            var player = actorComponent.PlayerSession;

            if (!_transformSystem.TryGetMapOrGridCoordinates(target.Value, out var coords))
            {
                shell.WriteLine("Failed to get coordinates for spawning.");
                return;
            }

            var stationUid = _stations.GetOwningStation(target.Value);
            var profile = _gameTicker.GetPlayerProfile(player);

            // Spawn the player with the specified job
            var mobUid = _spawning.SpawnPlayerMob(coords.Value, jobId, profile, stationUid);

            // Transfer the mind from the old entity to the new one
            if (EntityManager.TryGetComponent<MindContainerComponent>(target.Value, out var mindContainer) &&
                mindContainer.Mind != null)
            {
                _mindSystem.TransferTo(mindContainer.Mind.Value, mobUid, true);
            }

            shell.WriteLine($"Successfully spawned player {player.Name} as {jobPrototype.Name} at {coords.Value}.");
        }
    }
}