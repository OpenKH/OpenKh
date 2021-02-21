# Weapon Format

It controls parameters related to the weapon's stats.

This data is located with the game's executable.

## Weapon Info

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int8  | Strength
| 0x1     | int8  | Magic
| 0x2     | uint8 | Critical Rate (Percentage)
| 0x3     | uint8 | Critical Damage (Divided by 100)
| 0x4     | uint8 | Chain?
| 0x5     | int8  | Offset X
| 0x6     | int8  | Offset Y
| 0x7     | int8  | Offset Z
| 0x8     | int8  | Degree Z
| 0x9     | uint8 | Reach
| 0xA     | uint8 | Hit Stars
| 0xB     | uint8 | SCD File ID