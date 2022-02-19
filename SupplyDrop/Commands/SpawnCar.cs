using System;
using CommandSystem;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.Permissions.Extensions;
using Respawning;
using UnityEngine;

namespace SupplyDrop.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    internal class SpawnCar : ICommand
    {
        public string Command { get; } = "spawncar";

        public string[] Aliases { get; } = new[] { "sd_spawncar", "spawnchaosdrop" };

        public string Description { get; } = "Force spawn Chaos Insurgency supply drop. Bypasses drop limit";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("supplydrop.spawncar"))
            {
                response = "<color=red>No permission!</color>";
                return false;
            }
            else if (!Round.IsStarted)
            {
                response = "<color=orange>Round hasn't started yet.</color>";
                return false;
            }
            else
            {
                response = "<color=green>Chaos Supply Drop commissioned.";

                RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, SpawnableTeamType.ChaosInsurgency);

                Map.Broadcast(SupplyDrop.Singleton.Config.CarBroadcastTime, SupplyDrop.Singleton.Config.CarBroadcast);

                Vector3 spawn = RoleExtensions.GetRandomSpawnProperties(RoleType.ChaosRifleman).Item1;


                if (SupplyDrop.Singleton.Config.ChaosItems == null)
                {
                    Log.Warn("ChaosItems config is null. Check your config for any errors.");
                    response = "ChaosItems config is null.";
                    return false;
                }

                System.Random random = Exiled.Loader.Loader.Random;
                foreach (var dropItems in SupplyDrop.Singleton.Config.ChaosItems)
                {
                    int spawned = 0;
                    Item item = Item.Create(dropItems.Item);
                    if (ItemExtensions.IsAmmo(item.Type) && (SupplyDrop.Singleton.Config.CarPosAmmo != Vector3.zero)) spawn = SupplyDrop.Singleton.Config.CarPosAmmo;
                    if (ItemExtensions.IsArmor(item.Type) && (SupplyDrop.Singleton.Config.CarPosArmors != Vector3.zero)) spawn = SupplyDrop.Singleton.Config.CarPosArmors;
                    if (ItemExtensions.IsKeycard(item.Type) || ItemExtensions.IsMedical(item.Type) || ItemExtensions.IsUtility(item.Type) || ItemExtensions.IsScp(item.Type) && (SupplyDrop.Singleton.Config.CarPosItems != Vector3.zero)) spawn = SupplyDrop.Singleton.Config.CarPosItems;
                    if (ItemExtensions.IsWeapon(item.Type, true) || ItemExtensions.IsThrowable(item.Type) && (SupplyDrop.Singleton.Config.CarPosWeapons != Vector3.zero)) spawn = SupplyDrop.Singleton.Config.CarPosWeapons;
                    Log.Debug($"Coordinates choosed for {dropItems.Item} - {spawn}", SupplyDrop.Singleton.Config.Debug);
                    int r = random.Next(100);
                    Log.Debug($"Preparing to spawn {dropItems.Quantity} {dropItems.Item}(s) with a {dropItems.Chance} chance for each one.", SupplyDrop.Singleton.Config.Debug);
                    for (int i = 0; i < dropItems.Quantity; i++)
                    {
                        r = random.Next(100);
                        if (r <= dropItems.Chance)
                        {
                            item.Spawn(spawn, default);
                            spawned++;
                            Log.Debug($"Spawning {dropItems.Item}. Luck - {r}/{dropItems.Chance}", SupplyDrop.Singleton.Config.Debug);
                        }
                        else Log.Debug($"Item {dropItems.Item} didn't have enought luck.", SupplyDrop.Singleton.Config.Debug);
                    }
                    Log.Debug($"Spawned {spawned}/{dropItems.Quantity} {dropItems.Item}(s)", SupplyDrop.Singleton.Config.Debug);
                }
                return true;
            }
        }
    }
}
