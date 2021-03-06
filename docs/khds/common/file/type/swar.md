# [Kingdom Hearts DS Games](../../file-type.md) - SWAR File Format

[All information was referenced from this document](http://www.feshrine.net/hacking/doc/nds-sdat.html)

## Header

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'SWAR'
| 0x4 | uint32 | magic | 0x0100feff
| 0x8 | uint32 | nFileSize | Size of this SWAR file
| 0xC | uint16 | nSize | Size of this structure = 16
| 0xE | uint16 | nBlock | Number of Blocks = 1

## Data

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'DATA'
| 0x4 | uint32 | nSize | Size of this structure
| 0x8 | uint32[8] | reserved | reserved 0s, for use in runtime
| 0x28 | uint32 | nSample | Number of Samples

NB. After the array of offsets, the binary samples follow. Each sample has a SWAVINFO structure before the sample data. Therefore, it is easy to make a SWAV from the samples in SWAR.
