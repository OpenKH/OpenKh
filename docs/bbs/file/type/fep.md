# FEP Format

FEP stands for *Funny Effects for Psp*.

This format contains particle effects.



## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | char[3]  | File identifier, always `FEP`.
| 0x3    | uint8    | Reserved
| 0x4    | int16    | Major Version
| 0x6    | int16    | Minor Version
| 0x8    | uint32   | Pointer to List of [FED Version Check](#FED-Version-Check) Pointers
| 0xC    | int32    | Flag
| 0x10   | int32    | FED Unique ID
| 0x14   | int32    | Size
| 0x18   | uint16   | Mode
| 0x1A   | uint16   | Zone
| 0x1C   | uint16   | fer_C
| 0x1E   | uint16   | ed_C
| 0x20   | uint16   | Texture Count
| 0x22   | uint16   | Model Count
| 0x24   | uint16   | Animation Count
| 0x26   | uint16   | Vertex Count
| 0x28   | uint32   | [FED FER Data](#FED-FER-Data) Pointer
| 0x2C   | uint32   | [FED EFFECT Data](#FED-EFFECT-Data) Pointer
| 0x30   | uint32   | [FED Texture Resource](#FED-RESOURCE-HEADER) Pointer
| 0x34   | uint32   | [FED Model Resource](#FED-RESOURCE-HEADER) Pointer
| 0x38   | uint32   | [FED Animation Resource](#FED-RESOURCE-HEADER) Pointer
| 0x3C   | uint32   | [FED Vertex List Resource](#FED-RESOURCE-HEADER) Pointer
| 0x40   | uint32   | [FED Leaves Header](#FED-Leaves-Header) Pointer
| 0x44   | uint32   | FED String ID Pointer

---

# FED Version Check

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint8   | csbp_c
| 0x1    | uint8   | cswp_c
| 0x2    | uint8   | necessity_c
| 0x3    | uint8   | res
| 0x4    | uint16  | csbp_size
| 0x6    | uint16  | cswp_size

# FED FER Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32   | Unique ID
| 0x4    | uint16   | ed Index
| 0x6    | uint16   | [Leaf](#FED-Leaf) Count
| 0x8    | uint32   | [Leaf](#FED-Leaf) Pointer

### FED Leaf

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32   | Unique ID
| 0x4    | uint16  | Flag
| 0x6    | uint8   | EA
| 0x7    | uint8   | Fog Mode
| 0x8    | uint8   | Mode
| 0x9    | uint8   | XYZ
| 0xA    | uint8   | Clip Mode
| 0xB    | uint8   | Fadeout Mode
| 0xC    | uint32  | Data Pointer
| 0x10   | Vector4 | Position
| 0x20   | Vector4 | Rotation
| 0x30   | Vector4 | Scale
| 0x40   | Vector4 | Clip Size
| 0x50   | Vector4 | Clip Offset
| 0x60   | Vector4 | EA Rotation
| 0x70   | Vector4 | EA Range
| 0x80   | Vector4 | EA Range 0
| 0x90   | uint16  | BG ID
| 0x90   | uint8   | Fog ID
| 0x90   | uint8   | Padding
| 0x90   | uint16  | Wait Frame
| 0x90   | uint16  | Draw Group
| 0x90   | float   | Fadeout Near
| 0x90   | float   | Fadeout Far
| 0x90   | uint32  | Offset

# FED EFFECT Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32   | Flag
| 0x4    | int32   | Total Frames
| 0x8    | uint16  | Loop Live Frame
| 0xA    | uint8   | Draw Sort
| 0xB    | uint8   | Draw Sort Depth
| 0xC    | uint32  | Loop Exit Type
| 0x10   | uint16  | Sequence Count
| 0x12   | uint16  | UD Count
| 0x14   | uint16  | CSI Count
| 0x16   | uint16  | Layer Count
| 0x18   | uint16  | FD Count
| 0x1A   | uint16  | FDCSDI Count
| 0x1C   | uint32  | [FED EFFECT SEQ Data](#FED-EFFECT-SEQ-Data) Pointer
| 0x20   | uint32  | [FED UNIT Data](#FED-UNIT-Data) Pointer
| 0x24   | uint32  | PCSI List Pointer (uint16 list)
| 0x28   | uint32  | [FED EFFECT LAYER Data](#FED-EFFECT-LAYER-Data) Pointer
| 0x2C   | uint32  | [FED FOLDER Data](#FED-FOLDER-Data) Pointer
| 0x30   | uint32  | PPFDCSDI Pointer (uint32 list)

### FED EFFECT SEQ Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int16  | Rel Frame
| 0x2    | int16  | UD Count
| 0x4    | uint32 | UD List Pointer (uint16 list)

### FED UNIT Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int16  | Flag
| 0x2    | int16  | Worksize
| 0x4    | uint16 | Total Frames
| 0x6    | uint16 | Unit Number
| 0x8    | uint8  | CSD Count
| 0x9    | uint8  | Process 2
| 0xA    | uint8  | W Allign
| 0xB    | uint8  | Coordinate
| 0xC    | uint16 | Loop Start Frame
| 0xE    | uint16 | Loop End Frame
| 0x10   | uint8  | Draw Sort
| 0x11   | uint8  | Draw Sort Depth
| 0x12   | uint8  | Construct Count
| 0x13   | uint8  | Anchor Count
| 0x14   | uint8  | Calc Count
| 0x15   | uint8  | Draw Count
| 0x16   | uint8  | Destruct Count
| 0x17   | uint8  | Loop Count
| 0x18   | uint8  | Debug Count
| 0x19   | uint8  | Zero Count
| 0x1A   | uint8  | Nesting Count
| 0x1B   | uint8  | Exception Count
| 0x1C   | uint32 | [FED CS Data](#FED-CS-Data) Pointer
| 0x20   | uint32 | Construct [FED FRAME DATA](#FED-FRAME-Data) Pointer
| 0x24   | uint32 | Anchor [FED FRAME DATA](#FED-FRAME-Data) Pointer
| 0x28   | uint32 | Calc Pointer (uint16 Data)
| 0x2C   | uint32 | Draw Pointer (uint16 Data)
| 0x30   | uint32 | Destruct Pointer (uint16 Data)
| 0x34   | uint32 | Loop Pointer (uint16 Data)
| 0x38   | uint32 | Debug Pointer (uint16 Data)
| 0x3C   | uint32 | Zero Pointer (uint16 Data)
| 0x40   | uint32 | Nesting Pointer (uint16 Data)
| 0x44   | uint32 | Exception Pointer (uint16 Data)

### FED CS Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16  | CS Prog
| 0x2    | uint16  | Work Off
| 0x4    | uint32  | Necessity Count
| 0x8    | uint16  | Necessity Pointer

### FED FRAME Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int16   | Rel Frame
| 0x2    | int16   | Live Frame
| 0x4    | uint16  | CSD Count
| 0x6    | uint32  | CSD List Pointer ([FUDF9](#FUDF9) Type)


### FED EFFECT LAYER Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint32  | Texture Count
| 0x4    | uint32  | Texture Pointer (int16 Data)

### FED FOLDER Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16  | UD Count
| 0x0    | uint16  | Padding
| 0x0    | uint32  | UD List Pointer ([FFD9](#FFD9) Type)


# FED RESOURCE HEADER

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float   | Scale
| 0x4    | int32   | Unique ID
| 0x8    | int32   | Size
| 0xC    | uint32   | Data Pointer
| 0x10   | int32   | Handle
| 0x14   | int32   | Flag


# FED Leaves Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16   | Leaves Count
| 0x2    | uint16   | Res
| 0x4    | uint32   | [Leaves Data](#FED-Leaves-Data) Pointer

# FED Leaves Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32    | Unique ID
| 0x4    | uint16   | Flag
| 0x6    | uint16   | Leaf Count
| 0x8    | uint32   | Leaf ID Pointer

# Other Data Types

### FUDF9

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16   | CSD Index
| 0x2    | uint16   | Base Offset
| 0x4    | int16    | Rel Frame
| 0x6    | int16    | Live Frame

### FFD9

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16   | Rel Frame
| 0x2    | uint16   | UD Index

---

These following data structure are unknown as to where you use them.

# Effect Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint32   | ID
| 0x4    | uint32   | Padding
| 0x8    | uint32   | Flags
| 0xC    | uint16   | Version
| 0xE    | uint16   | Count

## Effect Data One

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16   | Count
| 0x2    | uint16   | File
| 0x4    | uint8    | Fade Time
| 0x5    | uint8    | Padding
| 0x6    | uint16   | Group

## Effect Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16   | [Schedule Type](#Schedule-Type)
| 0x2    | uint32   | Schedule Line Pointer
| 0x6    | uint32   | Effect Track Count

### Schedule Type

| Value | Name  | Description
|--------|-------|------------
| 0      | TYPE_ROOT   | 
| 1      | TYPE_SYSTEM   | 
| 2      | TYPE_CHARACTOR   | 
| 3      | TYPE_MESSAGE   | 
| 4      | TYPE_EFFECT   | 
| 5      | TYPE_MAP   | 
| 6      | TYPE_CAMERA   | 
| 7      | TYPE_BGM   | 
| 8      | TYPE_SE   | 
| 9      | TYPE_VOICE   | 
| 10     | TYPE_SYS_READ   | 
| 11     | TYPE_SYS_SPEED   | 
| 12     | TYPE_WALL_TEX_0   | 
| 13     | TYPE_WALL_FADE_0   | 
| 14     | TYPE_WALL_TEX_1   | 
| 15     | TYPE_WALL_FADE_1   | 
| 16     | TYPE_MAP_AREA   | 
| 17     | TYPE_MAP_FADE   | 
| 18     | TYPE_MAP_BLUR   | 
| 19     | TYPE_MAP_BLUR_POS_X   | 
| 20     | TYPE_MAP_BLUR_POS_Y  | 
| 21     | TYPE_MAP_BLUR_X  | 
| 22     | TYPE_MAP_BLUR_Y  | 
| 23     | TYPE_MAP_BLUR_ROT  | 
| 24     | TYPE_MAP_BLUR_ALPHA  | 
| 25     | TYPE_MAP_FOG  | 
| 26     | TYPE_MAP_FOG_NEAR  | 
| 27     | TYPE_MAP_FOG_FAR  | 
| 28     | TYPE_MAP_FOG_COLOR  | 
| 29     | TYPE_MAP_FLAG  | 
| 30     | TYPE_CAM_DATA  | 
| 31     | TYPE_CAM_CLIP  | 
| 32     | TYPE_CHARA_NAME | 
| 33     | TYPE_CHARA_HIDE  | 
| 34     | TYPE_CHARA_FLAG  | 
| 35     | TYPE_CHARA_MOTION  | 
| 36     | TYPE_CHARA_ATTACH  | 
| 37     | TYPE_CHARA_TRANS  | 
| 38     | TYPE_CHARA_ROTATE  | 
| 39     | TYPE_CHARA_SCALE  | 
| 40     | TYPE_CHARA_SHADOW  | 
| 41     | TYPE_CHARA_NECK  | 
| 42     | TYPE_CHARA_FADE  | 
| 43     | TYPE_EFFECT_TRACK  | 
| 44     | TYPE_MES_LAYER_TOP  | 
| 45     | TYPE_MES_LAYER_L2  | 
| 46     | TYPE_MES_LAYER_L1  | 
| 47     | TYPE_MES_LAYER_L0  | 
| 48     | TYPE_BGM_TRACK_0  | 
| 49     | TYPE_BGM_VOL_0  | 
| 50     | TYPE_BGM_TRACK_1  | 
| 51     | TYPE_BGM_VOL_1  | 
| 52     | TYPE_SE_TRACK_0  | 
| 53     | TYPE_SE_TRACK_1  | 
| 54     | TYPE_SE_TRACK_2  | 
| 55     | TYPE_SE_TRACK_3  | 
| 56     | TYPE_VOICE_MOT  | 
| 57     | TYPE_VOICE_TRACK_0  | 
| 58     | TYPE_VOICE_TRACK_1  | 
| 59     | TYPE_VOICE_TRACK_2  | 
| 60     | TYPE_VOICE_TRACK_3  | 
| 61     | TYPE_CUSTOM_TRACK  | 
| 62     | TYPE_MAX  | 
