# [Kingdom Hearts II](../../index) - 00battle.bin

This is an essential file for booting [Kingdom Hearts II](../../index) and it contains everything related to the battle system.

* [LVUP](#lvup)
* [LVPM](#lvpm)
* [ENMP](#enmp)
* [FMLV](#fmlv)

## Lvup

Contains the level-up table for every playable character.

### Lvup Structure

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | byte[64] | Header. Currently unknown. 
| 4 	 | Character[0..13] | Character informations with a fixed sequence 

### Character sequence
 - Sora / Roxas
 - Donald
 - Goofy
 - Mickey
 - Auron
 - Ping / Mulan
 - Aladdin
 - Sparrow
 - Biest
 - Jack
 - Simba
 - Tron
 - Riku

### Lvup 'Character' Entry

| Offset | Type | Description |
|--------|------|-------------|
| 00 	 | int32 | Unknown 
| 04 	 | LevelUp[0..99] | Holds informations for the level up 

### Lvup 'LevelUp' Entry

| Offset | Type | Description |
|--------|------|-------------|
| 00 	 | int  | Needed EXP 
| 04 	 | byte | Strength of Character 
| 05 	 | byte | Magic of Character 
| 06 	 | byte | Defense of Character 
| 07 	 | byte | AP of Character 
| 08 	 | short | Ability given when using Sword route (03system.bin --> ITEM sub file) 
| 0A 	 | short | Ability given when using Shield route (03system.bin --> ITEM sub file) 
| 0C 	 | short | Ability given when using Staff route (03system.bin --> ITEM sub file) 
| 0E 	 | short | Padding 

## Lvpm

Contains the level-up table for the enemies. Based on the level of an enemy, a specific level, containing multiplying values, will be applied over the base statistics of an enemy.

## Enmp

Contains enemy statistics

### Enemy list

| Id | Description
|----|-------------
| 0B | Morning Star
| A1 | Luxord Data cards
| C8 | Vexen Data
| D6 | Lexaeus Data
| D7 | Marluxia Data
| D9 | Zexion Data
| DA | Vexen Anti Sora LV1
| DB | Vexen Anti Sora LV2
| DC | Vexen Anti Sora LV3
| DD | Vexen Anti Sora LV4
| DE | Vexen Anti Sora LV5
| DF | Zexion Data book illusion
| E0 | Zexion Data book trap
| EF | Xemnas Data
| F0 | Demyx Data
| F1 | Demyx Data minions
| F2 | Roxas Data
| F3 | Luxord Data
| F4 | Terra
| F5 | Axel Data
| F6 | Xaldin Data
| FC | Xigbar Data
| FD | Saix Data

## Fmlv

Contains the level-up table for summons and drive forms.

### Header

| Offset | Type  | Description 
|--------|-------|-------------
| 00     | int   | File type; always 2
| 04     | int   | Entries count
| 08     | Entry[Count] | Entries

### Entry

| Offset | Type  | Description 
|--------|-------|-------------
| 00     | byte  | First digit is the Form id, second digit is the Form level (e.g. 0x13 is Valor Form Level 3)
| 01     | byte  | Level of the movement ability (High Jump, Quick Run etc.)
| 02     | short | Ability obtained through level up
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