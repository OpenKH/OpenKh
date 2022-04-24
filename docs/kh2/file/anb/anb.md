# [Kingdom Hearts II](../../index.md) - ANB (Animation Binary)

ANB files are used by the game engine to animate a 3D model and trigger events in specific frames. They are used for both gameplay and cutscenes. All the gameplay ANB files are located inside [MSET](mset.md) files in `obj/`, while for cutscenes they are located in `anm/{WORLD}/{MODEL}/`.
Internally they are just [BAR archives](../type/bar.md) and for what it's known the only two files that can be found in are [motion data](#motion-data) and [effect data](#effect-data).

# Motion data

This file is responsible of animating a [3D model](../type/mdlx.md). It comes in two forms, interpolated and RAW. Due to the bigger complexity of interpolated-type motion files, they will be documented in a [separate document](motion.md).

The game engine assumes that the maximum frame rate is `60`; this is referred as Global Frame Rate (or GFR). But every motion can run at a different frame rate; this is referred as Local Frame Rate (or LFR).

The RAW animation type just takes an array of matrices for each frame and applies them to the bones of a model. It is a very cheap technique in terms of CPU usage, but it requires a big amount of memory.

## Motion header

Like model files, the first `0x90` are reserved an always set to `00`. Offsets and file size ignores the first `0x90`. This document refers to the motion type `1`, which is the RAW one. When the motion type is `0`, the rest of the file after the header will be very different; refer to [interpolated motion document](motion.md).

| Offset | Type | Description
|--------|------|--------------
| 0      | int  | Type
| 4      | int  | Subtype
| 8      | int  | Extra Offset (Equal to size of motion)
| 12     | int  | Extra Size

### Motion Types

Types for the unknown extra data.

| Channel | Description
|---------|---------------
| 0       | Interpolated
| 1       | Raw

### Motion Subtypes

Types for the unknown extra data.

| Channel | Description
|---------|---------------
| 0       | Normal
| 1       | Ignore Scale

### Extra Types

Types for the unknown extra data.

| Channel | Description
|---------|---------------
| 0       | Weapon Cns
| 1       | Terminate

## Raw motion header

This is located straight after the motion header.

| Offset | Type   | Description
|--------|--------|--------------
| 0x00   | int    | Bone count. Must match with the bone count of the model.
| 0x04   | int[3] | Reserved
| 0x10   | int    | Frame count: Amount of GFR in a loop (`FrameEnd - FrameLoop`)
| 0x14   | int    | Total Frame count, expressed in LFR
| 0x18   | int    | Animation Matrix Offset
| 0x1c   | int    | [Position Matrix](#raw-motion-more-matrices) Offset
| 0x20   | vec4f  | Bounding Box minimum
| 0x30   | vec4f  | Bounding Box maximum
| 0x40   | float  | Frame start
| 0x44   | float  | Frame end
| 0x48   | float  | Frames per second, defines the LFR
| 0x4c   | float  | Frame return, expressed in LFR

## Raw motion matrices

An array of array of 4x4 matrices (`Matrix4x4[][]`). The first dimension of the array refers to the total amount of frames, while the second dimension is the bone count. For each frame (LFR), the game engine takes BoneCount amount of `Matrix4x4` and multiplies them to the mesh that corresponds to a specific bone.

## Raw motion more matrices

This is optionally defined and its purpose is unknown. It is an array of 4x4 matrices (`Matrix4x4[]`), where the size of the array is equal to the bone count. For the few files that has been analysed, those matrices are always a Matrix Identity.

# Motion Triggers

This file describe triggers that happens during a [motion](#motion-data) and it's believed they are located into a different file to favour decoupling.

## Motion Triggers structure

| Amount | Description |
|--------|---------------|
| Single |  [Header](#effect-entry-header)
| Array  |  [Range Trigger](#range-trigger-list) count (Happens during X frames)
| Array  |  [Frame Trigger](#frame-trigger-list) count (Triggered on a specific frame)

## Motion Trigger Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | byte | [Range Trigger](#range-trigger-list) count
| 1      | byte | [Frame Trigger](#frame-trigger-list) count
| 2      | short | Frame Triggers offset

## Range Trigger

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | short | Start frame
| 2      | short | End frame
| 4      | byte | [Range Trigger ID](#range-trigger-list)
| 5      | byte | Param size as shorts
| 6      | short[] | Param

## Frame Trigger

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | short | Trigger frame
| 2      | byte | [Frame Trigger ID](#frame-trigger-list)
| 3      | byte | Param size as shorts
| 4      | short[] | Param

## Range Trigger list

| ID | Trigger | Parameter Size | Parameter Desc
|--------|---------------|-------------|-------------|
| 0 | STATE: Grounded | 0 | 
| 1 | STATE: Falling | 0 | 
| 2 | STATE: Grounded 2 | 0 | 
| 3 | STATE: No gravity | 0 | 
| 4 | Enable collision flag | 1 | 
| 5 | Disable collision flag | 1 | 
| 6 | Enable RECOM flag | 2 | 
| 7 | Disable RECOM flag (UNUSED?) | ? | 
| 8 | \<unknown\> (Prints "using NO_DAMAGE_REACTION attribute"?) (UNUSED?) | ? | 
| 9 | STATE: \<unknown\> | 0 | 
| 10 | Motion attack on | 2 | 
| 11 | STATE: Allows combo attack | 0 | 
| 12 | STATE: \<unknown\> | 0 | 
| 13 | STATE: \<unknown\> | 0 | 
| 14 | STATE: Allows Battle RCs | 0 | 
| 15 | STATE: \<unknown\> (UNUSED?) | ? | 
| 16 | \<unknown\> ("Mobility enhancement", eg: LW's dash) | 0 | 
| 17 | \<unknown\> (AI combo 1) | 0 | 
| 18 | \<unknown\> (AI combo 2) | 0 | 
| 19 | \<unknown\> ("Disable forced i-frames", eg: LW's idle, ground and air) | 0 | 
| 20 | Reaction Command (Self) | 2 | 
| 21 | STATE: \<unknown\> | 0 | 
| 22 | Turn to target | 1 | Turn speed
| 23 | Texture animation | 1 | 
| 24 | PROPERTY: \<unknown\> (Disable gravity but keeps kinetics) | 1 | 
| 25 | PROPERTY: \<unknown\> (AnmatrCommand; Reaction command on other object) | ? | 
| 26 | PROPERTY: \<unknown\> (AnmatrCommand) | 1 | 
| 27 | STATE: Hitbox off (Can't be hit) | 0 | 
| 28 | Turn to lock on | 1 | Turn speed
| 29 | STATE: \<unknown\> (Can't fall off edges easily) | 0 | 
| 30 | STATE: \<unknown\> (Freeze animation? Immovable?) | 0 | 
| 31 | STATE: \<unknown\> | 0 | 
| 32 | STATE: \<unknown\> | 0 | 
| 33 | motion attack on (Enemy) | 3 | 
| 34 | STATE: \<unknown\> (UNUSED?) | ? | 
| 35 | STATE: \<unknown\> (Eg: Fire. Makes fire count as combo) | 0 | 
| 36 | Pattern enable (LW attacks. Teleport to player?) | 0 | 
| 37 | Pattern disable | 0 | 
| 38 | STATE: \<unknown\> | 0 | 
| 39 | STATE: \<unknown\> (UNUSED?) | ? | 
| 40 | STATE: \<unknown\> | 0 | 
| 41 | STATE: \<unknown\> | 0 | 
| 42 | STATE: \<unknown\> (Eg: landing, using items) | 0 | 
| 43 | STATE: \<unknown\> (UNUSED?) | ? | 
| 44 | STATE: \<unknown\> (Eg: Explosion finisher, LW bow. Not movable by other objects?) | 0 | 
| 45 | STATE: \<unknown\> (UNUSED?) | ? | 
| 46 | STATE: \<unknown\> (UNUSED?) | ? | 
| 47 | STATE: \<unknown\> | 0 | 
| 48 | STATE: \<unknown\> (UNUSED?) | ? | 
| 49 | STATE: \<unknown\> (Eg: Dodge roll) | 0 | 
| 50 | STATE: \<unknown\> (Allows combo finisher next) | 0 | 
| 51 | Play singleton SE | 3 | 
| 52 | STATE: \<unknown\> | 0 | 
| 53 | STATE: \<unknown\> | 0 | 

## Frame Trigger list

| ID | Trigger | Parameter Size | Parameter Desc
|----|---------------|-------------|-------------|
| 0 | Jump State | 0 | 
| 1 | Play PAX effect from a.fm | 1 | PAX effect Id
| 2 | Play footstep sound | 2 | Sound Id
| 3 | Dusk Jump State (animation in slot 628) | 0 | 
| 4 | Texture animation start | 1 | 
| 5 | Texture animation stop (UNUSED?) | ? | 
| 6 | Use Item effect | 0 | 
| 7 | \<unknown\> (LIMIT: AnmatrCallback; Limit RC?) | 2 | 
| 8 | Play SE sound from a.fm | 4 | 
| 9 | \<unknown\> (VariousTrigger 1) | 0 | 
| 10 | \<unknown\> (VariousTrigger 2) | 0 | 
| 11 | \<unknown\> (VariousTrigger 4) | 0 | 
| 12 | \<unknown\> (VariousTrigger 8) | 0 | 
| 13 | Plays an enemy vsb voice | 1/2 | VSB Id
| 14 | Plays an ally vsb voice | 1 | VSB Id
| 15 | Turn to Target | 1 | 
| 16 | \<unknown\> (DisableCommandTime; Eg: When hit on air) | 1 | 
| 17 | Magic cast | 1 | 
| 18 | \<UNDEFINED\> |  | 
| 19 | Apply footstep effect (Footprint, water splashes...) | 1 | 
| 20 | \<UNDEFINED\> |  | 
| 21 | Turn to lock on | 1 | 
| 22 | Makes the keyblade appear | 0 | 
| 23 | Fade start (Opacity decrease) | 1 | 
| 24 | Fade start (Opacity increase) | 1 | 
| 25 | \<unknown\> (Related to the party) | 0 | 
| 26 | Set mesh color | 2 | 
| 27 | Reset mesh color | 2 | 
| 28 | Revenge check | 0 | 
| 29 | Plays a Keyblade appearance sprite | 0 | 
| 30 | \<unknown\> (LIMIT: PlayVoice) (UNUSED?) | ? | 
| 31 | Trigger vibration | 1 | 
| 32 | \<UNDEFINED\> |  | 
| 33 | \<UNDEFINED\> |  | 
| 34 | Check for Dodge Roll to Airslide (Quick Run) | 0 | 
| 35 | \<unknown\> (MOTION: start; Eg: dodge roll) | 0

Note: This data has been checked on normal MSET files. Those marked as (UNUSED?) are probably used in RC and Limit files.