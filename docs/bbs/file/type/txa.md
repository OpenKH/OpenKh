# TXA Format 

TXA stands for *TeXture Animation*.

This file contains the list of possible states a model's textures can take. This is usually used to change the facial expression of a low poly version of a character.

## Header 

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `TXA`. Null terminated.
| 0x4     | uint16  | File version `1`
| 0x6     | uint16  | Count
| 0x8     | uint32[2]  | Padding


## TXA Data (Type 0)

The way this data structure is read is still unknown.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[16]   | Expression Name
| 0x10    | char[24]   | Destination Texture Name
| 0x28    | TEXANMSURFACES   | Pointer to Texture Data
| 0x10    | int16   | Destination Height
| 0x10    | int16   | Destination Width
| 0x10    | int32   | Clut Offset
| 0x10    | uint32   | Pointer to Data Clut
| 0x10    | int32   | Destination Width-Height
| 0x10    | uint32   | Count
| 0x10    | uint32   | Definition Count
| 0x10    | uint32   | Offset
| 0x10    | uint32   | Pointer to Texture Data (Type 1)

## Texture Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Magic Value
| 0x0     | int32   | Count
| 0x0     | uint32   | Pointer to Texture Entry

Texture Entries are unknown.