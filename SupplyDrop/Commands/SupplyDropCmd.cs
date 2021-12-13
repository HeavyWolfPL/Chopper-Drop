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
    internal class SupplyDropCmd : ICommand
    {
        public string Command { get; } = "supplydrop";

        public string[] Aliases { get; } = new[] { "sd" };

        public string Description { get; } = "Manage SupplyDrop drops.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("supplydrop.manage"))
            {
                response = "<color=red>No permission!</color>";
                return false;
            }
            else if (arguments.Count < 1)
            {
                response = $"<color=yellow>Usage: {Command} <status/enable/disable/chopper/car>";
                return false;
            }
            else
            {
                switch (arguments.At(0))
                {
                    case "enable":
                        if (EventHandlers.chopperEnabled && EventHandlers.carEnabled && EventHandlers.dropsEnabled)
                        {
                            response = "Supply Drops are already enabled.";
                            return false;
                        }
                        EventHandlers.dropsEnabled = true;
                        EventHandlers.carEnabled = true;
                        EventHandlers.chopperEnabled = true;
                        response = "Supply Drops <color=green>enabled</color>.";
                        return true;
                    case "disable":
                        if (!EventHandlers.chopperEnabled && !EventHandlers.carEnabled && !EventHandlers.dropsEnabled)
                        {
                            response = "Supply Drops are already disabled.";
                            return false;
                        }
                        EventHandlers.dropsEnabled = false;
                        EventHandlers.carEnabled = false;
                        EventHandlers.chopperEnabled = false;
                        response = "Supply Drops <color=red>disabled</color>.";
                        return true;
                    case "chopper":
                        if (!EventHandlers.chopperEnabled)
                        {
                            EventHandlers.chopperEnabled = true;
                            response = "Chopper Supply Drop <color=green>enabled</color>.";
                            return true;
                        }
                        EventHandlers.chopperEnabled = false;
                        response = "Chopper Supply Drop <color=red>disabled</color>.";
                        return true;
                    case "car":
                        if (!EventHandlers.carEnabled)
                        {
                            EventHandlers.carEnabled = true;
                            response = "Car Supply Drop <color=green>enabled</color>.";
                            return true;
                        }
                        EventHandlers.carEnabled = false;
                        response = "Car Supply Drop <color=red>disabled</color>.";
                        return true;
                    case "status":
                        response = $"Supply Drop Status: \n- Drops: {EventHandlers.dropsEnabled} \n- Chopper: {EventHandlers.chopperEnabled} \n- Car: {EventHandlers.carEnabled} \nTrue = Enabled \nFalse = Disabled.";
                        return true;
                    default:
                        response = "Argument not found. Available: status/enable/disable/chopper/car/";
                        return false;
                }
            }
        }
    }
}
