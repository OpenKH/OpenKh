# TXA Format 

TXA stands for *TeXture Animation*.

This file contains the list of possible states a model's textures can take. This is usually used to change the facial expression of a low poly version of a character.

Fields marked `Reserved` are used by the engine and should be 0 in the file.

## Header 

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `TXA`. Null terminated.
| 0x4     | uint16  | File version `1`
| 0x6     | uint16  | Group Count
| 0x8     | uint32[2]  | Padding


## Anim Group

The Anim Groups immediatly follow the header.

| Offset | Type  | Description
|--------|-------|------------
| 0x0 | char[16] | Group Name
| 0x10 | char[24] | Destination Texture Name
| 0x28 | uint32 | Reserved
| 0x2C | int16 | Destination Height
| 0x2E | int16 | Destination Width
| 0x30 | int16 | Anim Count
| 0x32 | int16 | Default Anim
| 0x34 | uint32 | Offset of Anim Data

## Anim

| Offset | Type  | Description | Notes
|--------|-------|-------------|------
| 0x0 | char[16] | Anim Name |
| 0x10 | int16 | Unkown | Seems to always be -1 * frame count
| 0x12 | int16 | Frame Count |
| 0x14 | uint32 | Offset of Frame Data |

## Frame

| Offset | Type  | Description | Notes
|--------|-------|-------------|------
| 0x0 | uint32 | Offset of Pixel Data | If this is zero, this frame is the original texture
| 0x4 | int16 | Unknown | These 2 seem to be related to how long the frame lasts
| 0x6 | int16 | Unknown |
| 0x8 | int8 | Reserved |
| 0x9 | uint8 | Reserved |
| 0xA | int16 | Padding |

## Usage

A frame is applied by more or less pasting it's pixel data over the destination texture. Note that the destination texture is needed to know the pixel format and thus the size of the data.

Either square textures or "LIP" animations (it is not currently certain which, although it's assumed to be "square textures") are 'twiddled' in a simmilar way that the mtx file from a [Font Arc](../../font.md) is.