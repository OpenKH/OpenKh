# L2D Format

L2D stands for *Layout 2 Dimensional*.

This file type contains all types of menus or interactible 2D widgets.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `L2D@`.
| 0x4     | char[4]   | Version
| 0x8     | char[8]   | Date
| 0x10    | char[4]   | Name
| 0x14    | uint8[4]  | Reserved
| 0x18    | uint8[8]  | Reserved
| 0x20    | int32     | [SQ2P](#SQ2P-Header) Count
| 0x24    | int32     | [SQ2P](#SQ2P-Header) Offset
| 0x28    | int32     | [LY2](#LY2-Header) Offset
| 0x2C    | int32     | File Size
| 0x30    | uint8[16] | Reserved

## SQ2P Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `SQ2P`.
| 0x4     | char[4]   | Version
| 0x8     | uint8[8]  | Reserved
| 0x10    | uint32    | SP2 Offset
| 0x14    | uint32    | SQ2 Offset
| 0x18    | uint32    | TM2 Offset
| 0x1C    | uint8[36] | Reserved

### SP2 Header

SP stands for **sprite**.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `SP2@`.
| 0x4     | char[4]   | Version
| 0x8     | uint8[8]  | Reserved
| 0x10    | int32     | [Parts](#SP2-Parts) Count
| 0x14    | int32     | [Parts](#SP2-Parts)  Offset
| 0x18    | int32     | [Group](#SP2-Group) Count
| 0x1C    | int32     | [Group](#SP2-Group) Offset
| 0x20    | int32     | [Sprite](#SP2-Sprite) Count
| 0x24    | int32     | [Sprite](#SP2-Sprite) Offset
| 0x28    | int8[24]  | Reserved

#### SP2 Parts

| Offset | Type   | Description
|--------|--------|------------
| 0x0    | int16  | U0
| 0x2    | int16  | V0
| 0x4    | int16  | U1
| 0x6    | int16  | V1
| 0x8    | uint32 | RGBA 0
| 0xC    | uint32 | RGBA 1
| 0x10   | uint32 | RGBA 2
| 0x14   | uint32 | RGBA 3

#### SP2 Group

| Offset | Type   | Description
|--------|--------|------------
| 0x0    | int16  | X0
| 0x2    | int16  | Y0
| 0x4    | int16  | X1
| 0x6    | int16  | Y1
| 0x8    | uint16 | IDX Parts
| 0xA    | uint16 | Attribute

##### Group Attribute

| Value  | Name   | Description
|--------|--------|------------
| 0x100  | ATTR_XYUV  | 
| 0x200  | ATTR_SCISSOR_ON  | 
| 0x400  | ATTR_SCISSOR_OFF  | 

#### SP2 Sprite

| Offset | Type   | Description
|--------|--------|------------
| 0x0    | uint16 | Group Value
| 0x2    | uint16 | Group IDX

---

## SQ2 Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `SQ2@`.
| 0x4     | char[4]   | Version
| 0x8     | uint8[8]  | Reserved
| 0x10    | int32     | [Sequence](#SQ2-Sequence) Count
| 0x14    | int32     | [Sequence](#SQ2-Sequence) Offset
| 0x18    | int32     | [Control](#SQ2-Control) Count
| 0x1C    | int32     | [Control](#SQ2-Control) Offset
| 0x20    | int32     | [Animation](#SQ2-Animation) Count
| 0x24    | int32     | [Animation](#SQ2-Animation) Offset
| 0x28    | int32     | [Key](#SQ2-Key) Count
| 0x2C    | int32     | [Key](#SQ2-Key) Offset
| 0x30    | int32     | Sequence Name Offset
| 0x34    | int32     | Sequence ID Offset
| 0x38    | int8[8]   | Reserved

### SQ2 Sequence

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Control IDX
| 0x4    | int16 | Control Number
| 0x6    | int16 | Layer Number

### SQ2 Control

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Max Frame
| 0x4    | int32 | Return Frame
| 0x8    | int32 | Animation IDX
| 0xC    | int16 | Loop Number
| 0xE    | int16 | Animation Number

### SQ2 Animation

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Max Frame Number
| 0x4    | int16 | Sprite Number
| 0x6    | int16 | nOfsKeyData
| 0x8    | char[11] | Key Data Array
| 0x13   | uint8 | [Kind](#Anim-Kind)
| 0x14   | uint8 | [Blend Type](#Anim-Blend-Type)
| 0x15   | uint8 | [Anim Bitflag](#Anim-Bitflag)
| 0x16   | uint8 | Scissor Number
| 0x17   | uint8 | Z-Depth

#### Anim Kind

| Value  | Name | Description
|--------|-------|------------
| 0      | KindParent | 
| 1      | KindNormal | 
| 2      | KindFont | 
| 3      | KindMax | 

#### Anim Blend Type

| Value  | Name | Description
|--------|-------|------------
| 0      | BlendBlend | No blending?
| 1      | BlendAdd | Blend by addition
| 2      | BlendSub | Blend by subtraction

#### Anim Bitflag

| Bit    | Count | Name
|--------|-------|------------
| 0      | 6 | Dummy
| 6      | 1 | Dither Off
| 7      | 1 | Bilinear

### SQ2 Key

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Key
| 0x4    | int32 | Value
| 0x8    | uint8[4] | Data

#### SQ2 Key Value

| Value  | Name  | Description
|--------|-------|------------
| 0xFF808080    | DefColor | 
| 0x3f800000    | DefScaleX |
| 0x3f800000    | DefScaleY |
| 0x0    | DefBaseY |
| 0x0    | DefRotateY |
| 0x0    | DefRotateX |
| 0x0    | TypeLinear |
| 0x0    | DefRotateZ |
| 0x0    | KindStatus |
| 0x0    | DefStatus |
| 0x0    | DefBaseX |
| 0x0    | DefOffsetX |
| 0x0    | DefOffsetY |
| 0x1    | KindBaseX |
| 0x2    | KindBaseY |
| 0x3    | KindOffsetX |
| 0x4    | KindOffsetY |
| 0x5    | KindRotateX |
| 0x6    | KindRotateY |
| 0x7    | KindRotateZ |
| 0x8    | KindScaleX |
| 0x9    | KindScaleY |
| 0xA    | KindColor |
| 0xB    | KindMax |
| 0x40   | TypeSpline |
| 0x80   | TypeSway |
| 0xC0   | TypePoint |
| 0xC1   | TypeMax |

## LY2 Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `SQ2P`.
| 0x4     | char[4]   | Version
| 0x8     | uint8[8]  | Reserved
| 0x10    | int32     | [Layout](#LY2-Layout) Count
| 0x14    | int32     | [Layout](#LY2-Layout) Offset
| 0x18    | int32     | Control Count
| 0x1C    | int32     | Control Offset
| 0x20    | int32     | [Node](#LY2-Node) Count
| 0x24    | int32     | [Node](#LY2-Node) Offset
| 0x28    | int32     | [Font Info](#LY2-Font-Info) Count
| 0x2C    | int32     | [Font Info](#LY2-Font-Info) Offset
| 0x30    | int32     | String Count
| 0x34    | int32     | String Offset
| 0x38    | int32     | Layout Name Offset
| 0x3C    | int32     | Layout ID Offset

### LY2 Layout

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Max Frame
| 0x4    | int32 | Node IDX
| 0x8    | int16 | Node
| 0xA    | int16 | Layer
| 0xC    | int16 | X
| 0xE    | int16 | Y

### LY2 Node

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Max Frame
| 0x4    | int16 | SQ2 Base Number
| 0x6    | int16 | Sequence IDX
| 0x8    | int8  | Affect Translation
| 0x9    | int8  | Affect Color
| 0xA    | int8  | Reserve
| 0xB    | int8  | Reserve
| 0xC    | int32 | Font Info
| 0x10   | int32 | Font Info IDX
| 0x14   | int16 | Parent IDX
| 0x16   | int16 | X
| 0x18   | int16 | Y
| 0x1A   | int16 | ID
| 0x1C   | int32 | Reserve

### LY2 Font Info

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | String IDX
| 0x4    | int32 | Font Color
| 0x8    | int8  | Font Size
| 0x9    | int8  | Font Kind
| 0xA    | int8  | Font Center
| 0xB    | int8  | Font Type
| 0xC    | int32 | Reserve