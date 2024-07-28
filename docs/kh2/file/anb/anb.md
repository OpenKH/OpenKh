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

Flag: The bitflag field position in memory from the MOTION object and the bit set, starting from 0.

| ID | Trigger | Parameter Size | Parameter Desc | Flag | Samples
|---|---|------------|-------------|-------------|-------------|
| 0 | STATE: Grounded | 0 |  | 0x20: 2 | B_EX210_EH - 00A; W_EX010_LK - L140
| 1 | STATE: Air | 0 |  | 0x20: 3 | WM_CURSOR - 02A; B_EX130 - AM82_0010
| 2 | STATE: Blend to idle | 0 |  | 0x20: 4 | P_WI030 - A010; B_EX100 - AM20_0140
| 3 | STATE: No gravity | 0 |  | 0x20: 5 | B_AL100_ICE - AN72_0010; P_WI030 - A400
| 4 | Enable collision | 1 | Collision group Id |  | B_AL020 - 40A; WM_SYMBOL_EH - 10A
| 5 | Disable collision | 1 | Collision group Id |  | B_AL100_FIRE - 40A; WM_SYMBOL_EH - 10A
| 6 | Enable conditional RC | 2 | Condition command?, Collision group Id |  | F_BB050 - 00A (Enables "Catch" after "Step Vault" is triggered)
| 7 | Disable conditional RC | 2 | Condition command?, Collision group Id |  | ?
| 8 | (Prints "using NO_DAMAGE_REACTION attribute") (UNUSED?) | 0 |  |  | ?
| 9 | STATE: Jump/Land | 0 |  | 0x20: 9 | B_CA010 - 03A; P_EX100_HTLF_BTL - 005A
| 10 | Attack hitbox | 2 | Atkp entry Id, Collision group Id |  | B_AL020 - 50A; W_MU000_PIN - B330
| 11 | STATE: Allow combo | 0 |  | 0x20: 10 | P_EX100 - A320; P_LM100 - M140
| 12 | STATE: Weapon swing (Display trail) | 0 |  | 0x20: 11 | B_BB100 - AM10_0011; W_TR000 - A400
| 13 | STATE: \<unknown\> | 0 |  | 0x20: 15 | B_AL020 - AP20_0010; P_CA000 - A402
| 14 | STATE: Allow RC | 0 |  | 0x20: 19 | P_EX100 - A005; P_LK100 - L404
| 15 | STATE: \<unknown\> (UNUSED?) | 0 |  | 0x20: 16 | ?
| 16 | AI: Special movement | 0 |  | 0xf8: 0 | B_AL100_FIRE - 52A; W_EX010_ROXAS_LIGHT - 16B
| 17 | AI: \<unknown\> (AI combo 1) | 0 |  | 0xf8: 1 | B_AL100_ICE - 57A; W_EX010_ROXAS_LIGHT - 16B
| 18 | AI: \<unknown\> (AI combo 2) | 0 |  | 0xf8: 2 | B_AL100_ICE - 57A; W_EX010_ROXAS_LIGHT - 17B
| 19 | AI: Disable invincibility | 0 |  | 0xf8: 3 | B_EX150 - 01A; W_EX010_ROXAS_LIGHT - 12B
| 20 | Reaction Command (Self) | 2 | Command Id, Collision group Id |  | B_AL020 - AP20_0010; P_EX220 - A122
| 21 | STATE: \<unknown\> | 0 |  | 0x20: 18 | B_EX120_HB - 54A; B_AL100_FIRE - AN70_0010
| 22 | ACTION: Turn to target | 1 | Turn speed | 0x28: 3 | B_AL020 - 53A; WM_CURSOR - 10A
| 23 | Texture animation | 1 | Animation Id (It is unknown where this points) |  | B_AL020 - 40A; P_WI030 - A221
| 24 | STATE: No gravity, keep kinetics | 1 | Unknown, related to the movement | 0x20: 6 | B_EX170 - 52A; P_AL010 - 81A
| 25 | STATE: AnmatrCommand (Reaction command on other object) | 1 | Command Id | 0x20: 20 | M_EX770 - AA31_0010
| 26 | STATE: AnmatrCommand 2 | 1 | Command Id | 0x20: 21 | P_EX220 - A122; B_BB110 - AM51_0010
| 27 | STATE: Hitbox off (Can't be hit) | 0 |  | 0x20: 22 | P_EX220 - A121; B_AL020 - AP20_0010
| 28 | ACTION: Turn to lock | 1 | Turn speed | 0x28: 3 | P_LK100 - L401; B_EX150 - AN20_0010
| 29 | STATE: Can't leave ground | 0 |  | 0x20: 23 | B_CA010 - 80A; P_LK100 - L402
| 30 | STATE: \<unknown\> (Freeze animation? Immovable?) | 0 |  | 0x20: 24 | P_EX350 - 51A; B_BB110 - AM51_0010
| 31 | STATE: \<unknown\> | 0 |  | 0x20: 25 | B_CA050 - 54A; P_EX100_KH1F - K422
| 32 | STATE: \<unknown\> | 0 |  | 0x20: 26 | B_HE030_RTN - R00A; P_WI030_RTN - R00A
| 33 | Attack hitbox (Combo) | 3 | Atkp entry Id, Collision group Id, Combo Id |  | B_CA010 - 50A; W_HE000 - A400
| 34 | STATE: \<unknown\> | 0 |  | 0x20: 27 | F_WI390 - AY15_0010
| 35 | STATE: Allow combo (Magic) | 1 |  | 0x20: 10 | P_EX100 - A362; P_LK100 - L362
| 36 | Pattern enable | 1 | Pattern Id |  | B_AL020 - 55A; P_EX130 - 53A
| 37 | Pattern disable | 1 | Pattern Id |  | B_EX170_LAST - 00M; B_LK120 - 20A
| 38 | STATE: Allow drop from on top | 0 |  | 0x20: 28 | B_LK120 - 03A; F_TT000 - 01A (Allows falling off TT's train)
| 39 | STATE: \<unknown\> | 0 |  | 0x20: 29 | B_BB110 - AM50_0010
| 40 | STATE: \<unknown\> | 0 |  | 0x20: 30 | P_EX330 - 99A
| 41 | STATE: Allow movement | 0 |  | 0x20: 31 | WM_CURSOR - 04A; B_EX220 - AP60_0010
| 42 | PHYSICS: Keep momentum & restrict movement | 0 |  | 0x24: 0 | P_EX100 - A005; P_LK100 - L404
| 43 | \<NOT CODED\> | 1 |  |  | B_BB100 - AM10_0010 (For some reason this is used here)
| 44 | PHYSICS: Immovable by friction | 0 |  | 0x24: 1 | B_AL100_FIRE - 51A; P_LK100 - L404
| 45 | ACTION: \<unknown\> | 0 |  | 0x28: 0 | P_AL010 - A811_0010
| 46 | ACTION: \<unknown\> | 0 |  | 0x28: 1 | ?
| 47 | ACTION: Rotate towards movement direction | 0 |  | 0x28: 2 | WM_CURSOR - 04A; B_EX220 - AP60_0010
| 48 | \<NOT CODED\> | - |  |  | -
| 49 | ACTION: Maintain motion on ground leave | 0 |  | 0x28: 4 | P_LK030 - L400; B_AL020 - AP24_0010
| 50 | ACTION: Allow combo finisher | 0 |  | 0x28: 5 | P_EX100 - A363; P_LK100 - L402
| 51 | Play singleton sound effect | 3 | index (0/1), position, sound effect |  | F_PO090 - 00A; P_EX100_AL_CARPET - G002
| 52 | ACTION: Stop actions | 0 |  | 0x28: 6 | N_EX500_BTL - 40A
| 53 | ACTION: \<unknown\> | 0 |  | 0x28: 7 | P_LM100 - M141; P_LM100 - M142

## Frame Trigger list

| ID | Trigger | Parameter Size | Parameter Desc | Sample
|----|---------------|-------------|-------------|-------------|
| 0 | Action: Jump | 0 |  | B_BB110 - 03A; P_WI030 - A003
| 1 | Trigger effect caster (APDX) | 1 | Effect caster Id | B_AL020 - 42A; W_EX010_ROXAS_LIGHT - 16B
| 2 | Play footstep sound | 2 (1 uint) | Footstep sound Id | P_WI030_MEMO - A001; B_AL100_ICE - AN72_0010
| 3 | Action: Jump (Dusk) (animation in slot 628) | 0 |  | M_EX990 - 14A
| 4 | Texture animation start | 1 | Animation Id (It is unknown where this points) | M_EX350_01_MEMO - 00C
| 5 | Texture animation stop (UNUSED?) | 1 | Animation Id (It is unknown where this points) | ?
| 6 | Use item | 0 |  | P_AL000 - A110; P_WI030 - A110
| 7 | Game effect (Effects more complex than particle effects) | 2 | Effect Id?, ? | B_AL020 - 50A; P_EX330 - 80A
| 8 | Play sound effect (APDX) | 4 | Package Id, Sound Id, ?, ? | B_AL020 - 50A; W_MU000_PIN - B330
| 9 | VariousTrigger 1 | 0 |  | B_AL020 - 50A; W_EX010_ROXAS_LIGHT - 10B (Fat Bandit flamethrower start)
| 10 | VariousTrigger 2 | 0 |  | B_BB120 - 41A; P_HE000 - A400 (Fat Bandit flamethrower end)
| 11 | VariousTrigger 4 | 0 |  | B_CA050 - 54A; P_EX130 - 52A
| 12 | VariousTrigger 8 | 0 |  | B_CA050 - 54A; F_EX040_SPARROW - A953
| 13 | Play vsb voice | 1/2 | VSB Id | 1 - B_HE030_ALL - 20A; B_HE030_ALL - 24A / 2 - B_AL020 - 40A; P_EX350 - 90A
| 14 | Play vsb voice | 1 | VSB Id | P_WI030 - A331; B_AL100_FIRE - AN70_0010
| 15 | Turn to Target | 1 | Turn speed | P_WI030 - A301; B_EX110 - 70A
| 16 | \<unknown\> (DisableCommandTime) | 1 | ? | P_LK100 - L403; F_WI360 - AX00_0010
| 17 | Magic cast | 1 | Magic Id? | P_EX100 - A362; P_LK100 - L362
| 18 | \<NOT CODED\> | - |  | -
| 19 | Apply footstep effect (Footprint, water splashes...) | 1 | Footstep type Id | P_LK100_MEMO - L000; B_BB100 - AM10_0010
| 20 | \<NOT CODED\> | - |  | -
| 21 | Turn to lock on | 1 | Turn speed | P_LK100 - L403; P_AL010 - A814_0010
| 22 | Make the weapon appear | 0 |  | P_TR000 - A300
| 23 | Fade start (Opacity decrease) | 1 | Frames to fade | P_EX220 - A121; B_EX100 - 56A
| 24 | Fade start (Opacity increase) | 1 | Frames to fade | B_EX100 - 58A; F_EX040_SPARROW - A953
| 25 | Calls a function from the entity's pvTable | 0 |  | B_EX130 - 51A; F_WI060_PETE - 51A
| 26 | Set mesh color to 0x808080 | 2 | Part Id, Time (float) | WM_SYMBOL_NM - 10A; B_EX100 - AM28_0140
| 27 | Reset mesh color | 2 | Part Id, Time (float) | WM_SYMBOL_NM - 10A; B_EX100 - AM28_0140
| 28 | Revenge check | 0 |  |P_EX130 - 20A;  B_AL100_FIRE - 20A
| 29 | Make the weapon appear with effect | 0 |  | P_EH000 - A401; P_LK020 - L221
| 30 | LIMIT: PlayVoice (UNUSED?) | 1 | Voice Id, Priority | ?
| 31 | Trigger vibration | 1 | Vibration type Id | P_LK100 - L402; B_AL100_FIRE - 52A
| 32 | \<NOT CODED\> | - |  | -
| 33 | \<NOT CODED\> | - |  | -
| 34 | Quick run check | 0 |  | P_EX100 - A180; P_EX100_KH1F - A180
| 35 | Transition to fall if on air | 0 |  | P_EX100 - A180; P_EX100_KH1F - A180