# [Kingdom Hearts II](../../index.md) - 03system.bin

This is an essential file to boot the game engine.

* [ITEM](#item)
* [FTST](#ftst)
* [TRSR](#trsr)

## FTST

This is a table that contains the font palette for each world.

The FTST is a binary file that contains N amount of palettes (9 for FM version), where every palette contain an unique ID (or key) and exactly 19 different colors.

Each color correspond to the [world index](../../worlds.md) and it is loaded based on the current world.

## ITEM

Describe an item, that could be anything from a consumable, to a weapon or materials.

There are two tables. The first one is the Item descriptor itself, the second one is for the statistics the item is affecting.

### Table header

Every table has the following header

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | uint32 | The identifier of the file, 6 for the first table and 0 for the second. |
| 4      | uint32 | Descriptor count for the specified table. |

### Item descriptor

| Offset | Type | Description |
|--------|---------------|-------------|
| 0      | uint16 | Unique item identifier, used from the game engine to refer to a specific item. |
| 2      | uint8  | Category. Refer to the table below to know what categories are recognized. |
| 3      | uint8  | Unknown |
| 4      | uint8  | Unknown |
| 5      | uint8  | Rank (C, B, A, S) for Synthesis items. |
| 6      | uint16 | [Status ID](#status-descriptor). Used to assign a certain status change to an item when equipped. |
| 8      | uint16 | Name message ID |
| 10     | uint16 | Description message ID |
| 12     | uint16 | Shop buy ID |
| 14     | uint16 | Shop sell ID |
| 16     | uint16 | Command. Purpose unknown. |
| 18     | uint16 | Slot. Purpose unknown. |
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

### Status descriptor

| Offset | Type   | Description |
|--------|--------|-------------|
| 0      | uint16 | Unique status identifier. |
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
