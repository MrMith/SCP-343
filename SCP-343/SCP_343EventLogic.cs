using Smod2;
using Smod2.EventHandlers;
using Smod2.Events;
using Smod2.API;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCP_343
{
	/// <summary>
	/// This is the main class where everything happens, we decide which events we want by extending this class with any IEventHandler like IEventHandlerPlayerPickupItem (Called whenever someone picks up any item)
	/// </summary>
	public class EventLogic : IEventHandlerPlayerPickupItem, IEventHandlerRoundStart, IEventHandlerDoorAccess, IEventHandlerSetRole, IEventHandlerPlayerHurt, IEventHandlerWarheadStartCountdown, IEventHandlerWarheadStopCountdown, IEventHandlerCheckEscape, IEventHandlerCheckRoundEnd, IEventHandlerPlayerDie, IEventHandlerPocketDimensionEnter,IEventHandlerRoundEnd, IEventHandlerWarheadChangeLever, IEventHandlerCallCommand, IEventHandlerPlayerTriggerTesla
	{
		private System.Random RNG = new System.Random();

		/// <summary>
		/// This only exists because people like to reset their badge to default so people don't think they're SCP-343.
		/// </summary>
		private DateTime BadgeEnforcement = new DateTime();

		public Dictionary<Smod2.API.TeamType, int> teamAliveCount =
			 new Dictionary<Smod2.API.TeamType, int>();

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
			if (plugin.GetConfigBool("343_disable"))
			{
				plugin.PluginManager.DisablePlugin(plugin);
				return;
			}
			
			_343Config.UpdateValues(plugin);
			   
			int randomNumber = RNG.Next(0, 100);

			List<Smod2.API.Player> DClassList = new List<Smod2.API.Player>();

			if (!(randomNumber <= (float)plugin.GetConfigFloat($"{plugin.Details.configPrefix}_spawnchance")))
			{
				return;
			}

			foreach (Smod2.API.Player player in plugin.PluginManager.Server.GetPlayers())
			{
				if (player.TeamRole.Role == Smod2.API.RoleType.CLASSD)
				{
					DClassList.Add(player);
				}
			}

			if (DClassList.Count > 0 && plugin.PluginManager.Server.GetPlayers().Count > 2)
			{
				Player TheChosenOne = DClassList[RNG.Next(DClassList.Count)];
				Add343Manager(TheChosenOne);
				SCP_343Manager _343Manager = Get343Manager(TheChosenOne);
				_343Manager.Is343 = true;

				if(TheChosenOne.GetUserGroup().BadgeText == null)
				{
					_343Manager.PreviousBadgeColor = "";
					_343Manager.PreviousBadgeName = "";
				}
				else
				{
					_343Manager.PreviousBadgeColor = TheChosenOne.GetUserGroup().Color;
					_343Manager.PreviousBadgeName = TheChosenOne.GetUserGroup().BadgeText;
				}

				foreach(Smod2.API.Item item in TheChosenOne.GetInventory())
				{
					item.Remove();
				}

				TheChosenOne.GiveItem(Smod2.API.ItemType.FLASHLIGHT);

				if (_343Config.SCP343_shouldbroadcast)
				{
					TheChosenOne.PersonalBroadcast(5, "You're SCP-343! Check your console for more information about SCP-343.", true);
					TheChosenOne.SendConsoleMessage("\n----------------------------------------------------------- \n" + _343Config.SCP343_broadcastinfo + "\n ----------------------------------------------------------- ");
				}

				if (_343Config.SCP343_HP != -1)
				{
					TheChosenOne.HP = _343Config.SCP343_HP;
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
			if (Is343(ev.Player))
			{
				SCP_343Manager _343Manager = Get343Manager(ev.Player);
				_343Manager.Is343 = false;
				ev.Player.SetRank(_343Manager.PreviousBadgeColor, _343Manager.PreviousBadgeColor);
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
			if (Is343(ev.Player))
			{
				if (_343Config.SCP343_convertitems)
				{
					if (_343Config.ItemConvertList.Contains((int)ev.Item.ItemType))
					{
						ev.ChangeTo = (Smod2.API.ItemType)_343Config.ConvertedItemList[RNG.Next(_343Config.ConvertedItemList.Length - 1)];
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
			if (Is343(ev.Player) && PluginManager.Manager.Server.Round.Duration >= _343Config.SCP343_opendoortime)
			{
				ev.Allow = true;
			}
		}
		#endregion

		#region OnPlayerHurt
		/// <summary>
		/// Makes sure 343 doesn't take damage from any damage source if SCP343_HP = -1
		/// </summary>
		public void OnPlayerHurt(PlayerHurtEvent ev)
		{
			if (Is343(ev.Player))
			{
				if (ev.Attacker?.TeamRole.Team == TeamType.SCP)
				{
					ev.Damage = 0;
				}

				if (_343Config.SCP343_HP == -1)
				{
					if(ev.Player.HP < 1000)
					{
						ev.Player.HP += 1000; //This is a bandage patch for plugins like 008 or anything that might come in the future
					}
					ev.Damage = 0;
				}

				if (ev.DamageType == DamageType.LURE || ev.DamageType == DamageType.DECONT || ev.DamageType == DamageType.WALL || ev.DamageType == DamageType.NUKE)
				{
					ev.Damage = ev.Player.HP + 100;
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
				if (Is343(ev.Activator))
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
				if (Is343(ev.Activator))
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
			if (Is343(ev.Player))
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
			if (!_343Config.SCP343_debug)
			{
				teamAliveCount.Clear();
				///Converts the <see cref="TeamType"/> enum into a array of the <see cref="TeamType"/> values so we can iterate through it 
				foreach (Smod2.API.TeamType team in Enum.GetValues(typeof(Smod2.API.TeamType)))
				{
					teamAliveCount[team] = 0;
				}

				int _343AliveCount = 0;
				foreach (Player player in PluginManager.Manager.Server.GetPlayers())
				{
					if (DateTime.Now >= BadgeEnforcement)
					{
						if (Is343(player))
						{
							_343AliveCount++;
							///Grab the Player's UnityEngine Gameobject so we can reference SCP:SL's scripts like <see cref="ServerRoles"/>
							UnityEngine.GameObject playerGameObject = ((UnityEngine.GameObject)player.GetGameObject());
							if (playerGameObject?.GetComponent<ServerRoles>()?.NetworkMyText != "SCP-343" || playerGameObject?.GetComponent<ServerRoles>()?.NetworkMyColor != "red")
							{
								player.HideTag(false);
								player.SetRank("red","SCP-343");
								BadgeEnforcement = DateTime.Now.AddSeconds(1);
							}
						}
					}
					teamAliveCount[player.TeamRole.Team]++;
				}

				
				if (_343Config.SCP343_HP == -1)
				{
					//Please note, if you can avoid below you should.
					if (teamAliveCount[Smod2.API.TeamType.SCP] == 0
						&& teamAliveCount[Smod2.API.TeamType.CHAOS_INSURGENCY] == 0
						&& teamAliveCount[Smod2.API.TeamType.CLASSD] == _343AliveCount
						&& teamAliveCount[Smod2.API.TeamType.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 1;
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						ev.Status = ROUND_END_STATUS.MTF_VICTORY;
					}//If SCPs, Chaos, ClassD and Scientists are dead then MTF win.
					else if (teamAliveCount[Smod2.API.TeamType.SCP] == 0
						&& teamAliveCount[Smod2.API.TeamType.CLASSD] == _343AliveCount
						&& teamAliveCount[Smod2.API.TeamType.SCIENTIST] == 0
						&& teamAliveCount[Smod2.API.TeamType.NINETAILFOX] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.CI_VICTORY;
					}//If SCPs, ClassD, Scientists and MTF are dead then Chaos win.
					else if (teamAliveCount[Smod2.API.TeamType.NINETAILFOX] == 0
						&& teamAliveCount[Smod2.API.TeamType.CHAOS_INSURGENCY] == 0
						&& teamAliveCount[Smod2.API.TeamType.CLASSD] == _343AliveCount
						&& teamAliveCount[Smod2.API.TeamType.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_VICTORY;
					} //If MTF, Chaos, ClassD and Scientists are dead then SCPs win.
					else if (teamAliveCount[Smod2.API.TeamType.NINETAILFOX] == 0
						&& teamAliveCount[Smod2.API.TeamType.CLASSD] == _343AliveCount
						&& teamAliveCount[Smod2.API.TeamType.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_CI_VICTORY;
					}//If MTF, ClassD and Scientists are dead then SCPS & Chaos win.
				}
				else
				{
					if (teamAliveCount[Smod2.API.TeamType.NINETAILFOX] == 0
						&& teamAliveCount[Smod2.API.TeamType.CHAOS_INSURGENCY] == 0
						&& teamAliveCount[Smod2.API.TeamType.CLASSD] == _343AliveCount
						&& teamAliveCount[Smod2.API.TeamType.SCIENTIST] == 0)
					{
						Smod2.PluginManager.Manager.Server.Round.Stats.ClassDEscaped = 0;
						Smod2.PluginManager.Manager.Server.Round.Stats.ScientistsEscaped = 0;
						ev.Status = ROUND_END_STATUS.SCP_VICTORY;
					} //If MTF, Chaos, ClassD and Scientists are dead then SCPs win.
					else if (teamAliveCount[Smod2.API.TeamType.NINETAILFOX] == 0
						&& teamAliveCount[Smod2.API.TeamType.CLASSD] == _343AliveCount
						&& teamAliveCount[Smod2.API.TeamType.SCIENTIST] == 0)
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
			if (Is343(ev.Player))
			{
				SCP_343Manager _343Manager = Get343Manager(ev.Player);
				_343Manager.Is343 = false;
				ev.Player.SetRank(_343Manager.PreviousBadgeColor, _343Manager.PreviousBadgeColor);
			}
		}
		#endregion

		#region OnPocketDimensionEnter
		/// <summary>
		/// Prevent 106 from grabbing 343.
		/// </summary>
		public void OnPocketDimensionEnter(PlayerPocketDimensionEnterEvent ev)
		{
			if (Is343(ev.Player))
			{
				ev.Allow = false;
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
				if (Is343(player))
				{
					SCP_343Manager _343Manager = Get343Manager(player);
					_343Manager.Is343 = false;
					player.SetRank(_343Manager.PreviousBadgeColor, _343Manager.PreviousBadgeColor);
					player.HP = 100;
				}
			}
		}
		#endregion

		#region OnChangeLever
		/// <summary>
		/// If <see cref="PluginOptions.Nuke_Interact"/> is true then we allow SCP-343 to interact with the nuke, if it false then SCP-343 touching buttons will not do anything.
		/// </summary>
		public void OnChangeLever(WarheadChangeLeverEvent ev)
		{
			if (Is343(ev.Player))
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

		#region CallCommand & TriggerTesla
		/// <summary>
		/// This is called whenever a client types <see cref=".revert343"/> in client console (One you open with ~) but the . is stripped out so we can check if the command is equal to revert343
		/// </summary>
		/// <param name="ev"></param>
		public void OnCallCommand(PlayerCallCommandEvent ev)
		{
			if (!_343Config.SCP343_revert) return;
			
			if (ev.Command.ToLower() == "revert343")
			{
				if(Is343(ev.Player))
				{
					if (plugin.Server.Round.Duration >= _343Config.SCP343_RevertTime)
					{
						ev.ReturnMessage = "It's too late! You're 343 till death do you part.";
						return;
					}

					///We change the player to a default D-Class and feed the function true (so it resets items and HP) and false so it doesn't teleport them to the default spawn location.
					ev.Player.ChangeRole(Smod2.API.RoleType.CLASSD, true, false);

					ev.ReturnMessage = "You're no longer SCP-343.";
					return;
				}
				ev.ReturnMessage = "Wait you're not 343!";
				return;
			}
		}

		/// <summary>
		/// This prevents SCP-343 from triggering any Tesla Gates if he is in passive mode (Passive mode is if he has -1 HP and they're not counted as a player)
		/// </summary>
		/// <param name="ev"></param>
		public void OnPlayerTriggerTesla(PlayerTriggerTeslaEvent ev)
		{
			if (Is343(ev.Player) && _343Config.SCP343_HP == -1)
			{
				ev.Triggerable = false;
			}
		}
		#endregion

		#region 343 Manager
		public SCP_343Manager Add343Manager(ReferenceHub Ply)
		{
			/// If we already have the <see cref="SCP_343Manager"/> then we know they're already SCP-343.
			if (Ply.TryGetComponent(out SCP_343Manager _343Manager))
				return _343Manager;

			/// Don't allow the dedicated server to become SCP-343 (The server is a non-active player that doesn't do anything)
			if (Ply == ReferenceHub.HostHub) return null;

			/// We add <see cref="SCP_343Manager"/> to the player and it returns it after we add it so we can set some values to match the config values.
			SCP_343Manager _343ManagerComp = Ply.gameObject.AddComponent<SCP_343Manager>();

			return _343ManagerComp;
		}

		public SCP_343Manager Add343Manager(Player Ply)
		{
			return Add343Manager(ReferenceHub.GetHub((GameObject)Ply.GetGameObject()));
		}

		public bool Is343(ReferenceHub Ply)
		{
			///If the <see cref="SCP_343Manager.Is343"/> is true on the Player then we know that player is an active instance of SCP-343.
			if (Ply.TryGetComponent(out SCP_343Manager _343Manager))
			{
				return _343Manager.Is343;
			}
			else
			{
				Add343Manager(Ply);
			}
			return false;
		}

		public bool Is343(Player Ply)
		{
			/// Here we get the Smod2 player object and then we use the <see cref="Player.GetGameObject()"/> which returns a default C# Object and then we cast into a UnityEngine GameObject so SCP:SL's ReferenceHub can find the player.

			GameObject PlayerGameObject = (GameObject)Ply.GetGameObject();
			ReferenceHub hub = ReferenceHub.GetHub(PlayerGameObject);
			return Is343(hub);

			///Could also be done like return Is343(ReferenceHub.GetHub((GameObject)Ply.GetGameObject()));
		}

		public SCP_343Manager Get343Manager(ReferenceHub Ply)
		{
			if (Ply.TryGetComponent(out SCP_343Manager _343Manager))
			{
				return _343Manager;
			}

			return Add343Manager(Ply);
		}

		public SCP_343Manager Get343Manager(Player Ply)
		{
			/// Here we get the Smod2 player object and then we use the <see cref="Player.GetGameObject()"/> which returns a default C# Object and then we cast into a UnityEngine GameObject so SCP:SL's ReferenceHub can find the player.
			return Get343Manager(ReferenceHub.GetHub((GameObject)Ply.GetGameObject()));
		}
		#endregion
	}
}