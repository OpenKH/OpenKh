# ABC Format

ABC stands for *Attach Body Collision*.

It's responsible for collisions the model itself has to perform.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `@ABC`.
| 0x4     | uint32   | Version, `3`
| 0x8     | int32   | ABC Data Count
| 0xC     | uint32   | Pointer to ABC Data

## ABC Extra Header

This extra header contains a definition of all bones used.

However, the data structure is unknown.

It seems to be `char[128]` except for the first entry which is `char[32]`.

## ABC Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32    | Pointer to Bone 1
| 0x4     | Vector3f  | Offset 1
| 0x10    | uint32    | Pointer to Bone 2
| 0x14    | Vector3f  | Offset 2
| 0x20    | float     | Radius
| 0x20    | uint32    | [ABC Flag](#ABC-Flag)
| 0x20    | uint32    | Pointer to Effect FEP
| 0x20    | uint32    | Pointer to Effect Name

### ABC Flag

| Value | Count  | Description
|--------|-------|------------
| 0     | 8   | Padding
| 8     | 4   | Attack Number
| 12    | 8   | Attack Kind
| 20    | 4   | Collision Shape
| 24    | 4   | Collision Kind
| 28    | 1   | Attack
| 29    | 1   | Heavy
| 30    | 1   | Body
| 31    | 1   | Damage

