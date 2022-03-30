# [Kingdom Hearts Dream Drop Distance](../index.md) - rbin

rbin is the container format used by the 3DS version of DDD to hold all its files.

Each rbin holds the files for a given folder in the games file system, refered to as that rbin's 'mount point'. 

## Header

| Position | Type | Description |
|----------|------|-------------|
| 0x00 | uint32   | File identifier, always `CRAR` |
| 0x04 | uint16   | Assumed to be File Version, always 1 |
| 0x06 | uint16   | File Entry count |
| 0x08 | uint32   | Offset to start of file data |
| 0x0C | uint32   | Reserved |
| 0x10 | char[16] | Mount point |

The header is immediatly followed by the list of File Entries.

## File Entry

| Position | Type | Description |
|----------|------|-------------|
| 0x00 | uint32   | Assumed to be some kind of hash, unknown algorithm |
| 0x04 | uint32   | Offset of name (see note 1 below) |
| 0x08 | uint32   | Size/Compression flag (see note 2 below) |
| 0x0C | uint32   | Offset to file data |

* NOTE 1: The offset to the name string is relative to that field in the structure, not relative to the start of the file. Names are stored as null-terminated strings.
* NOTE 2: The size of a file occupies the lower 31 bits of the field. The top bit, what would be the sign bit in a signed integer, indicates if the file is compressed or not. File data is compressed with an algorithm known as "BLZ", which is the same as LZSS, but starts at the end of the file and works backwards.