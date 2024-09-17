# EDP Format

EDP stands for *Experience Data Parameters* and it controls all of the changes made when enabling EXP 0 in game.

# Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | string   | File identifier, always `@EDP`
| 0x4     | uint32   | Version, `2`
| 0x8     | uint32   | Enemy Data Count
| 0xC     | uint32   | Boss Data Count

# Attack Type Elemental

| Offset | Type  | Description
|--------|-------|------------
| 0x0   | [AttackElement[152]](#attack-element) | List of attack elements.

## Attack Element

| Bit | Count | Description 
|-----|-------|-------------
|  0 | 4 | [Element 1](#attack-type)
|  4 | 4 | [Element 2](#attack-type)

## Attack Type

| Value  | Name  | Description
|--------|-------|------------
| 0x0    | ATK_TYPE_LOW  | 
| 0x1    | ATK_TYPE_MIDDLE  | 
| 0x2    | ATK_TYPE_HIGH  | 
| 0x3    | ATK_TYPE_MAX  | 


# Cause Damage Pack Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | char[4]  | Code, always `@CDP`
| 0x4    | uint8  | ID
| 0x5    | uint8  | UseBoss
| 0x6    | uint8[2]  | RESERVED
| 0x8    | [HPRange](#cause-damage-pack-hp-range)  | HP Range
| 0x10   | [ScalePack[3]](#cause-damage-pack-scale-pack)  | Damage Scaling


# Cause Damage Pack HP Range

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32    | Minimum
| 0x4    | int32    | Maximum

# Cause Damage Pack Scale Pack

This structures repeats for as many animations need their parameters set.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int32  | Divisor
| 0x4     | int32  | Revision
| 0x8     | int32  | Scale