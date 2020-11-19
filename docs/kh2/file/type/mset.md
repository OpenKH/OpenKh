# [Kingdom Hearts II](../../index.md) - MSET (Moveset)

The MSET files contain animations and the effects related to them (I-frames, hitboxes, effects...)
It is a [BAR file](bar.md) that contains a list of "slots". Each slot may or may not have a "motion".
The various actions in the game point to a specific motion by a slot position in this list, thus many of the motions are dummies, since the characters only use some of them.
Eg: For Sora, slot 0 is idle, slot 4 is walking and slot 604 is the basic attack.

Each motion is a BAR file with 2 entries, the model animation and the effects.

## Animation structure

???

## Effects structure

| Amount | Description |
|--------|---------------|
| 1 	 |  Header
| X 	 |  Type A effect (Happens during X frames)
| X 	 |  Type B effect (Triggered on a specific frame)

### Effects header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | byte | Type A effect count
| 1 	 | byte | Type B effect count
| 2 	 | short | Start offset of the type B effects

### Type A effect

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | Start frame
| 2 	 | short | End frame
| 4 	 | byte | Effect A ID
| 5 	 | byte | Param size as shorts
| 6 	 | short[] | Param

### Type B effect

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 	 | short | Trigger frame
| 2 	 | byte | Effect B ID
| 3 	 | byte | Param size as shorts
| 4 	 | short[] | Param

## Effect A list

| ID | Effect | Parameter | Size
|--------|---------------|-------------|-------------|
| 0 	 | Allows controls (But brings to idle animaiton) | 
| 1 	 | Allows controls (But blocks the animation) | 
| 2 	 | Allows controls | 
| 3 	 | Blocks the animation (But disables gravity) | 
| 4 	 | Blocks controls (But allows gravity) | 
| 10 	 | Activates hitbox | ? | ?
| 20 	 | Performs a reaction command (On current model) | ? | ?
| 23 	 | Draws an additional texture | ? | ?
| 25 	 | Performs a reaction command (On another model) | ? | ?
| 27 	 | Makes invincible | 
| 30 	 | Blocks everything | 
| 34 	 | Blocks reaction command | 
| 41 	 | Allows controls (But disables model rotation) | 

## Effect B list

| ID | Effect | Parameter | Size
|--------|---------------|-------------|-------------|
| 1 	 | Plays PAX sprite | PAX sprite ID | ?
| 2 	 | Plays footstep sound | Sound ID | ?
| 3 	 | Plays animation in slot 628 | 
| 13 	 | Plays an enemy vsb voice | vsb ID | ?
| 14 	 | Plays an ally vsb voice | vsb ID | ?
| 22 	 | Makes the keyblade appear | ? | ?
| 23 	 | Makes model opacity decrease | ? | ?
| 24 	 | Makes model opacity increase | ? | ?
| 26 	 | Makes a mesh disappear | ? | ?
| 27 	 | Makes a mesh appear | ? | ?
| 29 	 | Plays a Keyblade appearance sprite | 