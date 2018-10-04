using Smod2;
using Smod2.API;
using Smod2.Commands;
using System;
using System.Collections.Generic;

namespace SCP_343
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
                Player TheChosenOne = null;
                List<Smod2.API.Player> iList = new List<Smod2.API.Player>();
                List<Int32> iListPlayerId = new List<Int32>();
                iList = PluginManager.Manager.Server.GetPlayers();
                foreach (Player Playa in iList)
                {
                    plugin.Info(Playa.PlayerId.ToString() + " playerid test");
                    plugin.Info(args[0] + " arg test");
                    if (int.TryParse(args[0], out int playaID))
                    {
                        if (Playa.PlayerId == playaID)
                        {
                            TheChosenOne = Playa;
                            TheChosenOne.ChangeRole(Smod2.API.Role.TUTORIAL, true, false);
                            if (ConfigManager.Manager.Config.GetIntValue("SCP343_HP", -1, false) == -1)
                            {
                                TheChosenOne.SetGodmode(true);
                            }
                            else { TheChosenOne.SetHealth(ConfigManager.Manager.Config.GetIntValue("SCP343_HP", -1, false)); }

                            TheChosenOne.SetRank("red", "SCP-343");
                            return new string[] { "Made " + Playa.Name + " SCP343!" };
                        }
                        else { return new string[] { "Couldn't find player." }; }
                    }
                    else { return new string[] { "Couldn't parse it dawg" }; }
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
            return "SCP343_Version";
        }

        public string[] OnCall(ICommandSender sender, string[] args)
        {
            return new string[] { "This is version " + plugin.Details.version };
        }
    }
}
