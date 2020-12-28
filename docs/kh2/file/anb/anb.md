# [Kingdom Hearts II](../../index.md) - ANB (Animation Binary)

ANB files are used by the game engine to animate a 3D model and trigger events in specific frames. They are used for both gameplay and cutscenes. All the gameplay ANB files are located inside [MSET](mset.md) files in `obj/`, while for cutscenes they are located in `anm/{WORLD}/{MODEL}/`.
Internally they are just [BAR archives](../type/bar.md) and for what it's known the only two files that can be found in are [motion data](#motion-data) and [effect data](#effect-data).

## Motion data

This file is responsible of animating a [3D model](../type/mdlx.md). It comes in two forms, interpolated and RAW. Due to the bigger complexity of interpolated-type motion files, they will be documented in a [separate document](motion.md).

The game engine assumes that the maximum frame rate is `60`; this is referred as Global Frame Rate (or GFR). But every motion can run at a different frame rate; this is referred as Local Frame Rate (or LFR).

The RAW animation type just takes an array of matrices for each frame and applies them to the bones of a model. It is a very cheap technique in terms of CPU usage, but it requires a big amount of memory.

### Motion header

Like model files, the first `0x90` are reserved an always set to `00`. Offsets and file size ignores the first `0x90`. This document refers to the motion type `1`, which is the RAW one. When the motion type is `0`, the rest of the file after the header will be very different; refer to [interpolated motion document](motion.md).

| Offset | Type | Description
|--------|------|--------------
| 0      | int  | Motion 'type', 0=Interpolated, 1=Raw
| 4      | int  | Unknown. It can be `0` or `1`
| 8      | int  | Size of the file
| 12     | int  | Always 0

### Raw motion header

This is located straight after the motion header.

| Offset | Type | Description
|--------|------|--------------
| 0x00   | int  | Bone count. Must match with the bone count of the model.
| 0x04   | int  | Always 0
| 0x08   | int  | Always 0
| 0x0C   | int  | Always 0
| 0x10   | int  | Amount of GFR in a loop (`FrameEnd - FrameLoop`)
| 0x14   | int  | Total amount of frames, expressed in LFR
| 0x18   | int  | Unknown
| 0x1c   | int  | Offset to the [second matrix table](#raw-motion-more-matrices)
| 0x20   | vec4f | Bounding Box minimum
| 0x30   | vec4f | Bounding Box maximum
| 0x40   | float | Frame loop
| 0x44   | float | Frame end
| 0x48   | float | Frames per second, defines the LFR
| 0x4c   | float | Frame count, expressed in LFR

### Raw motion matrices

An array of array of 4x4 matrices (`Matrix4x4[][]`). The first dimension of the array refers to the total amount of frames, while the second dimension is the bone count. For each frame (LFR), the game engine takes BoneCount amount of `Matrix4x4` and multiplies them to the mesh that corresponds to a specific bone.

### Raw motion more matrices

This is optionally defined and its purpose is unknown. It is an array of 4x4 matrices (`Matrix4x4[]`), where the size of the array is equal to the bone count. For the few files that has been analysed, those matrices are always a Matrix Identity.

## Effect data

This file describe triggers that happens during a [motion](#motion-data) and it's believed they are located into a different file to favour decoupling.

### Effect main header

| Amount | Description |
|--------|---------------|
| Single |  [Header](#effect-entry-header)
| Array  |  [Type A effect](#Type-A-effect) (Happens during X frames)
| Array  |  [Type B effect](#Type-B-effect) (Triggered on a specific frame)

### Effect entry header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | byte | [Type A effect](#Type-A-effect) count
| 1      | byte | [Type B effect](#Type-B-effect) count
| 2      | short | Start offset of the type B effects

### Type A effect

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | short | Start frame
| 2      | short | End frame
| 4      | byte | [Effect A ID](#effect-a-list)
| 5      | byte | Param size as shorts
| 6      | short[] | Param

### Type B effect

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | short | Trigger frame
| 2      | byte | [Effect B ID](#effect-b-list)
| 3      | byte | Param size as shorts
| 4      | short[] | Param

### Effect A list

| ID | Effect | Parameter | Size
|--------|---------------|-------------|-------------|
| 0      | Allows controls (But brings to idle animaiton) | |
| 1      | Allows controls (But blocks the animation) | |
| 2      | Allows controls | |
| 3      | Blocks the animation (But disables gravity) | |
| 4      | Blocks controls (But allows gravity) | |
| 10     | Activates hitbox | ? | ?
| 20     | Performs a reaction command (On current model) | ? | ?
| 23     | Draws an additional texture | ? | ?
| 25     | Performs a reaction command (On another model) | ? | ?
| 27     | Makes invincible | |
| 30     | Blocks everything | |
| 34     | Blocks reaction command | |
| 41     | Allows controls (But disables model rotation) | |

### Effect B list

| ID | Effect | Parameter | Size
|----|---------------|-------------|-------------|
| 1  | Plays PAX sprite | PAX sprite ID | ?
| 2  | Plays footstep sound | Sound ID | ?
| 3  | Plays animation in slot 628 | |
| 13 | Plays an enemy vsb voice | vsb ID | ?
| 14 | Plays an ally vsb voice | vsb ID | ?
| 22 | Makes the keyblade appear | ? | ?
| 23 | Makes model opacity decrease | ? | ?
| 24 | Makes model opacity increase | ? | ?
| 26 | Makes a mesh disappear | ? | ?
| 27 | Makes a mesh appear | ? | ?
| 29 | Plays a Keyblade appearance sprite | |
