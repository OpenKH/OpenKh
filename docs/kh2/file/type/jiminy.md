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

---

## Header

Each sub-file starts with the same header. Only the Magic Code differs.

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | int    | Magic Code
| 04     | int    | Version
| 08     | int    | Entries count
| 0C     | int    | Padding

---

## Entries

---

## Worl

Contains metadata about the worlds. The world IDs don't align with the one's used in the rest of the game.

Magic Code `JMWO`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | Index
| 01     | char[3]| ID
| 04     | ushort | Text used in the title bar
| 06     | ushort | Text used in menus (Treasures, Puzzle Pieces etc.)
| 08     | ushort | Story Flag to use this text
| 0A     | ushort | Alternative text used in the title bar
| 0C     | ushort | Alternative text used in menus (Treasures, Puzzle Pieces etc.)
| 0E     | ushort | Alternative Story Flag to use the alternative text

Only the world `hb` utilized the alternative texts and story flag fields to switch from `Hollow Bastion` to `Radiant Garden` later in the game.

---

## Stor

Contains informations about the entries shown in the "Story" section of each world.

Magic Code `JMST`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | [World](#worl)
| 01     | byte[3] | Padding
| 04     | ushort | Text used in the "Summary" textbox
| 06     | ushort | Text used in the "Objective" textbox
| 08     | ushort | Text used in the "Story" section of the world
| 0A     | ushort | Story Flag when to show the texts

---

## Albu

Contains informations about the entries shown in the "Album" section of each world.

Magic Code `JMAL`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | [World](#worl)
| 01     | char[5]| Picture Id (menu/"region"/jm_photo/"world_id"_"picture_id".bin)
| 06     | ushort | Story Flag when to show entry
| 08     | ushort | Title Text
| 0A     | ushort | Description Text

---

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
| 0A     | ushort | Source Title
| 0C     | ushort | [Object ID](./00objentry.md#structure)
| 0E     | ushort | Motion
| 10     | ushort | Stat
| 12     | short  | Object X Position
| 14     | short  | Object Y Position
| 16     | short  | Object Y Rotation
| 18     | short  | Object X Position 2
| 1A     | short  | Object Y Position 2
| 1C     | float  | Scale
| 20     | float  | Scale 2

---

## Anse

Contains the texts for the Secret Ansem Reports.

Magic Code `JMAN`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | [Item ID](./03system.md#item)
| 02     | ushort | Title
| 04     | ushort | Text
| 06     | ushort | Padding

---

## Diag

Seems to contain story flags. This sub-file houses two different structures.

Magic Code `JMDI`

### Structure 1 (Data Info)

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | DrawProgress
| 02     | ushort | HideProgress
| 04     | byte   | World
| 05     | byte   | Count
| 06     | byte   | Type
| 07     | byte   | Padding
| 08     | uint   | Address 

### Structure 2 (Character Info)

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | Label
| 02     | short  | X Position
| 04     | short  | Y Position
| 06     | byte   | Draw
| 07     | byte   | Padding 

---

## Limi

Contains informations about the section "Limits".

Magic Code `JMLI`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | Command Id
| 02     | ushort | Title
| 04     | ushort | Description
| 06     | ushort | Padding

---

## Mini

Contains informations about the section "Minigames".

Magic Code `JMMG`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | World
| 02     | ushort | Title
| 04     | ushort | Highscore Text
| 06     | ushort | Game Id

---

## Ques

Contains informations about the section "Missions".

Magic Code `JMQU`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | World
| 02     | ushort | Category Text
| 04     | ushort | Title
| 06     | ushort | [Status](#status)
| 08     | ushort | Story Flag
| 0A     | ushort | Game Id
| 0C     | ushort | Score
| 0E     | ushort | Clear Condition

### Status

| Type | Description |
|------|-------------|
| 0    | Disabled
| 1    | Draw
| 2    | Cleared
| 3    | 100% Cleared

---

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