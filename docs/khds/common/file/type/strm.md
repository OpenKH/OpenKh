# [Kingdom Hearts DS Games](../../file-type.md) - STRM File Format

[All information was referenced from this document](http://www.feshrine.net/hacking/doc/nds-sdat.html)

STRM stands for "Stream". It is an individual mono/stereo wave file (PCM8, PCM16 or ADPCM).

## File Header

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'STRM'
| 0x4 | uint32 | magic | 0x0100feff
| 0x8 | uint32 | nFileSize | Size of this STRM file
| 0xC | uint16 | nSize | Size of this structure = 16
| 0xE | uint16 | nBlock | Number of Blocks = 2

## Stream Head

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'HEAD'
| 0x4 | uint32 | nSize | Size of this structure
| 0x8 | uint8 | nWaveType | 0 = PCM8, 1 = PCM16, 2 = (IMA-)ADPCM
| 0x9 | uint8 | bLoop | Loop flag = TRUE/FALSE
| 0xA | uint8 | nChannel | Channels
| 0x10 | uint8 | unknown | always 0
| 0x11 | uint16 | nSampleRate | Sampling Rate (perhaps resampled from the original) 
| 0x13 | uint16 | nTime | (1.0 / rate * ARM7_CLOCK / 32) [ARM7_CLOCK: 33.513982MHz / 2 = 1.6756991e7]
| 0x15 | uint32 | nLoopOffset | Loop Offset (samples) 
| 0x19 | uint32 | nSample | Number of Samples 
| 0x1D | uint32 | nDataOffset | Data Offset (always 68h)
| 0x21 | uint32 | nBlock | Number of Blocks 
| 0x25 | uint32 | nBlockLen | Block Length (Per Channel) 
| 0x29 | uint32 | nBlockSample | Samples Per Block (Per Channel)
| 0x2D | uint32 | nLastBlockLen | Last Block Length (Per Channel)
| 0x31 | uint32 | nLastBlockSample | Samples Per Last Block (Per Channel)
| 0x34 | uint8[32] | reserved | always 0

## Stream Data

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'DATA'
| 0x4 | uint32 | nSize | Size of this structure
| 0x8 | uint8[1] | data | Arrays of wave data

## Wave Data

A Block is the same as SWAV Wave Data.

__Mono (SWAV)__

```
Block 1
Block 2
...
Block N (Last Block)
```

__Stereo (STRM)__

```
Block 1 L
Block 1 R
Block 2 L
Block 2 R
...
Block N L (Last Block)
Block N R (Last Block)
```
