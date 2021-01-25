# EPD Format
EPD probably stands for Entity Parameter Data and contains all the stats related to the character such as HP or damage dealt by every move.

The format consists on the following structures in order:  
[Header](###Header)  
[General Parameters](###General-Parameters)  
[Animation List](###Animation-List)  
[Other Parameters](###Other-Parameters)  
[Animation Parameters](###Animation-Parameters)  
[AI Parameters](###AI-Parameters)  

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | string   | File identifier, always `@EPD`
| 04     | int   | Version, `9`

### General Parameters

| Offset | Type  | Description
|--------|-------|------------
| 00     | byte  | 
| 01     | byte  | 
| 02     | byte  | 
| 03     | byte  | 
| 04     | float | Max HP (Bosses) Health Multiplier (Regular enemies)
| 08     | float | Experience Multiplier
| 0C     | unk  | 
| 10     | float  | Physical Damage Multiplier
| 14     | float  | Fire Damage Multiplier
| 18     | float  | Ice Damage Multiplier
| 1C     | float  | Thunder Damage Multiplier
| 20     | float  | Darkness Damage Multiplier
| 24     | float  | Non-Elemental Damage Multiplier

### Animation List

| Offset | Type  | Description
|--------|-------|------------
| 00     | string[24]  | Animation List

### Other Parameters

| Offset | Type  | Description
|--------|-------|------------
| 00     | short  | Damage Ceiling
| 02     | short  | Damage Floor?
| 04     | float  | 
| 08     | int    | 
| 0C     | int    | 
| 10     | int    | Number of [Animation Parameters](###Animation-Parameters).
| 14     | int    | Size of all [Animation Parameters](###Animation-Parameters).
| 18     | int    | unknown
| 1C     | int    | Size of all [AI Parameters](###AI-Parameters).
| 20     | int    | Number of [AI Parameters](###AI-Parameters).
| 24     | int    | Size of all [AI Parameters](###AI-Parameters).

### Animation Parameters

This structures repeats for as many animations need their parameters set.

| Offset | Type  | Description
|--------|-------|------------
| 00     | float  | Hit Damage Multiplier
| 04     | byte  | Animation index link?
| 05     | byte  | [Attack Kind](###Attack-Kind)
| 06     | byte  | [Guard State](###Guard-State)
| 07     | byte  | unknown `Usually always 0x64`

### AI Parameters  

This structure contains AI parameters that can be change the character's behavior.

| Offset | Type  | Description
|--------|-------|------------
| 00     | char[0xC]  | Parameter Name
| 0C     | float  | Parameter Value

#### Attack Kind
| Id | Kind | Description |
|----|-------|-----------|
| 0x01 | Small Damage | 
| 0x02 | Big Damage | 
| 0x03 | Blow Damage | 
| 0x04 | Toss Damage | 
| 0x05 | Beat Damage | 
| 0x06 | Flick Damage | 
| 0x07 | Poison | 
| 0x08 | Slow | 
| 0x09 | Stop | 
| 0x0A | Bind | 
| 0x0B | Stun | 
| 0x0C | Freeze | 
| 0x0D | Burn | 
| 0x0E | Confu | 
| 0x0F | Blind | 
| 0x10 | Death | 
| 0x11 | Kill | 
| 0x12 | Capture | 
| 0x13 | Magnet | 
| 0x14 | Zero Gravity | 
| 0x15 | Aero | Fly in circles.
| 0x16 | Tornado | More violent version of Aero
| 0x17 | Degenerator | No effect on player.
| 0x18 | Without | Long launch backwards.
| 0x19 | Eat | No effect on player.
| 0x1A | Treasure Raid | Bound by attack (MF Doom for example) Infinite loop
| 0x1B | Sleeping Death | Long launch backwards.
| 0x1C | Sleep | No Zs appear.
| 0x1D | Magnet Munny | Same as sleep but Zs appear on top of the character.
| 0x1E | Magnet HP | No effect on player.
| 0x1F | Magnet Focus | No effect on player.
| 0x20 | Mini | No effect on player.
| 0x21 | Quake | Mini.
| 0x22 | Recover | Normal hit.
| 0x23 | Discommand | Doesn't work on player.
| 0x24 | Disprize_M | Blow away commands?
| 0x25 | Disprize_H | Normal hit.
| 0x26 | Disprize_F | Normal hit.
| 0x27 | Detone | Long launch backwards.
| 0x28 | GM_BLOW | Blown upwards and stunned on landing.
| 0x29 | Blast | Long launch backwards and stunned on landing.
| 0x2A | Magnet Spiral | Character goes crazy moving around the whole arena.
| 0x2B | Glacial Arts | Long launch backwards.
| 0x2C | Transcendence | Long launch backwards.
| 0x2D | Vengeance | Same as "Blind".
| 0x2E | Magnet Breaker | Long launch backwards.
| 0x2F | Magic Impulse CF | Casts Confu or Freeze.
| 0x30 | Magic Impulse CFB | Casts Confu.
| 0x31 | Magic Impulse CFBB | Casts Confu, Freeze or Bind.
| 0x32 | Rise Damage | Launch forces you to be in the air.
| 0x33 | Stumble | Forced to be in Idle animation for 8 seconds.
| 0x34 | Mount | Forced to be in Idle animation while "Press O/X" appears on screen for a few seconds.
| 0x35 | Imprisonment | Character is attached to enemy's position.
| 0x36 | Slow Stop | Can result in Slow or Stop.
| 0x37 | Gathering | Freezes character for a long time.
| 0x38 | Exhausted | 1HP, No Focus, No D-Link, All Commands in cooldown.

#### Guard State

| Id | Kind | Description |
|----|-------|-----------|
| 0x02 | Half-Block | 
| 0x09 | Unblockable | 
| 0x81 | Blockable | 
