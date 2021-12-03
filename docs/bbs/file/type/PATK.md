# PATK Format

PATK stands for *Player ATtacK*.

It controls various aspects of attack data.

The file can be located in `arc/pc/p00common.arc` as `PAtkData.bin`.

This file has no header.

## PATK Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int16   | frPlayEnd
| 0x2     | int16   | Group Effect
| 0x4     | uint16  | [Flag](#PATK-Flag)
| 0x6     | uint16  | Dummy
| 0x8     | uint8   | Animation 1
| 0x9     | uint8   | Animation 2
| 0xA     | uint8   | Animation 3
| 0xB     | uint8   | Animation 4
| 0xC     | uint8   | frCombo Enable
| 0xD     | uint8   | frChange Enable
| 0xE     | uint8   | SE Group
| 0xF     | uint8   | Dummy
| 0x10    | uint8   | Group Attack 1
| 0x11    | uint8   | Group Attack 2
| 0x12    | uint8   | Group Attack 3
| 0x13    | uint8   | Group Attack 4
| 0x14    | uint8   | frTrigger 1
| 0x15    | uint8   | frTrigger 2
| 0x16    | uint8   | frTrigger 3
| 0x17    | uint8   | frTrigger 4
| 0x18    | uint8   | Bullet
| 0x19    | uint8   | Camera
| 0x1A    | uint8   | Attack Power
| 0x1B    | uint8   | Attack Attribute
| 0x1C    | uint8   | frMark Start
| 0x1D    | uint8   | frMark End
| 0x1E    | uint8   | frMove Start
| 0x1F    | uint8   | frMove End
| 0x20    | uint8   | Maximum Distance
| 0x21    | uint8   | Translation
| 0x22    | uint8   | Range
| 0x23    | uint8   | Speed
| 0x24    | uint8   | Rate
| 0x25    | uint8   | Ex Dash
| 0x26    | uint8   | Ex Rise
| 0x27    | uint8   | Dummy

### PATK Flag

| Value | Name  | Description
|--------|-------|------------
| 1     | MUTEKI  | Invincibility
| 2     | AERIAL  | 
| 4     | GROUND  | 
| 8     | EXMOVE  | 
| 16    | RISE  | 
| 32    | DOWN  | 
| 64    | BULLET  | 
| 128   | NOSCALE  | 
| 256   | SLIDE  | 
| 512   | WARP  | 
| 1024  | CHAIN  | 
| 2048  | START  | 
| 4096  | NEXT  | 
| 8192  | FLOAT  | 