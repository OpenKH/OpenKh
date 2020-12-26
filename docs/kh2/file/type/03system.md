# [Kingdom Hearts II](../../index.md) - 03system.bin

This is an essential file to boot the game engine.

* [RCCT](#rcct) - ???
* [CMD](#cmd) - Commands
* [WENT](#went) - Weapon Entities
* [WMST](#wmst) - Weapon Movesets
* [ARIF](#arif) - ???
* [ITEM](#item) - Items
* [TRSR](#trsr) - Treasure
* [MEMT](#memt) - Member Table
* [FTST](#ftst) - Font Style
* [SHOP](#shop) - Shops
* [SKLT](#sklt) - ???
* [PREF](#pref) - Preferences?
* [EVTP](#evtp) - ???
* [IPIC](#ipic) - ???

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
| 0 	 | short | Id - [COMMAND LIST](../../dictionary/commands.md)
| 2 	 | short | Unk Id 1
| 4 	 | short | Unk Id 2
| 6 	 | byte | Submenu
| 7 	 | byte | Icon
| 8 	 | short | Text
| 10 	 | short | Unk10
| 12 	 | int | Unk12
| 16 	 | short | Unk16
| 18 	 | short | Unk18
| 20 	 | int | Unk20
| 24 	 | byte | Unk24
| 25 	 | byte | Unk25
| 26 	 | short | Unk26
| 28 	 | short | Mp/Drive cost
| 30 	 | int | Unk30
| 34 	 | short | Unk34
| 36 	 | byte | Unk36
| 37 	 | byte | Unk37
| 38 	 | short | Unk38
| 40 	 | short | Unk40
| 42 	 | short | Unk42
| 44 	 | short | Unk44
| 46 	 | short | Unk46

## Went

Weapon entity table.
Contains a list of pointers that point to the offset of a weapon set. There are multiple pointers for multiple sets.
Weapon sets contain the list of weapon models a character use in certain situations.

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

## Arif

Unknown.

## ITEM

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
| 2      | uint8  | Category. Refer to the table below to know what categories are recognized. |
| 3      | uint8  | Unknown |
| 4      | uint8  | Unknown |
| 5      | uint8  | Rank (C, B, A, S) for Synthesis items. |
| 6      | uint16 | [Status ID](#status-descriptor). Used to assign a certain status change to an item when equipped. |
| 8      | uint16 | Name message ID |
| 10     | uint16 | Description message ID |
| 12     | uint16 | Shop buy price |
| 14     | uint16 | Shop sell price |
| 16     | uint16 | Command. | - [COMMAND LIST](../../dictionary/commands.md)
| 18     | uint16 | Slot (Order in the menu) |
| 20     | uint16 | Picture linked to the image. |
| 22     | uint8  | Unknown |
| 23     | uint8  | Unknown |

#### Categories

| Type | Name
|------|------|
| 0    | Consumable
| 1    | Boost
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
| 8      | uint8  | Unknown |
| 9      | uint8  | Fire resistance |
| 10     | uint8  | Ice resistance |
| 11     | uint8  | Lightning resistance |
| 12     | uint8  | Dark resistance |
| 13     | uint8  | Unknown |
| 14     | uint8  | General resistance |
| 15     | uint8  | Unknown |

## TRSR

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

## Arif

Unknown.

## Memt

Defines which character [objects](../../obj.md) to load in certain situations.

### Memt Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Memt header
| 37 	 | Memt entries
| 1 	 | 28 unknown bytes

### Memt Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | File type (5)
| 4 	 | uint | Entry Count

### Memt Entry

| Offset | Type   | Description |
|--------|--------|-------------|
| 0      | ushort | [World ID](../../worlds.md)
| 2      | ushort[7] | Unknown
| 16      | ushort  | Player (Sora)
| 18      | ushort  | Party 1 (Donald)
| 20      | ushort  | Party 2 (Goofy)
| 22      | ushort  | Party 3 (World character)
| 24      | ushort  | Player (Valor)
| 26      | ushort  | Player (Wisdom)
| 28      | ushort  | Player (Limit)
| 30      | ushort  | Player (Master)
| 32      | ushort  | Player (Final)
| 34      | ushort  | Player (Anti)
| 36      | ushort  | Player (Mickey)
| 38      | ushort  | Player (Sora H)
| 40      | ushort  | Player (Valor H)
| 42      | ushort  | Player (Wisdom H)
| 44      | ushort  | Player (Limit H)
| 46      | ushort  | Player (Master H)
| 48      | ushort  | Player (Final H)
| 50      | ushort  | Player (Sora H)

## FTST

This is a table that contains the font palette for each world.

The FTST is a binary file that contains N amount of palettes (9 for FM version), where every palette contain an unique ID (or key) and exactly 19 different colors.

Each color correspond to the [world index](../../worlds.md) and it is loaded based on the current world.

## Shop

This file contains data for the shops.
There's a list of products available. Products are grouped in Inventories. Shops sell products in their inventories.
Each inventory has the product on its "Product offset" and the following "Product count" products.
Each shop has the inventory on its "Inventory offset" and the following "Inventory count" products.

### Shop Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Shop header
| 21 	 | Shop entries
| 333 	 | Inventory entries
| 536 	 | Product entries

NOTE: there are 464 products + 72 empty (There may be padding)

### Shop Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | char[4] | File ID (TZSH)
| 4 	 | ushort | File type (7)
| 6 	 | ushort | Shop List count
| 8 	 | 4B | Unk8
| 12 	 | 4B | Unk12

### Shop Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | Id?
| 2 	 | short | Unlock (The items that will be unlocked in TT & HB)
| 4 	 | short | Name Id
| 6 	 | short | Shop keeper entity - [OBJ LIST](../../dictionary/obj.md)
| 8 	 | short | PosX
| 10 	 | short | PosY
| 12 	 | short | PosZ
| 14 	 | short | Sound Id (When the shop is open)
| 16 	 | short | Inventory count
| 18 	 | byte | Unk18 (Id?) - [SHOP LIST](../../dictionary/shops.md)
| 19 	 | byte | Unk19
| 20 	 | short | Inventory Offset
| 22 	 | short | Padding

### Inventory Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | Unlock event Id (FFFF means always unlocked)
| 2 	 | short | Product count
| 4 	 | short | Product size
| 6 	 | short | Padding

### Product Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | uint | Item Id - [Item LIST](../../dictionary/inventory.md)

## Sklt

Unknown.

## Pref

Defines preferences. It is a [BAR](bar.md) file containing the following subfiles:

| File | Description |
|--------|---------------|
| plyr 	 | Player
| fmab 	 | Form Abilities
| prty 	 | Party
| sstm 	 | System
| magi 	 | Magic

### plyr
Each pointer leads to a specific entry's offset.

### plyr Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer count [uint]
| 59 	 | Pointers [uint]
| 10 	 | Plyr entries

### plyr Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | byte[116] | Unknown

### fmab
Each pointer leads to a specific entry's offset.

### fmab Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer count [uint]
| 5 	 | Pointers [uint]
| 5 	 | fmab entries

### fmab Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | byte[68] | Unknown

### prty
Each pointer leads to a specific entry's offset.

### prty Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer count [uint]
| 70 	 | Pointers [uint]
| 5 	 | prty entries

### prty Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | byte[68] | Unknown

### sstm
It looks like it uses the common structure file type + file size. Other than that, the structure is unknown.

### magi
Each pointer leads to a specific entry's offset.

### magi Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer count [uint]
| 36 	 | Pointers [uint]
| 5 	 | magi entries

### magi Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | byte[124] | Unknown

## Evtp

Unknown.

## Ipic

Unknown.