# [Kingdom Hearts II](../../index.md) - COCT

COCT is a set of data to realize [collision detection](https://en.wikipedia.org/wiki/Collision_detection). The targets are player (also NPC) and map mesh model.

## COCT file structure

```txt
CollisionMeshGroup: (Has: BBox, Links to sub CollisionMeshGroup)
CollisionMesh: (Has: BBox, Unk1, Unk2)
Collision: (Has: Unk1)
Vertex: Single triangle or quad represents shape of Collision
Plane: xyzd
BBox: minXYZ, maxXYZ
SurfaceFlags: uint32

.
└── CollisionMeshGroup (Has one or more CollisionMesh)
    └── CollisionMesh (Has one or more Collision)
        └── Collision (3 or 4 Vertex. One Plane. 0 or 1 BBox. One SurfaceFlags)
            ├── Vertex
            ├── Plane
            ├── BBox
            └── SurfaceFlags
```

_Note:_ BBox is short for bounding-box.

Here is association of table number and named tables:

```txt
Table1: CollisionMeshGroup
Table2: CollisionMesh
Table3: Collision
Table4: Vertex
Table5: Plane
Table6: BBox
Table7: SurfaceFlags
```

### File header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | char[4] | The identifier of the file (Should be always 0x54434F43) |
| 4 | uint32_t | Version: always `1`
| 8 | uint32_t | Unknown
| 12 | uint32_t | Unknown
| 16 | uint32_t | Offset Header: always `0`
| 20 | uint32_t | Length Header: always `0x50`
| 24 | uint32_t | Offset Table1
| 28 | uint32_t | Length Table1
| 32 | uint32_t | Offset Table2
| 36 | uint32_t | Length Table2
| 40 | uint32_t | Offset Table3
| 44 | uint32_t | Length Table3
| 48 | uint32_t | Offset Table4
| 52 | uint32_t | Length Table4
| 56 | uint32_t | Offset Table5
| 60 | uint32_t | Length Table5
| 64 | uint32_t | Offset Table6
| 68 | uint32_t | Length Table6
| 72 | uint32_t | Offset Table7
| 76 | uint32_t | Length Table7

### CollisionMeshGroup (Table1)

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | int16_t | Child1
| 2 | int16_t | Child2
| 4 | int16_t | Child3
| 6 | int16_t | Child4
| 8 | int16_t | Child5
| 10 | int16_t | Child6
| 12 | int16_t | Child7
| 14 | int16_t | Child8
| 16 | int16_t | MinX
| 18 | int16_t | MinY
| 20 | int16_t | MinZ
| 22 | int16_t | MaxX
| 24 | int16_t | MaxY
| 26 | int16_t | MaxZ
| 28 | uint16_t | First index to CollisionMesh (Table2)
| 30 | uint16_t | Last index to CollisionMesh (Table2)

_Notes:_

- ChildX: `-1` to mean nothing to point.
- (X,Y,Z) are inverted (-X,-Y,-Z).


### CollisionMesh (Table2)

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | int16_t | MinX
| 2 | int16_t | MinY
| 4 | int16_t | MinZ
| 6 | int16_t | MaxX
| 8 | int16_t | MaxY
| 10 | int16_t | MaxZ
| 12 | uint16_t | First index to Collision (Table3)
| 14 | uint16_t | Last index to Collision (Table3)
| 16 | uint16_t | Unknown
| 18 | uint16_t | Unknown

_Note:_ (X,Y,Z) are inverted (-X,-Y,-Z).


### Collision (Table3)

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | int16_t | Unknown
| 2 | int16_t | Vertex1
| 4 | int16_t | Vertex2
| 6 | int16_t | Vertex3
| 8 | int16_t | Vertex4: can be `-1` in case of triangle
| 10 | int16_t | Index to Plane (Table5)
| 12 | int16_t | Index to BBox (Table6), otherwise use `-1` is to apply Table2's BBox
| 14 | uint16_t | Index to SurfaceFlags (Table7)

_Notes:_

- Vertex1,2,3 composes one triangle.
- Vertex1,2,3,4 composes one triangle fan (usually its shape is quadrangle).


### Vertex

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | float | X
| 4 | float | Y
| 8 | float | Z
| 12 | float | W: always `1`

_Note:_ (X,Y,Z) are inverted (-X,-Y,-Z).


### Plane

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | float | X
| 4 | float | Y
| 8 | float | Z
| 12 | float | D

_Note:_ (X,Y,Z,D) are inverted (-X,-Y,-Z,-D).


### BBox

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | int16_t | MinX
| 2 | int16_t | MinY
| 4 | int16_t | MinZ
| 6 | int16_t | MaxX
| 8 | int16_t | MaxY
| 10 | int16_t | MaxZ

_Note:_ (X,Y,Z) are inverted (-X,-Y,-Z).


### SurfaceFlags

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | uint32_t | Surface Flags

Bit fields:

| Bit | Working | Description |
|--:|---|---|
| 0 | partyStand | 
| 1 | entityFallOverride | 
| 2 | unkFallFlag1 | seems to only affect the player
| 3 | unkFallFlag2 | 
| 4 | partyCollide | 
| 5 | objectCollide | affects enemies and field objects like the skateboard
| 6 | unk01_1 | 
| 7 | attackCollide | 
| 8 | unk02_1 | 
| 9 | unk02_2 | 
| 10 | ledgeGrab | 
| 11 | dispBarrier | 
| 12 | dispMessage | 
| 13 | unk02_3 | 
| 14 | unk02_4 | 
| 15 | unk02_5 | 
| 16 | unk03_1 | 
| 17 | unk03_2 | 
| 18 | unk03_3 | 
| 19 | unk03_4 | 
| 20 | unk03_5 | 
| 21 | unk03_6 | 
| 22 | unk03_7 | 
| 23 | unk03_8 | 
| 24 | unk04_1 | 
| 25 | unk04_2 | 
| 26 | unk04_3 | 
| 27 | unk04_4 | 
| 28 | unk04_5 | 
| 29 | unk04_6 | 
| 30 | unk04_7 | 
| 31 | unk04_8 | 

