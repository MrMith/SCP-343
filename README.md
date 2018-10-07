# [SCP-343](http://www.scp-wiki.net/scp-343)

| Config Option              | Value Type      | Default Value | Description |
|   :---:                    |     :---:       |    :---:      |    :---:    |
| SCP343_spawnchance         | Float           | 10            | Percent chance for SPC-343 to spawn at the start of the round. |
| SCP343_opendoortime        | Integer         | 300           | How many seconds till SCP-343 can open any door.               |
| SCP343_HP                  | Integer         | -1            | How much health should SCP-343 have, set to -1 for GodMode.    | 
| SCP343_nuke_interact       | Boolean         | true          | Should SCP-343 beable to interact with the nuke?               |
| SCP343_itemconverttoggle   | Boolean         | false         | Should SPC-343 convert items?                                  |
| SCP343_itemdroplist        | Integer List    | 0,1,2,3,4,5,6,7,8,9,10,11,14,17,19,22,27,28,29 | What items SCP-343 drops instead of picking up.|
| SCP343_itemconversionlist  | Integer List    | 13,16,20,21,23,24,25,26 | What items SCP-343 converts. |
| SCP343_itemconvertedlist   | Integer List    | 15         | What a item should be converted to.       |

| Command(s)                 | Value Type      | Description                              |
|   :---:                    |     :---:       |    :---:                                 |
| SpawnSCP343                | PlayerID        | Spawn SCP-343. Example = "SpawnSCP343 2", if for some reason this breaks you can use the forceclass command Example = "forceclass playerid 14"|
| SCP343_Version             | N/A             | Get the version of this plugin           |

SCP-343 is a passive immortal D-Class Personnel. He spawns with one Flashlight and any weapon he picks up is morphed to prevent violence. He seeks to help out who he deems worthy. 

Technically speaking hes a D-Class set to the role of TUTORIAL with godmode enabled or HP with the config option and spawns with the D-Class. After a X minute period set by the server he can open every door in the game. Also to make sure he is passive every weapon he picks up or spawns with is converted to a flashlight or something the server config can change. So people can know who he is, their rank is set to a red "SCP-343"

The TUTORIAL class doesn't affect who wins and he talks the same way a normal D-Class does.

If you see how stuff can be improved don't feel afraid to send me a pm or submit a issue.
