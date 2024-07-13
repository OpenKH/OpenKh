# CAMP CMD WORK

This isn't a file, but data specification that appears instantiated in memory.

## CAMP COMMAND DATA

| Offset | Type  | Description
|---------|--------|------------
| 0x0     | [COMMAND](./Command.md#Command) | Command Data
| 0x8     | uint16 | Camp Command Flag

### CAMP COMMAND FLAG

First 8 bits belong to the Board command, the second 8 bits to the Camp Menu command.

| Bit     | Length | Description
|---------|--------|------------
| 0  | 1 | fgBdDel
| 1  | 1 | fgBdTmpNew
| 2  | 1 | fgBdInvalid
| 3  | 1 | fgBdRef
| 4  | 1 | fgBdTmpGet
| 5  | 1 | fgBdTake
| 6  | 1 | fgBdCard
| 7  | 1 | fgBdPanelSet
| 8  | 2 | Padding
| 10 | 1 | Master Notice
| 11 | 1 | Master
| 12 | 1 | Item
| 13 | 3 | fgSet