# [Kingdom Hearts DS Games](../../file-type.md) - SWAV File Format

[All information was referenced from this document](http://www.feshrine.net/hacking/doc/nds-sdat.html)

SWAV doesn't appear in SDAT. They may be found in the ROM elsewhere. They can also be readily extracted from a SWAR file (see below).

## Sample Info (SWAVINFO)

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | uint8 | nWaveType | 0 = PCM8, 1 = PCM16, 2 = (IMA-)ADPCM
| 0x1 | uint8 | bLoop | Loop flag = TRUE|FALSE
| 0x2 | uint16 | nSampleRate | Sampling Rate
| 0x4 | uint16 | nTime | (ARM7_CLOCK / nSampleRate) [ARM7_CLOCK: 33.513982MHz / 2 = 1.6756991 E +7]
| 0x6 | uiin16 | nLoopOffset | Loop Offset (expressed in words (32-bits))
| 0x8 | uint32 | nNonLoopLen | Non Loop Length (expressed in words (32-bits))

## Header

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'SWAV'
| 0x4 | uint32 | magic | 0x0100feff
| 0x8 | uint32 | nFileSize | Size of this SWAV file
| 0xC | uint16 | nSize | Size of this structure = 16
| 0xE | uint16 | nBlock | Number of Blocks = 1

## Data 

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'DATA'
| 0x4 | uint32 | nSize | Size of this structure
| 0x8 | SWAVINFO | info | info about the sample
| 0x14 | uint8[1] | data | array of binary data
