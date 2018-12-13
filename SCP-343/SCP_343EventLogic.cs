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

		public void OnRoundStart(RoundStartEvent ev)
		{
			if (plugin.GetConfigBool("scp343_disable"))
			{
				Smod2.PluginManager.Manager.DisablePlugin(plugin.Details.id);
				return;
			}

			_343Config.UpdateValues();

			int randomNumber = RNG.Next(0, 100);

			//Fuck this looks like garbo but I don't wanna rewrite it 

			List<Smod2.API.Player> DClassList = new List<Smod2.API.Player>();

			SCP343.Active343AndBadgeDict.Clear();

			foreach (Smod2.API.Player Playa in plugin.pluginManager.Server.GetPlayers())
			{
				if (randomNumber <= (float)plugin.GetConfigFloat("scp343_spawnchance"))
				{
					if (Playa.TeamRole.Role == Smod2.API.Role.CLASSD)
					{
						DClassList.Add(Playa);
					}
				}
			}

			if (DClassList.Count > 0 && plugin.pluginManager.Server.GetPlayers().Count > 2)
			{
				Player TheChosenOne = DClassList[RNG.Next(DClassList.Count)];
				SCP343.Active343AndBadgeDict.Add(TheChosenOne.SteamId, new SCP343.PlayerInfo(TheChosenOne.GetUserGroup().Name, TheChosenOne.GetUserGroup().Color));
				
				if (_343Config.SCP343_HP == -1)
				{
					TheChosenOne.SetGodmode(true);
				}
				else { TheChosenOne.SetHealth(_343Config.SCP343_HP); }
				TheChosenOne.SetRank("red", "SCP-343");
			}
		}//Clears existing playerlist and active343s then stores current and based off spawnchance config option there might be SCP-343.

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if (ev.Player.GetGodmode())
				{
					ev.Player.SetGodmode(false);
				}
				ev.Player.SetRank(SCP343.Active343AndBadgeDict[ev.Player.SteamId].BadgeColor, SCP343.Active343AndBadgeDict[ev.Player.SteamId].BadgeName);
				SCP343.Active343AndBadgeDict.Remove(ev.Player.SteamId);
			}
		}//Checks if you're supposed to be SCP-343 but changes if you're a different class.

		public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if (_343Config.SCP343_ConvertItems)
				{
					if (_343Config.itemConvertList.Contains((int)ev.Item.ItemType))
					{
						ev.ChangeTo = (ItemType)_343Config.convertedItemList[RNG.Next(_343Config.convertedItemList.Length - 1)];
					}
					if (_343Config.itemBlackList.Contains((int)ev.Item.ItemType))
					{
						ev.Item.Drop();//Idk how to not have it picked up
						ev.Allow = false;// This deletes the item :(
					}
				}
				else if (_343Config.SCP343_ConvertItems == false && PluginManager.Manager.Server.Round.Duration >= 3)
				{
					if (_343Config.itemBlackList.Contains((int)ev.Item.ItemType) || _343Config.itemConvertList.Contains((int)ev.Item.ItemType))
					{
						ev.Item.Drop();//Idk how to not have it picked up
						ev.Allow = false;// This deletes the item :(
					}
				}
			}
		}//The server operater can set if SCP-343 should convert items or just drop items. This is due to things like 100 flashlights lagging the server.

		public void OnDoorAccess(PlayerDoorAccessEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId) && PluginManager.Manager.Server.Round.Duration >= _343Config.SCP343_OpenDoorTime)
			{
				ev.Allow = true;
			}
		}// Allows 343 to open every door after a set amount of seconds.

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if (ev.Attacker != null)
				{
					if (ev.Attacker.TeamRole.Team == Team.SCP)
					{
						ev.Damage = 0;
					}
				}

				if (_343Config.SCP343_HP == -1)
				{
					if(ev.DamageType == DamageType.NUKE)
					{
						ev.Damage = 10000 + ev.Player.GetHealth();
					}
					else
					{
						ev.Damage = 0;
						ev.Player.SetGodmode(true);
					}
				}

				if (ev.DamageType == DamageType.LURE)
				{
					{
						ev.Player.Kill(DamageType.LURE);
					}
				}
			}
		}// Cannot be damaged by SCP forces.

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
		}//OnStartCountdown and OnStopCountdown so the server owner can configure if SCP-343 can interact with the nuke. Stopping the nuke is buggy and resets it a couple of seconds.

		public void OnCheckEscape(PlayerCheckEscapeEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.AllowEscape = false;
			}
		}//Prevent 343 from escaping.
		
		public void OnCheckRoundEnd(CheckRoundEndEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.Count >= 1 && !_343Config.scp343_debug)
			{
				SCP343.teamAliveCount.Clear();
				foreach(Team team in Enum.GetValues(typeof(Team)))
				{
					SCP343.teamAliveCount[team] = 0;
				}
				
				foreach(Player player in PluginManager.Manager.Server.GetPlayers())
				{
					SCP343.teamAliveCount[player.TeamRole.Team]++;
				}

				if (SCP343.teamAliveCount[Team.SCP] == 0
					&& SCP343.teamAliveCount[Team.CHAOS_INSURGENCY] == 0
					&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count
					&& SCP343.teamAliveCount[Team.SCIENTISTS] == 0)
				{
					ev.Status = ROUND_END_STATUS.MTF_VICTORY;
					PluginManager.Manager.Server.Round.EndRound();
				}//If SCPs, Chaos, ClassD and Scientists are dead then MTF win.
				else if (SCP343.teamAliveCount[Team.SCP] == 0
					&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count
					&& SCP343.teamAliveCount[Team.SCIENTISTS] == 0
					&& SCP343.teamAliveCount[Team.NINETAILFOX] == 0)
				{
					ev.Status = ROUND_END_STATUS.CI_VICTORY;
					PluginManager.Manager.Server.Round.EndRound();
				}//If SCPs, ClassD, Scientists and MTF are dead then Chaos win.
				else if (SCP343.teamAliveCount[Team.NINETAILFOX] == 0 
					&& SCP343.teamAliveCount[Team.CHAOS_INSURGENCY] == 0
					&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count 
					&& SCP343.teamAliveCount[Team.SCIENTISTS] == 0)
				{
					ev.Status = ROUND_END_STATUS.SCP_VICTORY;
					PluginManager.Manager.Server.Round.EndRound();
				} //If MTF, Chaos, ClassD and Scientists are dead then SCPs win.
				else if (SCP343.teamAliveCount[Team.NINETAILFOX] == 0 
					&& SCP343.teamAliveCount[Team.CLASSD] == SCP343.Active343AndBadgeDict.Count 
					&& SCP343.teamAliveCount[Team.SCIENTISTS] == 0)
				{
					ev.Status = ROUND_END_STATUS.SCP_CI_VICTORY;
					PluginManager.Manager.Server.Round.EndRound();
				}//If MTF, ClassD and Scientists are dead then SCPS & Chaos win.
			}
		}//Check for alive people minus SCP343.
		 //To-do rewrite this to be better.

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if (ev.Player.GetGodmode())
				{
					ev.Player.SetGodmode(false);
				}
				ev.Player.SetRank(SCP343.Active343AndBadgeDict[ev.Player.SteamId].BadgeColor, SCP343.Active343AndBadgeDict[ev.Player.SteamId].BadgeName);
				SCP343.Active343AndBadgeDict.Remove(ev.Player.SteamId);
			}
		}// Make sure to remove the player from the active343List so the win condition can go through


		public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
		{
			if (SCP343.Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.TargetPosition = ev.LastPosition;
			}
		}//Preventing 343 from going into the pocket dimension.

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
		}//Checks every 2 seconds if someone flagged for SCP-343 is within 3 x and z units and within 0.5 y units (I think thats a meter?) and teleports them to the top of the elevator. I didn't use the PlayerElevatorUseEvent because I wanted them to be on the top and no chance to squeeze through aka admin teleport.

		public void OnRoundEnd(RoundEndEvent ev)
		{
			foreach (Player playa in Smod2.PluginManager.Manager.Server.GetPlayers())
			{
				if (SCP343.Active343AndBadgeDict.ContainsKey(playa.SteamId))
				{
					if (playa.GetGodmode())
					{
						playa.SetGodmode(false);
					}
					playa.SetRank(SCP343.Active343AndBadgeDict[playa.SteamId].BadgeColor, SCP343.Active343AndBadgeDict[playa.SteamId].BadgeName);
					SCP343.Active343AndBadgeDict.Remove(playa.SteamId);
				}
			}
			SCP343.Active343AndBadgeDict.Clear();
		}//Makes it so 343 loses his divine touch when the round ends.
		public class PluginOptions
		{
			public int SCP343_HP;
			public int[] itemConvertList;
			public int[] convertedItemList;
			public int[] itemBlackList;
			public int SCP343_OpenDoorTime;
			public bool Nuke_Interact;
			public bool SCP343_ConvertItems;
			public bool scp343_debug;

			public void UpdateValues()
			{
				SCP343_HP = SCP343.plugin.GetConfigInt("scp343_hp");
				itemConvertList = SCP343.plugin.GetConfigIntList("scp343_itemstoconvert");
				convertedItemList = SCP343.plugin.GetConfigIntList("scp343_converteditems");
				itemBlackList = SCP343.plugin.GetConfigIntList("scp343_itemdroplist");
				SCP343_OpenDoorTime = SCP343.plugin.GetConfigInt("scp343_opendoortime");
				Nuke_Interact = SCP343.plugin.GetConfigBool("scp343_nuke_interact");
				SCP343_ConvertItems = SCP343.plugin.GetConfigBool("scp343_itemconverttoggle");
				scp343_debug = SCP343.plugin.GetConfigBool("scp343_debug");
			}
		}
	}
}