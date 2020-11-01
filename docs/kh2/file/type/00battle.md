# [Kingdom Hearts II](../../index.md) - 00battle.bin

This is an essential file for booting [Kingdom Hearts II](../../index.md) and it contains everything related to the battle system.

It is a [BAR](bar.md) file and contains the following subfiles:

* [ATKP](#atkp) - Attack Params
* [PTYA](#ptya) - ???
* [PRZT](#przt) - Prize Table
* [VTBL](#vtbl) - ???
* [LVUP](#lvup) - Level Up
* [BONS](#bons) - Bonus
* [BTLV](#btlv) - Battle Level
* [LVPM](#lvpm) - Level Params
* [ENMP](#enmp) - Enemy Params
* [PATN](#patn) - Partners
* [PLRP](#plrp) - ???
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
| 0 	 | uint | File type (6)
| 4 	 | uint | Entry Count

### Atkp Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | ushort | SubId
| 2 	 | ushort | Id
| 4 	 | byte | Pierce (0 normal, 1 pierces armor)
| 5 	 | byte | Damage reduction (0 normal, 01 half damage, 2 no damage) (Needs confirmation)
| 6 	 | ushort | Power
| 8 	 | byte | Target (0/1/2 Enemies, 3/4/5 Enemies and allies) (Needs confirmation)
| 9 	 | byte | Element (0 phys, 1 fire, 2 blizz, 3 thun...)
| 10 	 | byte | Knockback Type (Check below)
| 11 	 | byte | Effect on hit (0 none, other values = different effects)
| 12 	 | short | Knockback Strength 1 (Distance depends on enemy weight)
| 14 	 | short | Knockback Strength 2 (Distance depends on enemy weight)
| 16 	 | short | ???
| 18 	 | byte | Attack type (Eg: 20/22 can defeat bosses)
| 19 	 | short | ???
| 21 	 | byte | Reflected motion (Points to the slot in the MSET to be triggered when the attack is reflected)
| 22 	 | short | ???
| 24 	 | short | ???
| 26 	 | short | ???
| 28 	 | uint | ID of the sound effect 28
| 32 	 | short | ???
| 34 	 | 4B | ??? (Single byte flags)
| 38 	 | byte | Multihit (1 hit every X frames)
| 39 	 | short | ???
| 41 	 | byte | Drive drain (Adds on normal state, reduces when in a form)
| 42 	 | byte | Revenge damage
| 43 	 | 4B | ??? (Single byte flags)
| 47 	 | byte | HP drain (Adds on normal state, reduces when in a form)

## Ptya

Unknown.

### Ptya Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

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
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

### Przt Entry

| Offset | Type  | Description
|--------|-------|--------------
| 00     | ushort | ID
| 02     | byte  | Small HP orbs
| 03     | byte  | Big HP orbs
| 04     | byte  | Big Money orbs
| 05     | byte  | Medium Money orbs
| 06     | byte  | Small Money orbs
| 07     | byte  | Small MP orbs
| 08     | byte  | Big MP orbs
| 09     | byte  | Small Drive orbs
| 0A     | byte  | Big Drive orbs
| 0B     | byte  | Unknown
| 0C     | ushort  | Item 1 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0E     | short  | Item 1 Drop Percentage
| 10     | ushort  | Item 2 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 12     | short  | Item 2 Drop Percentage
| 14     | ushort  | Item 3 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 16     | short  | Item 3 Drop Percentage

## Vtbl

??? ( entries of 14 bytes each)

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
| 0 	 | 14B | ??? 

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
| 0 	 | uint | File type (2)
| 4 	 | uint | Character pointer count

### Character pointer

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | Offset of the character (Measured in 4 bytes, so 10 means offset 40)
| 4 	 | uint | Padding

### Character Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Character header
| 99 	 | Lvup Entry

### Character header

| Offset | Type | Description |
|--------|------|-------------|
| 00     | uint  | Lvup entry count
| 04     | uint  | Padding

### Lvup Entry

| Offset | Type | Description |
|--------|------|-------------|
| 00     | int  | Needed EXP for next level
| 04     | byte | Strength of Character
| 05     | byte | Magic of Character
| 06     | byte | Defense of Character
| 07     | byte | AP of Character
| 08     | short | Ability given when using Sword route (03system.bin --> ITEM sub file) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0A     | short | Ability given when using Shield route (03system.bin --> ITEM sub file) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0C     | short | Ability given when using Staff route (03system.bin --> ITEM sub file) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 0E     | short | Padding

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
| 00     | int32 | File type (2)
| 04     | int32 | Number of 'Bons' entries

### Bons entry

| Offset | Type  | Description
|--------|-------|--------------
| 00     | byte  | ID - [EVENT LIST](../../dictionary/events.md)
| 01     | byte  | Character Id
| 02     | byte  | HP Increase
| 03     | byte  | MP Increase
| 04     | byte  | Drive Gauge Upgrade
| 05     | byte  | Item Slot Upgrade
| 06     | byte  | Accessory Slot Upgrade
| 07     | byte  | Armor Slot Upgrade
| 08     | short | Bonus Item 1 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 10     | short | Bonus Item 2 (Refer to ITEM from 03system) - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 12     | int   | Unknown

## Btlv

???

### Btlv Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Btlv header
| 20 	 | Btlv entries

### Btlv Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (1)
| 4 	 | uint | Entry Count

### Btlv Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0 	 | 32B | ??? 

## Lvpm

Contains the level-up table for the enemies. Based on the level of an enemy, a specific level, containing multiplying values, will be applied over the base statistics of an enemy.

### Lvpm Structure

| Amount | Description |
|--------|---------------|
| 99 	 | Lvpm entries

### Lvpm Entry

| Offset | Type  | Description
|--------|------ |--------------
| 00     | short | HP level. The formula is `(EnemyHp * LevelHp + 99) / 100`.
| 02     | short | Strength
| 04     | short | Defense
| 06     | short | ???
| 08     | short | ???
| 0A     | short | Exp

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
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

### Enmp Entry

| Offset | Type  | Description
|--------|------|--------------
| 00     | short | Identifies the enemy. - [Enemy LIST](../../dictionary/enemy.md)
| 02     | short | Level of the enemy. Must be between 1 and 99. (0 uses the world's battle level)
| 04     | short[32] | Health amount. It is multiplied by Hp from [LVPM](#lvpm).
| 44     | short | Damage Cap. (The higher, the less damage received)
| 46     | short | ???
| 48     | short | Physical weakness.
| 4A     | short | Fire weakness.
| 4C     | short | Blizzard weakness.
| 4E     | short | Thunder weakness.
| 50     | short | Dark weakness.
| 52     | short | Neutral weakness.
| 54     | short | General weakness.
| 56     | short | Exp multiplier.
| 58     | short | Unknown
| 5A     | short | Unknown

## Patn

Data on partner allies.

### Patn Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Patn header
| 18 	 | Patn entries

### Patn Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

### Patn Entry

| Offset | Type  | Description
|--------|------|--------------
| 00     | byte | Id
| 01     | byte [19] | ???

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
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

### Plrp Entry

| Offset | Type  | Description
|--------|-------|--------------
| 00     | short | Unknown
| 02     | byte  | Unknown
| 03     | byte  | HP
| 04     | byte  | MP
| 05     | byte  | AP
| 06     | short | Unknown
| 08     | short | Unknown
| 0A     | short | Unknown
| 0C     | short[58] | Starting items (abilities, magic etc. Refer to ITEM from 03system). These are obtained after the dusks fight in Station of Awakening. - [ITEM/ABILITY LIST](../../dictionary/inventory.md)

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
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

### Limt Entry

| Offset | Type  | Description
|--------|-------|--------------
| 00     | byte | ID
| 01     | byte  | Character - [Character/Summon LIST](../../dictionary/characters.md)
| 02     | byte  | Summon - [Character/Summon LIST](../../dictionary/characters.md)
| 03     | byte  | Group (3 requires all of the characters to be alive)
| 04     | char[32]  | Name
| 36     | 4B | ???
| 40     | 4B | ???
| 44     | 4B | ???
| 48     | 16B | Padding

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
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

### Sumn Entry

| Offset | Type  | Description
|--------|-------|--------------
| 00     | 2B[7] | ???
| 14     | 50B | Padding

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
| 0 	 | uint | File type (1)
| 4 	 | uint | Entry Count

### Magc Entry

| Offset | Type  | Description
|--------|-------|--------------
| 0     | byte | Type
| 1     | byte | Level
| 2     | 2B | ???
| 4     | char[32] | Filename
| 36     | 2B | ???
| 38     | short | Data - [Command LIST](../../dictionary/commands.md)
| 40     | short | Ground animation
| 42     | short | Ground ???
| 44     | short | Air animation
| 46     | short | Air ???
| 48     | short | Finisher animation
| 50     | short | Finisher ???
| 52     | byte | ???
| 53     | byte | ???
| 54     | byte | ???
| 55     | byte | Padding

## Vbrt

[BAR](bar.md) file containing unknown data

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
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

### Fmlv Entry

| Offset | Type  | Description
|--------|-------|-------------
| 00     | byte  | First digit is the Form id, second digit is the Form level (e.g. 0x13 is Valor Form Level 3)
| 01     | byte  | Level of the movement ability in the form (High Jump, Quick Run etc.)
| 02     | short | Ability obtained through level up - [ITEM/ABILITY LIST](../../dictionary/inventory.md)
| 04     | int   | EXP needed for level up

### Forms

Standard (JP/US/EU)

| ID | Form
|----|-----
| 00 | Summon
| 01 | Valor
| 02 | Wisdom
| 03 | Master
| 04 | Final
| 05 | Anti

Final Mix (JP/PS3/PS4)

| ID | Form
|----|-----
| 00 | Summon
| 01 | Valor
| 02 | Wisdom
| 03 | Limit
| 04 | Master
| 05 | Final
| 06 | Anti

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
| 0 	 | uint | File type (1)
| 4 	 | uint | Entry Count

### Stop Entry

| Offset | Type  | Description
|--------|-------|-------------
| 00     | ushort  | ID
| 02     | ushort  | ???


## 0A

Unknown.