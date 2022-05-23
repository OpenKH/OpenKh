# CAMP WORK

This isn't a file, but data specification that appears instantiated in memory.

## CAMP WORK DATA

| Offset | Type  | Description
|---------|--------|------------
| 0x0     | [CmdWork](./CmdWork.md) | Command Work
| 0x18CA  | Unknown | Unknown
| 0x18CB  | Unknown | Unknown
| 0x18CC  | [AbiFlag](#Ability-Flag)[30] | Ability Flags
| 0x1944  | [EtcFlag](#Etc-Flag)[55] | Etc Flags
| 0x197B  | [HelpFlag](#Help-Flag)[40] | Help Flags
| 0x19A3  | NickNameWork | Nickname Work
| 0x19C9  | Unknown | Unknown
| 0x19CA  | Unknown | Unknown
| 0x19CB  | Unknown | Unknown
| 0x19CC  | [StatusInfo](./StatusInfo.md) | Status Info
| 0x2584  | [GameInfo](./GameInfo.md) | Game Info
| 0x2594  | DeckWork[3] | Deck Work
| 0x27E0  | int32[4] | Padding
| 0x27F0  | uint16 | Deck Selected
| 0x27F2  | uint16 | Deck Max Slots
| 0x27F4  | boolean | Is Initialized
| 0x27F5  | uint8 | Mirage Arena Explain
| 0x27F6  | boolean | Is Tutorial
| 0x27F7  | boolean | Is Player Status
| 0x27F8  | uint8 | Tutorial State
| 0x27F9  | uint8 | Battle Dice Explain
| 0x27FA  | uint16 | Deck Shortcut
| 0x27FC  | uint16[4][2] | Arena Flag Array
| 0x280C  | int8 | Number of Trophy Info
| 0x280D  | uint8[7] | Padding


### ABILITY FLAG


| Bit     | Length | Description
|-----|--------|------------
| 0   | 11 | Padding
| 11  | 3 | Master Notice
| 14  | 2 | fgNewReport
| 16  | 2 | fgNewCamp
| 18  | 5 | Master On
| 23  | 3 | Master
| 26  | 3 | Equip
| 29  | 3 | Total

### ETC FLAG


| Bit     | Length | Description
|---------|--------|------------
| 0   | 4 | Padding
| 4   | 2 | fgNewReport
| 6   | 2 | fgNewCamp

### HELP FLAG


| Bit     | Length | Description
|---------|--------|------------
| 0   | 3 | Padding
| 3   | 3 | Category
| 6   | 2 | fgNewCamp
