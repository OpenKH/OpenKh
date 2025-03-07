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

---

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
| 0 	 | uint32 | File type (6)
| 4 	 | uint32 | Entry Count

### Atkp Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint16 | SubId
| 2 	 | uint16 | Id
| 4 	 | uint8 | [Type](#type)
| 5 	 | uint8 | Critical Adjust (0 normal, 1 half damage, 2 no damage)
| 6 	 | uint16 | Power
| 8 	 | uint8 | Team (Deal damage to: 0/1/2 Enemies, 3/4/5 Enemies and allies...)
| 9 	 | uint8 | Element (0 phys, 1 fire, 2 blizz, 3 thun...)
| 10 	 | uint8 | Enemy Reaction (Whether an enemy is flinched, knocked...)
| 11 	 | uint8 | Effect on hit\*
| 12 	 | int16 | Knockback Strength 1 (Distance depends on enemy weight)
| 14 	 | int16 | Knockback Strength 2 (Distance depends on enemy weight)
| 16 	 | int16 | ???
| 18 	 | uint8 | [Flags](#flags)
| 19 	 | uint8 | [Refact Self](#refact)
| 20 	 | uint8 | [Refact Other](#refact)
| 21 	 | uint8 | Reflected motion (Points to the slot in the MSET to be triggered when the attack is reflected)
| 22 	 | int16 | Reflect Hit Back
| 24 	 | int32 | Reflect Action
| 28 	 | int32 | Hit Sound Effect
| 32 	 | uint16 | Reflect RC
| 34 	 | uint8 | Reflect Range
| 35 	 | int8 | Reflect Angle
| 36 	 | uint8 | Damage Effect
| 37 	 | uint8 | Switch
| 38 	 | uint16 | Interval (1 hit every X frames)
| 40 	 | uint8 | Floor Check
| 41 	 | uint8 | Drive drain (Adds on normal state, reduces when in a form)
| 42 	 | uint8 | Revenge damage
| 43 	 | uint8 | [Tr Reaction](#tr-reaction)
| 44 	 | uint8 | Combo Group
| 45 	 | uint8 | Random Effect
| 46 	 | uint8 | [Kind](#kind)
| 47 	 | uint8 | HP drain (Adds on normal state, reduces when in a form)

\* Effect on Hit is a bitmask that corresponds to the value of the objects PAX's "Category" value. Most attacks use 0, which displays no extra effects on hit.
However attacks like projectiles typically display extra effects on-hit, so this value is used in these cases. </br>
Example: If the PAX's "Category" value has a value of 1, use a value of 1 for Effect On Hit, which corresponds to bitmask 0x01. If the PAX's "Category" value has a value of 3, use a value of 0x4 for Effect On Hit, which corresopnds to bitmask 0x04. </br>
Additionally, the upper four bytes in "Category" also correspond to when the Effect on Hit will activate. </br> 0x01 activates on-hit with an enemy. </br> 0x02 activates on wall-hit. </br> 0x10 activates on colliding with a "Guard" hitbox. </br> 0x100 activates on hitting the ground.

#### Type

| Id | Description |
|----|-------------|
| 0  | Normal attack
| 1  | Pierce armor
| 2  | Guard
| 3  | S Guard
| 4  | Special
| 5  | Cure
| 6  | C Cure

#### Flags

| Position | Size | Description |
|----------|------|-------------|
| 0 | 1 | BG Hit
| 1 | 1 | Limit PAX
| 2 | 1 | Land
| 3 | 1 | Capture PAX
| 4 | 1 | Thank you
| 5 | 1 | Kill Boss

#### Refact

| Id | Description |
|----|-------------|
| 0  | Reflect
| 1  | Guard
| 2  | Nothing

#### Tr Reaction

| Id | Description |
|----|-------------|
| 0  | Attack
| 1  | Charge
| 2  | Crash
| 3  | Wall

#### Kind

| Position | Size | Description |
|----------|------|-------------|
| 0 | 1 | Combo Finisher
| 1 | 1 | Air Combo Finisher
| 2 | 1 | Reaction Command

---

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
| 0     | uint8 | Id
| 1     | uint8 | Type
| 2     | int8 | Sub
| 3     | int8 | Combo Offset
| 4     | uint32 | Flag
| 8     | uint16 | Motion Id \*
| 10     | uint16 | Next Motion Id \*
| 12     | float | Jump
| 16     | float | Jump Max
| 20     | float | Jump Min
| 24     | float | Speed Min
| 28     | float | Speed Max
| 32     | float | Near
| 36     | float | Far
| 40     | float | Low
| 44     | float | High
| 48     | float | Inner Min
| 52     | float | Inner Max
| 56     | float | Blend Time
| 60     | float | Distance Adjust
| 64     | uint16 | Ability - SubId on [Item](./03System#Item)
| 66     | uint16 | Score

\* Multiply by 4 to get the slot of the motion in the entity's [moveset file](../anb/mset.md).

---

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
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Entry Count

### Przt Entry

| Offset | Type  | Description
|--------|-------|--------------
| 00     | uint16 | ID
| 02     | uint8  | Small HP orbs
| 03     | uint8  | Big HP orbs
| 04     | uint8  | Big Money orbs
| 05     | uint8  | Medium Money orbs
| 06     | uint8  | Small Money orbs
| 07     | uint8  | Small MP orbs
| 08     | uint8  | Big MP orbs
| 09     | uint8  | Small Drive orbs
| 10     | uint8  | Big Drive orbs
| 11     | uint8  | Padding
| 12     | uint16  | Item 1 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 14     | int16  | Item 1 Drop Percentage
| 16     | uint16  | Item 2 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 18     | int16  | Item 2 Drop Percentage
| 20     | uint16  | Item 3 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 22     | int16  | Item 3 Drop Percentage

---

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
| 0 	 | uint32 | File type (1)
| 4 	 | uint32 | Entry Count

### Vtbl Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0 	 | uint8 | Character - [CHARACTER LIST](../../dictionary/characters.md)
| 1 	 | uint8 | Id
| 2 	 | uint8 | Priority
| 3 	 | uint8 | Reserved
| 4 	 | int8 | Voice 1
| 5 	 | int8 | Voice 1 Chance
| 6 	 | int8 | Voice 2
| 7 	 | int8 | Voice 2 Chance
| 8 	 | int8 | Voice 3
| 9 	 | int8 | Voice 3 Chance
| 10 	 | int8 | Voice 4
| 11 	 | int8 | Voice 4 Chance
| 12 	 | int8 | Voice 5
| 13 	 | int8 | Voice 5 Chance

---

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
* Biest
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
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Character pointer count

### Character pointer

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint32 | Offset of the character (Measured in 4 bytes, so 10 means offset 40)
| 4 	 | uint32 | Padding

### Character Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Character header
| 99 	 | Lvup Entry

### Character header

| Offset | Type | Description |
|--------|------|-------------|
| 00     | uint32  | Lvup entry count
| 04     | uint32  | Padding

### Lvup Entry

| Offset | Type | Description |
|--------|------|-------------|
| 00     | int32  | Needed EXP for next level
| 04     | uint8 | Strength of Character
| 05     | uint8 | Magic of Character
| 06     | uint8 | Defense of Character
| 07     | uint8 | AP of Character
| 08     | uint16 | Ability given when using Sword route  - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 10     | uint16 | Ability given when using Shield route - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 12     | uint16 | Ability given when using Staff route  - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 14     | uint8[2] | Padding

---

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
|  0     | int32 | File type (2)
|  4     | int32 | Number of 'Bons' entries

### Bons entry

| Offset | Type  | Description
|--------|-------|--------------
| 00     | uint8  | ID - [EVENT LIST](../../dictionary/events.md)
| 01     | uint8  | Character Id
| 02     | uint8  | HP Increase
| 03     | uint8  | MP Increase
| 04     | uint8  | Drive Gauge Upgrade
| 05     | uint8  | Item Slot Upgrade
| 06     | uint8  | Accessory Slot Upgrade
| 07     | uint8  | Armor Slot Upgrade
| 08     | uint16 | [Bonus Item 1](03system.md#item) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 10     | uint16 | [Bonus Item 2](03system.md#item) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 12     | int32   | Padding

---

## Btlv

Contains the table for battle level of each world. Whether each entry is enabled or not is determined by a progress flag and the final battle level will be the sum of all of the world's enabled battle level entries.

### Btlv Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Btlv header
| 20 	 | Btlv entries

### Btlv Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint32 | File type (1)
| 4 	 | uint32 | Entry Count

### Btlv Entry

| Offset | Type  | Description
|--------|-------|--------------
| 00 	 | int32  | Id
| 04     | int32  | Progress Flag
| 08     | uint8  | World ZZ
| 09     | uint8  | World of Darkness
| 10     | uint8  | Twilight Town
| 11     | uint8  | Destiny Islands
| 12     | uint8  | Hollow Bastion
| 13     | uint8  | Beast's Castle
| 14     | uint8  | Olympus Coliseum
| 15     | uint8  | Agrabah
| 16     | uint8  | Land of Dragons
| 17     | uint8  | 100 Acre Woods
| 18     | uint8  | Pride Lands
| 19     | uint8  | Atlantica
| 20     | uint8  | Disney Castle
| 21     | uint8  | Timeless River
| 22     | uint8  | Halloween Town
| 23     | uint8  | World Map
| 24     | uint8  | Port Royal
| 25     | uint8  | Space Paranoids
| 26     | uint8  | The World That Never Was
| 27     | uint8[5] | Padding

---

## Lvpm

Contains the level-up table for the enemies. Based on the level of an enemy, a specific level, containing multiplying values, will be applied over the base statistics of an enemy.

### Lvpm Structure

| Amount | Description |
|--------|---------------|
| 99 	 | Lvpm entries

### Lvpm Entry

| Offset | Type  | Description
|--------|------ |--------------
| 00     | uint16 | HP level. The formula is `(EnemyHp * LevelHp + 99) / 100`.
| 02     | uint16 | Strength
| 04     | uint16 | Defense
| 06     | uint16 | Max Strength
| 08     | uint16 | Min Strength
| 10     | uint16 | Exp

---

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
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Entry Count

### Enmp Entry

| Offset | Type  | Description
|--------|------|--------------
| 00     | uint16 | Id - [Enemy LIST](../../dictionary/enemy.md)
| 02     | uint16 | Level of the enemy. Must be between 1 and 99. (0 uses the world's battle level)
| 04     | uint16[32] | Health amount. It is multiplied by Hp from [LVPM](#lvpm).
| 68     | uint16 | Damage Cap (The higher, the less damage received)
| 70     | uint16 | Minimum Damage
| 72     | uint16 | Physical weakness
| 74     | uint16 | Fire weakness
| 76     | uint16 | Blizzard weakness
| 78     | uint16 | Thunder weakness
| 80     | uint16 | Dark weakness
| 82     | uint16 | Light weakness
| 84     | uint16 | General weakness
| 86     | uint16 | Exp multiplier
| 88     | uint16 | Prize
| 90     | uint16 | Bonus Level

---

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
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Entry Count

### Patn Entry

| Offset | Type  | Description
|--------|------|--------------
| 00     | uint8 | Id
| 01     | uint8 | ???
| 02     | uint8 | ???
| 03     | uint8 | ???
| 04     | uint8 | ???
| 05     | uint8 | Magnet Burst effect\*
| 06     | uint8 | Magnet\*
| 07     | uint8 | ???
| 08     | uint8 | ???
| 09     | uint8 | ???
| 10     | uint8 | ???
| 11     | uint8 | ???
| 12     | uint8 | ???
| 13     | uint8 | ???
| 14     | uint8 | ???
| 15     | uint8 | ???
| 16     | uint8 | ???
| 17     | uint8 | ???
| 18     | uint8 | ???
| 19     | uint8 | ???
| 20     | uint8[12] | Padding

\* (00 is immune, 01/0C/0D draws in, 02/03 flinches, 04/07/09/0B knocks out, 05 pulls in(magnet), 06/08/0A is flinch + draw, 0E is flinch + insta revenge...)

---

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
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Entry Count

### Plrp Entry

| Offset | Type  | Description
|--------|-------|--------------
| 00     | uint16 | Id
| 02     | uint8  | Character ID - [Character LIST](../../dictionary/characters.md)
| 03     | uint8  | HP
| 04     | uint8  | MP
| 05     | uint8  | AP
| 06     | uint8  | Strength
| 07     | uint8  | Magic
| 08     | uint8  | Defense
| 09     | uint8  | Armor Slots
| 10     | uint8  | Accessory Slots
| 11     | uint8  | Item Slots
| 12     | uint16[32] | Starting items (abilities, magic etc*). These are obtained after the dusks fight in Station of Awakening. - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 72     | uint8[52] | Padding

NOTE: Abilities that are enabled by default begin with an 8.

---

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
| 00     | uint8 | ID
| 01     | uint8  | Character - [Character/Summon LIST](../../dictionary/characters.md)
| 02     | uint8  | Summon - [Character/Summon LIST](../../dictionary/characters.md)
| 03     | uint8  | Group (3 requires all of the characters to be alive)
| 04     | char[32]  | Filename
| 36     | uint32 | Spawn Id - [OBJ LIST](../../dictionary/obj.md)
| 40     | uint16 | Command - [Command LIST](../../dictionary/commands.md)
| 42     | uint16 | Limit - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 44     | uint8 | World (Used for Timeless River versions (0D))
| 45     | uint8[19] | Padding

---

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
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Entry Count

### Sumn Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0     | uint16 | Command - [Command LIST](../../dictionary/commands.md)
| 2     | uint16 | Item - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 4     | uint32 | Entity Spawned 1 - [OBJ LIST](../../dictionary/obj.md)
| 8     | uint32 | Entity Spawned 2 - [OBJ LIST](../../dictionary/obj.md)
| 12     | uint16 | Limit Command - [Command LIST](../../dictionary/commands.md)
| 14     | uint8[50] | Padding

---

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
| 0 	 | uint32 | File type (1)
| 4 	 | uint32 | Entry Count

### Magc Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0     | uint8 | Id
| 1     | uint8 | Level
| 2     | uint8 | World
| 3     | uint8 | Padding
| 4     | char[32] | Filename
| 36     | uint16 | Item
| 38     | uint16 | Command - [Command LIST](../../dictionary/commands.md)
| 40     | int16 | Ground motion - [Motion LIST](../../file/anb/mset.html)
| 42     | int16 | Ground blend
| 44     | int16 | Finish motion
| 46     | int16 | Finish blend
| 48     | int16 | Air motion
| 50     | int16 | Air blend
| 52     | uint8 | Voice
| 53     | uint8 | Voice Finisher
| 54     | uint8 | Voice Self
| 55     | uint8 | Padding

---

## Vbrt

[BAR](bar.md) file containing unknown data.

* vibr
* auto
* bliz
* v0lo
* l_bo (2 entries)

---

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
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Entry Count

### Fmlv Entry

| Offset | Type  | Description
|--------|-------|-------------
| 00     | uint8 : 0-3 | [Id](#forms)
| 00     | uint8 : 4-7 | Level
| 01     | uint8 : 0-3 | Anti Rate
| 01     | uint8 : 4-7 | Ability Level
| 02     | uint16 | Ability obtained through level up - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 04     | int32   | EXP needed for level up

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

---

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
| 0 	 | uint32 | File type (1)
| 4 	 | uint32 | Entry Count

### Stop Entry

| Offset | Type  | Description |
|--------|-------|-------------|
| 00     | uint16  | ID
| 02     | uint16  | [Flags](#flags)

#### Flags

| Position | Size | Description |
|----------|------|-------------|
| 0 | 1 | Exist
| 1 | 1 | Disable Damage Reaction
| 2 | 1 | Star
| 3 | 1 | Disable Draw

---

## 0A

Unknown.
