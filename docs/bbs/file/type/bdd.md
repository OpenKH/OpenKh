# BDD Format

BDD stands for *Board Dice ?*.

It controls something related to the command board's panels.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `BDD`. Null terminated.
| 0x4     | int32   | Version, always `0xC131708`
| 0x8     | uint8   | Route W
| 0x9     | uint8   | Route H
| 0xA     | int16   | Max Panels
| 0xC     | int32   | Padding

## BDD Settings

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int32   | Start BP
| 0x4     | uint8   | Padding
| 0x5     | uint8   | Max Commands
| 0x6     | uint16   | Turn Check
| 0x8     | int32   | Padding
| 0xC     | int32   | Padding
| 0x10     | int32   | Padding
| 0x14     | int32   | Padding
| 0x18     | int32   | Padding
| 0x1C     | int32   | Padding

## BDD Route

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int16   | Panel IDX

Two unknown fields here.

## BDD Panel

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint8   | Type
| 0x1     | uint8   | Area
| 0x2     | int16   | Padding
| 0x4     | uint32   | Price
| 0x8     | int16[4]   | Item
| 0x10     | uint8[4]   | Per

## BDD Commands

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | [BDD Command Flag](#BDD-Command)

### BDD Command

| Bit | Count  | Description
|--------|-------|------------
| 0     |  4  | Level
| 4     |  12  | Item