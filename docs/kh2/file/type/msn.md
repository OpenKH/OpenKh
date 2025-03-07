# [Kingdom Hearts II](../../index.md) - MSN

Mission file. They are located in `msn/{language}/` and describe how a certain map should behave. Internally they are just [bar](bar.md) files.

## Mission

This is the entry point of the file. It is a binary-type file and it's name is always `{WORLD_ID}{MAP_INDEX}`. Example: for the file `EH21_MS101`, where `EH` is the [world](../../worlds.md) and `21` is the map index, the mission name is called `EH21`.

This file is splitted into two parts: The first `0x1C` bytes are the [header](#header). This length is always fixed. After that comes a block of variable length which contains [opcodes](#opcodes). This is terminated by a 0x00 int.

### Header

| Offset | Type   | Description |
|--------|--------|-------------|
| 0x00   | uint16 | Magic Code, always `2`
| 0x02   | uint16 | Id (used in [ARD](ard.md))
| 0x04   | uint16 | [Flags](#flags)
| 0x06   | uint16 | Information Bar Text Id (loaded from `msg\{LANGUAGE}\{WORLD_ID}.bar`)
| 0x08   | byte/BitArray | [Pause Menu Controller ID](14mission.md)
| 0x09   | byte | Padding
| 0x0A   | uint16 | Pause Menu Information Text Id (loaded from `msg\{LANGUAGE}\{WORLD_ID}.bar`)
| 0x0C   | byte | Entry number in BAR to SEQD file for "Help" screen
| 0x0D   | byte | [Bonus Reward](00battle.md#bons)
| 0x0E   | byte | Antiform Multiplier
| 0x0F   | byte | Padding
| 0x10   | int | Sound effect when mission is started
| 0x14   | int | Sound effect when mission is finished
| 0x18   | int | Sound effect when mission is failed

#### Flags

| Bit | Description |
|-----|-------------|
| 0x1 | Is Boss Battle
| 0x2 | Is Drive Disabled
| 0x4 | Is Enable Place
| 0x8 | unused
| 0x10 | Is Show Weapon
| 0x20 | No Leave
| 0x40 | No Prize
| 0x80 | No Prizebox
| 0x100 | Hide Minimap
| 0x200 | Is Mickey spawnable
| 0x400 | Is No Experience
| 0x800 | Is Magic Disabled
| 0x1000 | Is Retry possible
| 0x2000 | Is Free Summon (Summon alone)
| 0x4000 | Is Summon Disabled
| 0x8000 | unused

#### Pause Menu Controller

Defines the type of Pause Menu Available when pressing start.

| Value | Description |
|-------|-------------|
| 00 | Only pause menu
| 01 | Only pause menu
| 02 | 4 button pause menu with text (cont, retry, help, quit)
| 03 | Only pause menu
| 04 | Only pause menu
| 05 | Pause menu with text
| 06 | Pause menu with text
| 07 | Only pause menu
| 08 | 3 button pause menu (retry, help, quit)
| 09 | Jiminy's journal
| 0A | Only pause menu
| 0B | Only pause menu
| 0C | Only pause menu
| 0D | Only pause menu
| 0E | Only pause menu
| 0F | Only pause menu
| -|-
| 1C | Pause Menu with Text and 3 Options (Continue, Jiminy's Journal, Save)
| 1E | Pause Menu with Text and 4 Options (Continue, Retry, Help, Quit)
| 1F | ???

This is most likely a BitArray and needs to be explored further.

#### Antiform Multiplier
Multiplies the chance of getting Antiform in a battle. Seems to affect the frequency with which Mickey can save Sora, too.

### Opcodes

This micro-code is responsible for certain elements to be used in missions.

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | [Operation code](#operation-code)
| 02     | short | Parameter count for the operation
| 04     | int[] | Parameters

### Operation code

There is a total of 13 operation codes for the mission script. The parser can be found at [sub_181d30].

- 01: [CameraStart](#camerastart)
- 02: [CameraComplete](#cameracomplete)
- 03: [CameraFailed](#camerafailed)
- 04: [Timer](#timer)
- 05: [Counter](#counter)
- 06: [Gauge](#gauge)
- 07: [ComboCounter](#combocounter)
- 08: [MissionScore](#missionscore)
- 09: [Watch](#watch)
- 0a: [LimitCost](#limitcost)
- 0b: [DriveRefillRatio](#driverefillratio)
- 0c: [AddDrive](#adddrive)
- 0d: [CameraPrize](#cameraprize)


#### CameraStart

Intro to missions. Either camera transitions or content from a SEQD.

| Offset | Type  | Description
|--------|-------|------------
| 00     | byte  | Entry number in BAR to AnimationLoader file (for camera)
| 01     | byte  | Entry number in BAR to SEQD file
| 02     | ushort | Object Id which camera will focus on

Used 227 times.
Generally uses Actor Object Id's instead of specific ones. I.e, it will use Id 566 for the PLAYER actor, instead of Id 84 for Base Sora.

#### CameraComplete

Ending to missions. Either camera transitions or content from a SEQD. Has the same structure as [CameraStart](#camerastart).

Used 240 times.

#### CameraFailed

Plays when an event is failed (for example when a struggle battle is lost). Either camera transitions or content from a SEQD. Has the same structure as [CameraStart](#camerastart).

Used 88 times.

#### Timer

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Initial value
| 04     | int   | Max value
| 08     | int   | Min value
| 0C     | byte  | Entry number in BAR to SEQD file. If no file is present the timer will be invisible.
| 0D     | byte  |
| 0E     | byte  | Position
| 0F     | byte  | Type

If `Initial value` >= `Max value` the timer will count down. Otherwise it will count up.
If `Position` is 0, the gauge will appear on the left-side of the screen.
If `Position` is 1, the gauge will appear on the right-side of the screen.
If `Position` is 2, the gauge will appear in the center of the screen.
`Timer` may also use `Type`, but it is currently unknown how.

Used 78 times.

#### Counter

This is used to count for example Struggle orbs, enemies, medals in Olympus Cups etc. Takes 4 arguments. The order of counters will determine their position on the screen, with earlier entries getting the top-most position.

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Initial value
| 04     | int   | Max value
| 08     | int   | Min value
| 0C     | byte  | Entry number in BAR to SEQD file. If no file is present the counter will be invisible.
| 0D     | byte  |
| 0E     | byte  | Position
| 0F     | byte  | Type

Used 208 times.
`Type` determines if the current counter should display an overall record, or a current count.
Ex. If `Type` is 0x01, then the gauge will show the highest recorded value achieved by the player. If `Type` is 0x00, then the gauge will start from 0 and count for the current mission.

#### Gauge

Used 55 times.

#### ComboCounter
This is used to count the number of times a player has hit an enemy.
| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Initial value
| 04     | int   | Max value
| 08     | int   | Min value
| 0C     | byte  | Entry number in BAR to SEQD file. If no file is present the counter will be invisible.
| 0D     | byte  |
| 0E     | byte  | Position
| 0F     | byte  | Type

Used 40 times.
`Type` may crash if set to 1, though there may be instances it gets used. It is currently unknown how.

#### MissionScore

Used 0 times.

#### Watch

Used 11 times.

#### LimitCost

Defines how much MP a Limit consumes.

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | MP cost

Used 10 times.

#### DriveRefillRatio

Defines how fast the drive gauge fills/drains, and also determines if Sora can use other Drive Forms when alone. 
| Offset | Type  | Description
|--------|-------|------------
| 00     | float | Refill Multiplier

Used 10 times.
Refill Multiplier cannot be equal to 1, otherwise Sora will not have access to other forms when alone.

#### AddDrive

Used 11 times.

#### CameraPrize

Currently unknown. Either camera transitions or content from a SEQD. Has the same structure as [CameraStart](#camerastart).

Used 42 times.

## The `miss` entry

Present for most of the mission files, it is a PAX file that it is always named `miss`.

Its purpose is to play distortion effects for different cuts used by the Camera.

## Event files

A mission file can contain between 0 and multiple Event files. Their names are usually `0a`, `1a`.

Its purpose seems to be defining how the camera should move before the battle starts. 

## Sequences

Most of the missions have a pair of IMGD and SEQD. The most common file names is `ct_e`, `st_h`, `ed_h`, `gh_h`, `cb_e`, `ed_t`, `ti_e`, `st_t` and many others.

How they are used is unknown and the meaning of their file names is currently unknown.

## Boss script

A mission that hosts a boss battle has a `ms_b` file, which is an AI script.

## Unknown scripts

`ms_d`, `ms_m`, `ms_a`, `ms_g`, `kino` are some of the script names where their purposes is still unknown.
