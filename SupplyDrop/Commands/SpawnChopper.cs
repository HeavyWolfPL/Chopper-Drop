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
    internal class SpawnChopper : ICommand
    {
        public string Command { get; } = "spawnchopper";

        public string[] Aliases { get; } = new[] { "sd_spawnchopper", "spawnmtfdrop" };

        public string Description { get; } = "Force spawn MTF supply drop. Bypasses drop limit";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("supplydrop.spawnchopper"))
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
                response = "<color=green>MTF Supply Drop commissioned.";

                RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, SpawnableTeamType.NineTailedFox);

                Map.Broadcast(SupplyDrop.Singleton.Config.ChopperBroadcastTime, SupplyDrop.Singleton.Config.ChopperBroadcast);


                Vector3 spawn = RoleExtensions.GetRandomSpawnProperties(RoleType.NtfPrivate).Item1;

                if (SupplyDrop.Singleton.Config.MtfItems == null)
                {
                    Log.Warn("MtfItems config is null. Check your config for any errors.");
                    response = "MtfItems config is null.";
                    return false;
                }

                System.Random random = Exiled.Loader.Loader.Random;
                foreach (var dropItems in SupplyDrop.Singleton.Config.MtfItems)
                {
                    int spawned = 0;
                    Item item = Item.Create(dropItems.Item);
                    if (ItemExtensions.IsAmmo(item.Type) && (SupplyDrop.Singleton.Config.ChopperPosAmmo != Vector3.zero)) spawn = SupplyDrop.Singleton.Config.ChopperPosAmmo;
                    if (ItemExtensions.IsArmor(item.Type) && (SupplyDrop.Singleton.Config.ChopperPosArmors != Vector3.zero)) spawn = SupplyDrop.Singleton.Config.ChopperPosArmors;
                    if (ItemExtensions.IsKeycard(item.Type) || ItemExtensions.IsMedical(item.Type) || ItemExtensions.IsUtility(item.Type) || ItemExtensions.IsScp(item.Type) && (SupplyDrop.Singleton.Config.ChopperPosItems != Vector3.zero)) spawn = SupplyDrop.Singleton.Config.ChopperPosItems;
                    if (ItemExtensions.IsWeapon(item.Type, true) || ItemExtensions.IsThrowable(item.Type) && (SupplyDrop.Singleton.Config.ChopperPosWeapons != Vector3.zero)) spawn = SupplyDrop.Singleton.Config.ChopperPosWeapons;
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
