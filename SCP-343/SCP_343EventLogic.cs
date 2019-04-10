using Smod2;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SCP_343
{
	public class EventLogic : IEventHandlerPlayerPickupItem, IEventHandlerRoundStart, IEventHandlerDoorAccess, IEventHandlerSetRole, IEventHandlerPlayerHurt, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown, IEventHandlerCheckEscape, IEventHandlerCheckRoundEnd, IEventHandlerPlayerDie, IEventHandlerPocketDimensionEnter,IEventHandlerRoundEnd, IEventHandlerWarheadChangeLever, IEventHandlerCallCommand
	{
		Random RNG = new Random();

		DateTime timeOnEvent = DateTime.Now;

		public Dictionary<string, PlayerInfo> Active343AndBadgeDict =
			new Dictionary<string, PlayerInfo>();

		public Dictionary<Smod2.API.Team, int> teamAliveCount =
			 new Dictionary<Smod2.API.Team, int>();

		public PluginOptions _343Config = new PluginOptions();

		private Plugin plugin;
		public EventLogic(Plugin plugin)
		{
			this.plugin = plugin;
		}

		#region OnRoundStart
		/// <summary>
		/// Plugin checks if its disabled and if not it updates all config values and goes through every person on the server and adds all D-Class to a list and randomly generates a number to see if 343 should spawn and randomly picks one d-class to be 343.
		/// </summary>
		public void OnRoundStart(RoundStartEvent ev)
		{
			if (plugin.GetConfigBool("scp343_disable"))
			{
				plugin.PluginManager.DisablePlugin(plugin);
				return;
			}
			
			_343Config.UpdateValues(plugin);
			   
			int randomNumber = RNG.Next(0, 100);

			List<Smod2.API.Player> DClassList = new List<Smod2.API.Player>();

			Active343AndBadgeDict.Clear();

			if (!(randomNumber <= (float)plugin.GetConfigFloat("scp343_spawnchance")))
			{
				return;
			}

			foreach (Smod2.API.Player player in plugin.PluginManager.Server.GetPlayers())
			{
				if (player.TeamRole.Role == Smod2.API.Role.CLASSD)
				{
					DClassList.Add(player);
				}
			}

			if (DClassList.Count > 0 && plugin.PluginManager.Server.GetPlayers().Count > 2)
			{
				Player TheChosenOne = DClassList[RNG.Next(DClassList.Count)];
				if(TheChosenOne.GetUserGroup().BadgeText == null)
				{
					Active343AndBadgeDict.Add(TheChosenOne.SteamId, new PlayerInfo("", ""));
				}
				else
				{
					Active343AndBadgeDict.Add(TheChosenOne.SteamId, new PlayerInfo(TheChosenOne.GetUserGroup().BadgeText, TheChosenOne.GetUserGroup().Color));
				}
				TheChosenOne.GiveItem(ItemType.FLASHLIGHT);

				if (_343Config.SCP343_shouldbroadcast)
				{
					TheChosenOne.PersonalBroadcast(5, "You're SCP-343! Check your console for more information about SCP-343.", true);
					TheChosenOne.SendConsoleMessage("\n----------------------------------------------------------- \n" + _343Config.SCP343_broadcastinfo + "\n ----------------------------------------------------------- ");
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
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.Player.SetRank(Active343AndBadgeDict[ev.Player.SteamId].BadgeColor, Active343AndBadgeDict[ev.Player.SteamId].BadgeName);
				Active343AndBadgeDict.Remove(ev.Player.SteamId);
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
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
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
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId) && PluginManager.Manager.Server.Round.Duration >= _343Config.SCP343_opendoortime)
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
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if (ev.Attacker?.TeamRole.Team == Smod2.API.Team.SCP)
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
				if (Active343AndBadgeDict.ContainsKey(ev.Activator.SteamId))
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
				if (Active343AndBadgeDict.ContainsKey(ev.Activator.SteamId))
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
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
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
			if (Active343AndBadgeDict.Count >= 1 && !_343Config.SCP343_debug)
			{
				teamAliveCount.Clear();
				foreach (Smod2.API.Team team in Enum.GetValues(typeof(Smod2.API.Team)))
				{
					teamAliveCount[team] = 0;
				}
				
				foreach (Player player in PluginManager.Manager.Server.GetPlayers())
				{
					if (DateTime.Now >= timeOnEvent)
					{
						if (Active343AndBadgeDict.ContainsKey(player.SteamId))
						{
							UnityEngine.GameObject playerGameObject = ((UnityEngine.GameObject)player.GetGameObject());
							if (playerGameObject?.GetComponent<ServerRoles>()?.NetworkMyText != "SCP-343" || playerGameObject?.GetComponent<ServerRoles>()?.NetworkMyColor != "red")
							{
								player.HideTag(false);
								player.SetRank("red","SCP-343");
								timeOnEvent = DateTime.Now.AddSeconds(1);
							}
						}
					}
					teamAliveCount[player.TeamRole.Team]++;
				}

				if (_343Config.SCP343_HP == -1)
				{
					if (teamAliveCount[Smod2.API.Team.SCP] == 0
						&& teamAliveCount[Smod2.API.Team.CHAOS_INSURGENCY] == 0
						&& teamAliveCount[Smod2.API.Team.CLASSD] == Active343AndBadgeDict.Count
						&& teamAliveCount[Smod2.API.Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 1;
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						ev.Status = ROUND_END_STATUS.MTF_VICTORY;
					}//If SCPs, Chaos, ClassD and Scientists are dead then MTF win.
					else if (teamAliveCount[Smod2.API.Team.SCP] == 0
						&& teamAliveCount[Smod2.API.Team.CLASSD] == Active343AndBadgeDict.Count
						&& teamAliveCount[Smod2.API.Team.SCIENTIST] == 0
						&& teamAliveCount[Smod2.API.Team.NINETAILFOX] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.CI_VICTORY;
					}//If SCPs, ClassD, Scientists and MTF are dead then Chaos win.
					else if (teamAliveCount[Smod2.API.Team.NINETAILFOX] == 0
						&& teamAliveCount[Smod2.API.Team.CHAOS_INSURGENCY] == 0
						&& teamAliveCount[Smod2.API.Team.CLASSD] == Active343AndBadgeDict.Count
						&& teamAliveCount[Smod2.API.Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_VICTORY;
					} //If MTF, Chaos, ClassD and Scientists are dead then SCPs win.
					else if (teamAliveCount[Smod2.API.Team.NINETAILFOX] == 0
						&& teamAliveCount[Smod2.API.Team.CLASSD] == Active343AndBadgeDict.Count
						&& teamAliveCount[Smod2.API.Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_CI_VICTORY;
					}//If MTF, ClassD and Scientists are dead then SCPS & Chaos win.
				}
				else
				{
					if (teamAliveCount[Smod2.API.Team.NINETAILFOX] == 0
						&& teamAliveCount[Smod2.API.Team.CHAOS_INSURGENCY] == 0
						&& teamAliveCount[Smod2.API.Team.CLASSD] == Active343AndBadgeDict.Count
						&& teamAliveCount[Smod2.API.Team.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_VICTORY;
					} //If MTF, Chaos, ClassD and Scientists are dead then SCPs win.
					else if (teamAliveCount[Smod2.API.Team.NINETAILFOX] == 0
						&& teamAliveCount[Smod2.API.Team.CLASSD] == Active343AndBadgeDict.Count
						&& teamAliveCount[Smod2.API.Team.SCIENTIST] == 0)
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
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.Player.SetRank(Active343AndBadgeDict[ev.Player.SteamId].BadgeColor, Active343AndBadgeDict[ev.Player.SteamId].BadgeName);
				Active343AndBadgeDict.Remove(ev.Player.SteamId);
			}
		}
		#endregion

		#region OnPocketDimensionEnter
		/// <summary>
		/// Prevent 106 from grabbing 343.
		/// </summary>
		public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
		{
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				ev.TargetPosition = ev.LastPosition;
			}
		}
		#endregion

		#region OnRoundEnd
		/// <summary>
		/// Makes it so 343 loses his divine touch when the round ends.
		/// </summary>
		public void OnRoundEnd(RoundEndEvent ev)
		{
			foreach (Player player in Smod2.PluginManager.Manager.Server.GetPlayers())
			{
				if (Active343AndBadgeDict.ContainsKey(player.SteamId))
				{
					player.SetRank(Active343AndBadgeDict[player.SteamId].BadgeColor, Active343AndBadgeDict[player.SteamId].BadgeName);
					player.SetHealth(100);
					Active343AndBadgeDict.Remove(player.SteamId);
				}
			}
			Active343AndBadgeDict.Clear();
		}
		#endregion

		#region OnChangeLever
		public void OnChangeLever(WarheadChangeLeverEvent ev)
		{
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if (_343Config.Nuke_Interact)
				{
					ev.Allow = true;
				}
				else
				{
					ev.Allow = false;
				}
			}
		}
		#endregion

		#region CallCommand
		public void OnCallCommand(PlayerCallCommandEvent ev)
		{
			if (!_343Config.SCP343_heck) return;
			
			if (Active343AndBadgeDict.ContainsKey(ev.Player.SteamId))
			{
				if(ev.Command.ToLower() == "heck343")
				{
					if(plugin.Server.Round.Duration >= 30)
					{
						ev.ReturnMessage = "It's too late! You're 343 till death do you part.";
						return;
					}
					ev.Player.ChangeRole(Role.CLASSD, true,false);
					ev.ReturnMessage = "You're no longer SCP-343.";
					return;
				}
			}
			ev.ReturnMessage = "Wait you're not 343!";
		}
		#endregion
	}
}