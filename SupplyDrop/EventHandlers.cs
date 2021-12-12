using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using MEC;
using Respawning;
using UnityEngine;
using Map = Exiled.API.Features.Map;
using Object = UnityEngine.Object;
using ItemExtensions = Exiled.API.Extensions.ItemExtensions;
using SupplyDrop.ConfigObjects;

namespace SupplyDrop
{
    public class EventHandlers
    {
        private readonly SupplyDrop pl;
        public EventHandlers(SupplyDrop plugin) => this.pl = plugin;

        private int chopper_dropsNumber = 0;
        private int car_dropsNumber = 0;

        public List<CoroutineHandle> coroutines = new List<CoroutineHandle>();

        internal void RoundStart()
        {
            foreach (CoroutineHandle handle in coroutines)
                Timing.KillCoroutines(handle);
            coroutines.Clear();
            Log.Info("Starting Chopper Coroutine.");
            coroutines.Add(Timing.RunCoroutine(ChopperThread(), "ChopperThread"));
            Log.Info("Starting Car Coroutine.");
            coroutines.Add(Timing.RunCoroutine(CarThread(), "CarThread"));
        }

        internal void WaitingForPlayers()
        {
            foreach (CoroutineHandle handle in coroutines)
                Timing.KillCoroutines(handle);
            coroutines.Clear();
        }

        public IEnumerator<float> ChopperThread()
        {
            while (Round.IsStarted)
            {
                if ((Server.PlayerCount >= pl.Config.MinPlayers) && ((pl.Config.ChopperDropsLimit == -1) || (pl.Config.ChopperDropsLimit > chopper_dropsNumber)))
                {
                    yield return Timing.WaitForSeconds(pl.Config.ChopperTime); // Wait seconds (10 minutes by default)
                    Log.Info("Spawning chopper!");

                    RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, SpawnableTeamType.NineTailedFox);

                    Map.Broadcast(pl.Config.ChopperBroadcastTime, pl.Config.ChopperBroadcast);

                    yield return Timing.WaitForSeconds(15); // Wait 15 seconds

                    Vector3 spawn = Exiled.API.Extensions.RoleExtensions.GetRandomSpawnProperties(RoleType.NtfPrivate).Item1;

                    if (pl.Config.MtfItems == null)
                    {
                        Log.Warn("MtfItems config is null. Check your config for any errors.");
                        break;
                    }

                    //Thanks to JesusQC for his help with making the entire plugin work. Love you
                    //Honorable mention - sanyae2439 for "hell code".
                    System.Random random = Exiled.Loader.Loader.Random;
                    foreach (var dropItems in pl.Config.MtfItems) 
                    {
                        int spawned = 0;
                        Item item = new Item(dropItems.Item);
                        if (ItemExtensions.IsAmmo(item.Type) && (pl.Config.ChopperPosAmmo != Vector3.zero)) spawn = pl.Config.ChopperPosAmmo;
                        if (ItemExtensions.IsArmor(item.Type) && (pl.Config.ChopperPosArmors != Vector3.zero)) spawn = pl.Config.ChopperPosArmors;
                        if (ItemExtensions.IsKeycard(item.Type) || ItemExtensions.IsMedical(item.Type) || ItemExtensions.IsUtility(item.Type) || ItemExtensions.IsScp(item.Type) && (pl.Config.ChopperPosItems != Vector3.zero)) spawn = pl.Config.ChopperPosItems;
                        if (ItemExtensions.IsWeapon(item.Type, true) || ItemExtensions.IsThrowable(item.Type) && (pl.Config.ChopperPosWeapons != Vector3.zero)) spawn = pl.Config.ChopperPosWeapons;
                        Log.Debug($"Coordinates choosed for {dropItems.Item} - {spawn}", pl.Config.Debug);
                        int r = random.Next(100);
                        Log.Debug($"Preparing to spawn {dropItems.Quantity} {dropItems.Item}(s) with a {dropItems.Chance} chance for each one.", pl.Config.Debug);
                        for (int i = 0; i < dropItems.Quantity; i++)
                        {
                            r = random.Next(100);
                            if (r <= dropItems.Chance)
                            {
                                item.Spawn(spawn, default);
                                spawned++;
                                Log.Debug($"Spawning {dropItems.Item}. Luck - {r}/{dropItems.Chance}", pl.Config.Debug);
                            }
                            else Log.Debug($"Item {dropItems.Item} didn't have enought luck.", pl.Config.Debug);
                        }
                        Log.Debug($"Spawned {spawned}/{dropItems.Quantity} {dropItems.Item}(s)", pl.Config.Debug);
                    }

                    chopper_dropsNumber++;
                    Log.Debug($"Drops used - {chopper_dropsNumber}/{pl.Config.ChopperDropsLimit}", pl.Config.Debug);
                    yield return Timing.WaitForSeconds(15); // Wait 15 seconds to let the chopper leave.
                }
                else {
                    if (chopper_dropsNumber == pl.Config.ChopperDropsLimit) Log.Debug("Drops limit has been reached.", pl.Config.Debug);
                    yield return Timing.WaitForSeconds(60); // Wait 60 seconds for more players.
                }
            }
        }

        public IEnumerator<float> CarThread()
        {
            while (Round.IsStarted)
            {
                yield return Timing.WaitForSeconds(pl.Config.TimeDifference);

                if ((Server.PlayerCount >= pl.Config.MinPlayers) && ((pl.Config.CarDropsLimit == -1) || (pl.Config.CarDropsLimit > car_dropsNumber)))
                {
                    yield return Timing.WaitForSeconds(pl.Config.CarTime); // Wait seconds (10 minutes by default)
                    Log.Info("Spawning car!");

                    RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.Selection, SpawnableTeamType.ChaosInsurgency);

                    Map.Broadcast(pl.Config.CarBroadcastTime, pl.Config.CarBroadcast);

                    yield return Timing.WaitForSeconds(15); // Wait 15 seconds

                    Vector3 spawn = Exiled.API.Extensions.RoleExtensions.GetRandomSpawnProperties(RoleType.ChaosRifleman).Item1;


                    if (pl.Config.ChaosItems == null)
                    {
                        Log.Warn("ChaosItems config is null. Check your config for any errors.");
                        break;
                    }

                    System.Random random = Exiled.Loader.Loader.Random;
                    foreach (var dropItems in pl.Config.ChaosItems)
                    {
                        int spawned = 0;
                        Item item = new Item(dropItems.Item);
                        if (ItemExtensions.IsAmmo(item.Type) && (pl.Config.CarPosAmmo != Vector3.zero)) spawn = pl.Config.CarPosAmmo;
                        if (ItemExtensions.IsArmor(item.Type) && (pl.Config.CarPosArmors != Vector3.zero)) spawn = pl.Config.CarPosArmors;
                        if (ItemExtensions.IsKeycard(item.Type) || ItemExtensions.IsMedical(item.Type) || ItemExtensions.IsUtility(item.Type) || ItemExtensions.IsScp(item.Type) && (pl.Config.CarPosItems != Vector3.zero)) spawn = pl.Config.CarPosItems;
                        if (ItemExtensions.IsWeapon(item.Type, true) || ItemExtensions.IsThrowable(item.Type) && (pl.Config.CarPosWeapons != Vector3.zero)) spawn = pl.Config.CarPosWeapons;
                        Log.Debug($"Coordinates choosed for {dropItems.Item} - {spawn}", pl.Config.Debug);
                        int r = random.Next(100);
                        Log.Debug($"Preparing to spawn {dropItems.Quantity} {dropItems.Item}(s) with a {dropItems.Chance} chance for each one.", pl.Config.Debug);
                        for (int i = 0; i < dropItems.Quantity; i++)
                        {
                            r = random.Next(100);
                            if (r <= dropItems.Chance)
                            {
                                item.Spawn(spawn, default);
                                spawned++;
                                Log.Debug($"Spawning {dropItems.Item}. Luck - {r}/{dropItems.Chance}", pl.Config.Debug);
                            }
                            else Log.Debug($"Item {dropItems.Item} didn't have enought luck.", pl.Config.Debug);
                        }
                        Log.Debug($"Spawned {spawned}/{dropItems.Quantity} {dropItems.Item}(s)", pl.Config.Debug);
                    }

                    car_dropsNumber++;
                    Log.Debug($"Drops used - {car_dropsNumber}/{pl.Config.CarDropsLimit}", pl.Config.Debug);
                    yield return Timing.WaitForSeconds(15); // Wait 15 seconds to let the car leave.
                }
                else
                {
                    if (car_dropsNumber == pl.Config.CarDropsLimit) Log.Debug("Drops limit has been reached.", pl.Config.Debug);
                    yield return Timing.WaitForSeconds(60); // Wait 60 seconds for more players.
                }
            }
        }
    }
}
