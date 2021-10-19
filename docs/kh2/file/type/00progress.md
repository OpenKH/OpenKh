# [Kingdom Hearts II](../../index.md) - 00progress.bin

This is the file that determines most of the effects of story flags. It is a BAR file with the following subfiles:
- [World-Specific](#World-Specific) - The name of the subfile correspond to the [internal name of the world](../../worlds.md).
- [DSA](#DSA)   - ??? (Dictionary for inaccessible areas)
- [WLDS](#WLDS) - World Spawn
- [EPSD](#EPSD) - Episode
- [DARK](#DARK) - Darkness
- [WLDF](#WLDF) - World Flag
- [LINK](#LINK) - Link
- [WMKY](#WMKY) - World Map Keys

---

## World-Specific

Contains scripts that're executed when a world's story flag is raised. Consists of 2 sections: Header and Operations.

### Header

The header consists of 2-byte pointers toward offsets within the subfile. When a flag is raised, the game will execute the operations starting from the offset until termination. If the value of the pointer is 0 or it is above the length of the subfile, no operation will be done.

The pointer used by a world's x-th flag is located at offset 2x (for example, a world's 0th flag is at offset 0 and a world's 20th flag is at offset 0x28). There are no defined amount of story flags or headers. Therefore, it is possible for the game to read junk data.

### Operation Structure

| Offset | Type   | Description
|--------|--------|------------
| 0 	 | uint8  | Operation Code
| 1   	 | uint8  | Argument Count
| 2      | uint16 | Arguments

### Operation Codes

- 0: [Termination](#termination)
- 1: [Change ARD program](#change-ard-program)
- 2: [Block area](#block-area)
- 3: [Unblock area](#unblock-area)
- 4: [Add World Point](#add-world-point)
- 5: [Remove World Point](#remove-world-point)
- 6: [Change BGM Set](#change-bgm-set)
- 7: [Lower progress flag](#lower-progress-flag)
- 8: [Raise menu flag](#raise-menu-flag)
- 9: [Lower menu flag](#lower-menu-flag)
- C: [Raise progress flag](#raise-progress-flag)
- D: [Change world map status](#change-world-map-status)

#### Termination

Stops the script execution.

#### Change ARD Program

Changes the Spawn ID stored in the save file, which determines the program used by the room's [ARD script.](areadata.md). The number of arguments is a multiple of 4, which repeats the following pattern: Area, MAP, BTL, and EVT. A value of -1 for MAP, BTL, or EVT means the corresponding Spawn ID is unchanged.

#### Block Area

Makes an area inaccessible, including from the World Map. Contains 3 arguments: [DSA code](#dsa), text that appear when Sora tries to walk to said area, and ???.

#### Unblock Area

Removes the effect of the above operation. Uses [DSA code](#dsa).

#### Add World Point

Grants access to the area from the World Map without needing to visit it. Uses [DSA code](#dsa).

#### Remove World Point

Revokes access to the area from the World Map, requiring Sora to revisit it. Uses [DSA code](#dsa).

#### Change BGM Set

Changes which BGM set plays, as defined on ARIF in [03system.bin](03system.md).

#### Lower Progress Flag

Lowers the progress flag, enabling it to be reraised including all the operations tied to it.

#### Raise Menu Flag

Raises a menu flag.

#### Lower Menu Flag

Lowers a menu flag.

#### Raise Progress Flag

Raises another progress flag, executing the script triggered by it.

#### Change World Map Status

Changes the status of a world as seen on the world map (darked out, normal, glowing, etc)

## DSA

Defines codes to be used by some operations in the world-specific subfiles.

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint16 | Entry count

### Entry

| Offset | Type  | Description
|--------|-------|------------
| 0      | uint8 | World ID
| 1      | uint8 | Room ID

A code of x correspond x-th entry, which is located in offset 2x from the header (or 2x+2 from start of subfile). For example, DSA code 0x69 correspond to offset 0xD2 from the header (or 0xD4 from start of subfile).

## WLDS

Determines where the spawn area from the World Map if none of the World Points are available (for example, at start of visits).

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | File type (1)
| 4      | uint32 | Entry count

### Entry

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint8  | [World ID](../../worlds.md)
| 1      | uint8  | Episode
| 2      | uint16 | Argument

When the argument's value is 50 or less, it refers to an area. If it's more than 50, it refers to ARD Program while the area is defined in [07localset.bin](07localset.md)

## EPSD

Determines a world's summary as seen from the World Map.

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | File type (1)
| 4      | uint32 | Entry count

### Entry

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint16 | Entry ID
| 2      | uint8  | World
| 3      | uint8  | Entry ID within world
| 4      | uint16 | Episode Title Text ID
| 6      | uint16 | Episode Summary Text ID

Entry IDs (both regular & within world) don't seem to do anything. All entries for a given world must be next to each other and are shown sequentially. Any entries for a world after a break will not be shown.

## DARK

Determines the boundaries of the World Map, including the boundaries of the world map and the shrouds covering the worlds (but not the worlds' reflective barriers)

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | File type (1)
| 4      | uint32 | Entry count

### Entry

Each entry is 24 bytes long, but it is unknown how exactly they work.

## WLDF

Purpose unknown.

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | File type (1)
| 4      | uint32 | Entry count

### Entry

0  | uint8  | Unknown
1  | uint8  | Unknown
2  | uint16 | Unknown
4  | uint16 | Story Flag
6  | uint16 | Unknown
8  | uint32 | Unknown
12 | uint32 | Pointer to another part of subfile

### Pointed Area

0 | uint16 | Unknown
2 | uint16 | Argument count
4 | uint16 | Arguments

## LINK

Links flags together, making it possible to conditionally trigger a flag based on the status of other flags.

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | File type (1)
| 4      | uint32 | Entry count

### Entry

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint16 | Category of target
| 2      | uint16 | Argument of target
| 4      | uint16 | Category of condition 1
| 6      | uint16 | Argument of condition 1
| 8      | uint16 | Category of condition 2
| 10     | uint16 | Argument of condition 2
| 12     | uint16 | Category of condition 3
| 14     | uint16 | Argument of condition 3
| 16     | uint16 | Category of condition 4
| 18     | uint16 | Argument of condition 4
| 20     | uint16 | Category of condition 5
| 22     | uint16 | Argument of condition 5
| 24     | uint16 | Category of condition 6
| 26     | uint16 | Argument of condition 6
| 28     | uint16 | Category of condition 7
| 30     | uint16 | Argument of condition 7
| 32     | uint16 | Category of condition 8
| 34     | uint16 | Argument of condition 8
| 36     | uint16 | Category of condition 9
| 38     | uint16 | Argument of condition 9
| 40     | uint16 | Category of condition 10
| 42     | uint16 | Argument of condition 10
| 44     | uint16 | Category of condition 11
| 46     | uint16 | Argument of condition 11
| 48     | uint16 | Category of condition 12
| 50     | uint16 | Argument of condition 12
| 52     | uint16 | Category of condition 13
| 54     | uint16 | Argument of condition 13
| 56     | uint32 | Required conditions

When the required amount of condition bits are true (bit set to 1), the target will also be set to true.

### Flag Categories

- 1 - Progress Flag
- 2 - World Flag
- 3 - Menu Flag

## WMKY

Determines if a world's Keyhole is shown, hidden or only shown as gray square in the World Map.

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | File type (1)
| 4      | uint32 | Entry count

### Entry

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | World ID
| 4      | uint16 | Progress Flag to Show Key 1
| 6      | uint16 | Progress Flag to Complete Key 1
| 8      | uint16 | Progress Flag to Show Key 2
| 10     | uint16 | Progress Flag to Complete Key 2
| 12     | uint16 | Progress Flag to Show Key 3
| 14     | uint16 | Progress Flag to Complete Key 3
