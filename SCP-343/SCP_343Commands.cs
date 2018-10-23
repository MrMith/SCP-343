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
					int PlayerIDInt = Int32.Parse(PlayerIDString);
					if (Playa.PlayerId == PlayerIDInt)
					{
						Playa.ChangeRole(Smod2.API.Role.CLASSD,true,true,true);
						
						if (plugin.GetConfigInt("scp343_hp") == -1)
						{
							Playa.SetGodmode(true);
						}
						else { Playa.SetHealth(plugin.GetConfigInt("scp343_hp")); }
						
						SCP343.checkSteamIDIf343Dict[Playa.SteamId] = true;
						SCP343.active343List.Add(Playa.SteamId);
						if(!SCP343.checkSteamIDforBadgeColor.ContainsKey(Playa.SteamId))
						{
							SCP343.checkSteamIDforBadgeColor[Playa.SteamId] = Playa.GetUserGroup().Color;
						}
						if(!SCP343.checkSteamIDforBadgeName.ContainsKey(Playa.SteamId))
						{
							SCP343.checkSteamIDforBadgeName[Playa.SteamId] = Playa.GetUserGroup().Name;
						}

						Playa.SetRank("red", "SCP-343");
						return new string[] { "Made " + Playa.Name + " SCP343!" };
					}
				}
				return new string[] { "" };
			}
			else { return new string[] { "You must put a playerid." }; }
		}
	}//Checks if the player's playerid is equal to the one given in the command args.

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
