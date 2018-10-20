using Smod2;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SCP_343
{
	public class EventLogic : EventArgs, IEventHandlerPlayerPickupItem, IEventHandlerRoundStart, IEventHandlerDoorAccess, IEventHandlerSetRole, IEventHandlerPlayerHurt, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown, IEventHandlerCheckEscape, IEventHandlerCheckRoundEnd, IEventHandlerPlayerDie, IEventHandlerUpdate, IEventHandlerPocketDimensionEnter
	{
		Random RNG = new Random();

		private Player TheChosenOne;

		DateTime timeOnEvent = DateTime.Now;
		float bottomXPos;
		float bottomZPos;
		double XZdistance;
		double Ydistance;
		List<Vector> nukeElevator = new List<Vector>();

		private Plugin plugin;
		public EventLogic(Plugin plugin)
		{
			this.plugin = plugin;
		}

		public void OnRoundStart(RoundStartEvent ev)
		{

			int randomNumber = RNG.Next(0, 100);

			List<Smod2.API.Player> DClassList = new List<Smod2.API.Player>();

			SCP343.checkSteamIDIf343Dict.Clear();
			SCP343.active343List.Clear();

			foreach (Smod2.API.Player Playa in plugin.pluginManager.Server.GetPlayers())
			{
				SCP343.checkSteamIDIf343Dict.Add(Playa.SteamId, false);

				if (randomNumber <= ConfigManager.Manager.Config.GetFloatValue("scp343_spawnchance", 10f, false))
				{
					if (Playa.TeamRole.Role == Smod2.API.Role.CLASSD)
					{
						DClassList.Add(Playa);
					}
				}
			}

			if (DClassList.Count > 0 && plugin.pluginManager.Server.GetPlayers().Count >= 3)
			{
				TheChosenOne = DClassList[RNG.Next(DClassList.Count)];
				SCP343.checkSteamIDIf343Dict[TheChosenOne.SteamId] = true;
				SCP343.active343List.Add(TheChosenOne.SteamId);

				if (ConfigManager.Manager.Config.GetIntValue("scp343_hp", -1, false) == -1)
				{
					TheChosenOne.SetGodmode(true);
				}
				else { TheChosenOne.SetHealth(ConfigManager.Manager.Config.GetIntValue("scp343_hp", -1, false)); }
				TheChosenOne.SetRank("red", "SCP-343");
			}
		}//Clears existing playerlist and active343s then stores current and based off spawnchance config option there might be SCP-343.

		public void OnSetRole(PlayerSetRoleEvent ev)
		{
			if (SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Player.SteamId, out SCP343.value) && ev.Player.TeamRole.Role != Smod2.API.Role.CLASSD)
			{
				if (SCP343.value)
				{
					if (ev.Player.GetGodmode())
					{
						ev.Player.SetGodmode(false);
					}
					ev.Player.SetRank("", "");
					SCP343.checkSteamIDIf343Dict[ev.Player.SteamId] = false;
					SCP343.active343List.Remove(ev.Player.SteamId);
				}
			}
		}//Checks if you're supposed to be SCP-343 but changes if you're a different class.

		public void OnPlayerPickupItem(PlayerPickupItemEvent ev)
		{
			if (SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Player.SteamId, out SCP343.value) && ConfigManager.Manager.Config.GetBoolValue("scp343_itemconverttoggle", false, false) == true)
			{
				if (SCP343.value)
				{
					int[] itemConvertList = ConfigManager.Manager.Config.GetIntListValue("scp343_itemstoconvert", new int[] { 13, 16, 20, 21, 23, 24, 25, 26 }, false);
					if (itemConvertList.Contains((int)ev.Item.ItemType))
					{
						int[] convertedItemList = ConfigManager.Manager.Config.GetIntListValue("scp343_converteditems", new int[] { 15 }, false);
						ev.ChangeTo = (ItemType)convertedItemList[RNG.Next(convertedItemList.Length - 1)];
					}

					int[] itemBlackList = ConfigManager.Manager.Config.GetIntListValue("scp343_itemdroplist", new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 14, 17, 19, 22, 27, 28, 29 }, false);
					if (itemBlackList.Contains((int)ev.Item.ItemType))
					{
						ev.Item.Drop();//Idk how to not have it picked up
						ev.Allow = false;// This deletes the item :(
					}
				}
				else if (SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Player.SteamId, out SCP343.value) && ConfigManager.Manager.Config.GetBoolValue("scp343_itemconverttoggle", false, false) == false && PluginManager.Manager.Server.Round.Duration >= 3)
				{
					if (SCP343.value)
					{
						int[] itemWhiteList = ConfigManager.Manager.Config.GetIntListValue("scp343_alloweditems", new int[] { 12, 15 }, false);
						if (!(itemWhiteList.Contains((int)ev.Item.ItemType)))
						{
							ev.Item.Drop();//Idk how to not have it picked up
							ev.Allow = false;// This deletes the item :(
						}
					}
				}
			}
		}//The server operater can set if SCP-343 should convert items or just drop items. This is due to things like 100 flashlights lagging the server.

		public void OnDoorAccess(PlayerDoorAccessEvent ev)
		{
			if (SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Player.SteamId, out SCP343.value) && PluginManager.Manager.Server.Round.Duration >= ConfigManager.Manager.Config.GetFloatValue("scp343_opendoortime", 300, false))
			{
				if (SCP343.value)
				{
					ev.Allow = true;
				}
			}
		}// Allows 343 to open every door after a set amount of seconds.

		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Player.SteamId, out SCP343.value) && ev.Attacker.TeamRole.Team == Smod2.API.Team.SCP)
			{
				if (SCP343.value)
				{
					ev.Damage = 0;
				}
			}
		}// Cannot be damaged by SCP forces.

		public void OnStartCountdown(WarheadStartEvent ev)
		{
			if (ev.Activator != null)
			{
				if ((SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Activator.SteamId, out SCP343.value)))
				{
					if (SCP343.value)
					{
						if (ConfigManager.Manager.Config.GetBoolValue("scp343_nuke_interact", true, false) == true)
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
		}

		public void OnStopCountdown(WarheadStopEvent ev)
		{
			if (ev.Activator != null)
			{
				if ((SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Activator.SteamId, out SCP343.value)))
				{
					if (SCP343.value)
					{
						if (ConfigManager.Manager.Config.GetBoolValue("scp343_nuke_interact", true, false) == true)
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
		}//OnStartCountdown and OnStopCountdown so the server owner can configure if SCP-343 can interact with the nuke. Stopping the nuke is buggy and resets it a couple of seconds.

		public void OnCheckEscape(PlayerCheckEscapeEvent ev)
		{
			if (SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Player.SteamId, out SCP343.value))
			{
				if (SCP343.value)
				{
					ev.AllowEscape = false;
				}
			}
		}//Prevent 343 from escaping.

		public void OnCheckRoundEnd(CheckRoundEndEvent ev)
		{
			if (SCP343.active343List.Count >= 1)
			{
				if (PluginManager.Manager.Server.Round.Stats.SCPAlive == 0 && PluginManager.Manager.Server.Round.Stats.CiAlive == 0 && PluginManager.Manager.Server.Round.Stats.ClassDAlive == SCP343.active343List.Count && PluginManager.Manager.Server.Round.Stats.ScientistsAlive == 0)
				{
					ev.Status = ROUND_END_STATUS.MTF_VICTORY;
					PluginManager.Manager.Server.Round.EndRound();
				}//If SCPs, Chaos, ClassD and Scientists are dead then MTF win.
				else if (PluginManager.Manager.Server.Round.Stats.SCPAlive == 0 && PluginManager.Manager.Server.Round.Stats.ClassDAlive == SCP343.active343List.Count && PluginManager.Manager.Server.Round.Stats.ScientistsAlive == 0 && PluginManager.Manager.Server.Round.Stats.NTFAlive == 0)
				{
					ev.Status = ROUND_END_STATUS.CI_VICTORY;
					PluginManager.Manager.Server.Round.EndRound();
				}//If SCPs, ClassD, Scientists and MTF are dead then Chaos win.
				else if (PluginManager.Manager.Server.Round.Stats.NTFAlive == 0 && PluginManager.Manager.Server.Round.Stats.CiAlive == 0 && PluginManager.Manager.Server.Round.Stats.ClassDAlive == SCP343.active343List.Count && PluginManager.Manager.Server.Round.Stats.ScientistsAlive == 0)
				{
					ev.Status = ROUND_END_STATUS.SCP_VICTORY;
					PluginManager.Manager.Server.Round.EndRound();
				} //If MTF, Chaos, ClassD and Scientists are dead then SCPs win.
				else if (PluginManager.Manager.Server.Round.Stats.NTFAlive == 0 && PluginManager.Manager.Server.Round.Stats.ClassDAlive == SCP343.active343List.Count && PluginManager.Manager.Server.Round.Stats.ScientistsAlive == 0)
				{
					ev.Status = ROUND_END_STATUS.SCP_CI_VICTORY;
					PluginManager.Manager.Server.Round.EndRound();
				}//If MTF, ClassD and Scientists are dead then SCPS & Chaos win.
			}
		}//Check for alive people minus SCP343.
		 //To-do rewrite this to be better.

		public void OnPlayerDie(PlayerDeathEvent ev)
		{
			if (SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Player.SteamId, out SCP343.value))
			{
				if (SCP343.value)
				{
					SCP343.active343List.Remove(ev.Player.SteamId);
				}
			}
		}// Make sure to remove the player from the active343List so the win condition can go through


		public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
		{
			if (SCP343.checkSteamIDIf343Dict.TryGetValue(ev.Player.SteamId, out SCP343.value))
			{
				if (SCP343.value)
				{
					ev.TargetPosition = ev.LastPosition;
				}
			}
		}

		public void OnUpdate(UpdateEvent ev)
		{
			if (DateTime.Now >= timeOnEvent)
			{
				timeOnEvent = DateTime.Now.AddSeconds(2.0);
				if (SCP343.active343List.Count >= 1)
				{
					foreach (Player player in PluginManager.Manager.Server.GetPlayers())
					{
						foreach (String steamid in SCP343.active343List)
						{
							if (steamid == player.SteamId)
							{
								foreach (var elevator in Smod2.PluginManager.Manager.Server.Map.GetElevators())
								{
									if (elevator.ElevatorType == Smod2.API.ElevatorType.WarheadRoom)
									{
										nukeElevator = elevator.GetPositions();
										bottomXPos = (nukeElevator[1].x - player.GetPosition().x) * 2;
										bottomZPos = (nukeElevator[1].z - player.GetPosition().z) * 2;
										XZdistance = Math.Sqrt(Math.Abs(bottomXPos) + Math.Abs(bottomZPos));
										Ydistance = Math.Abs(player.GetPosition().y) - Math.Abs(nukeElevator[1].y); 
										if (XZdistance <= 10 && Ydistance <= 2)
										{
											player.Teleport(new Vector(nukeElevator[0].x, nukeElevator[0].y + 1, nukeElevator[0].z));
										}
									}
								}
							}
						}
					}
				}
			}
		}//Checks every 2 seconds if someone flagged for SCP-343 is within 10 x and z and within 2 y units (I think thats a meter?) and teleports them to the top of the elevator. I didn't use the PlayerElevatorUseEvent because I wanted them to be on the top and no chance to squeeze through aka admin teleport.
	}
}