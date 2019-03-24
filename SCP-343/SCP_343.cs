using Smod2;
using Smod2.Attributes;

namespace SCP_343
{
	[PluginDetails(
	author = "Mith",
	name = "SCP-343",
	description = "SCP-343 is a passive immortal D-Class Personnel. He spawns with one Flashlight and any weapon he picks up is morphed to prevent violence. He seeks to help out who he deems worthy.",
	id = "Mith.SCP-343",
	version = "1.3.9",
	SmodMajor = 3,
	SmodMinor = 4,
	SmodRevision = 0
	)]
	class SCP343 : Plugin
	{
		public SCP343 plugin;

        public EventLogic eventLogicVar;

		public override void OnDisable()
		{
			this.Info("SCP-343 "+ this.Details.version + " has been Disabled.");
		}
		
		public override void OnEnable()
		{
			plugin = this;
			this.Info("SCP-343 "+ this.Details.version +" has been Enabled.");
		}

		public override void Register()
		{
			this.AddEventHandlers(eventLogicVar = new EventLogic(this));

			this.AddCommand("spawn343", new SpawnSCP343(this, eventLogicVar));
			this.AddCommand("scp343_version", new SCP343_Version(this));
			this.AddCommand("scp343_disable", new SCP343_Disable(this));

			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_spawnchance", 10f, true, "Percent chance for SPC-343 to spawn at the start of the round."));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_itemconverttoggle", false, true, "Should SPC-343 convert items?"));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_opendoortime", 300, true, "How long in seconds till SPC-343 can open any door."));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_hp", -1, true, "How much health should SCP-343 have. Set to -1 for GodMode and if set to anything but -1 then he is counted as a normal SCP and MTF must kill him like a normal SCP."));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_nuke_interact", false, true, "Should SPC-343 beable to interact with the nuke."));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_disable", false, true, "Should SPC-343 beable to interact with the nuke."));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_debug", false, true, "Internal testing config so I stop pushing commits that are broken >:("));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_broadcast", true, true, "When 343 spawns should that person be given information about 343"));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_broadcastinfo", "", true, "What 343 is shown if scp343_broadcast is true."));

			//https://github.com/Grover-c13/Smod2/wiki/Enum-Lists#itemtype
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_itemdroplist", new int[] {0,1,2,3,4,5,6,7,8,9,10,11,14,17,19,22,27,28,29 }, true, "What items SCP-343 drops instead of picking up."));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_itemstoconvert", new int[]{13,16,20,21,23,24,25,26,30}, true, "What items SCP-343 converts."));
			this.AddConfig(new Smod2.Config.ConfigSetting("scp343_converteditems", new int[]{15}, true, "What a item should be converted to."));
		}
	}
}