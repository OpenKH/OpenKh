# PCAM Format

PCAM stands for *Player CAMera*.

It controls various aspects of a specific player's camera.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `@PCA`.
| 0x4     | uint32  | [Kind](#PCAM-Kind)
| 0x8     | float  | View Angle
| 0xC     | float  | Size

The next data chunk is repeated twice. Normal and Extended.

### PCAM Kind
| Value | Name  | Description
|--------|-------|------------
| 0     | PCAM_KIND_NONE   | 
| 1     | PCAM_KIND_NORMAL   | 
| 2     | PCAM_KIND_BOSS   | 
| 3     | PCAM_KIND_ACTION   | 
| 4     | PCAM_KIND_MAX   | 

---

## PCAM Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | [Type](#PCAM-Type)
| 0x4     | float   | Adjustment
| 0x8     | float   | Timer
| 0xC     | uint32   | [Flag](#PCAM-Flag)
| 0x10     | Vector4f   | Eye
| 0x20     | Vector4f   | Aim

### PCAM Type
| Value | Name  | Description
|--------|-------|------------
| 0     | PCAM_TYPE_NONE   | 
| 1     | PCAM_TYPE_FOLLOW   | 
| 2     | PCAM_TYPE_LOCKON   | 
| 3     | PCAM_TYPE_FOCUS   | 
| 4     | PCAM_TYPE_END   | 

### PCAM Flag

| Bit | Count | Description 
|-----|-------|-------------
|  0 | 1 | Watch
|  1 | 1 | Look Down
|  2 | 1 | Look Up
|  3 | 1 | Dummy
|  4 | 1 | Dummy
|  5 | 1 | State Change
|  6 | 1 | Dead Set
|  7 | 1 | Change Data
|  8 | 1 | Lock Adjust
|  9 | 1 | Event Transfer
| 10 | 1 | Look Back
| 11 | 1 | Boss Set
| 12 | 1 | Start Set
| 13 | 1 | Dir Control
| 14 | 1 | Fix Position
| 15 | 1 | Backshot On
| 16 | 1 | Shotlock On
| 17 | 1 | Control Shotlock
| 18 | 1 | Control Lock On Dir
| 19 | 1 | Ex Control Reset
| 20 | 1 | Reset Mode
| 21 | 1 | Aim Near
| 22 | 1 | Object Hit
| 23 | 1 | Map Hit
| 24 | 1 | Auto Behind Off
| 25 | 1 | Debug Reset
| 26 | 1 | Ex Control
| 27 | 1 | Player Fix Dir
| 28 | 1 | Player Fix Pos
| 29 | 1 | Control
| 30 | 1 | Reset
| 31 | 1 | Stop