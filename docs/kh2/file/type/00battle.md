# [Kingdom Hearts II](index.md) - 00battle.bin

This is an essential file for booting [Kingdom Hearts II](../../index) and it contains everything related to the battle system.

* [LVUP](#lvup)
* [LVPM](#lvpm)
* [ENMP](#enmp)

## Lvup

This sub-file is found within 00battle.bin. It stores informations about the
- needed EXP for the level up
- Strength, Magic, Defense and AP stats after the level up
- given ability for the corresponding route

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

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | int32_t | Unknown 
| 4 	 | LevelUp[0..99] | Holds informations for the level up 

### Lvup 'LevelUp' Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	| int32_t | Needed EXP 
| 4 	| byte | Strength of Character 
| 5 	| byte | Magic of Character 
| 6 	| byte | Defense of Character 
| 7 	| byte | AP of Character 
| 8 	| short | Ability given when using Sword route (03system.bin --> ITEM sub file) 
| 10 	| short | Ability given when using Shield route (03system.bin --> ITEM sub file) 
| 12 	| short | Ability given when using Staff route (03system.bin --> ITEM sub file) 
| 14 	| short | Padding 

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