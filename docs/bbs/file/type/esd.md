# ESD Format

ESD stands for `Entity Setting Data`.

These files are contained within the `.arc` files that inherit from any other entity in the game. Mainly used to inherit data from enemies and bosses that reuse data.

## Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | char[16]  | Entity Model
| 0x10   | char[16]  | Entity Animation Pack
| 0x20   | char[16]  | Entity Collision