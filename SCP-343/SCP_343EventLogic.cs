using Smod2;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SCP_343
{
	public class EventLogic : EventArgs, IEventHandlerPlayerPickupItem, IEventHandlerRoundStart, IEventHandlerDoorAccess, IEventHandlerSetRole, IEventHandlerPlayerHurt, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown, IEventHandlerCheckEscape, IEventHandlerCheckRoundEnd, IEventHandlerPlayerDie, IEventHandlerUpdate, IEventHandlerPocketDimensionEnter,IEventHandlerRoundEnd
	{
		Random RNG = new Random();

		public static PluginOptions _343Config = new PluginOptions();

		private Plugin plugin;
		public EventLogic(Plugin plugin)
		{
			this.plugin = plugin;
		}

		#region OnRoundStart
		/// <summary>
		/// Plugin checks if its disabled and if not it updates all config values and goes through every person on the server and adds all D-Class to a list and randomly generates a number to see if 343 should spawn and randomly picks one to be 343.
		/// </summary>
		public void OnRoundStart(RoundStartEvent ev)
		{
			if (plugin.GetConfigBool("scp343_disable"))
			{
				Smod2.PluginManager.Manager.DisablePlugin(plugin.Details.id);
				return;
			}

			_343Config.UpdateValues();

			int randomNumber = RNG.Next(0, 100);

			List<Smod2.API.Player> DClassList = new List<Smod2.API.Player>();

			SCP343.Active343AndBadgeDict.Clear();

			if (!(randomNumber <= (float)plugin.GetConfigFloat("scp343_spawnchance")))
			{
				return;
			}

			foreach (Smod2.API.Player Playa in plugin.pluginManager.Server.GetPlayers())
			{
				if (Playa.TeamRole.Role == Smod2.API.Role.CLASSD)
				{
					DClassList.Add(Playa);
				}
			}

			if (DClassList.Count > 0 && plugin.pluginManager.Server.GetPlayers().Count > 2)
			{
				Player TheChosenOne = DClassList[RNG.Next(DClassList.Count)];
				if(TheChosenOne.GetUserGroup().BadgeText == null)
				{
					SCP343.Active343AndBadgeDict.Add(TheChosenOne.SteamId, new SCP343.PlayerInfo("", ""));
				}
				else
				{
					SCP343.Active343AndBadgeDict.Add(TheChosenOne.SteamId, new SCP343.PlayerInfo(TheChosenOne.GetUserGroup().BadgeText, TheChosenOne.GetUserGroup().Color));
				}
				TheChosenOne.GiveItem(ItemType.FLASHLIGHT);

				if (EventLogic._343Config.SCP343_shouldbroadcast)
				{
					TheChosenOne.PersonalBroadcast(5, "You're SCP-343! Check your console for more information about SCP-343.", true);
					TheChosenOne.SendConsoleMessage("----------------------------------------------------------- \n" + EventLogic._343Config.SCP343_broadcastinfo + "\n ---------------------------------------------------------- - ");
				}

				if (_343Config.SCP343_HP != -1)
				{
					TheChosenOne.SetHealth(_343Config.SCP343_HP);
				}
				TheChosenOne.SetRank("red", "SCP-343");
			}
		}
		#endregion

		#region OnSetRole
		/// <summary>
		/// If you change your role while being 343 it gives you back your old rank name and color (Not permissions).
		/// </summary>
		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.Player.SetRank(SCP343.Active343AndBadgeDict[ev.Player.SteamId].BadgeColor, SCP343.Active343AndBadgeDict[ev.Player.SteamId].BadgeName);
				SCP343.Active343AndBadgeDict.Remove(ev.Player.SteamId);
			}
		}
		#endregion

		#region OnPlayerPickUpItem
		/// <summary>
		/// Does handling to see if 343 should either convert to flashlight (Or different item depends on config) or drop the item.
		/// </summary>
		public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
		{
			//plugin.Info((int)ev.Item.ItemType + ":" + ev.Item.ItemType.ToString().Length + ":" + ((int)ev.Item.ItemType).ToString().Length);
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if (_343Config.SCP343_convertitems)
				{
					if (_343Config.ItemConvertList.Contains((int)ev.Item.ItemType))
					{
						ev.ChangeTo = (ItemType)_343Config.ConvertedItemList[RNG.Next(_343Config.ConvertedItemList.Length - 1)];
					}
					if (_343Config.ItemBlackList.Contains((int)ev.Item.ItemType))
					{
						ev.Item.Drop();//Idk how to not have it picked up
						ev.Allow = false;// This deletes the item :(
					}
				}
				else if (_343Config.SCP343_convertitems == false && PluginManager.Manager.Server.Round.Duration >= 3)
				{
					if (_343Config.ItemBlackList.Contains((int)ev.Item.ItemType) || _343Config.ItemConvertList.Contains((int)ev.Item.ItemType))
					{
						ev.Item.Drop();//Idk how to not have it picked up
						ev.Allow = false;// This deletes the item :(
					}
				}
			}
		}
		#endregion

		#region OnDoorAccess
		/// <summary>
		/// Checks if 343 should open the door.
		/// </summary>
		public void OnDoorAccess(PlayerDoorAccessEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId) && PluginManager.Manager.Server.Round.Duration >= _343Config.SCP343_opendoortime)
			{
				ev.Allow = true;
			}
		}
		#endregion

		#region OnPlayerHurt
		/// <summary>
		/// Checks if 343 should be in godmode and sets damage to 0 if SCP343_HP is set to -1. 343 dies to nuke or femur crusher. (I don't use godmode here because it 
		/// </summary>
		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if (ev.Attacker?.TeamRole.Team == Team.SCP)
				{
					ev.Damage = 0;
				}

				if (_343Config.SCP343_HP == -1)
				{
					if(ev.Player.GetHealth() < 1000)
					{
						ev.Player.AddHealth(1000); //This is a bandage patch for plugins like 008 or anything that might come in the future
					}
					ev.Damage = 0;
				}

				if (ev.DamageType == DamageType.LURE || ev.DamageType == DamageType.DECONT || ev.DamageType == DamageType.WALL || ev.DamageType == DamageType.NUKE)
				{
					ev.Damage = ev.Player.GetHealth() + 100;
				}
			}
		}
		#endregion

		#region OnStartCountdown and OnStopCountdown
		/// <summary>
		/// Checks config to see if 343 should interact with the nuke or not. This is really buggy is set to false.
		/// </summary>
		public void OnStartCountdown(WarheadStartEvent ev)
		{
			if (ev.Activator != null)
			{
				if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Activator.SteamId))
				{
					if (_343Config.Nuke_Interact)
					{
						ev.Cancel = false;
					}
					else
					{
						ev.Cancel = true;
					}
				}
			}
		}
		
		public void OnStopCountdown(WarheadStopEvent ev)
		{
			if (ev.Activator != null)
			{
				if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Activator.SteamId))
				{
					if (_343Config.Nuke_Interact)
					{
						ev.Cancel = false;
					}
					else
					{
						ev.Cancel = true;
					}
				}
			}
		}
		#endregion

		#region OnCheckEscape
		/// <summary>
		/// 343 Can't Escape.
		/// </summary>
		/// <param name="ev"></param>
		public void OnCheckEscape(PlayerCheckEscapeEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.AllowEscape = false;
			}
		}
		#endregion

		#region OnCheckRoundEnd
		/// <summary>
		/// Checks all teams to do custom round end if 343 is alive and all MTF for example, and if hp is set to anything but -1 then he is counted as a normal SCP and MTF must kill him like a normal SCP.
		/// </summary>
		public void OnCheckRoundEnd(CheckRoundEndEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.Count >= 1 && !_343Config.SCP343_debug)
			{
				SCP343.teamAliveCount.Clear();
				foreach (Team team in Enum.GetValues(typeof(Team)))
				{
					SCP343.teamAliveCount[team] = 0;
				}
				
				foreach (Player player in PluginManager.Manager.Server.GetPlayers())
				{
					SCP343.teamAliveCount[player.TeamRole.Team]++;
				}

				if (_343Config.SCP343_HP == -1)
				{
					if (SCP343.teamAliveCount[Team.SCP] == 0
						&& SCP343.teamAliveCount[Team.CHAOS_INSURGENCY] == 0
						&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count
						&& SCP343.teamAliveCount[Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 1;
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						ev.Status = ROUND_END_STATUS.MTF_VICTORY;
					}//If SCPs, Chaos, ClassD and Scientists are dead then MTF win.
					else if (SCP343.teamAliveCount[Team.SCP] == 0
						&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count
						&& SCP343.teamAliveCount[Team.SCIENTIST] == 0
						&& SCP343.teamAliveCount[Team.NINETAILFOX] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.CI_VICTORY;
					}//If SCPs, ClassD, Scientists and MTF are dead then Chaos win.
					else if (SCP343.teamAliveCount[Team.NINETAILFOX] == 0
						&& SCP343.teamAliveCount[Team.CHAOS_INSURGENCY] == 0
						&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count
						&& SCP343.teamAliveCount[Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_VICTORY;
					} //If MTF, Chaos, ClassD and Scientists are dead then SCPs win.
					else if (SCP343.teamAliveCount[Team.NINETAILFOX] == 0
						&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count
						&& SCP343.teamAliveCount[Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_CI_VICTORY;
					}//If MTF, ClassD and Scientists are dead then SCPS & Chaos win.
				}
				else
				{
					if (SCP343.teamAliveCount[Team.NINETAILFOX] == 0
						&& SCP343.teamAliveCount[Team.CHAOS_INSURGENCY] == 0
						&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count
						&& SCP343.teamAliveCount[Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_VICTORY;
					} //If MTF, Chaos, ClassD and Scientists are dead then SCPs win.
					else if (SCP343.teamAliveCount[Team.NINETAILFOX] == 0
						&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count
						&& SCP343.teamAliveCount[Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 1;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_CI_VICTORY;
					}//If MTF, ClassD and Scientists are dead then SCPS & Chaos win.
				}
			}
		}
		#endregion

		#region OnPlayerDie
		/// <summary>
		/// In case 343 dies this will remove him from the list.
		/// </summary>
		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.Player.SetRank(SCP343.Active343AndBadgeDict[ev.Player.SteamId].BadgeColor, SCP343.Active343AndBadgeDict[ev.Player.SteamId].BadgeName);
				SCP343.Active343AndBadgeDict.Remove(ev.Player.SteamId);
			}
		}
		#endregion

		#region OnPocketDimensionEnter
		/// <summary>
		/// Prevent 106 from grabbing 343.
		/// </summary>
		public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.TargetPosition = ev.LastPosition;
			}
		}
		#endregion

		#region OnUpdate
		/// <summary>
		/// Checks every 4 seconds to see if 343 is close to the bottom of the nuke elevator if 343 nuke interact is set to false.
		/// </summary>
		public void OnUpdate(UpdateEvent ev)
		{
			DateTime timeOnEvent = DateTime.Now;
			if (DateTime.Now >= timeOnEvent)
			{
				if (!(SCP343.Active343AndBadgeDict.Count >= 1) || _343Config.Nuke_Interact)
				{
					return;
				}
				timeOnEvent = DateTime.Now.AddSeconds(4.0);
				foreach (Player player in PluginManager.Manager.Server.GetPlayers())
				{
					if(SCP343.Active343AndBadgeDict.ContainsKey(player.SteamId))
					{
						foreach (var elevator in Smod2.PluginManager.Manager.Server.Map.GetElevators())
						{
							if (elevator.ElevatorType == Smod2.API.ElevatorType.WarheadRoom)
							{
								List<Vector> nukeElevator = new List<Vector>();
								nukeElevator = elevator.GetPositions();
								float bottomXPos = (nukeElevator[1].x - player.GetPosition().x) * 2;
								float bottomZPos = (nukeElevator[1].z - player.GetPosition().z) * 2;
								double XZdistance = Math.Sqrt(Math.Abs(bottomXPos) + Math.Abs(bottomZPos));
								double Ydistance = Math.Abs(player.GetPosition().y) - Math.Abs(nukeElevator[1].y);
								if (XZdistance <= 3.25 && Ydistance <= 1.25)
								{
									player.Teleport(new Vector(nukeElevator[0].x, nukeElevator[0].y + 1, nukeElevator[0].z));
								}
							}
						}
					}
				}
			}
		}
		#endregion

		#region OnRoundEnd
		/// <summary>
		/// Makes it so 343 loses his divine touch when the round ends.
		/// </summary>
		public void OnRoundEnd(RoundEndEvent ev)
		{
			foreach (Player playa in Smod2.PluginManager.Manager.Server.GetPlayers())
			{
				if (SCP343.Active343AndBadgeDict.ContainsKey(playa.SteamId))
				{
					playa.SetRank(SCP343.Active343AndBadgeDict[playa.SteamId].BadgeColor, SCP343.Active343AndBadgeDict[playa.SteamId].BadgeName);
					playa.SetHealth(100);
					SCP343.Active343AndBadgeDict.Remove(playa.SteamId);
				}
			}
			SCP343.Active343AndBadgeDict.Clear();
		}
		#endregion

		#region PluginOptions
		/// <summary>
		/// This is all of the config options to meet with the plugin requirements.
		/// </summary>
		public class PluginOptions
		{
			public int SCP343_HP;
			public int[] ItemConvertList;
			public int[] ConvertedItemList;
			public int[] ItemBlackList;
			public int SCP343_opendoortime;
			public bool Nuke_Interact;
			public bool SCP343_convertitems;
			public bool SCP343_debug;
			public bool SCP343_shouldbroadcast;
			public string SCP343_broadcastinfo;

			public void UpdateValues()
			{
				SCP343_HP = SCP343.plugin.GetConfigInt("scp343_hp");
				ItemConvertList = SCP343.plugin.GetConfigIntList("scp343_itemstoconvert");
				ConvertedItemList = SCP343.plugin.GetConfigIntList("scp343_converteditems");
				ItemBlackList = SCP343.plugin.GetConfigIntList("scp343_itemdroplist");
				SCP343_opendoortime = SCP343.plugin.GetConfigInt("scp343_opendoortime");
				Nuke_Interact = SCP343.plugin.GetConfigBool("scp343_nuke_interact");
				SCP343_convertitems = SCP343.plugin.GetConfigBool("scp343_itemconverttoggle");
				SCP343_debug = SCP343.plugin.GetConfigBool("scp343_debug");

				SCP343_shouldbroadcast = SCP343.plugin.GetConfigBool("scp343_broadcast");
				SCP343_broadcastinfo = SCP343.plugin.GetConfigString("scp343_broadcastinfo");

				string _343info;
				if(SCP343_broadcastinfo.Length != 0)
				{
					_343info = SCP343_broadcastinfo.Replace("343DOORTIME", SCP343_opendoortime.ToString());
				}
				else if (SCP343_HP == -1)
				{
					_343info = "You are SCP-343, a passive SCP.\n(To be clear this isn't the correct wiki version SCP-343) \nAfter 343DOORTIME seconds you can open any door in the game \nAny weapon/grenade you pick up is morphed into a flashlight.\nYou are NOT counted towards ending the round (Example the round will end if its all NTF and you) \nYou cannot die to anything but lure (106 femur crusher), decontamination, crushed (jumping off at t intersections at heavy) and the nuke.".Replace("343DOORTIME",SCP343_opendoortime.ToString());
				}
				else
				{
					_343info = "You are SCP-343, a passive SCP.\n(To be clear this isn't the correct wiki version SCP-343) \nAfter 343DOORTIME seconds you can open any door in the game \nAny weapon/grenade you pick up is morphed into a flashlight.\nYou are counted towards ending the round (Example the round will not end if its all NTF and you, it needs to be all SCP and or CI)".Replace("343DOORTIME", SCP343_opendoortime.ToString());
				}
				SCP343_broadcastinfo = _343info;
			}
		}
		#endregion
	}
}