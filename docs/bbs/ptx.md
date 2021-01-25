# PTX Format

Fields marked with `optional` mean that such fields may not be included in the file.

### Structure

| Offset | Type  | Description
|--------|-------|------------
| 00     | Content[0...] | Entries of Content

### Content

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Flag to load
| 02     | short | unk2 
| 04     | short | unk3 // Always 0x1
| 06     | short | unk4 // Always 0x1
| 08     | string | OLO substring to load. For example if vs01-b70_.olo, it stores b70_. Not null-terminated.
| 12     | short | `optional` unk6
| 14     | short | `optional` unk7
| 16     | short | `optional` Song index to play 
| 18     | short | `optional` unk9 // Seems to be 0xFFFF most of the time
