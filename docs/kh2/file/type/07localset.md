# [Kingdom Hearts II](../../index.md) - 07localset.bin

This file determines which room to load when a specific ARD Program is called. It is a BAR file where the x-th subfile correspond to [World ID](../../worlds.md) x.

## Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | File type (1)
| 4      | uint32 | Entry count

## Entry

| Offset | Type  | Description
|--------|-------|------------
| 0      | uint8 | ARD Program
| 1      | uint8 | Area

It is possible to have duplicate ARD Programs within different worlds/subfiles.
