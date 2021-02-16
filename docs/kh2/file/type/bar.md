# [Kingdom Hearts II](../../index.md) - BAR (Binary ARchives)

Most of the game's information is stored within these files in order to keep everything organized and easily accessable by the game. These files are kind of like ZIP files, with certain limitations.

Those limitations include the file's limitations, like the 4 character file names, file types having to be declared within the header, not all file types being declareable, but it also includes PS2's limitations, like the offsets of the files within having to be divisible by 16, even though the size of the previous file is not, file names being unable to contain some characters, etc.

## BAR Structure

BARs can come in all shapes and sizes and forms. Some have the ".bin" extension, meaning it is a system file. Some have the ".mset" extension, meaning it is a moveset file. Some have the ".mdlx" extension, meaning it is a model file. However, no matter the type, it still follows the basic file structure of the BAR.

In this structure, the names do not matter. They can be whatever as long as the PS2 is OK with it. They are just there in order to identify what file it is. This may or may not be the case in 03system, however.

The game seeks the files by their index numbers. For example: An MDLX-BAR seeks index 0 for the VIF information, index 1 for IMGD information, index 2 for the AI and index 3 for the collision in case the MDLX-BAR has 4 files within.

All the values are Little Endian in the PS2/PS4 Versions, while they are Big Endian in the PS3 Version. The "always" values given are in Little Endian.

### BAR Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | char[4] | The identifier of the file (Should be always 0x01524142) |
| 4 | uint32_t | The sub-file count of the BAR File. |
| 8 | uint32_t | Always zero. Padding for a lookup address at runtime.
| 12 | int32_t | [MSET type](../anb/mset.md#slot-system). Can be 0, 1 or 2.

### BAR Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | uint16_t | The sub-file's type (List given below) |
| 2 | uint16_t | Duplicate flag (if the entry is already included in the BAR) |
| 4 | char[4] | The name of the sub-file. Empty characters are registered as 0x00.|
| 8 | uint32_t | Sub-file's offset/location. Must be divisible by 0x10. Can be padded. |
| 12 | uint32_t | Sub-file's size. Everything that comes after [ offset + size ] will be ignored for that specific sub-file.

### BAR Remains

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| BAREntry[index]->fileOffset | byte[BAREntry[index]->fileSize | The sub-file itself in RAW Data (Uncompressed). |

### Kaitai file structure

```yml
meta:
  id: kh2_bar
  endian: le
seq:
  - id: magic
    contents: [0x42, 0x41, 0x52, 0x01]
  - id: num_files
    type: s4
  - id: padding
    size: 8
  - id: files
    type: file_entry
    repeat: expr
    repeat-expr: num_files
types:
  file_entry:
    seq:
      - id: type
        type: u2
      - id: duplicate
        type: u2
      - id: name
        type: str
        size: 4
        encoding: UTF-8
      - id: offset
        type: s4
      - id: size
        type: s4
    instances:
      file:
        io: _root._io
        pos: offset
        size: size
```

## BAR File Types

Keep in mind that this list is still incomplete and will be changed over the course of this project:

| Value | Description | General Location/File Type |
|--------|---------------|-------------|
| 0 | Temporary File (Should not be used) | gummiblock/pxl.bar |
| 1 | Binary Archive | Varies.
| 2 | Independent Format (ItemList, TreasureList, StringList, etc.) | 03system.bin (Varies) - msg/jp/xxx.bar (Always StringList)
| 3 | [`BDX` scripting](../ai/index.md) | `MDLX`, `ARD`, `MAG`
| 4 | 3D Model data (Encapsulated VIF packets containing Vertices, Skinning, Bones for MDLX, etc.) | MDLX - MAP
| 5 | [`OCD` Mesh Occlusion for Culling](../map.md#ocd) | MAP
| 6 | [`OCC` Collision Data](../map.md#occ) | MAP
| 7 | [RAW Texture](../raw-texture.md) | MDLX - MAP
| 8 | `DPX` effect container | PAX
| 9 | [Motion Data](../anb/anb.md#motion-data) | Animation
| 10 | [`TM2` texture](../../../common/tm2.md) | MAP - minigame/xxx.bar
| 11 | `OCH` Camera Collision | MAP
| 12 | Spawn Point Data | MAP
| 13 | Spawn Point Script | ARD
| 14 | `FOG` for Diffuse object coloring | MAP
| 15 | [`OCL` light collision](../map.md#ocl) | MAP
| 16 | [Animation triggers](../anb/anb.md#effect-data) | `ANB`
| 17 | [Animation Binary Archive (ANB)](../anb/anb.md) | `obj/*.mset`, `anm/*`
| 18 | `PAX` Effect | A.FM - MAG - MDLX
| 19 | `OWA` Map Collision Data | MAP
| 20 | Motionset | MSET - limit/*
| 21 | [`BOP` Background Object Placement](../map.md#bop) | MAP
| 22 | Animation Loader | ARD - A.FM - limit/*
| 23 | Model Collision | MDLX
| 24 | [`IMD` image](image.md#imgd) | Varies
| 25 | [Sequence animation (SQD)](./2ld.md#sequence) | 2DD, MAP, A.FM, fontinfo.bar
| 26 | Unknown | Unknown
| 27 | Unknown | Unknown
| 28 | [Layout animation (LAD)](./2ld.md#layout) | `menu/*`, `gumi/sprite/*`
| 29 | [`IMZ` multi-image archive](image.md#imgz) | Varies.
| 30 | Binary Archive | MAP
| 31 | Sound Effect Block (SEB) | MDLX - A.FM
| 32 | BGM Instrument Data (WD) | BGM - MDLX - A.FM
| 33 | Unknown | Unknown
| 34 | IopVoice Sound (VSB) | Varies.
| 35 | SPRD (This is also a bit unknown) | mg_heft.2ld
| 36 | `rgb` raw bitmap | fontimage.bar
| 37 | PS2 Memory Card Icon | menu/save.2ld
| 38 | Wrapped Collision Data (Whatever that is) | Unknown
| 39 | Unknown | Unknown
| 40 | Unknown | Unknown
| 41 | Unknown | Unknown
| 42 | Moogle and Minigame Independent Files | minigame/* - 03system.bin/shop.bin
| 43 | Jimminy Journal Stuff | menu/*
| 44 | Unknown | 00progress.bin
| 45 | Synthesis Data | menu/mixdata.bar
| 46 | Binary Archive | Unknown
| 47 | Vibration Data | vibration.bar
| 48 | Sony Audio Format (VAG) | Varies.
