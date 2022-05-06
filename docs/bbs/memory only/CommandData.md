# Command Data

It controls parameters related to command data.

This data is located with the game's executable.

## Command Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint8  | Type
| 0x1    | uint8  | Category
| 0x2    | uint8  | Attribute
| 0x3    | uint8  | Sub-Category
| 0x4    | uint8  | Class & Cost
| 0x5    | uint8  | Flag
| 0x6    | uint8  | Dummy
| 0x7    | uint8  | Local
| 0x8    | uint32 | Pointer to Name
| 0xC    | uint32 | Pointer to Resource

## Command Class-Cost

| Bit    | Count | Description
|--------|-------|------------
| 0      | 4     | Class
| 4      | 4     | Cost