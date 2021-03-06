# [Kingdom Hearts DS Games](../../file-type.md) - SBNK File Format

[All information was referenced from this document](http://www.feshrine.net/hacking/doc/nds-sdat.html)

SBNK stands for "Sound Bank". A bank is linked to up to 4 SWAR files which contain the samples. It define the instruments by which a SSEQ sequence can use. You may imagine SSEQ + SBNK + SWAR are similar to module music created by trackers.

## Instrument (SBNKINS)

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | uint8 | fRecord | can be either 0, 1..4, 16 or 17
| 0x1 | uint16 | nOffset | absolute offset of the data in file
| 0x3 | uint8 | reserved | must be zero

## Header

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'SBNK'
| 0x4 | uint32 | magic | 0x0100feff
| 0x8 | uint32 | nFileSize | Size of this SBNK file
| 0xC | uint16 | nSize | Size of this structure = 16
| 0xE | uint16 | nBlock | Number of Blocks = 1

## Data

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'DATA'
| 0x4 | uint32 | nSize | Size of this structure
| 0x8 | uitn32[8] | reserved | reserved 0s, for use in runtime
| 0x28 | uint32 | nCount | number of instrument
| 0x2C | SBNKINS[1] | Ins | Instrument data

So, after SBNK::data, there come SBNK::data::nCount of SBNKINS. After the last SBNKINS, there will be SBNK::data::nCount of instrument records. In each instrument records, we can find one or more wave/note definitions.

## Instrument Record

If SBNKINS::fRecord = 0, it is empty. SBNKINS::nOffset will also = 0.

If SBNKINS::fRecord < 16, the record is a note/wave definition. I have seen values 1, 2 and 3. But it seems the value does not affect the wave/note definition that follows. Instrument record size is 16 bytes.

```
swav number 	2 bytes	// the swav used
swar number	2 bytes	// the swar used. NB. cross-reference to "1.3.2 Info Block - Entry, Record 2 BANK" 
note number	1 byte 	// 0..127
Attack Rate	1 byte	// 0..127
Decay Rate	1 byte	// 0..127
Sustain Level	1 byte	// 0..127
Release Rate	1 byte	// 0..127
Pan		1 byte	// 0..127, 64 = middle
```

If SBNKINS::fRecord = 16, the record is a range of note/wave definitions. The number of definitions = 'upper note' - 'lower note' + 1. The Instrument Record size is 2 + no. of definitions * 12 bytes.

```
lower note	1 byte 	// 0..127
upper note	1 byte 	// 0..127

unknown		2 bytes	// usually == 01 00
swav number 	2 bytes	// the swav used
swar number	2 bytes	// the swar used. 
note number	1 byte
Attack Rate	1 byte
Decay Rate	1 byte
Sustain Level	1 byte
Release Rate	1 byte
Pan		1 byte

...
...
...

unknown		2 bytes	// usually == 01 00
swav number 	2 bytes	// the swav used
swar number	2 bytes	// the swar used. 
note number	1 byte
Attack Rate	1 byte
Decay Rate	1 byte
Sustain Level	1 byte
Release Rate	1 byte
Pan		1 byte
```

For example, lower note = 30, upper note = 40, there will be 40 - 30 + 1 = 11 wave/note definitions.
The first wave/note definition applies to note 30.
The second wave/note definition applies to note 31.
The third wave/note definition applies to note 32.
...
The eleventh wave/note definition applies to note 40.

If SBNKINS::fRecord = 17, the record is a regional wave/note definition.

```
The first 8 bytes defines the regions. They divide the full note range [0..127] into several regions (max. is 8)
An example is:
25  35  45  55  65  127 0   0 (So there are 6 regions: 0..25, 26..35, 36..45, 46..55, 56..65, 66..127)
Another example:
50  59  66  83  127 0   0   0 (5 regions: 0..50, 51..59, 60..66, 67..84, 85..127)

Depending on the number of regions defined, the corresponding number of wave/note definitions follow:

unknown		2 bytes	// usually == 01 00
swav number 	2 bytes	// the swav used
swar number	2 bytes	// the swar used. 
note number	1 byte	
Attack Rate	1 byte
Decay Rate	1 byte
Sustain Level	1 byte
Release Rate	1 byte
Pan		1 byte
...
...

In the first example, for region 0..25, the first wave/note definition applies.
For region 26..35, the 2nc wave/note definition applies.
For region 36..45, the 3rd wave/note definition applies.
... 
For region 66..127, the 6th wave/note definition applies.
```
  
REMARKS: Unknown bytes before wave/defnition definition = 5, not 1 in stage_04_bank.sbnk, stage_04.sdat, Rom No.1156
  
## Articulation Data

The articulation data affects the playback of the SSEQ file. They are 'Attack Rate', 'Decay Rate', 'Sustain Level' and 'Release Rate' (all have a value in range [0..127])

```
amplitude (%)

100% |    /\
     |   /  \__________
     |  /              \
     | /                \
0%   |/__________________\___ time (sec)
```

Imagine how the amplitude of a note varies from begin to the end.

The graph above shows the amplitude envelope when a note is sound. The y-axis is Amplitude, x-axis is time.

* __Attack rate__ determines how fast the note reaches 100% amplitude. (See the first upward curve). Thus the highest value 127 means the sound reaches 100% amplitude in the shortest time; 0 means the longest time.

* __Decay rate__ determines how fast the amplitude decays to 0% amplitude. Of course the sound will not drop to 0% but stops at sustain level. (See the first downward curve). Thus the highest value 127 means the sound reachs the sustain level in the shortest time; 0 means the longest time.

* __Sustain level__ determines the amplitude at which the sound sustains. (See the horizonal part). Thus the highest value 127 means the sound sustains at 100% amplitude (no decay), while 0 means 0% (full decay).

* __Release rate__ determines how fast the amplitude drops from 100% to 0%. Not from sustain level to 0%. (See the second downward curve). The value has the same meaning as Decay rate.

The __raw data__ column is the transformed value used for calculation.

The SEQ Player treats 0 as the 100% amplitude value and -92544 (723*128) as the 0% amplitude value. The starting ampltitude is 0% (-92544).

During the _attack phase,_ in each cycle, the SSEQ Player calculates the new amplitude value: amplitude value = attack rate * amplitude value / 255. The attack phase stops when amplitude reaches 0.

The __times__ column shows how many cycles are needed to reach 100% amplitude value.

The __sec__ column shows the corresponding time needed to reach 100% amplitude value.

The __scale__ column is the corresponding value to feed in DLS Bank.

During the decay phase, in each cycle, the SSEQ Player calculates the new amplitude value: amplitude value = amplitude value - decay rate. Note the starting amplitude value is 0. The decay phase stops when amplitude reaches sustain level.

The other columns are self-explanatory.
