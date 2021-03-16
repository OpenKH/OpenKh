# ECM Format

ACM stands for *Exusia CaMera*.

This file controls how camera moves or behaves. Mostly for NPCs.

# ECM Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[16]   | Model Name
| 0x10    | int32      | End Frame
| 0x14    | uint8      | [Action Flag](#EXUSIA-CAMERA-ACTION-FLAG)
| 0x15    | uint8      | Parameter 0
| 0x16    | uint8      | Parameter 1
| 0x17    | uint8      | Shadow Effect Range
| 0x18    | [Camera Curve](#CAMERA-CURVE)     | Translation X
| 0x28    | [Camera Curve](#CAMERA-CURVE)     | Translation Y
| 0x38    | [Camera Curve](#CAMERA-CURVE)     | Translation Z
| 0x48    | [Camera Curve](#CAMERA-CURVE)     | Eye X
| 0x58    | [Camera Curve](#CAMERA-CURVE)     | Eye Y
| 0x68    | [Camera Curve](#CAMERA-CURVE)     | Eye Z
| 0x78    | [Camera Curve](#CAMERA-CURVE)     | Roll
| 0x88    | [Camera Curve](#CAMERA-CURVE)     | Projection
| 0x98    | [Camera Curve](#CAMERA-CURVE)     | Focus Near
| 0xA8    | [Camera Curve](#CAMERA-CURVE)     | Focus Far

## CAMERA CURVE

| Offset | Type     | Description
|--------|----------|------------
| 0x0    | int32    | Camera Point Count
| 0x4    | uint32   | Pointer to [Camera Point Data](#CAMERA-POINT)

Camera Point usually follows this small data.

## CAMERA POINT

| Offset | Type     | Description
|--------|----------|------------
| 0x0    | [Camera Hand](#CAMERA-HAND)    | Point
| 0x8    | [Camera Hand](#CAMERA-HAND)    | Left Hand
| 0x10   | [Camera Hand](#CAMERA-HAND)    | Right Hand

## CAMERA HAND

| Offset | Type     | Description
|--------|----------|------------
| 0x0    | float    | Value 1
| 0x4    | float    | Value 2 (Unknown use)

## EXUSIA CAMERA ACTION FLAG

| Value | Name         | Description
|-------|--------------|------------
| 0     | EXUSIA_CAM_ACTION_FLAG_NONE  | 
| 1     | EXUSIA_CAM_ACTION_FLAG_RELATIVE   | 
| 2     | EXUSIA_CAM_ACTION_FLAG_VALID_FOCUS_FAR   |
| 4     | EXUSIA_CAM_ACTION_FLAG_VALID_FOCUS_NEAR   |
| 8     | EXUSIA_CAM_ACTION_FLAG_VALID_INTERPOLATION   |
| 16    | EXUSIA_CAM_ACT_ENABLE_MAP_COLLISION   |
| 32    | EXUSIA_CAM_ACT_ENABLE_OBJ_COLLISION   |
| 64    | EXUSIA_CAM_ACT_RELATIVE_SEARCH_OF_CHARID  |
| 128   | EXUSIA_CAM_ACT_BUG_FIX  |

## EXUSIA CAMERA FLAG

| Value | Name         | Description
|-------|--------------|------------
| 1     | EXUSIA_CAM_FLAG_AUTO_KILL   | 
| 2     | EXUSIA_CAM_FLAG_ENABLE_COLLISION  |

## EXUSIA CAMERA FLAG

| Value | Name         | Description
|-------|--------------|------------
| 0     | EXUSIA_CAM_STATE_NONE   | 
| 1     | EXUSIA_CAM_STATE_RUN   |
| 2     | EXUSIA_CAM_STATE_INTERPOLATION   |
| 3     | EXUSIA_CAM_STATE_STOP   |
| 4     | EXUSIA_CAM_STATE_END   |