# Mission Format

Mission files control the flow of mission events.

### File List

Here's the full list of files that use this format.

| Arc File | Internal File  | Description
|--------|-------|------------
| common_cd.arc | cdMis.bin | 
| common_dc.arc | dcMis.bin | 
| common_di.arc | diMis.bin | 
| common_dp.arc | dpMis.bin | 
| common_he.arc | heMis.bin | 
| common_jb.arc | jbMis.bin | 
| common_kg.arc | kgMis.bin | 
| common_ls.arc | lsMis.bin | 
| common_pp.arc | ppMis.bin | 
| common_rg.arc | rgMis.bin | 
| common_sb.arc | sbMis.bin | 
| common_sw.arc | swMis.bin | 
| common_vs.arc | vsMis.bin | 

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `MIS`. Null terminated.
| 0x4     | uint16  | Version. Always `3`.
| 0x6     | uint16  | Data Count

## Mission Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[12]   | Mission Name
| 0xC     | uint32   | Information CTD
| 0x10    | char[12]   | Start EXA file
| 0x1C    | char[12]   | Script
| 0x28    | char[12]   | EXA on Success
| 0x34    | char[12]   | EXA on Failure
| 0x40    | uint32   | Pause Information CTD
| 0x44    | uint8   | [Mission Kind](#Mission-Kind)
| 0x45    | uint8   | Flag
| 0x46    | uint8   | Bonus 1
| 0x47    | uint8   | Bonus 2
| 0x48    | uint16  | Bonus 1 Parameters
| 0x4A    | uint16  | Bonus 2 Parameters
| 0x4C    | uint8   | Navigation
| 0x4D    | uint8   | Present 1
| 0x4E    | uint8   | Present 2
| 0x4F    | uint8   | [General Path](#General-Path)
| 0x50    | uint16  | Present 1 Parameters
| 0x52    | uint16  | Present 2 Parameters
| 0x54    | uint8   | HP Recovery
| 0x55    | uint8   | Extra Recovery
| 0x56    | uint8   | Padding
| 0x57    | uint8   | Padding

### Bonus

Bonuses are rewards you receive as the boss is defeated or the event is over.

The type of bonus to receive.

Bonuses cannot give you **debug commands or abilities/enchantments/d-links**. Any command style can be given at any time.

**It is not possible to give two rewards of the same type.**

| Value | Description
|--------|-------
| 0     | None
| 1     | Maximum HP Increase
| 2     | Deck Capacity Increase
| 3     | Obtain Command

### Present

Presents are rewards you receive after the battle phase is over.

This field controls the type of present to receive.

Presents cannot give you **debug commands or abilities/enchantments**. Giving command styles out of order can also result in a crash.

Trying to give items that don't belong to the specific character result in a crash.

**It is possible to get two presents of the same type.**

| Value | Description
|--------|-------
| 0     | None
| 1     | Obtain Item
| 2     | Obtain Command

### General Path

| Value | Description
|--------|-------
| 0     | GENERAL_PATH_START
| 1     | GENERAL_PATH_END


### Mission Kind

This is the "name" attributed to the mission. Serves as an identifier.

| Value | Name  | Description
|--------|-------|------------
| -1    | MISSION_NONE   | 
| 0     | DEAD_ALL   | 
| 1     | BOSS_DEAD   | 
| 2     | SERCH_MIDGET   | 
| 3     | ESCAPE_FOREST   | 
| 4     | GUARD_CINDERELLA_0   | 
| 5     | GUARD_CINDERELLA_1   | 
| 6     | GUARD_JAQ   | 
| 7     | JOINT_STRUGGLE_PHILLIP_0   | 
| 8     | JOINT_STRUGGLE_PHILLIP_1   | 
| 9     | BTL_MDRAGON_PHILLIP   | 
| 10    | BTL_VANITAS_KING   | 
| 11    | BTL_DEAD_ALL_KING   | 
| 12    | BTL_DEAD_ALL_HERCULES   | 
| 13    | BTL_DEAD_ALL_ZACKS_HERCULES   | 
| 14    | BTL_DEAD_ALL_STITCH   | 
| 15    | BTL_PARTS_AQ_VE   | 
| 16    | BTL_PARTS_VE_TE   | 
| 17    | BTL_PARTS_AQ_TE   | 
| 18    | BTL_ROCK_TITAN_HERCULES   | 
| 19    | BTL_METAL_STITCH   | 
| 20    | BTL_IRON_AYA_RAGNA   | 
| 21    | MINIGAME_POT_BREAK_0   | 
| 22    | MINIGAME_POT_BREAK_1   | 
| 23    | BTL_GUNTU_STITCH   | 
| 24    | MINIGAME_RIDE_BATTLE   | 
| 25    | PARTITION_CLOSE   | 
| 26    | BTL_MANY_ENEMY_HERCULES   | 
| 27    | BTL_MANY_ENEMY   | 
| 28    | FIGHT_RALLY   | 
| 29    | MINIGAME_RIDE_RACE_EVENT   | 
| 30    | MINIGAME_RIDE_RACE_A   | 
| 31    | MINIGAME_RIDE_RACE_B   | 
| 32    | MINIGAME_RIDE_RACE_C   | 
| 33    | MINIGAME_RIDE_BATTLE_NMAP   | 
| 34    | MINIGAME_RIDE_BATTLE_METAMO   | 
| 35    | MINIGAME_ICE_MISSION   | 
| 36    | MINIGAME_ICE_M1_NORMAL   | 
| 37    | MINIGAME_ICE_M1_MANIACK   | 
| 38    | MINIGAME_ICE_M2_NORMAL   | 
| 39    | MINIGAME_ICE_M2_MANIACK   | 
| 40    | MINIGAME_BALL_MISSION   | 
| 41    | MINIGAME_BALL_VS_DEBU   | 
| 42    | MINIGAME_BALL_VS_TIP   | 
| 43    | MINIGAME_BALL_VS_DARK   | 
| 44    | MINIGAME_BALL_VS_JAST   | 
| 45    | MINIGAME_BALL_VS_HIDE   | 
| 46    | MINIGAME_POT_BREAK_2   | 
| 47    | BOSS_DEAD_TIMEov_or_HPxx   | 
| 48    |    | 
| 49    |    | 
| 50    | TUTORIAL_MOVE_AND_CAMERA   | 
| 51    | TUTORIAL_NORMAL_ATTACK   | 
| 52    | TUTORIAL_D_LINK   | 
| 53    | TUTORIAL_GUARD   | 
| 54    | TUTORIAL_CMD_GAUGE   | 
| 55    | TUTORIAL_SHOOT_LOCK   | 
| 56    | TUTORIAL_TEST_SELECT_CHARA   | 
| 57    | TUTORIAL_DUMMY   | 
| 58    | TUTORIAL_BTL_DICE   | 
| 59    | TUTORIAL_SAVE_POINT   | 
| 60    | TUTORIAL_PLAYER_SELECT   | 
| 61    | TUTORIAL_TEST_BTL_TERA   | 
| 62    | TUTORIAL_TEST_BTL_AQUA   | 
| 63    | TUTORIAL_TEST_BTL_VEN   | 
| 64    | TUTORIAL_MAX   | 
| 100   | BOSS_DEAD_EXT_METAMOL   | 
| 101   | BOSS_DEAD_EXT_PAN   | 
| 102   | BOSS_DEAD_EXT_VANITUS   | 
| 103   | BOSS_DEAD_EXT_VAN_VEN   | 
| 104   | BOSS_DEAD_EXT_XEA_VAN   | 
| 105   | BOSS_DEAD_TERA_ZEA   | 
| 106   | MINIGAME_ICE_M1_SPECIAL   | 
| 107   | MINIGAME_ICE_M2_SPECIAL   | 
| 108   | LIGHT_BALL   | 
| 109   | MINIGAME_ICE_M3_NORMAL   | 
| 110   | MINIGAME_ICE_M3_MANIACK   | 
| 111   | MINIGAME_ICE_M3_SPECIAL   | 
| 112   | MINIGAME_ICE_M4_NORMAL   | 
| 113   | MINIGAME_ICE_M4_MANIACK   | 
| 114   | MINIGAME_ICE_M4_SPECIAL   | 
| 115   | MINIGAME_ICE_M5_NORMAL   | 
| 116   | MINIGAME_ICE_M5_MANIACK   | 
| 117   | MINIGAME_ICE_M5_SPECIAL   | 
| 118   | MINIGAME_RIDE_RACE_D   | 
| 119   | FM_MISSION_ILLUSION_A   | 
| 120   | FM_MISSION_ILLUSION_B   | 
| 121   | FM_MISSION_ILLUSION_C   | 
| 122   | FM_MISSION_ILLUSION_D   | 
| 123   | FM_MISSION_ILLUSION_E   | 
| 124   | FM_MISSION_ILLUSION_F   | 
| 125   | FM_MISSION_ILLUSION_G   | 
| 126   | FM_MISSION_ILLUSION_H   | 
| 127   | FM_MISSION_ILLUSION_I   | 
| 128   |    | 