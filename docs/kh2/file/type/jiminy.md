# [Kingdom Hearts II](../../index) - jiminy.bar

This file contains different informations about Jiminy's Journal. Internally it is a [bar](bar.md) file.

* [Sub-file Header](#header)
* Entries
    * [WORL](#worl)
    * [STOR](#stor)
    * [ALBU](#albu)
    * [CHAR](#char)
    * [ANSE](#anse)
    * [DIAG](#diag)
    * [LIMI](#limi)
    * [MINI](#mini)
    * [QUES](#ques)
    * [PUZZ](#puzz)

## Header

Each sub-file starts with the same header. Only the Magic Code differs.

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | int    | Magic Code
| 04     | int    | Version
| 08     | int    | Entries count
| 0C     | int    | Padding

## Entries

## Worl

Contains metadata about the worlds. The world IDs don't align with the one's used in the rest of the game.

Magic Code `JMWO`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | Index
| 01     | char[2]| ID
| 03     | byte   | Padding
| 04     | ushort | Text used in the title bar
| 06     | ushort | Text used in menus (Treasures, Puzzle Pieces etc.)
| 08     | ushort | Story Flag to use this text
| 0A     | ushort | Alternative text used in the title bar
| 0C     | ushort | Alternative text used in menus (Treasures, Puzzle Pieces etc.)
| 0E     | ushort | Alternative Story Flag to use the alternative text

Only the world `hb` utilized the alternative texts and story flag fields to switch from `Hollow Bastion` to `Radiant Garden` later in the game.

## Stor

Contains informations about the entries shown in the "Story" section of each world.

Magic Code `JMST`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | [World](#worl)
| 01     | byte   | Unknown, always 0
| 02     | ushort | Unknown, always 0
| 04     | ushort | Text used in the "Summary" textbox
| 06     | ushort | Text used in the "Objective" textbox
| 08     | ushort | Text used in the "Story" section of the world
| 0A     | ushort | Story Flag when to show the texts

## Albu

Contains informations about the entries shown in the "Album" section of each world.

Magic Code `JMAL`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | [World](#worl)
| 01     | char[2]| Picture Id (menu/"region"/jm_photo/"world_id"_"picture_id".bin)
| 03     | byte   | Padding
| 04     | ushort | Unknown, always 0
| 06     | ushort | Story Flag when to show entry
| 08     | ushort | Title Text
| 0A     | ushort | Description Text

## Char

Contains informations about the "Character" section.

Magic Code `JMCH`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | [World](#worl)
| 01     | byte   | Picture (Index of menu/"region"/jm_photo/jmface_"world_id".bin)
| 02     | byte   | Background color Picture
| 03     | byte   | Padding
| 04     | ushort | ID
| 06     | ushort | Title
| 08     | ushort | Description
| 0A     | ushort | Second Title (used for Disney and FF characters)
| 0C     | ushort | [Object ID](./00objentry.md#structure)
| 0E     | ushort |
| 10     | ushort |
| 12     | short  | Object Position X
| 14     | short  | Object Position Y
| 16     | short  | Object Rotation X
| 18     | short  | 
| 1A     | short  | 
| 1C     | float  | 
| 20     | float  | 

## Anse

Contains the texts for the Secret Ansem Reports.

Magic Code `JMAN`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | [Item ID](./03system.md#item)
| 02     | ushort | Title
| 04     | ushort | Text
| 06     | ushort | Padding

## Diag

Seems to contain story flags. This sub-file houses two different structures.

Magic Code `JMDI`

### Structure 1

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | ID
| 02     | ushort |
| 04     | byte   | World
| 05     | byte   | Room
| 06     | byte   |
| 07     | byte   | Padding
| 08     | int    | 

### Structure 2

Currently unknown. Read in z_un_002a18d0.

## Limi

Contains informations about the section "Limits".

Magic Code `JMLI`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort |
| 02     | ushort | Title
| 04     | ushort | Description
| 06     | ushort | Padding

## Mini

Contains informations about the section "Minigames".

Magic Code `JMMG`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | World
| 02     | ushort | Title
| 04     | ushort | Highscore Text
| 06     | ushort |

## Ques

Contains informations about the section "Missions".

Magic Code `JMQU`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | World
| 02     | ushort | Category Text
| 04     | ushort | Title
| 06     | ushort | 
| 08     | ushort | Story Flag
| 0A     | ushort |
| 0C     | ushort |
| 0E     | ushort |

## Puzz

Contains metadata about the puzzle images. This sub-file is exclusive to the FM releases.

Magic Code `JMPZ`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | ID
| 01     | byte : 0-3 | Piece Count
| 01     | byte : 4-7 | Are pieces rotatable?
| 02     | ushort | Name
| 04     | ushort | [Reward Item](./03system.md#item)
| 06     | char[10] | File Name (menu/"region"/jm_puzzle/"filename".bin)

### Piece Sizes

| Type | Description |
|------|-------------|
| 0    | 12
| 1    | 48

### Rotations

| Type | Description |
|------|-------------|
| 0    | Fixed
| 1    | Rotatable