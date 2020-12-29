# [Kingdom Hearts II](../../index.md) - 00worldpoint.bin

Contains a list of save points where the player can teleport to from the world map.

The structure of this file is very simple as it is just an array of a structure we will call Area:

| Offset | Description
|--------|-------------
| 0      | [World ID](../../worlds.md)
| 1      | Area ID
| 2      | Entrance
| 3      | padding

The entrance will always have a decimal value of `99` as that is the one used to teleport onto a savepoint.

For Kingdom Hearts II: Final Mix there is a total of 54 teleport points.
