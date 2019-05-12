# Lvup
This sub-file is found within 00battle.bin. It stores informations about the
- needed EXP for the level up
- Strength, Magic, Defense and AP stats after the level up
- given ability for the corresponding route

## Lvup Structure

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