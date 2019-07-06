# [Kingdom Hearts II](../../index) - BAR (Binary ARchives)
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
| 0 | int32_t | The identifier of the file (Should be always 0x01524142) |
| 4 | int32_t | The sub-file count of the BAR File. |
| 8 | int64_t | File padding, has no effect. (Always 0x00)

### BAR Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | int32_t | The sub-file's type (List given below) |
| 4 | char[4] | The name of the sub-file. Empty characters are registered as 0x00.|
| 8 | uint32_t | Sub-file's offset/location. Must be divisible by 0x10. Can be padded. |
| 12 | uint32_t | Sub-file's size. Everything that comes after [ offset + size ] will be ignored for that specific sub-file.

### BAR Remains

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| BAREntry[index]->fileOffset | byte[BAREntry[index]->fileSize | The sub-file itself in RAW Data (Uncompressed). |

## BAR File Types

Keep in mind that this list is still incomplete and will be changed over the course of this project:

| Value | Description | General Location/File Type |
|--------|---------------|-------------|
| 0 | Temporary File (Should not be used) | gummiblock/pxl.bar |
| 1 | Binary Archive | Varies.
| 2 | Independent Format (ItemList, TreasureList, StringList, etc.) | 03system.bin (Varies) - msg/jp/xxx.bar (Always StringList)
| 3 | AI Code (Also should not be used, unless you can code an AI) | MDLX - ARD - MAG
| 4 | VIF Data (Vertices, Skinning, Bones, etc.) | MDLX - MAP
| 5 | Mesh Occlusion/Obstruction (Probably Culling) | MAP
| 6 | Map Collision Data | MAP 
| 7 | RAW Texture Data | MDLX - MAP
| 8 | DPX (A bit unknown) | PAX
| 9 | Animation Data | ANB
| 10 | Texture Data | MAP - minigame/xxx.bar
| 11 | Camera Collision Data | MAP
| 12 | Spawn Point Data | MAP
| 13 | Spawn Point Script | ARD
| 14 | Map Color Array/Diffuse Maps | MAP
| 15 | Lighting Data | MAP
| 16 | Moveset Instructions | ANB
| 17 | Animation Binary Archive (ANB) | MSET
| 18 | PAX Effect | A.FM - MAG - MDLX
| 19 | Map Collision Data | MAP 
| 20 | Binary Archive | MSET - limit/*
| 21 | Unknown | MAP
| 22 | Animation Loader | ARD - A.FM - limit/*
| 23 | Model Collision | MDLX
| 24 | Image Data (IMGD) | Varies
| 25 | Sequenced Layers (SEQD) | 2LD - 2DD - A.FM - fontinfo.bar
| 26 | Unknown | Unknown
| 27 | Unknown | Unknown
| 28 | LAYD (Layer Data?) | menu/* - gumi/sprite/*
| 29 | Multi-Image Data Archive (IMGZ) | Varies.
| 30 | Binary Archive | MAP
| 31 | Sound Effect Block (SEB) | MDLX - A.FM
| 32 | BGM Instrument Data (WD) | BGM - MDLX - A.FM
| 33 | Unknown | Unknown
| 34 | IopVoice Sound (VSB) | Varies.
| 35 | SPRD (This is also a bit unknown) | mg_heft.2ld
| 36 | RAW Bitmap | fontimage.bar
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
