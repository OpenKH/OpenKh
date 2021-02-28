# [Kingdom Hearts Dream Drop Distance](../../index.md) - lboard.bin

Located in `game.rbin` and describes the layout and behavior of the spirit boards for the different dream eaters. This file is in little endian.

## Header

The entry point of the file. It is unknown exactly how it works but it appears to have a header of 16 bytes and then be a table of 4 byte entries, where the nth entry corresponds to the nth board found in lboard.bin

| Offset | Type   | Description |
|--------|--------|-------------|
| 0x00   | byte   | Unknown but might be related to 16*(# of always visible nodes in a board)
| 0x01   | byte   | ID of board?
| 0x02   | byte   | Always 0
| 0x03   | byte   | Always 0

The header has a 16 byte footer that is 00 02 and then all 00

## Board

After the header is the definition of each spirit board. The boards are a series of 16 byte definitions for each node. Each board has a 16 byte header that is all 00 except for the 2nd byte that is 00 08 and then all 00

The definition for each node is as follows

| Offset | Type   | Description |
|--------|--------|-------------|
| 0x00   | byte   | Node Position
| 0x01   | byte   | Node Type
| 0x02   | byte   | Unknown 1
| 0x03   | byte   | Connections
| 0x04   | byte   | Disposition Requirement?
| 0x05   | uint16 | Unknown 2
| 0x07   | byte   | Unknown 3
| 0x08   | uint16 | Generic cost (used by most node types)
| 0x10   | byte   | Reward
| 0x11   | byte   | Unknown 4
| 0x12   | byte   | Unused (Always 0)
| 0x13   | byte   | Unused (Always 0)
| 0x14   | byte   | Unused (Always 0)
| 0x15   | byte   | Unused (Always 0)

### Node Position

The first 4 bits give the Y position, and last 4 give the X position, with 00 being the top left.

E.X. 23 would be the second row and the 3rd column

### Node Type

| Value  | Description   
|--------|--------|
| 00     | Invisible
| 01     | Empty Cloud (value not found in lboard.bin)
| 02     | Starting Point
| 03     | Purchasable Node
| 04     | Purchasable Node
| 06     | Level Checkpoint
| 07     | Secret (Green Question Mark)
| 08     | Invisible
| 16     | Link Checkpoint
| 17     | Secret (Green Question Mark)
| 26     | Item Checkpoint

### Unknown 1

Appears related to Secret nodes. If 03, turns the question mark red. If any value other than 03 or 00, turns the question mark purple

The following are the 4 nodes in lboard.bin where this byte is nonzero

```
04 17 03 00 00 00 00 01 32 00 02 06 00 00 00 00
36 17 03 00 00 00 00 01 96 00 02 07 00 00 00 00
36 17 03 00 02 00 00 03 5E 01 00 02 00 00 00 00
27 17 03 00 00 00 00 02 FA 00 00 01 00 00 00 00
```

### Connections

Specifies which connections to draw from this node to other nodes. 

| Color key |
| --------- |
| Pink - bidirectional
| Yellow - One way out of the node
| Blue - One way in to the node

| Value  | Right Connection | Down Connection |   
|--------|------------------|-----------------|
| 00     | No               | No
| 01     | Pink             | No
| 02     | Yellow           | No
| 03     | Blue             | No
| 04     | No               | Pink
| 05     | Pink             | Pink
| 06     | Yellow           | Pink
| 07     | Blue             | Pink
| 08     | No               | Yellow
| 09     | Pink             | Yellow
| 0A     | Yellow           | Yellow
| 0B     | Blue             | Yellow
| 0C     | No               | Blue
| 0D     | Pink             | Blue
| 0E     | Yellow           | Blue
| 0F     | Blue             | Blue


The last 4 bits appear to be all that matter. Although the following values can be found within lboard.bin, it is unknown what they change.

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

Index of the reward given by the node. It is indexed off the table of commands for that dream eater, although the table doesn't appear to be in the same order is displayed in game.

For Item Checkpoint nodes, this is the number of items required for the checkpoint.

### Unknown 4

Related to secret nodes somehow

Some example nodes

```
04 17 03 00 00 00 00 01 32 00 02 06 00 00 00 00
36 17 03 00 00 00 00 01 96 00 02 07 00 00 00 00
36 17 03 00 02 00 00 03 5E 01 00 02 00 00 00 00
27 17 03 00 00 00 00 02 FA 00 00 01 00 00 00 00
63 17 03 01 01 00 00 03 FA 00 06 02 00 00 00 00
```