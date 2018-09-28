using SCP_343Logic;
using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.EventSystem.Events;
using Smod2.API;
using Smod2.Commands;
using System;
using System.Collections.Generic;

namespace SCP_343
{
	[PluginDetails(
	author = "Mith",
	name = "SCP-343",
	description = "SCP-343 is a passive immortal D-Class Personnel. He spawns with one Flashlight and any item he picks up is instantly morphed into a Flashlight. He seeks to help out who he deems worthy.",
	id = "Mith.SCP-343",
	version = "1.08",
	SmodMajor = 3,
	SmodMinor = 1,
	SmodRevision = 18
	)]
	public class Main : Plugin
	{
		public override void OnDisable()
		{
			this.Info("SCP-343 has been Disabled.");
		}

		public override void OnEnable()
		{

            this.Info("SCP-343 has been Enabled.");
		}

        public override void Register()
		{
			this.AddEventHandler(typeof(IEventHandlerPlayerPickupItem), new MainLogic(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerRoundStart), new MainLogic(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerDoorAccess), new MainLogic(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerSetRole), new MainLogic(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerWarheadStartCountdown), new MainLogic(this), Priority.Normal);
			this.AddEventHandler(typeof(IEventHandlerWarheadStopCountdown), new MainLogic(this), Priority.Normal);
            this.AddCommand("SpawnSCP343", new SCP_343Commands.SpawnSCP343(this));
            this.AddCommand("SCP343_Version", new SCP_343Commands.SCP343_Version(this));

            this.AddConfig(new Smod2.Config.ConfigSetting("SCP343_spawnchance", 10f, Smod2.Config.SettingType.FLOAT, true, "Percent chance for SPC-343 to spawn at the start of the round."));
            this.AddConfig(new Smod2.Config.ConfigSetting("SCP343_flashlights", false, Smod2.Config.SettingType.BOOL, true, "Should SPC-343 turn everything into flashlights?"));
            this.AddConfig(new Smod2.Config.ConfigSetting("SCP343_opendoortime", 300, Smod2.Config.SettingType.NUMERIC, true, "How long in seconds till SPC-343 can open any door."));
            this.AddConfig(new Smod2.Config.ConfigSetting("SCP343_HP", -1, Smod2.Config.SettingType.NUMERIC, true, "How much health should SCP-343 have. Set to -1 for GodMode."));
            this.AddConfig(new Smod2.Config.ConfigSetting("SCP343_nuke_interact", true, Smod2.Config.SettingType.BOOL, true, "Should SPC-343 beable to interact with the nuke."));
        }
    }
}

namespace SCP_343Logic
{
	public class MainLogic : IEventHandlerPlayerPickupItem,IEventHandlerRoundStart,IEventHandlerDoorAccess, IEventHandlerSetRole, IEventHandlerPlayerHurt, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown

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
                if(ConfigManager.Manager.Config.GetIntValue("SCP343_HP", -1, false) == -1)
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
			if(ev.Player.TeamRole.Role == Role.TUTORIAL)
			{
				ev.Items.Add(Smod2.API.ItemType.FLASHLIGHT);
			}// 1st flashlight
		}

		public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
		{
			if (ev.Player.TeamRole.Role == Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_flashlights",false,false) == true)
			{
			    ev.ChangeTo = Smod2.API.ItemType.FLASHLIGHT;
			}else if (ev.Player.TeamRole.Role == Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_flashlights", false, false) == false && PluginManager.Manager.Server.Round.Duration >= 3)
            {
                ev.Item.Drop();//Idk how to not have it picked up
                ev.Allow = false;
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
                else { ev.Cancel = true; }
            }
        }

        public void OnStopCountdown(WarheadStopEvent ev)
        {
            if(ev.Activator != null)
            {
                if (ev.Activator.TeamRole.Role == Smod2.API.Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_nuke_interact", true, false) == false)
                {
                    ev.Cancel = false;
                }
                else { ev.Cancel = true; }
            }
        }
    }
}

namespace SCP_343Commands
{
    class SpawnSCP343 : ICommandHandler
    {
        private Plugin plugin;
        public SpawnSCP343(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public string GetCommandDescription()
        {
            return "This command spawns in someone as SPC-343.";
        }

        public string GetUsage()
        {
            return "SpawnSCP343 PlayerID";
        }

        public string[] OnCall(ICommandSender sender, string[] args)
        {
            if (args.Length > 0)
            {
                List<Smod2.API.Player> iList = new List<Smod2.API.Player>();
                List<Int32> iListPlayerId = new List<Int32>();
                iList = PluginManager.Manager.Server.GetPlayers();
                foreach (Player Playa in iList)
                {
                    if (Playa.PlayerId.ToString() == args[0])
                    {
                        Player TheChosenOne = Playa;
                        TheChosenOne.ChangeRole(Smod2.API.Role.TUTORIAL, true, false);
                        if (ConfigManager.Manager.Config.GetIntValue("SCP343_HP", -1, false) == -1)
                        {
                            TheChosenOne.SetGodmode(true);
                        }
                        else { TheChosenOne.SetHealth(ConfigManager.Manager.Config.GetIntValue("SCP343_HP", -1, false)); }

                        TheChosenOne.SetRank("red", "SCP-343");
                        return new string[] { "Made " + Playa.Name + " SCP343!" };
                    }
                    else { return new string[] { "Invalid PlayerID" }; }

                }
                return new string[] { "" };
            }
            else { return new string[] { "You must put a playerid." }; }

        }
    }
    class SCP343_Version : ICommandHandler
    {
        private Plugin plugin;
        public SCP343_Version(Plugin plugin)
        {
            this.plugin = plugin;
        }

        public string GetCommandDescription()
        {
            return "Version History for this plugin.";
        }

        public string GetUsage()
        {
            return "SpawnSCP343 PlayerID";
        }

        public string[] OnCall(ICommandSender sender, string[] args)
        {
            return new string[]{"This is version " + plugin.Details.version};
        }
    }
}
    