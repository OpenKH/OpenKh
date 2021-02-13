# BSD Format

BSD apparently stands for *Bad Status Data*.


## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `@BSD`.
| 0x4     | uint32   | Version, `2`
| 0x8     | uint32   | BSD Data Count
| 0xC     | uint32   | Pointer to BSD Data


## BSD Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Pointer to FEP name
| 0x4     | uint32   | Pointer to effect name
| 0x8     | uint32   | Pointer to bone name
| 0xC     | uint16   | Padding
| 0xE     | uint16   | m_nFixZ
| 0x10    | float    | m_fOfsZ
| 0x14    | uint32   | m_nFixZ32
| 0x18    | Vector3f   | Position
| 0x24    | Vector3f   | Scale
| 0x30    | Vector3f   | Rotation