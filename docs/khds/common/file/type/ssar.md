# [Kingdom Hearts DS Games](../../file-type.md) - SSAR File Format

[All information was referenced from this document](http://www.feshrine.net/hacking/doc/nds-sdat.html)

SSAR stands for "(Sound) Sequence Archive". It is a collection of sequences (used mainly for sound effect). Therefore, each archived SSEQ is usually short, with one or two notes.

## Record (SSARREC)

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | uint32 | nOffset | relative offset of the archived SEQ file, absolute offset = nOffset + SSAR::nDataOffset
| 0x4 | uint16 | bnk | bank
| 0x6 | uint8 | vol | volume
| 0x7 | uint8 | cpr | channel pressure 
| 0x8 | uint8 | ppr | polyphonic pressure
| 0x9 | uint8 | ply | play
| 0xA | uint8[2] | reserved
  
## Header

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'SSAR'
| 0x4 | uint32 | magic | 0x0100feff
| 0x4 | uint32 | nFileSize | Size of this SSAR file
| 0x8 | uint16 | nSize | Size of this structure = 16
| 0xA | uint16 | nBlock | Number of Blocks = 1
  
## Data
| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'DATA'
| 0x4 | uint32 | nSize | Size of this structure
| 0x8 | uint32 | nDataOffset | Offset of data
| 0xC | uint32 | nCount | nCount * 12 + 32 = nDataOffset
| 0x10 | SSARREC[1] | Rec | nCount of SSARREC
