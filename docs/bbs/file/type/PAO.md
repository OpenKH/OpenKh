# PAO Format

PAO stands for *Player Attack Order*.

It controls the way a character performs combos for command styles.

These files are usually contained within `pXXinit.arc` files with XX being the respective PCXX player and it's stored as a `.bin`.

This file has no header. You stop reading data once you find an empty entry.

## PAO Info

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint8   | [Kind](#PAO-Kind)
| 0x1     | uint8  | Order Count (If this value is 0, the amount or Orders is 4)
| 0x2     | int16  | ID
| 0x4     | PAO Order[4]  | Order Values

### PAO Kind

| Value | Name  | Description
|--------|-------|------------
| 0     | KIND_NONE   | 
| 1     | KIND_BASE   | 
| 2     | KIND_BASEFIN   | 
| 3     | KIND_COMBO   | 
| 4     | KIND_FINISH   | 
| 255   | KIND_END   | 

---

## PAO Order

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16  | [Switch](#PAO-Switch)
| 0x2     | uint16  | [Case](#PAO-Case)

### PAO Switch

| Value | Name  | Description
|--------|-------|------------
| 0     | SWITCH_NONE  | 
| 1     | SWITCH_PC_POS  | 
| 2     | SWITCH_OLD_ATK  | 
| 3     | SWITCH_EN_POS  | 
| 4     | SWITCH_EN_AREA  | 
| 5     | SWITCH_NOW_ATK  | 
| 6     | SWITCH_EACH  | 
| 7     | SWITCH_ELSE  | 
| 8     | SWITCH_OLD_ATKID  | 
| 9     | SWITCH_COUNT  | 

### PAO Case

| Value | Name  | Description
|--------|-------|------------
| 0     | CASE_NONE    | 
| 1     | CASE_AREA_AERIAL    | 
| 2     | CASE_AREA_JUMP    | 
| 3     | CASE_AREA_MIDDLE    | 
| 4     | CASE_AREA_SIDE    | 
| 5     | CASE_AREA_UNDER    | 