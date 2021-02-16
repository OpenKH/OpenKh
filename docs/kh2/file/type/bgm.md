# [Kingdom Hearts II](../../index.md) - BGM (BackGround Music)
Kingdom Hearts II use a custom format for its sequenced music called BGM, which
is handled internally by the Square Sound Library and is a similar format to
some Square games released at the time, such as Final Fantasy X.

## BGM Structure
It is a MIDI-like format, linked to a specific soundfont which can contain
several tracks, each of which having its own set of commands.

### BGM Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | char[4] | The identifier of the file (Should be always 0x204D4742 or "BGM ") |
| 4 | uint16_t | Sequence ID |
| 6 | uint16_t | Soundfont ID (WD) | 
| 8 | char | Track count |  
| 9 | char[3] | Unknown |  
| 0xC | int8_t | Volume |  
| 0xD | char | Unknown |  
| 0xE | uint16_t | Parts Per Quarter Note |  
| 0x10 | uint32_t | File Size |  
| 0x14 | char[12] | Reserved |  

### BGM Track

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | uint32_t | Track size |
| 4 | char[?] | Commands |


### BGM Track commands 
Commands:
	Each command consists of:
	  1) Delta time (1-4 bytes; variable length)
	  2) Command code (1 byte)
	  3) Command arguments (varies per command)
	 All timings seem to follow the official MIDI spec.
	00:	End of track
	02:	Loop begin
	03:	Loop end
	08:	Set tempo
		byte:	bpm
	0A
		byte
	0C:	Time signature
		ushort
	0D
		byte
	10:	Note on with previous key and velocity
	11:	Note on
		byte:	Key
		byte:	Velocity
	12:	Note on with previous velocity
		byte:	Key
	13:	Note on with previous key
		byte:	Velocity
	18:	Note off; Previous note
	19
		byte
		byte
	1A:	Note off
		byte:	Key
	20:	Program change
		byte: new program
	22:	Volume
		byte
	24:	Expression
		byte
	26:	Pan
		byte
	28
		byte
	31
		byte
	34
		byte
	35
		byte
	3C:	Sustain Pedal
		byte
	3E
		byte
	40
		byte
		byte
		byte
	47
		byte
		byte
	48
		byte
		byte
		byte
	50
		byte
		byte
		byte
	58
		byte
	5C
	5D:	Portamento?
		byte

### Kaitai file structure

TODO: actually define specific commands in the kaitai struct
```
meta:
  id: kh2_bgm
  endian: le
seq:
  - id: magic
    contents: [0x42, 0x47, 0x4D, 0x20]
  - id: seq_id
    type: u2
  - id: wd_id
    type: u2
  - id: track_cnt
    type: u1
  - id: unk1
    size: 3
  - id: volume
    type: s1
  - id: unk2
    size: 1
  - id: ppqn
    type: u2
  - id: filesize
    type: u4
  - id: res
    size: 12
  - id: tracks
    type: track
    repeat: expr
    repeat-expr: track_cnt

types:
  track:
    seq:
      - id: size
        type: u4
      - id: commands
        size: size
```


