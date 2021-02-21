# EPD Format

EPD stands for Entity Parameter Data and contains all the stats related to the character stats.

These files are contained within the `.arc` files of character beginning with `e` or `b`, for example, `b01ex00.arc` has a file named `b01ex00.epd` which contains its stats.

The format consists on the following structures in order:  
[Header](#Header)  
[General Parameters](#General-Parameters)  
[Animation List](#Animation-List)  
[Other Parameters](#Other-Parameters)  
[Technique Parameters](#Technique-Parameters)  
[Drop Items](#Drop-Items)  
[Extra Parameters](#Extra-Parameters)  

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | string   | File identifier, always `@EPD`
| 0x4     | int   | Version, `9`

## General Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint  | [Status Ailments](#Status-Ailments) flag
| 0x4     | float | Max HP (Bosses) Health Multiplier (Enemies)
| 0x8     | float | Experience Multiplier
| 0xC     | uint  | iSize (Unknown)
| 0x10     | float  | Physical Damage Multiplier
| 0x14     | float  | Fire Damage Multiplier
| 0x18     | float  | Ice Damage Multiplier
| 0x1C     | float  | Thunder Damage Multiplier
| 0x20     | float  | Darkness Damage Multiplier
| 0x24     | float  | Non-Elemental Damage Multiplier

### Status Ailments

| Bit | Count | Description 
|-----|-------|-------------
|  0 | 1 | Fly
|  1 | 1 | Small Damage Reaction
|  2 | 1 | Small Damage Reaction Only
|  3 | 1 | Hitback
|  4 | 1 | dummy4
|  5 | 1 | dummy5
|  6 | 1 | dummy6
|  7 | 1 | dummy7
|  8 | 1 | dummy8
|  9 | 1 | dummy9
| 10 | 1 | Poison
| 11 | 1 | Slow
| 12 | 1 | Stop
| 13 | 1 | Bind
| 14 | 1 | Faint
| 15 | 1 | Freeze
| 16 | 1 | Burn
| 17 | 1 | Confuse
| 18 | 1 | Blind
| 19 | 1 | Death
| 20 | 1 | Zero Gravity
| 21 | 1 | Minimum
| 22 | 1 | Magnet
| 23 | 1 | Degen
| 24 | 1 | Sleep
| 25 | 1 | dummy25
| 26 | 1 | dummy26
| 27 | 1 | dummy27
| 28 | 1 | dummy28
| 29 | 1 | dummy29
| 30 | 1 | dummy30
| 31 | 1 | dummy31

## Animation List

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]  | Animation List (mnDamageMotion)

Animation List has 20 instances and 8 filler bytes.

## Other Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | ushort  | Damage Ceiling
| 0x2     | ushort  | Damage Floor
| 0x4     | float  | fWeight
| 0x8     | uint    | [Effectiveness Flag](#Effectiveness-Flag)
| 0xC     | char    | Prize Box probability.
| 0xD     | char[3] | padding
| 0x10     | uint    | Number of [Technique Parameters](#Technique-Parameters).
| 0x14     | uint    | Offset to [Technique Parameters](#Technique-Parameters).
| 0x18     | uint    | Number of [Drop Items](#Drop-Items).
| 0x1C     | uint    | Offset to [Drop Items](#Drop-Items).
| 0x20     | uint    | Number of [Extra Parameters](#Extra-Parameters).
| 0x24     | uint    | Offset to [Extra Parameters](#Extra-Parameters).

### Effectiveness Flag

| Bit | Count | Description 
|-----|-------|-------------
|  0 | 2 | Effective Poison
|  2 | 2 | Effective Stop
|  4 | 2 | Effective Bind
|  6 | 2 | Effective Faint
|  8 | 2 | Effective Blind
| 10 | 2 | Effective Minimum
| 12 | 20| padding

## Technique Parameters

This structures repeats for as many animations need their parameters set.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | float  | Technique Damage Multiplier
| 0x4     | byte  | Technique Number
| 0x5     | byte  | [Attack Kind](#Attack-Kind)
| 0x6     | byte  | [Attack Attribute](#Attack-Attribute)
| 0x7     | byte  | Success Rate (Usually 0x64 [100])

## Drop Parameters

Items dropped by enemies.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint  | Value for [Drop Kind](#Drop-Kind)
| 0x4     | ushort  | Number
| 0x6     | ushort  | Probability

### Drop Kind

| Id | Kind  | Description
|--------|-------|------------
| 0x0     | ITEM_KIND_HP_SMALL  | Small HP orb.
| 0x1     | ITEM_KIND_HP_BIG  | Big HP orb.
| 0x2     | ITEM_KIND_MUNNY_SMALL  | Small Munny orb.
| 0x3     | ITEM_KIND_MUNNY_MIDDEL  | Middle Munny orb.
| 0x4     | ITEM_KIND_MUNNY_BIG  | Big Munny orb.
| 0x5     | ITEM_KIND_FOCUS_SMALL  | Small Focus orb.
| 0x6     | ITEM_KIND_FOCUS_BIG  | Big Focus orb.
| 0x7     | ITEM_KIND_DRAINMIST  | 
| 0x8     | ITEM_KIND_D_LINK  | 
| 0x9     | ITEM_KIND_MANDORAKE1  | 
| 0xA     | ITEM_KIND_MANDORAKE2  | 
| 0xB     | ITEM_KIND_JERRYBALL1  | 
| 0xC     | ITEM_KIND_JERRYBALL2  | 
| 0xD     | ITEM_KIND_JERRYBALL3  | 
| 0xE     | ITEM_KIND_JERRYBALL4  | 
| 0xF     | ITEM_KIND_JERRYBALL5  | 
| 0x10    | ITEM_KIND_JERRYBALL6  | 
| 0x11    | ITEM_KIND_JERRYBALL7  | 
| 0x12    | ITEM_KIND_JERRYBALL8  | 

## Extra Parameters  

This structure contains AI parameters that can change the values in variables used in the enemy's LUA.

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

#### Attack Attribute

| Id | Kind  
|----|-------
| 0x0 | Non/Elemental
| 0x1 | Physical
| 0x2 | Fire
| 0x3 | Ice
| 0x4 | Thunder
| 0x5 | Darkness
| 0x6 | Zero
| 0x7 | Special
| 0x8 | MAX
