# PPM Format

PPM stands for *Player ParaMeter*.

It controls parameters related to the player's movement.

These files are usually contained within `pXXinit.arc` files with XX being the respective PCXX player and it's stored as a `.bin`.

## PPM Info

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | float  | Walk Speed Max
| 0x4     | float  | Run Speed Max
| 0x8     | float  | Weight
| 0xC     | float  | Jump Power
| 0x10    | float  | Jump Max Height
| 0x14    | float  | Body Collision Radius
| 0x18    | float  | Attack Collision Radius
| 0x1C    | float  | Center Rise Offset
| 0x20    | float  | Map Collision Radius
| 0x24    | float  | Map Collision Shift Y
| 0x28    | float  | Hang Shift Y
| 0x2C    | float  | Body Collision Weight
| 0x30    | uint  | Start Max HP
| 0x34    | uint  | Dummy
| 0x38    | uint  | Dummy
| 0x3C    | uint  | Dummy