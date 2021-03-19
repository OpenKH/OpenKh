# EXB Format

EXB stands for *EXcel Binary*.

These files contain parameter data for various uses, but mainly for minigames.

This is the full list of files that use this format:
- WmRidParams.exb
- MgRbParams.exb
- MgRrBullet.exb
- MgRrParams.exb
- MgRrRsrc.exb

There's only one instance of this file, included inside `arc/gimmick/gimcommon.arc` in the subfile `GiPrdrda.gpd`.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int32    | Identifier, always `EXBN`.
| 0x4     | int32    | Version
| 0x8     | uint32   | Mapping to Binary Address
| 0xC     | int32    | String Key Word
| 0x10    | int32    | Sheet Count
| 0x14    | int32    | [Sheet Info](#Sheet-Info) Table Offset
| 0x18    | int32    | Padding
| 0x1C    | int32    | Padding

## Sheet Info

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32    | [Data Layout](#Data-Layout)
| 0x4    | int32    | [Sheet Cell Info](#Sheet-Cell-Info)
| 0xC    | int32    | Row Count
| 0x10    | uint32   | [Data Info](#Data-Info) Offset
| 0x14   | uint32   | Data Offset Table Offset
| 0x18   | uint32   | Sheet Name Offset
| 0x1C   | int32    | Header Skip Count
| 0x20   | int32    | Padding

### Data Layout

| Value  | Name  | Description
|--------|-------|------------
| 0      | TATE_RECORD    | 
| 1      | YOKO_RECORD    | 
| 2      | NONE_RECORD    | 

### Sheet Cell Info

**This data conflicts with the file's.**

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32    | Column Count
| 0x4    | int32    | Cell Count

## Data Info

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32    | [Data Type](#Data-Type)
| 0x4    | int32    | Column Position
| 0x8    | int32    | Row Position
| 0xC    | int32    | Offset
| 0x10   | int32    | ID

### Data Type

| Value  | Name  | Description
|--------|-------|------------
| 0      | TypeSByte    | 
| 1      | TypeByte    | 
| 2      | TypeShort    | 
| 3      | TypeUShort    | 
| 4      | TypeInt    | 
| 5      | TypeUInt    | 
| 6      | TypeFloat    | 
| 7      | TypeString    | 