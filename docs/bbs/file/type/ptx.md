# PTX Format

We don't know what PTX stands for.

This file controls things such as events triggered by OLO files or the music to play.

# Structure

| Offset | Type  | Description
|--------|-------|------------
| 00     | Content[0...] | Entries of Content

## MAP-BTL

Controls which spawns appear in the level and what flag needs to be risen for them to be triggered.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16 | Flag to load with
| 0x2     | uint16 | Size of string chunk + 4
| 0x4     | uint16 | unk3 // Always 0x1
| 0x6     | uint16 | String Count
| 0x8     | string[String Count] | Name of OLO files to spawn. Given an olo name `{world}{area}-{ID}.olo`, only the `ID` section is written, ending with a 0x0000FFFF entry.

## EVT

PTX files followed by `-evt` decide what happens when an event triggers, such as a teleporter to another location.

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16 | 
| 0x2    | uint16 | 
| 0x4    | uint16 | 
| 0x6    | uint16 | 
| 0x8    | uint32 | Start event? (1 auto-starts teleport)
| 0xC    | uint32 | (Anything other than 0x2 crashes)
| 0x10   | uint16 | 
| 0x12   | uint16 | World
| 0x14   | uint16 | Room
| 0x16   | uint16 | Entrance
| 0x18   | uint16 | 
| 0x1A   | uint16 | 
| 0x1C   | uint16 | `Always 0xFF`

## Music Section

This section can be added to change the music applied to an event, usually inside BTL.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16 | unk0 `Always 0x7`
| 0x2     | uint16 | unk2
| 0x4     | uint16 | Song index to play 
| 0x6     | uint16 | unk6 // Seems to be 0xFFFF most of the time

## Mission Section

This section can be added to change the music applied to an event, usually inside BTL.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16 | unk0 `Always 0x9`
| 0x2     | uint16 | unk2 `Always 0x4`
| 0x4     | char[16] | Mission to Trigger