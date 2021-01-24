# This plugin is archived meaning it will ***NOT*** be updated.


# [SCP-343](http://www.scp-wiki.net/scp-343)
This is a server plugin for [SCP:SL.](https://store.steampowered.com/app/700330/SCP_Secret_Laboratory)
## Install Instructions.
Put SCP-343.dll under the release tab into sm_plugins folder.


## Config Options.
| Config Option              | Value Type      | Default Value | Description |
|   :---:                    |     :---:       |    :---:      |    :---:    |
| 343_spawnchance         | Float           | 10            | Percent chance for SPC-343 to spawn at the start of the round. |
| 343_opendoortime        | Integer         | 300           | How many seconds after roundstart till SCP-343 can open any door in the game (Like door bypass).               |
| 343_hp                  | Integer         | -1            | How much health should SCP-343 have. Set to -1 for GodMode and if set to anything but -1 then he is counted as a normal SCP and MTF must kill him like a normal SCP.| 
| 343_nuke_interact       | Boolean         | false         | Should SCP-343 beable to interact with the nuke?               |
| 343_itemconverttoggle   | Boolean         | false         | Should SPC-343 convert items?                                  |
| 343_itemdroplist        | Integer List    | 0,1,2,3,4,5,6,7,8,9,10,11,14,17,19,22,27,28,29 | What items SCP-343 drops instead of picking up.|
| 343_itemstoconvert      | Integer List    | 13,16,20,21,23,24,25,26,30 | What items SCP-343 converts. |
| 343_converteditems      | Integer List    | 15            | What a item should be converted to.       |
| 343_disable             | Boolean         | false         | Disable all of SCP-343.       |
| 343_broadcast           | Boolean         | true          | When 343 spawns should that person be given information about 343       |
| 343_broadcastinfo       | String          | ""            | What 343 is shown if scp343_broadcast is true.       |
| 343_revert              | Boolean         | True          | Should players be allowed to use the .heck343 client command to respawn themselves as d-class within 343_revert_time seconds of round start.     |
| 343_revert_time         | Integer         | 30            | How long people should beable to respawn themselves as d-class.     |

For more information about scp343_broadcastinfo see [This page](https://github.com/MrMith/SCP-343/wiki/scp343_broadcastinfo)

| Command(s)                 | Value Type      | Description                              |
|   :---:                    |     :---:       |    :---:                                 |
| 343spawn                  | PlayerID        | Spawn SCP-343 from PlayerID (Number next to name with M menu). Example = "343spawn 2" |
| 343_version               | N/A             | Get the version of this plugin           |
| 343_disable               | N/A             | Disables all of SCP-343                  |

## Lore Friendly Description 
SCP-343 is a passive immortal D-Class Personnel. He spawns with one Flashlight and any weapon he picks up is morphed to prevent violence. He seeks to help out who he deems worthy. 
## Technical Description  

To be clear this isn't the correct wiki version SCP-343, its just a passive SCP inspired by my experiences of people being Tutorial running around messing with people.

Technically speaking hes a D-Class with godmode enabled or HP with the config option and spawns with the D-Class. After a X minute period set by the server he can open every door in the game. Also to make sure he is passive every weapon he picks up or spawns with is converted to a flashlight or something the server config can change. So people can know who he is, their rank is set to a red "SCP-343" and if they die or are set to a different class their rank name and color are reset to what it was orginally.
SCP-343 doesn't affect who wins.


If you see how stuff can be improved don't feel afraid to send me a pm or submit a issue.
