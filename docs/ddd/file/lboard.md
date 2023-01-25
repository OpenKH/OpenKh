# [Kingdom Hearts Dream Drop Distance](../index.md) - lboard

Location: /game/de/bin/ (`game.rbin` in non-PC)

Describes the layout and behavior of the spirits' boards.

## File Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Header (240 bytes)
| X 	 | Boards (Y bytes each)
| 1 	 | Padding (16 bytes)

## Header Structure

| Position | Type  | Description
|--------|-------|--------------
| 0   | int32 | File Identifier? (44 33 22 11)
| 8   | int32 | Board Count
| 16  | int8[55] | Board Offsets
| 236 | int8 | Padding

There should be as many Board Offsets as Board Count dictates. Each Board Offset has the address of the board within the file.

## Board Structure

| Amount | Description |
|--------|---------------|
| X 	 | Node Entries (16 bytes each)
| 1 	 | EOF Node (16 bytes; A node of type 08 with all other values as 00)

## Node Entry Structure

| Offset | Type   | Description |
|--------|--------|-------------|
| 0x00   | uint8    | Position
| 0x01   | uint8    | Type
| 0x02   | uint8    | Unknown 1
| 0x03   | uint8    | Connections
| 0x04   | uint8    | Disposition Requirement
| 0x05   | uint16   | Unknown 2
| 0x07   | uint8    | Unknown 3
| 0x08   | uint16   | Generic cost (used by most node types)
| 0x10   | uint8    | Reward
| 0x11   | uint8    | Unknown 4
| 0x12   | uint8[4] | Padding

### Node Position

The first 4 bits give the Y position, and last 4 give the X position, with 00 being the top left.

E.X. 23 would be the second row and the 3rd column

### Node Type

| Value  | Description   
|--------|--------|
| 00     | Invisible
| 01     | Empty Cloud (value not found in lboard.bin)
| 02     | Starting Point
| 03     | Purchasable Node (Stat)
| 04     | Purchasable Node (Item)
| 06     | Level Checkpoint
| 07     | Secret (Green Question Mark)
| 08     | EOF node
| 16     | Link Checkpoint
| 17     | Secret (Red Question Mark)
| 26     | Item Checkpoint

### Unknown 1

Related to Red Secret nodes. If 03, turns the question mark red. If any value other than 03 or 00, turns the question mark purple

The following are the 4 nodes in lboard.bin where this byte is nonzero

```
04 17 03 00 00 00 00 01 32 00 02 06 00 00 00 00
36 17 03 00 00 00 00 01 96 00 02 07 00 00 00 00
36 17 03 00 02 00 00 03 5E 01 00 02 00 00 00 00
27 17 03 00 00 00 00 02 FA 00 00 01 00 00 00 00
```

### Connections

Specifies which connections to draw from this node to other nodes. 

| Position | Size  | Description
|--------|-------|--------------
| 0 | 4 | < unknown >
| 4 | 2 | Right connection
| 6 | 2 | Down connection

| Value  | Description   
|--------|--------|
| 0     | None
| 1     | Both ways
| 2     | In
| 3     | Out

The unknown bits appear to be all that matter. Although the following values can be found within lboard.bin, it is unknown what they change.

`12, 14, 41, 44, 51`

### Disposition Requirement

Gives an ID? referring to the specific disposition the dream eater needs in order for the node to display. May also be related to the secret nodes in some way.

### Unknown 2

Looks like a 2 byte value but it is unknown what it does. Here are some nodes where the bytes are nonzero

```
10 04 00 05 00 01 00 00 0A 00 0C 00 00 00 00 00
33 16 00 51 00 02 02 00 01 00 00 00 00 00 00 00
21 04 00 05 00 01 00 00 0A 00 01 00 00 00 00 00
23 04 00 00 00 02 00 00 96 00 00 00 00 00 00 00
33 03 00 12 00 01 00 00 64 00 0A 00 00 00 00 00
```

### Unknown 3

Related to secret nodes somehow

Some example nodes

```
13 07 00 04 00 00 00 01 0A 00 00 00 00 00 00 00
52 07 00 05 00 00 00 01 0A 00 00 00 00 00 00 00
41 07 00 04 00 00 00 01 0A 00 00 00 00 00 00 00
04 17 03 00 00 00 00 01 32 00 02 06 00 00 00 00
```

### Generic Cost

2 byte value that is used differently depending on the node type

Purchasable Node - The cost in link points to acquire the node
Level Checkpoint - Level Requirement for the spirit eater
Link Checkpoint - # of times to link with the spirit eater to open the checkpoint
Item Checkpoint - Appears to be the ID of the item required for the item checkpoint

### Reward

Index of the reward given by the node. The rewards are stored in [lbt_list](./lbt_list.md).

For Item Checkpoint nodes, this is the number of items required for the checkpoint.

### Unknown 4

Related to Red Secret nodes somehow

Some example nodes

```
04 17 03 00 00 00 00 01 32 00 02 06 00 00 00 00
36 17 03 00 00 00 00 01 96 00 02 07 00 00 00 00
36 17 03 00 02 00 00 03 5E 01 00 02 00 00 00 00
27 17 03 00 00 00 00 02 FA 00 00 01 00 00 00 00
63 17 03 01 01 00 00 03 FA 00 06 02 00 00 00 00
```