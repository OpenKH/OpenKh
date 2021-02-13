# [Kingdom Hearts Birth By Sleep](index.md) - ARC format

The ARC format (presumability a shorten of Archive) is a container of files with a dependency system feature to other files in a [BBSA](bbsa.md) archive.

## File format

The `RESERVED` fields are used by BBS engine to store a pointer to the data.

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File identifier, `0x435241`
| 04     | short | Version, always `1`
| 06     | short | [Entry](#entry) count
| 08     | int   | RESERVED
| 0c     | int   | RESERVED

Right after the header, an array of [entry](#entry) is found.

### Entry

An entry can represents either a file contained in the archive or a file the archive depends on.

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Directory hash
| 04     | int   | Offset
| 08     | int   | Length
| 0c     | int   | RESERVED
| 10     | char[16] | File name

When the file exists in the archive, `Directory hash` is `0`.

When the entry is a link to anotehr file, `Directory hash` has a value and `Lenght` is `0` while `Offset` value is ignored.


## Additional info

Apparently the file `arc/menu/01_race.arc` from Birth By Sleep is corrupt and unreadable. It is unknown if the remastered actually uses that file; and if yes, how.