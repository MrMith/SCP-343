using Smod2;
using Smod2.API;
using Smod2.Commands;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SCP_343
{
	class SpawnSCP343 : ICommandHandler
	{
		EventLogic eventLogic;
		private readonly Plugin plugin;
		public SpawnSCP343(Plugin plugin, EventLogic EventLogicMain)
		{
			this.plugin = plugin;
			eventLogic = EventLogicMain;
		}

		public string GetCommandDescription()
		{
			return "This command spawns in someone as SPC-343.";
		}

		public string GetUsage()
		{
			return "spawnscp343 PlayerID";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (args.Length > 0)
			{
				PluginOptions pluginOptions = eventLogic._343Config;
				Dictionary<string, PlayerInfo> Active343AndBadgeDict = eventLogic.Active343AndBadgeDict;

				Regex regex = new Regex(@"\D+");
				string PlayerIDString = regex.Replace(args[0],"");

				foreach (Player player in PluginManager.Manager.Server.GetPlayers())
				{
					if (Int32.TryParse(PlayerIDString, out int PlayerIDInt))
					{
						if (player.PlayerId == PlayerIDInt)
						{
							player.ChangeRole(Smod2.API.Role.CLASSD, true, true, true);
							player.GiveItem(ItemType.FLASHLIGHT);

							if (pluginOptions.SCP343_HP != -1)
							{
								player.SetHealth(pluginOptions.SCP343_HP);
							}

							if (player.GetUserGroup().BadgeText == null)
							{
								Active343AndBadgeDict.Add(player.SteamId, new PlayerInfo("", ""));
							}
							else
							{
								Active343AndBadgeDict.Add(player.SteamId, new PlayerInfo(player.GetUserGroup().BadgeText, player.GetUserGroup().Color));
							}

							if (pluginOptions.SCP343_shouldbroadcast)
							{
								player.PersonalBroadcast(5, "You're SCP-343! Check your console for more information about SCP-343.", true);
								player.SendConsoleMessage("----------------------------------------------------------- \n" + pluginOptions.SCP343_broadcastinfo + "\n ----------------------------------------------------------- ");
							}

							player.SetRank("red", "SCP-343");
							return new string[] { "Made " + player.Name + " SCP343!" };
						}
					}
				}
				return new string[] { "Couldn't parse playerid." };
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
			return "Current version for this plugin.";
		}

		public string GetUsage()
		{
			return "scp343_version";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			return new string[] { "This is version " + plugin.Details.version };
		}
	}

	class SCP343_Disable : ICommandHandler
	{
		private Plugin plugin;

		public SCP343_Disable(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public string GetCommandDescription()
		{
			return "Enables or disables SCP-343.";
		}

		public string GetUsage()
		{
			return "scp343_disable";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			plugin.PluginManager.DisablePlugin(plugin);
			return new string[] { "Disabled " + plugin.Details.id };
		}
	}
}
