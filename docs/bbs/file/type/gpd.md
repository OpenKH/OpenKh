# GPD Format

GPD stands for *Gimmick Prize Data*.

This file type contains all the prizes dropped by Gimmick type entities.

There's only one instance of this file, included inside `arc/gimmick/gimcommon.arc` in the subfile `GiPrdrda.gpd`.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | File identifier. Always `GPD`, null terminated.
| 0x4     | uint16   | Version, always `1`.
| 0x6     | uint16   | Padding
| 0x8     | uint16   | [Drop Data](#Drop-Data) Count

How this data is read is still unknown.

## Drop Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Unique ID
| 0x2     | uint8    | Prize Kind
| 0x3     | uint8    | Prize Count
| 0x4     | uint8    | Drop Rate
| 0x5     | uint8[3] | Padding

## Unique Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Unique ID Data Count
| 0x2     | uint8    | Drop Rate (Boolean)
| 0x3     | uint8    | Padding

## Cont Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Unique ID
| 0x2     | uint8    | Change Rate (Boolean)
| 0x3     | uint8    | Padding