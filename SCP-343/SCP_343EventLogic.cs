using Smod2;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SCP_343
{
    public class MainLogic : IEventHandlerPlayerPickupItem, IEventHandlerRoundStart, IEventHandlerDoorAccess, IEventHandlerSetRole, IEventHandlerPlayerHurt, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown
    {
        Smod2.API.Player TheChosenOne;

        Random RNG = new Random();

        private Plugin plugin;
        public MainLogic(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public void OnRoundStart(RoundStartEvent ev)
        {
            List<Smod2.API.Player> PlayerList = new List<Smod2.API.Player>();
            List<Smod2.API.Player> DClassList = new List<Smod2.API.Player>();

            int randomNumber = RNG.Next(0, 100);
            if (randomNumber <= ConfigManager.Manager.Config.GetFloatValue("SCP343_spawnchance", 10f, false))
            {
                TheChosenOne = null;
                PlayerList = plugin.pluginManager.Server.GetPlayers();
                foreach (Smod2.API.Player Playa in PlayerList)
                {
                    if (Playa.TeamRole.Role == Smod2.API.Role.CLASSD)
                    {
                        DClassList.Add(Playa);
                    }
                }

                TheChosenOne = DClassList[RNG.Next(DClassList.Count)];
                TheChosenOne.ChangeRole(Smod2.API.Role.TUTORIAL, true, false);
                if (ConfigManager.Manager.Config.GetIntValue("SCP343_HP", -1, false) == -1)
                {
                    TheChosenOne.SetGodmode(true);
                }
                else { TheChosenOne.SetHealth(ConfigManager.Manager.Config.GetIntValue("SCP343_HP", -1, false)); }

                TheChosenOne.SetRank("red", "SCP-343");
                //plugin.Info(TheChosenOne.Name + " is the Chosen One!");
            }
        }

        public void OnSetRole(PlayerSetRoleEvent ev)
        {
            if (ev.Player.TeamRole.Role == Role.TUTORIAL)
            {
                ev.Items.Add(Smod2.API.ItemType.FLASHLIGHT);
            }// 1st flashlight
        }

        public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
        {
            if (ev.Player.TeamRole.Role == Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_itemconverttoggle", false, false) == true)
            {
                int[] itemAllowedList = ConfigManager.Manager.Config.GetIntListValue("SCP343_itemconversionlist", new int[] { 13, 16, 20, 21, 23, 24, 25, 26 }, false);

                if (itemAllowedList.Contains((int)ev.Item.ItemType))
                {
                    int[] itemConvertList = ConfigManager.Manager.Config.GetIntListValue("SCP343_itemconvertedlist", new int[] { 15 }, false);
                    ev.ChangeTo = (ItemType)itemConvertList[RNG.Next(itemConvertList.Length)];
                }
                
                int[] itemBlackList = ConfigManager.Manager.Config.GetIntListValue("SCP343_itemdroplist", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 14, 17, 19, 22, 27, 28, 29 }, false);
                if (itemBlackList.Contains((int)ev.Item.ItemType))
                {
                    ev.Item.Drop();//Idk how to not have it picked up
                    ev.Allow = false;// This deletes the item :(
                }
            }
            else if (ev.Player.TeamRole.Role == Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_itemconverttoggle", false, false) == false && PluginManager.Manager.Server.Round.Duration >= 3)
            {
                int[] itemWhiteList = ConfigManager.Manager.Config.GetIntListValue("SCP343_alloweditems", new int[] { 12, 15 }, false);
                if (!(itemWhiteList.Contains((int)ev.Item.ItemType)))
                {
                    ev.Item.Drop();//Idk how to not have it picked up
                    ev.Allow = false;// This deletes the item :(
                }

            } // duration here so 343 can have his first flashlight.
        }

        public void OnDoorAccess(PlayerDoorAccessEvent ev)
        {
            if (ev.Player.TeamRole.Role == Role.TUTORIAL && PluginManager.Manager.Server.Round.Duration >= ConfigManager.Manager.Config.GetFloatValue("SCP343_opendoortime", 300, false))
            {
                ev.Allow = true;
            }// Allows 343 to open every door after a set amount of seconds.
        }

        public void OnPlayerHurt(PlayerHurtEvent ev)
        {
            if (ev.Player.TeamRole.Role == Smod2.API.Role.TUTORIAL && ev.Attacker.TeamRole.Team == Smod2.API.Team.SCP)
            {
                ev.Damage = 0;
                plugin.Info((DamageType)1 + " ");
            }
        }

        public void OnStartCountdown(WarheadStartEvent ev)
        {
            if (ev.Activator != null)
            {
                if (ev.Activator.TeamRole.Role == Smod2.API.Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_nuke_interact", true, false) == false)
                {
                    ev.Cancel = false;
                }
                else if (ev.Activator.TeamRole.Role == Smod2.API.Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_nuke_interact", true, false) == true) { ev.Cancel = true; }
            }
        }

        public void OnStopCountdown(WarheadStopEvent ev)
        {
            if (ev.Activator != null)
            {
                if (ev.Activator.TeamRole.Role == Smod2.API.Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_nuke_interact", true, false) == false)
                {
                    ev.Cancel = false;
                }
                else if (ev.Activator.TeamRole.Role == Smod2.API.Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_nuke_interact", true, false) == true) { ev.Cancel = true; }
            }
        }
    }
}