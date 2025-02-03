# [Kingdom Hearts II](../../index.md) - 03system.bin

This is an essential file to boot the game engine.

* [RCCT](#rcct) - ???
* [CMD](#cmd) - Commands
* [WENT](#went) - Weapon Entities
* [WMST](#wmst) - Weapon Movesets
* [ARIF](#arif) - Area Info
* [ITEM](#item) - Items
* [TRSR](#trsr) - Treasure
* [MEMT](#memt) - Member Table
* [FTST](#ftst) - Font Style
* [SHOP](#shop) - Shops
* [SKLT](#sklt) - Skeleton
* [PREF](#pref) - Preferences
* [EVTP](#evtp) - ???
* [IPIC](#ipic) - ???

---

## Rcct

Unknown table.

### Rcct Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Rcct header
| 35 	 | Rcct entries

### Rcct Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (1)
| 4 	 | uint | Entry Count

### Rcct Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | Unk0
| 2 	 | short | Unk2
| 4 	 | short | Unk4
| 6 	 | short | Unk6
| 8 	 | short | Unk8
| 10 	 | short | Unk10 (Padding?)

---

## Cmd

Commands table.

### Cmd Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Cmd header
| 751 	 | Cmd entries

### Cmd Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (2)
| 4 	 | uint | Entry Count

### Cmd Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | ushort | Id - [COMMAND LIST](../../dictionary/commands.md)
| 2 	 | ushort | Execute
| 4 	 | short | Argument¹
| 6 	 | sbyte | Submenu
| 7 	 | byte | [Icon](#icon)
| 8 	 | int | Text
| 12 	 | uint | [Flags](#flags)
| 16 	 | float | Range
| 20 	 | float | Dir
| 24 	 | float | Dir Range
| 28 	 | byte | Mp/Drive cost
| 29 	 | byte | [Camera](#camera)
| 30 	 | byte | Priority
| 31 	 | byte | [Receiver](#receiver)
| 32 	 | ushort | Time
| 34 	 | ushort | Require
| 36 	 | byte | Mark
| 37 	 | byte | [Action](#action)
| 38 	 | ushort | Reaction Count
| 40 	 | ushort | Dist Range
| 42 	 | ushort | Score
| 44 	 | ushort | [Disable Form (BitFlags)](#disable-form)
| 46 	 | byte | Group
| 47 	 | byte | Reserve

¹: This can be Argument, Form Id or Magic Id

#### Icon

| Id | Description |
|----|-------------|
| 0 | None
| 1 | Attack
| 2 | Magic
| 3 | Item
| 4 | Form
| 5 | Summon
| 6 | Friend
| 7 | Limit


#### Flags

| Id | Description |
|----|-------------|
| 0x1 | Cursor
| 0x2 | Land
| 0x4 | Force
| 0x8 | Combo
| 0x10 | Battle
| 0x20 | Secure
| 0x40 | Require
| 0x80 | No Combo
| 0x100 | Drive
| 0x200 | Short
| 0x400 | Disable Sora
| 0x800 | Disable Roxas
| 0x1000 | Disable Lion King Sora
| 0x2000 | Disable Limit Form
| 0x4000 | Unused
| 0x8000 | Disable Skateboard
| 0x10000 | Battle Mode Only

#### Camera

| Id | Description |
|----|-------------|
| 0 | Null
| 1 | Watch
| 2 | Lock On
| 3 | Watch & Lock On

#### Receiver

| Id | Description |
|----|-------------|
| 0 | Player
| 1 | Target
| 2 | Both

#### Action

| Id | Description |
|----|-------------|
| 0 | Null
| 1 | Idle
| 2 | Jump

#### Disable Form

| Id | Description |
|----|-------------|
| 0x1 | Roxas and Non-Minigame Base Soras
| 0x2 | Valor Sora
| 0x4 | Wisdom Sora
| 0x8 | Limit Sora (Also has a dedicated Flag)
| 0x10 | Master Sora
| 0x20 | Anti Sora
| 0x40 | Lion King Sora
| 0x80 | Little Mermaid Sora
| 0x100 | Unknown
| 0x200 | Dual Wield Roxas
| 0x400 | Card/Dice/Carpet Escape Sora (Minigame Soras Except Lightcycle) and Mickey Mouse
| 0x800 | Unknown
| 0x1000 | Unknown
| 0x2000 | Unknown
| 0x4000 | Unknown
| 0x5000 | Unknown

---

## Went

Weapon entity table.
Contains a list of pointers that point to the offset of a weapon set. There are multiple pointers for multiple sets.
Weapon sets contain the list of weapon models a character use in certain situations.

The Id within a "Went set" is the weapon's subId on the [item table](#item).

### Went Structure

| Amount | Description |
|--------|---------------|
| 70 	 | Went pointers
| 24 	 | Went sets

### Went pointer

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | Points to the offset of a Went set

### Went set structure

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | Set size in 4 bytes length. (Including itself)
| 4 	 | uint[Set size - 1] | Weapon model. - [OBJ LIST](../../dictionary/obj.md)

### Went set order

<ul>
<li>Sora</li>
<li>Sora NM</li>
<li>Donald</li>
<li>Donald NM</li>
<li>Goofy</li>
<li>Goofy 2</li>
<li>Goofy NM</li>
<li>Aladdin</li>
<li>Auron</li>
<li>Mulan</li>
<li>Tron</li>
<li>Mickey</li>
<li>Beast</li>
<li>Jack</li>
<li>Simba</li>
<li>Sparrow</li>
<li>Riku</li>
<li>Sparrow Human</li>
<li>Sora TR</li>
<li>Sora WI</li>
<li>Donald TR</li>
<li>Donald WI</li>
<li>Goofy TR</li>
<li>Goofy WI</li>
</ul>

---

## Wmst

Weapon moveset list.

### Wmst Structure

| Amount | Description |
|--------|---------------|
| 140 	 | Wmst entries

### Wmst Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | char[32] | Weapon moveset filename

---

## Arif

Describes the information for each area.

Each Block corresponds to a world.

On PC, trying to load into a map in a world without a corresponding Arif entry will crash the game. 

Voice controls the value of the world-specific voice to load per-room. A value of 0 in Hollow Bastion will load in hb0_sora, whereas a value of 1 will load in hb1_sora. 

Navimap Item determines which item ID to look the player must have in their inventory before displaying the minimap.

Arif pointers follow the order of [Worlds](/docs/kh2/worlds.md)

### Arif Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Arif Header
| 19 	 | Arif Pointers
| 19 	 | Arif Blocks

### Arif Header

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | uint | File type (1)
| 4      | uint | Arif Pointer Count |

### Arif Pointer

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | ushort | Entry Count
| 2      | ushort | Block offset |

### Arif Block Structure

| Amount | Description |
|--------|---------------|
| [Entry Count] 	 | Arif Entry

### Arif Entry

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | uint | [Flag](#flag)
| 4      | int | Reverb
| 8      | int | Bg Set 1
| 12      | int | Bg Set 2
| 16      | [BGM](#BGM)[8] | Background Music
| 48      | ushort | Voice
| 50      | ushort | Navimap Item
| 52      | char | Command
| 53      | char[11] | Reserve

### BGM

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | ushort | Music 1
| 2      | ushort | Music 2

#### Flag

| Id | Description |
|----|-------------|
| 0x1 | Is Known Area
| 0x2 | In Door Area (Lowers FOV)
| 0x4 | Monochrome (Timeless River filter)
| 0x8 | No Shadow
| 0x10 | Has Glow

---

## Item

Describe an item, that could be anything from a consumable, to a weapon or materials.

There are two tables. The first one is the Item descriptor itself, the second one is for the statistics the item is affecting.

### Item Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Item table
| 1 	 | Status table

### Item table Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Item header
| 535 	 | Item entries

### Item Header

Every table has the following header

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | uint32 | File type (6)
| 4      | uint32 | Descriptor count for the specified table. |

### Item Entry

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | uint16 | Id - [ITEM LIST](../../dictionary/inventory.md)
| 2      | byte  | [Category](#Categories) |
| 3      | byte  | Flag |
| 4 - 7  |  | Dependent on the Category, see [variable structures](#variable-structures) |
| 8      | uint16 | Name message ID |
| 10     | uint16 | Description message ID |
| 12     | uint16 | Shop buy price |
| 14     | uint16 | Shop sell price |
| 16     | uint16 | Command - [COMMAND LIST](../../dictionary/commands.md)
| 18     | uint16 | Slot (Order in the menu) |
| 20     | uint16 | Picture linked to the image |
| 22     | byte  | [Prize Box](#Prize-boxes) |
| 23     | byte  | [Icon](../../dictionary/icons.md) |

*Used as recovery amount for consumables (% for ethers, halved on charge), AP cost for abilities, Id used in [Went](#went) for weapons.

#### Categories

| ID | Description
|------|------|
| 0    | Consumable (Equippable)
| 1    | Consumable (Menu)
| 2    | Keyblade
| 3    | Staff
| 4    | Shield
| 5    | Ping weapon
| 6    | Auron weapon
| 7    | Beast weapon
| 8    | Jack weapon
| 9    | Dummy weapon
| 10   | Riku weapon
| 11   | Simba weapon
| 12   | Jack Sparrow weapon
| 13   | Tron weapon
| 14   | Armor
| 15   | Accessory
| 16   | Synthesis
| 17   | Moguri recipe
| 18   | Magic
| 19   | Ability
| 20   | Summon
| 21   | Form
| 22   | Map
| 23   | Report

#### Flags

| Position | Size | Description |
|----------|------|-------------|
| 0 | 1 | Special
| 1 | 1 | Normal Form Only

#### Prize boxes

| ID | Description
|------|------|
| 0    | Red S (Synth1)
| 1    | Red L (Synth1)
| 2    | Red XL (Synth1)
| 3    | Blue S (Synth2)
| 4    | Blue L (Synth2)
| 5    | Blue XL (Synth2)
| 6    | Winged S (Equipment)
| 7    | Winged L (Equipment)
| 8    | Winged XL (Equipment)
| 9    | Purple S (Item)
| 10   | Purple L (Item)
| 11   | Purple XL (Item)

### Variable structures

#### Ability

| Offset | Type | Description |
|--------|------|-------------|
| 4 | uint16 | Id
| 6 | uint8 | Ap
| 7 | uint8 | Type

#### Consumables

| Offset | Type | Description |
|--------|------|-------------|
| 4 | uint16 | Cure Rate
| 6 | uint16 | Effect

#### Equipment (Armor, Accessory)

| Offset | Type | Description |
|--------|------|-------------|
| 4 | uint16 | Param
| 6 | uint16 | Unused

#### Magic

| Offset | Type | Description |
|--------|------|-------------|
| 4 | uint16 | Id
| 6 | uint16 | Unused

#### Synthesis

| Offset | Type | Description |
|--------|------|-------------|
| 4 | uint8 | Rank
| 5 | uint8 | Type

#### Report

| Offset | Type | Description |
|--------|------|-------------|
| 4 | uint16 | Id

#### Weapons

| Offset | Type | Description |
|--------|------|-------------|
| 4 | uint16 | Id
| 6 | uint16 | Param

### Status table Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Status header
| 151 	 | Status entries

### Status Header

Every table has the following header

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | uint32 | File type (0)
| 4      | uint32 | Descriptor count for the specified table. |

### Status Entry

| Offset | Type   | Description |
|--------|--------|-------------|
| 0      | uint16 | Id - [EQUIPMENT LIST](../../dictionary/equipment.md)
| 2      | uint16 | Ability ID |
| 4      | uint8  | Attack boost |
| 5      | uint8  | Magic boost |
| 6      | uint8  | Defense boost |
| 7      | uint8  | AP boost |
| 8      | uint8  | Physical damage |
| 9      | uint8  | Fire damage |
| 10     | uint8  | Ice damage |
| 11     | uint8  | Lightning damage |
| 12     | uint8  | Dark damage |
| 13     | uint8  | Light/Neutral damage |
| 14     | uint8  | General damage |
| 15     | uint8  | Reserve |

---

## Trsr

The treasure table describes what item cam be retrieved from a specific chest or event to a given map.

### Trsr Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Trsr header
| 430 	 | Trsr entries

### Trsr Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | ushort | File type (3)
| 2 	 | ushort | Entry Count

### Trsr Entry

| Offset | Type   | Description |
|--------|--------|-------------|
| 0      | uint16 | ID
| 2      | uint16 | [Item ID](#item-descriptor)
| 4      | uint8  | [Type](#treasure-type)
| 5      | uint8  | [World ID](../../worlds.md)
| 6      | uint8  | Room index
| 7      | uint8  | Room chest index
| 8      | uint16 | Event ID
| 10     | uint16 | Overall chest index

World ID and room index combined, gives the name of the map. (eg. for world ID = 4 and room index = 9, the map is `hb_09`)

### Treasure type

| Type | Name
|------|------|
| 0    | Chest
| 1    | Event

---

## Memt

Also known as Member Table, defines which [object](../../obj.md) to load in certain situations.

The game internally uses a pawn system. There are five pawns: PLAYER, FRIEND_1, FRIEND_2, ACTOR_SORA and ACTOR_SORA_H. The first three are used during gameplay sessions, while the other two are used during cutscenes. When the game finds those pawns, it will instead load the actual object to load given a specific map. This is, for example, how the game decides to load Halloween Sora in `NM` world, or when to load Ping or Mulan in `MU` world given certain story flags. This is true for cutscenes too, where if you go transformed as Final Form, Sora will appear with Final Form clothes even with his high-poly model rather than his default model.

The first [entry](#memt-entry) is the default one as it globally defined which pawn is which object. After that, a series of conditional entries are defined. For each entry that respects a certain condition, it will temporarily overwrite the default values. The first condition is the World ID, which is the current world that the game have loaded. Then there is a World Story Flag and World Story Flag Negation. Those are conditions to load a different entry based on the story progression. As this World Story Flag contains both the flag and the World ID, it is possible to tell to the game to load specific objects of a world based on the story progression of another world. While the first story flag checks if that part of the story is met, the negation one satisfies the condition when the specified story flag is not met.

Whenever a value of `0` in this structure is found, it is ignored. Story flags will always give a positive result, while pawns will fall back to the ones defined in the default entry.

### Memt Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | uint | File version (5)
| 4      | uint | Entry Count

### Memt entry

Note that on the Vanilla version of the game, this structure is `48` bytes long and not `52`, as Limit and Limti High poly are not existent. The file version remains `5`

| Offset | Type   | Description |
|--------|--------|-------------|
| 0      | ushort | [World ID](../../worlds.md)
| 2      | ushort | World story flag
| 4      | ushort | World story flag negation
| 6      | byte | Command flag (based on [ARIF](#arif) but uses 1-index as opposed to its 0-index)
| 7      | byte[9] | Unknown
| 16     | ushort  | Player (Sora)
| 18     | ushort  | Friend 1 (Donald)
| 20     | ushort  | Friend 2 (Goofy)
| 22     | ushort  | World character
| 24     | ushort  | Player (Valor)
| 26     | ushort  | Player (Wisdom)
| 28     | ushort  | Player (Limit)
| 30     | ushort  | Player (Master)
| 32     | ushort  | Player (Final)
| 34     | ushort  | Player (Anti)
| 36     | ushort  | Player (Mickey)
| 38     | ushort  | Player (Sora High poly)
| 40     | ushort  | Player (Valor High poly)
| 42     | ushort  | Player (Wisdom High poly)
| 44     | ushort  | Player (Limit High poly)
| 46     | ushort  | Player (Master High poly)
| 48     | ushort  | Player (Final High poly)
| 50     | ushort  | Player (Sora High poly)

### Memt party

This table, found straight after [the entries](#memt-entry), is used to decide which party members are used in a given portion of the game. This table is used by [AreaData scripts](./areadata.md#party) and controls what the values actually do. The index is the one for [the entries](#memt-entry) object array, so an index of `0` will check what's in the offset `16` and an index of `3` will check what's in the offset `22`. When the value is equal to `12` (or `10` for Vanilla), the game will not make that specific pawn available in the party. This table seems to be the one responsible to assign or remove specific party members.

| Offset | Type | Description
|--------|------|-------------
| 0      | byte | Member index for player
| 1      | byte | Member index for friend 1
| 2      | byte | Member index for friend 2
| 3      | byte | Member index for friend world

---

## Ftst

This is a table that contains the font palette for each world.

The FTST is a binary file that contains N amount of palettes (9 for FM version), where every palette contain an unique ID (or key) and exactly 19 different colors.

Each color correspond to the [world index](../../worlds.md) and it is loaded based on the current world.

---

## Shop

This file contains data for the shops.
There's a list of products available. Products are grouped in Inventories. Shops sell products in their inventories.
Each inventory has the product on its "Product offset" and the following "Product count" products.
Each shop has the inventory on its "Inventory offset" and the following "Inventory count" products.
"Valid Item Table" is always at the end of the file. It has no count and is read from the "Valid Items Offset" to the end of the file. In it is every possible Item that can be bought in any of the shops. Any item added to any of the shops must also add the Item ID to this table or the game will crash.

### Shop Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Shop header
| 21 	 | Shop entries
| 333 	 | Inventory entries
| 404 	 | Product entries
| 60     | Valid Item Table

### Shop Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | char[4] | File ID (TZSH)
| 4 	 | ushort | File type (7)
| 6 	 | ushort | Shop List count
| 8 	 | ushort | Inventory Entry count
| 10 	 | ushort | Product Entry count
| 12 	 | uint | Valid Items Offset

### Shop Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | [Command Argument](#cmd)
| 2 	 | short | Unlock Menu Flag (Unlocks items for extra inventory)
| 4 	 | short | Name Id
| 6 	 | short | Shop keeper entity - [OBJ LIST](../../dictionary/obj.md)
| 8 	 | short | PosX
| 10 	 | short | PosY
| 12 	 | short | PosZ
| 14 	 | byte | Extra Inventory Bitmask (Get other items from unlocked shops)
| 15 	 | byte | Sound Id (When the shop is opened)
| 16 	 | short | Inventory count
| 18 	 | byte | Shop Id - [SHOP LIST](../../dictionary/shops.md)
| 19 	 | byte | Unk19
| 20 	 | short | Inventory Offset
| 22 	 | short | Padding

### Inventory Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | Unlock event Id (FFFF means always unlocked)
| 2 	 | short | Product count
| 4 	 | short | Product Offset
| 6 	 | short | Padding

### Product Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | Item Id - [Item LIST](../../dictionary/inventory.md)

### Valid Item Table Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | Item Id - [Item LIST](../../dictionary/inventory.md)

---

## Sklt

Defines which bones the characters' weapons are attached to.

### Sklt Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Sklt header
| 26 	 | Sklt entries

### Sklt Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (1)
| 4 	 | uint | Entry Count

### Sklt Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | Character Id - [Character LIST](../../dictionary/characters.md)
| 4 	 | ushort | Bone number 1 (Primary weapon)
| 6 	 | ushort | Bone number 2 (Secondary weapon)

---

## Pref

Defines preferences.

Documented in [preferences.md](./preferences.md).

---

## Evtp

Unknown.

### Evtp Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Evtp header
| 18 	 | Evtp entries

### Evtp Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (1)
| 4 	 | uint | Entry Count

### Evtp Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | byte | Id
| 1 	 | short | Unk2
| 3 	 | byte[3] | Padding?
| 6 	 | short | Unk6

---

## Ipic

Unknown.
