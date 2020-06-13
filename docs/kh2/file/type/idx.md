# [Kingdom Hearts II](../../index.md) - IDX (Index file table)

Every game file is packed into an IMG file, a big chunk of data that contains everything. The file that contains information on how the files are stored is the IDX.

Whenever the game wants to load a specific file, the engine uses the IDX as a look-up table to search and retrieve the file needed.

**NOTE**: This document applies on Playstation 2 version only.

## Multiple IDXs

The game does not contain every file in the IMG into a single IDX. Instead the game loads specific IDX based on the situation. This is done in order to improve the table look-up speed and group data files in the disc to improve the seeking time.

`KH2.IDX` is the common index table, where all the common files are located. This is where all the core content is located.

`000{WorldId}.idx` is the table for a specific world. Since the gameplay nature of Kingdom Hearts II, every world-specific file can be isolated in their own IDX.

## File structure

The entries are stored ordered by the `Hash32`, so the game can use a binary-search algorithm to improve the table look-up speed even more.

When *block* is mentioned, it means a ISO9660 block, which is 2048 bytes. Every file in the IMG is block-aligned. The reason for that is because the CD-Drive reads the content using a 2048 length buffer.

When a file is *streamed*, it means that the engine should progressively load the file chunk by chunk instead of storing it entirely in the memory data. This is true for VAG and video files, since their content is never replayed. As far is known, a streamed file cannot be flagged as compressed.

NOTE: While seems to be logical leaving a file uncompressed to improve performance speed, in PS2-era compressed files loads faster from a disc.

### IDX header

| Offset | Type | Description |
|--------|------|-------------|
| 0      | int32 | Entry count in the IDX
| 4      | Entry[0..N] | Each entry represent a file in the IMG

### IDX Entry

| Offset | Type | Description |
|--------|------|-------------|
| 0      | uint32 | 32-bit hash of the original file name
| 4      | uint16 | 16-bit hash of the original file name, probably used to avoid hash collision
| 8      | uint16 : 0-13 | Number of physical blocks that the file takes. A value of 0 means that it takes 1 block.
| 8      | bool : 14 | Positive if the file is compressed.
| 8      | bool : 15 | Positive if the file is streamed.
| 12     | int    | Uncompressed length of the file in bytes.

### Kaitai file structure

```
meta:
  id: kh2_idx
  endian: le
seq:
  - id: num_files
    type: u4
  - id: files
    type: file_entry
    repeat: expr
    repeat-expr: num_files
types:
  file_entry:
    seq:
      - id: main_hash
        type: u4
      - id: second_hash
        type: u2
      - id: flags
        type: u2
        doc: |
            & 0x8000: Verify secondary hash
            & 0x4000: Is compressed
            & 0x3FFF: Compressed size = (flags + 1) * 2048
      - id: offset
        type: s4
        doc: offset in the IMG
      - id: size
        type: s4
```
