# AAC Format

AAC stands for *Attach Attack Collision*.

Responsible for collisions attached to a character's weapon or anything else that needs to perform a collision check.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `@AAC`.
| 0x4     | uint32   | Version, `1`
| 0x8     | uint32   | Tag Count

---
## AAC Data Table

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[12]   | Motion Name
| 0xC     | uint32     | Data Table Flag
| 0x10    | uint32     | Data Group Count
| 0x14    | uint32     | Pointer Group Data

### Data Table Flag

| Value | Count  | Description
|--------|-------|------------
| 0      | 30  | Padding
| 30     | 1   | No Guard Reaction
| 31     | 1   | BG Hit
---
## AAC Data Group

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Hit Effect Name
| 0x4     | uint32   | Hit Effect FEP
| 0x8     | uint32   | Data Group Flag
| 0xC     | int16    | Collision Kind
| 0xE     | int16    | Collision Shape
| 0x10    | int16    | Attack Kind
| 0x12    | int16    | Attack Number
| 0x14    | uint8    | Update Attack ID Interval
| 0x15    | int8[3]  | Parameters
| 0x18    | int16    | Group
| 0x1A    | int16    | Count
| 0x1C    | uint32   | Pointer to AAC Data

### Data Group Flag

| Value | Count  | Description
|--------|-------|------------
| 0      | 28  | Padding
| 28     | 1   | No Parent Rotation
| 29     | 1   | Combo
| 30     | 1   | No Loop Update Attack ID
| 31     | 1   | No Attach

---
## AAC Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32    | Pointer to Bone 1
| 0x4     | Vector3f  | Offset 1
| 0x10    | uint32    | Pointer to Bone 2
| 0x14    | Vector3f  | Offset 2
| 0x20    | float     | Radius
| 0x24    | int16     | Start Frame
| 0x26    | int16     | End Frame
| 0x28    | float     | IncAndDecValue
| 0x2C    | float     | Reflect Radius
| 0x30    | int16     | Reflect Start Frame
| 0x32    | int16     | Reflect End Frame