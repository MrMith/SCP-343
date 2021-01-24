using Smod2;

namespace SCP_343
{
	#region PluginOptions
	/// <summary>
	/// This is a nice and simple class to keep all of the config options that might be used in the future. This is attached to <see cref="EventLogic"/> because it needs to be activated after the config options are registered.
	/// </summary>
	public class PluginOptions
	{
		public float SCP343_HP { get; set; }
		public float SCP343_RevertTime { get; set; }
		public int[] ItemConvertList { get; set; }
		public int[] ConvertedItemList { get; set; }
		public int[] ItemBlackList { get; set; }
		public float SCP343_opendoortime { get; set; }
		public bool Nuke_Interact { get; set; }
		public bool SCP343_convertitems { get; set; }
		public bool SCP343_debug { get; set; }
		public bool SCP343_shouldbroadcast { get; set; }
		public bool SCP343_revert { get; set; }
		public string SCP343_broadcastinfo { get; set; }

		public void UpdateValues(Plugin plugin)
		{
			SCP343_HP = plugin.GetConfigFloat("343_hp");

			ItemConvertList = plugin.GetConfigIntList("343_itemstoconvert");
			ConvertedItemList = plugin.GetConfigIntList("343_converteditems");
			ItemBlackList = plugin.GetConfigIntList("343_itemdroplist");
			SCP343_opendoortime = plugin.GetConfigFloat("343_opendoortime");

			Nuke_Interact = plugin.GetConfigBool("343_nuke_interact");
			SCP343_convertitems = plugin.GetConfigBool("343_itemconverttoggle");
			SCP343_debug = plugin.GetConfigBool("343_debug");
			SCP343_shouldbroadcast = plugin.GetConfigBool("343_broadcast");
			SCP343_revert = plugin.GetConfigBool("343_revert");
			SCP343_RevertTime = plugin.GetConfigFloat("343_revert_time");

			SCP343_broadcastinfo = plugin.GetConfigString("343_broadcastinfo");

			string _343info;
			if (SCP343_broadcastinfo.Length != 0)
			{
				_343info = SCP343_broadcastinfo.Replace("343DOORTIME", SCP343_opendoortime.ToString()).Replace("343HECKTIME", SCP343_RevertTime.ToString());
			}
			else if (SCP343_HP == -1)
			{
				_343info = "You are SCP-343, a passive SCP.\n(To be clear this isn't the correct wiki version SCP-343) \nAfter 343DOORTIME seconds you can open any door in the game \nAny weapon/grenade you pick up is morphed into a flashlight.\nYou are NOT counted towards ending the round (Example the round will end if its all NTF and you) \nYou cannot die to anything but lure (106 femur crusher), decontamination, crushed (jumping off at t intersections at heavy) and the nuke.\nYou can use the command .heck343 to spawn as a normal D-Class within the first 343HECKTIME seconds of the round.".Replace("343DOORTIME", SCP343_opendoortime.ToString()).Replace("343HECKTIME", SCP343_RevertTime.ToString());
			}
			else
			{
				_343info = "You are SCP-343, a passive SCP.\n(To be clear this isn't the correct wiki version SCP-343) \nAfter 343DOORTIME seconds you can open any door in the game \nAny weapon/grenade you pick up is morphed into a flashlight.\nYou are counted towards ending the round (Example the round will not end if its all NTF and you, it needs to be all SCP and or CI)\nYou can use the command .heck343 to spawn as a normal D-Class within the first 30 seconds of the round.".Replace("343DOORTIME", SCP343_opendoortime.ToString()).Replace("343HECKTIME", SCP343_RevertTime.ToString());
			}
			SCP343_broadcastinfo = _343info;
		}
	}
	#endregion
}
