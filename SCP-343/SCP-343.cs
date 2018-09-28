using SCP_343Logic;
using Smod2;
using Smod2.Attributes;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.API;
using System;
using System.Collections.Generic;

namespace SCP_343
{
	[PluginDetails(
	author = "Mith",
	name = "SCP-343",
	description = "SCP-343 is a passive immortal D-Class Personnel. He spawns with one Flashlight and any item he picks up is instantly morphed into a Flashlight. He seeks to help out who he deems worthy.",
	id = "Mith.SCP-343",
	version = "1.02",
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

            this.AddConfig(new Smod2.Config.ConfigSetting("SCP343_spawnchance", 10f, Smod2.Config.SettingType.FLOAT, true, "Percent chance for SPC-343 to spawn at the start of the round."));
            this.AddConfig(new Smod2.Config.ConfigSetting("SCP343_flashlights", false, Smod2.Config.SettingType.BOOL, true, "Should SPC-343 turn everything into flashlights?"));
        }
    }
}

namespace SCP_343Logic
{
	public class MainLogic : IEventHandlerPlayerPickupItem,IEventHandlerRoundStart,IEventHandlerDoorAccess, IEventHandlerSetRole
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
				TheChosenOne.SetGodmode(true);
                TheChosenOne.SetRank("red", "SCP-343");
				plugin.Info(TheChosenOne.Name + " is the Chosen One!");
			}
		}

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if(ev.Player.TeamRole.Role == Role.TUTORIAL)
			{
				ev.Items.Add(Smod2.API.ItemType.FLASHLIGHT);
			}
		}

		public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
		{
			if (ev.Player.TeamRole.Role == Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_flashlights",false,false) == true)
			{
					ev.ChangeTo = Smod2.API.ItemType.FLASHLIGHT;
			}else if (ev.Player.TeamRole.Role == Role.TUTORIAL && ConfigManager.Manager.Config.GetBoolValue("SCP343_flashlights", false, false) == false && PluginManager.Manager.Server.Round.Duration >= 3)
            {
                ev.Allow = false;
            } // duration here so 343 can have his first flashlight.
		}

		public void OnDoorAccess(PlayerDoorAccessEvent ev)
		{
            if (ev.Player.TeamRole.Role == Role.TUTORIAL && PluginManager.Manager.Server.Round.Duration >= 300)
			{
				ev.Allow = true;
			}
		}
	}
}