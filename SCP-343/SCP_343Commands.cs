using Smod2;
using Smod2.API;
using Smod2.Commands;
using System;
using System.Text.RegularExpressions;

namespace SCP_343
{
	class SpawnSCP343 : ICommandHandler
	{ 
		
		private readonly Plugin plugin;
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
			return "spawnscp343 PlayerID";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (args.Length > 0)
			{
				foreach (Player Playa in PluginManager.Manager.Server.GetPlayers())
				{
					string PlayerIDString = Regex.Match(args[0], @"\d+").Value;
					if (Int32.TryParse(PlayerIDString, out int PlayerIDInt))
					{
						if (Playa.PlayerId == PlayerIDInt)
						{
							Playa.ChangeRole(Smod2.API.Role.CLASSD, true, true, true);

							if (EventLogic._343Config.SCP343_HP == -1)
							{
								Playa.SetGodmode(true);
							}
							else { Playa.SetHealth(EventLogic._343Config.SCP343_HP); }

							SCP343.Active343AndBadgeDict.Add(Playa.SteamId, new SCP343.PlayerInfo(Playa.GetUserGroup().Name, Playa.GetUserGroup().Color));

							Playa.SetRank("red", "SCP-343");
							return new string[] { "Made " + Playa.Name + " SCP343!" };
						}
					}
					return new string[] {"Couldn't parse playerid."};
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
			Smod2.PluginManager.Manager.DisablePlugin(plugin.Details.id);
			return new string[] { "Disabled " + plugin.Details.id };
		}
	}
}
