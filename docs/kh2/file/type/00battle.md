# [Kingdom Hearts II](../../index.md) - 00battle.bin

This is an essential file for booting [Kingdom Hearts II](../../index.md) and it contains everything related to the battle system.

It is a [BAR](bar.md) file and contains the following subfiles:

* [ATKP](#atkp) - Attack Params
* [PTYA](#ptya) - Party Attacks
* [PRZT](#przt) - Prize Table
* [VTBL](#vtbl) - Voice Table
* [LVUP](#lvup) - Level Up
* [BONS](#bons) - Bonus
* [BTLV](#btlv) - Battle Level
* [LVPM](#lvpm) - Level Params
* [ENMP](#enmp) - Enemy Params
* [PATN](#patn) - Pattern
* [PLRP](#plrp) - Player Params
* [LIMT](#limt) - Limits
* [SUMN](#sumn) - Summons
* [MAGC](#magc) - Magic
* [VBRT](#vbrt) - ???
* [FMLV](#fmlv) - Form Levels
* [STOP](#stop) - ???
* [0A](#0a) - ??? (3 entries)

## Atkp

Contains the parameters for the various actions in the game.
The damage effects' values on MSET files point to this table.

### Atkp Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Atkp header
| 2713 	 | Atkp entries

### Atkp Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (6)
| 0x4 	 | uint32 | Entry Count

### Atkp Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint16 | SubId
| 0x2 	 | uint16 | Id
| 0x4 	 | int8 | Type (0 normal, 1 pierces armor...)
| 0x5 	 | int8 | Critical Adjust (0 normal, 1 half damage, 2 no damage)
| 0x6 	 | uint16 | Power
| 0x8 	 | int8 | Team (Deal damage to: 0/1/2 Enemies, 3/4/5 Enemies and allies...)
| 0x9 	 | int8 | Element (0 phys, 1 fire, 2 blizz, 3 thun...)
| 0xA 	 | int8 | Reaction (Whether an enemy is flinched, knocked...)
| 0xB 	 | int8 | Effect on hit (0 none, other values = different effects)
| 0xC 	 | int16 | Knockback Strength 1 (Distance depends on enemy weight)
| 0xE 	 | int16 | Knockback Strength 2 (Distance depends on enemy weight)
| 0x10 	 | int16 | ???
| 0x12 	 | int8 | Flag (Eg: 20/22 can defeat bosses)
| 0x13 	 | int8 | Refact Self
| 0x14 	 | int8 | Refact Other
| 0x15 	 | int8 | Reflected motion (Points to the slot in the MSET to be triggered when the attack is reflected)
| 0x16 	 | int16 | Reflect Hit Back
| 0x18 	 | int32 | Reflect Action
| 0x1C 	 | int32 | Hit Sound Effect
| 0x20 	 | uint16 | Reflect RC
| 0x22 	 | int8 | Reflect Range
| 0x23 	 | int8 | Reflect Angle
| 0x24 	 | int8 | Damage Effect
| 0x25 	 | int8 | Switch
| 0x26 	 | uint16 | Interval (1 hit every X frames)
| 0x28 	 | int8 | Floor Check
| 0x29 	 | int8 | Drive drain (Adds on normal state, reduces when in a form)
| 0x2A 	 | int8 | Revenge damage
| 0x2B 	 | int8 | Tr Reaction
| 0x2C 	 | int8 | Combo Group
| 0x2D 	 | int8 | Random Effect
| 0x2E 	 | int8 | Kind
| 0x2F 	 | int8 | HP drain (Adds on normal state, reduces when in a form)

## Ptya

Contains data for the party's attacks.

Contains a list of pointers that point to the offset of a Ptya set. There are multiple pointers for multiple sets.
Ptya sets contain the list of attack animations a character use in certain situations in a combo.

### Ptya Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Ptya header
| 70 	 | Ptya pointers
| 15 	 | Ptya sets

### Ptya Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Pointer Count

### Ptya Set Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Ptya Set Header
| X 	 | Ptya Set Entry

### Ptya Set Header

| Offset | Type  | Description
|--------|-------|--------------
| 0     | uint32 | Ptya Set Entry Count

### Ptya Set Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0x0      | int8 | Id
| 0x1      | int8 | Type
| 0x2      | int8 | Sub
| 0x3      | int8 | Combo Offset
| 0x4      | uint32 | Flag
| 0x8      | uint16 | Motion Id \*
| 0xA      | uint16 | Next Motion Id \*
| 0xC      | float | Jump
| 0x10     | float | Jump Max
| 0x14     | float | Jump Min
| 0x18     | float | Speed Min
| 0x1C     | float | Speed Max
| 0x20     | float | Near
| 0x24     | float | Far
| 0x28     | float | Low
| 0x2C     | float | High
| 0x30     | float | Inner Min
| 0x34     | float | Inner Max
| 0x38     | float | Blend Time
| 0x3C     | float | Distance Adjust
| 0x40     | uint16 | Ability - SubId on [Item](./03System#Item)
| 0x42     | uint16 | Score

\* Multiply by 4 to get the slot of the motion in the entity's [moveset file](../anb/mset.md).

## Przt

Contains the item drop table.
The ID of the entry is assigned in the AI of the object.

### Przt Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Przt header
| 184 	 | Przt entries

### Przt Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (2)
| 0x4 	 | uint32 | Entry Count

### Przt Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0x0      | uint16 | ID
| 0x2      | int8  | Small HP orbs
| 0x3      | int8  | Big HP orbs
| 0x4      | int8  | Big Money orbs
| 0x5      | int8  | Medium Money orbs
| 0x6      | int8  | Small Money orbs
| 0x7      | int8  | Small MP orbs
| 0x8      | int8  | Big MP orbs
| 0x9      | int8  | Small Drive orbs
| 0xA      | int8  | Big Drive orbs
| 0xB      | int8  | Unknown
| 0xC      | uint16  | Item 1 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0xE      | int16  | Item 1 Drop Percentage
| 0x10     | uint16  | Item 2 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0x12     | int16  | Item 2 Drop Percentage
| 0x14     | uint16  | Item 3 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0x16     | int16  | Item 3 Drop Percentage

## Vtbl

Contains data for randomizing voice clips.

### Vtbl Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Vtbl header
| 241 	 | Vtbl entries

### Vtbl Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (1)
| 4 	 | uint | Entry Count

### Vtbl Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0x0 	 | byte | Character - [CHARACTER LIST](../../dictionary/characters.md)
| 0x1 	 | byte | Action 
| 0x2 	 | byte | Priority?
| 0x3 	 | byte | Padding?
| 0x4 	 | byte | Voice 1
| 0x5 	 | byte | Voice 1 Chance
| 0x6 	 | byte | Voice 2
| 0x7 	 | byte | Voice 2 Chance
| 0x8 	 | byte | Voice 3
| 0x9 	 | byte | Voice 3 Chance
| 0xA  	 | byte | Voice 4
| 0xB  	 | byte | Voice 4 Chance
| 0xC  	 | byte | Voice 5
| 0xD  	 | byte | Voice 5 Chance

## Lvup

Contains the level-up table for every playable character.
The Lvup entries follow this sequence:

### Character sequence

* Sora / Roxas
* Donald
* Goofy
* Mickey
* Auron
* Ping / Mulan
* Aladdin
* Sparrow
* Beast
* Jack
* Simba
* Tron
* Riku

NOTE: The first character pointer doesn't point to any character. May be some kind of padding.

### Lvup Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Lvup header
| 13 	 | Character pointer
| 13 	 | Character

### Lvup Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint | File type (2)
| 0x4 	 | uint | Character pointer count

### Character pointer

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint | Offset of the character (Measured in 4 bytes, so 10 means offset 40)
| 0x4 	 | uint | Padding

### Character Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Character header
| 99 	 | Lvup Entry

### Character header

| Offset | Type | Description |
|--------|------|-------------|
| 0x0      | uint32  | Lvup entry count
| 0x4      | uint32  | Padding

### Lvup Entry

| Offset | Type | Description |
|--------|------|-------------|
| 0x0      | int32  | Needed EXP for next level
| 0x4      | uint8 | Strength of Character
| 0x5      | uint8 | Magic of Character
| 0x6      | uint8 | Defense of Character
| 0x7      | uint8 | AP of Character
| 0x8      | int16 | Ability given when using Sword route (03system.bin --> ITEM sub file) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0xA      | int16 | Ability given when using Shield route (03system.bin --> ITEM sub file) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0xC      | int16 | Ability given when using Staff route (03system.bin --> ITEM sub file) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0xE      | int16 | Padding

## Bons

Contains reward items (GET! BONUS).
The ID is assigned in the msn file (first sub file, offset 0xD).

### Bons Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Bons header
| 179 	 | Bons entries

### Bons header

| Offset | Type | Description |
|--------|------|-------------|
|  0x0     | int32 | File type (2)
|  0x4     | int32 | Number of 'Bons' entries

### Bons entry

| Offset | Type  | Description
|--------|-------|--------------
|  0x0     | int8  | ID - [EVENT LIST](../../dictionary/events.md)
|  0x1     | int8  | Character Id
|  0x2     | int8  | HP Increase
|  0x3     | int8  | MP Increase
|  0x4     | int8  | Drive Gauge Upgrade
|  0x5     | int8  | Item Slot Upgrade
|  0x6     | int8  | Accessory Slot Upgrade
|  0x7     | int8  | Armor Slot Upgrade
|  0x8     | int16 | Bonus Item 1 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
|  0xA     | int16 | Bonus Item 2 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
|  0xC     | int32 | Unknown

## Btlv

Determines how the battle level bitmask affects each world's actual battle level.

### Btlv Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Btlv header
| 20 	 | Btlv entries

### Btlv Header

Contains the table for battle level of each world. Whether each entry is enabled or not is determined by a bitmask and the final battle level will be the sum of all of the world's enabled battle lv entries.

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (1)
| 0x4 	 | uint32 | Entry Count

### Btlv Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0x0 	 | uint32  | Entry Index
| 0x4      | int8[2] | Unknown
| 0x6      | int8  | World ZZ
| 0x7      | int8  | World of Darkness
| 0x8      | int8  | Twilight Town
| 0x9      | int8  | Destiny Islands
| 0xA      | int8  | Hollow Bastion
| 0xB      | int8  | Beast's Castle
| 0xC      | int8  | Olympus Coliseum
| 0xD      | int8  | Agrabah
| 0xE      | int8  | Land of Dragons
| 0xF      | int8  | 100 Acre Woods
| 0x10     | int8  | Pride Lands
| 0x11     | int8  | Atlantica
| 0x12     | int8  | Disney Castle
| 0x13     | int8  | Timeless River
| 0x14     | int8  | Halloween Town
| 0x15     | int8  | World Map
| 0x16     | int8  | Port Royal
| 0x17     | int8  | Space Paranoids
| 0x18     | int8  | The World that Never Was
| 0x19     | int8[7] | Unknown

## Lvpm

Contains the level-up table for the enemies. Based on the level of an enemy, a specific level, containing multiplying values, will be applied over the base statistics of an enemy.

### Lvpm Structure

| Amount | Description |
|--------|---------------|
| 99 	 | Lvpm entries

### Lvpm Entry

| Offset | Type  | Description
|--------|------ |--------------
| 0x0      | int16 | HP level. The formula is `(EnemyHp * LevelHp + 99) / 100`.
| 0x2      | int16 | Strength
| 0x4      | int16 | Defense
| 0x6      | int16 | ???
| 0x8      | int16 | ???
| 0xA      | int16 | Exp. The formula is similar to HP.

## Enmp

Contains enemy statistics.

All the weaknesses are represented in percentage unit. So a weakness with the value of 100 represents that the damage received is unfiltered. 200 the enemy receives double of the damage for that specific element, while 0 nullifies it.

A single enemy has also 32 different HP units, where the first one is the HP of the main entity. Some enemies uses the other 31 entries too, but their purpose is currently unknown.

Every enemy is associated to one or more IDs (eg. Organization members have different ID based if they are in their first fight or they are their data version). Different enemies can use the same ID. The way the game associates an ID to a specific MDLX is done by AI scripting.

### Enmp Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Enmp header
| 229 	 | Enmp entries

### Enmp Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (2)
| 0x4 	 | uint32 | Entry Count

### Enmp Entry

| Offset | Type  | Description
|--------|------|--------------
|  0x0     | uint16 | Identifies the enemy. - [Enemy LIST](../../dictionary/enemy.md)
|  0x2     | uint16 | Level of the enemy. Must be between 1 and 99. (0 uses the world's battle level)
|  0x4     | uint16[32] | Health amount. It is multiplied by Hp from [LVPM](#lvpm).
| 0x44     | uint16 | Damage Cap. (The higher, the less damage received)
| 0x46     | int16 | ???
| 0x48     | uint16 | Physical weakness.
| 0x4A     | uint16 | Fire weakness.
| 0x4C     | uint16 | Blizzard weakness.
| 0x4E     | uint16 | Thunder weakness.
| 0x50     | uint16 | Dark weakness.
| 0x52     | uint16 | Neutral weakness.
| 0x54     | uint16 | General weakness.
| 0x56     | uint16 | Exp multiplier.
| 0x58     | int16 | Unknown
| 0x5A     | int16 | Unknown

## Patn

Defines multiple parameters per group of entities. How an entity is assigned to a group is unknown.
EG: 05 is Mad Rider, 14 is Reckless, Xaldin, 15 is Morning Star, Assault Rider, Living Bones, Berserker...

### Patn Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Patn header
| 18 	 | Patn entries

### Patn Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (2)
| 0x4 	 | uint32 | Entry Count

### Patn Entry

| Offset | Type  | Description
|--------|------|--------------
|  0x0     | int8 | Id
|  0x1     | int8 | ???
|  0x2     | int8 | ???
|  0x3     | int8 | ???
|  0x4     | int8 | ???
|  0x5     | int8 | Magnet Burst effect\*
|  0x6     | int8 | Magnet\*
|  0x7     | int8 | ???
|  0x8     | int8 | ???
|  0x9     | int8 | ???
|  0xA     | int8 | ???
|  0xB     | int8 | ???
|  0xC     | int8 | ???
|  0xD     | int8 | ???
|  0xE     | int8 | ???
|  0xF     | int8 | ???
| 0x10     | int8 | ???
| 0x11     | int8 | ???
| 0x12     | int8 | ???
| 0x13     | int8 | ???
| 0x14     | int8[12] | Padding

\* (00 is immune, 01/0C/0D draws in, 02/03 flinches, 04/07/09/0B knocks out, 05 pulls in(magnet), 06/08/0A is flinch + draw, 0E is flinch + insta revenge...)

## Plrp

Contains informations about starting Character statistics, starting Abilities etc.

### Plrp Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Plrp header
| 64 	 | Plrp entries

### Plrp Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (2)
| 0x4 	 | uint32 | Entry Count

### Plrp Entry

| Offset | Type  | Description
|--------|-------|--------------
|  0x0     | int16 | ???
|  0x2     | int8  | Character ID - [Character LIST](../../dictionary/characters.md)
|  0x3     | uint8 | HP
|  0x4     | uint8 | MP
|  0x5     | uint8 | AP Boosts
|  0x6     | uint8 | Power Boosts
|  0x8     | uint8 | Magic Boosts
|  0xA     | uint8 | Defense Boosts
|  0xC     | uint16[32] | Starting items (abilities, magic etc. Refer to ITEM from 03system*). These are obtained after the dusks fight in Station of Awakening. - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
|  0x4C    | int8[34] | ???

NOTE: Abilities that are enabled by default begin with an 8.

## Limt

Data on limits.

### Limt Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Limt header
| 64 	 | Limt entries

### Limt Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Entry Count

### Limt Entry

| Offset | Type  | Description
|--------|-------|--------------
|  0x0     | int8 | ID
|  0x1     | int8  | Character - [Character/Summon LIST](../../dictionary/characters.md)
|  0x2     | int8  | Summon - [Character/Summon LIST](../../dictionary/characters.md)
|  0x3     | int8  | Group (3 requires all of the characters to be alive)
|  0x4     | char[32]  | Filename
| 0x24     | uint16 | Spawn - [OBJ LIST](../../dictionary/obj.md)
| 0x26     | int8[2] | Padding
| 0x28     | uint16 | Command - [Command LIST](../../dictionary/commands.md)
| 0x2A     | uint16 | Limit - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0x2C     | int8 | Used for Timeless River versions (0D) (May be more bytes)
| 0x2D     | int8[19] | Padding

## Sumn

Data on summons.

### Sumn Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Sumn header
| 4 	 | Sumn entries

### Sumn Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint | File type (2)
| 0x4 	 | uint | Entry Count

### Sumn Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0x0      | uint16 | Summon - [Command LIST](../../dictionary/commands.md)
| 0x2      | uint16 | Item - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0x4      | uint32 | Entity Spawned 1 - [OBJ LIST](../../dictionary/obj.md)
| 0x8      | uint32 | Entity Spawned 2 - [OBJ LIST](../../dictionary/obj.md)
| 0xC      | uint16 | Limit - [Command LIST](../../dictionary/commands.md)
| 0xE      | int8[50] | Padding

## Magc

Data on magic.

### Magc Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Magc header
| 36 	 | Magc entries

### Magc Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (1)
| 0x4 	 | uint32 | Entry Count

### Magc Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0x0     | int8 | Type
| 0x1     | int8 | Level
| 0x2     | int8[2] | ???
| 0x4     | char[32] | Filename
| 0x24     | int8[2] | ???
| 0x26     | int16 | Command - [Command LIST](../../dictionary/commands.md)
| 0x28     | int16 | Ground motion - [Motion LIST](../../file/anb/mset.html)
| 0x2A     | int16 | Ground ???
| 0x2C     | int16 | Finish motion
| 0x2E     | int16 | Finish ???
| 0x30     | int16 | Air motion
| 0x32     | int16 | Air ???
| 0x34     | int8 | ???
| 0x35     | int8 | ???
| 0x36     | int8 | ???
| 0x37     | int8 | Padding

## Vbrt

[BAR](bar.md) file containing unknown data.

* vibr
* auto
* bliz
* v0lo
* l_bo (2 entries)

## Fmlv

Contains the level-up table for summons and drive forms.

### Fmlv Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Fmlv header
| 45 	 | Fmlv entries

### Fmlv Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (2)
| 0x4 	 | uint32 | Entry Count

### Fmlv Entry

| Offset | Type  | Description
|--------|-------|-------------
|  0x0     | int8  | First digit is the Form id, second digit is the Form level (e.g. 0x13 is Valor Form Level 3)
|  0x1     | int8  | Level of the movement ability in the form (High Jump, Quick Run etc.)
|  0x2     | int16 | Ability obtained through level up - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
|  0x4     | int32 | EXP needed for level up

### Forms

Standard (JP/US/EU)

| ID | Form
|----|-----
|  0 | Summon
|  1 | Valor
|  2 | Wisdom
|  3 | Master
|  4 | Final
|  5 | Anti

Final Mix (JP/PS3/PS4)

| ID | Form
|----|-----
|  0 | Summon
|  1 | Valor
|  2 | Wisdom
|  3 | Limit
|  4 | Master
|  5 | Final
|  6 | Anti

## Stop

Data on unknown

### Stop Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Stop header
| 4 	 | Stop entries

### Stop Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0x0 	 | uint32 | File type (1)
| 0x4 	 | uint32 | Entry Count

### Stop Entry

| Offset | Type  | Description
|--------|-------|-------------
|  0x0     | uint16  | ID
|  0x2     | uint16  | ???


## 0A

Unknown.
