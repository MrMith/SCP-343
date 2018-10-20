using Smod2;
using Smod2.API;
using Smod2.Commands;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SCP_343
{
	class SpawnSCP343 : ICommandHandler
	{
		int PlayerID;
		String PlayerIDString;
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
			return "spawnscp343 PlayerID";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			if (args.Length > 0)
			{
				foreach (Player Playa in PluginManager.Manager.Server.GetPlayers())
				{
					PlayerIDString = Regex.Match(args[0], @"\d+").Value;
					PlayerID = Int32.Parse(PlayerIDString);
					plugin.Info(Int32.Parse(PlayerIDString) + " regex test");
					plugin.Info(PlayerID + " regex test");
					plugin.Info(Playa.PlayerId.ToString() + " Playerid test");
					if (Playa.PlayerId == PlayerID)
					{
						Playa.ChangeRole(Smod2.API.Role.CLASSD,true,true,true);
						if (ConfigManager.Manager.Config.GetIntValue("scp343_hp", -1, false) == -1)
						{
							Playa.SetGodmode(true);
						}
						else { Playa.SetHealth(ConfigManager.Manager.Config.GetIntValue("scp343_hp", -1, false)); }
						SCP343.checkSteamIDIf343Dict[Playa.SteamId] = true;
						Playa.SetRank("red", "SCP-343");
						return new string[] { "Made " + Playa.Name + " SCP343!" };
					}
				}
				return new string[] { "" };
			}
			else { return new string[] { "You must put a playerid." }; }
		}//Checks if the player's playerid is equal to the one given in the command args.
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
			return "scp343_version";
		}

		public string[] OnCall(ICommandSender sender, string[] args)
		{
			return new string[] { "This is version " + plugin.Details.version };
		}
	}//Return version for debugging purposes.
}
