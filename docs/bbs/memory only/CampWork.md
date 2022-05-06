# CAMP WORK

This isn't a file, but a memory region in the executable.

## CAMP WORK DATA

| Offset | Type  | Description
|---------|--------|------------
| 0x0     | CmdWork | Command Work
| 0x18CA  | Unknown | Unknown
| 0x18CB  | Unknown | Unknown
| 0x18CC  | AbilityFlag[30] | Ability Flags
| 0x1944  | EtcFlag[55] | Etc Flags
| 0x197B  | HelpFlag[40] | Help Flags
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