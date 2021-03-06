# [Kingdom Hearts DS Games](../../file-type.md) - SSEQ File Format

[All information was referenced from this document](http://www.feshrine.net/hacking/doc/nds-sdat.html)

SSEQ stands for "Sound Sequence". It is a converted MIDI sequence. Linked to a BANK for instruments.

## Header

| Position | Type | Name | Description 
|----------|------|------|------------
| 0x0 | char[4] | type | 'SSEQ'
| 0x4 | uint32 | magic | 0x0100feff
| 0x8 | uint32 | nFileSize | Size of this SSEQ file
| 0xC | uint16 | nSize | Size of this structure = 16
| 0xE | uint16 | nBlock | Number of Blocks = 1
| 0x10 | char[4] | type | 'DATA'
| 0x14 | uint32 | nSize | Size of this structure = nFileSize - 16
| 0x18 | uint32 | nDataOffset | Offset of the sequence data = 0x1c
| 0x1C | uint8[1] | data | Arrays of sequence data

NB. For the details of the SSEQ file, please refer to loveemu's sseq2mid

## Description

The design of SSEQ is more programming-oriented while MIDI is hardware-oriented. In MIDI, to produce a sound, a Note-On event is sent to the midi-instrument and then after a certain time, a Note-Off is sent to stop the sound (though it is also acceptable to send a Note-On message with 0 velocity). In SSEQ, a sound is produced by one event only which carries with data such as note, velocity and duration. So the SSEQ-sequencer knows exactly what and how to play and when to stop.

A SSEQ can have at maximum 16 tracks, notes in the range of 0..127 (middle C is 60). Each quartet note has a fixed tick length of 48. Tempo in the range of 1..240 BPM (Default is 120). The SSEQ will not be played correctly if tempo higher than 240.

The SEQ player uses Arm7's Timer1 for timing. The Arm7's 4 Timers runs at 33MHz (approximately 2^25). The SEQ player sets Timer1 reload value to 2728, prescaler to F/64. So on about every 0.0052 sec (64 * 2728 / 33MHz) the SEQ Player will be notified ( 1 cycle ). As a quartet note has fixed tick value of 48, the highest tempo that SEQ Player can handle is 240 BPM ( 60 / (0.0052 * 48) ).

During each cycle, the SEQ player adds the tempo value to a variable. Then it checks if the value exceeds 240. If it does, the SEQ player subtracts 240 from the variable, and process the SSEQ file. Using this method, the playback is not very precise but the difference is too small to be noticed.

Take an example with tempo = 160 BPM, the SSEQ file is processed twice in 3 notifications.

| Cycle | Variable | Action
|-------|----------|-------
| 1 | 0 | Add 160
| 2 | 160 | Add 160
| 3 | 320 | Subtract 240, process once, add 160
| 4 | 240	| Subtract 240, process once, add 160
| 5	| 160	| Add 160
| 6	| 320	| Subtract 240, process once, add 160
| 7	| 240	| Subtract 240, process once, add 160
| 8	| 160	| Add 160

## Events

| Status Byte	| Parameter	| Description
|-------------|-----------|------------
| 0xFE | 2 bytes. It indicates which tracks are used. Bit 0 for track 0, ... Bit 15 for track 15. If the bit is set, the corresponding track is used. | Indication begin of multitrack. Must be in the beginning of the first track to work. A series of event 0x93 follows.
| 0x93 | 4 bytes. 1st byte is track number [0..15]. The other 3 bytes are the relative adress of track data. Add nDataOffset (usually 0x1C) to find out the absolute address. | SSEQ is similar to MIDI in that track data are stored one after one track. Unlike mod music.
| 0x00 .. 0x7f | Velocity: 1 byte [0..127]. Duration: Variable Length | NOTE-ON. Duration is expressed in tick. 48 for quartet note. Usually it is NOT a multiple of 3.
| 0x80 | Duration: Variable Length | REST. It tells the SSEQ-sequencer to wait for a certain tick. Usually it is a multiple of 3.
| 0x81 | Bank & Program Number: Variable Length | bits[0..7] is the program number, bits[8..14] is the bank number. Bank change is seldomly found, so usually bank 0 is used.
| 0x94 | Destination Address: 3 bytes (Add nDataOffset (usually 0x1C) to find out the absolute address.) | JUMP. A jump must be backward. So that the song will loop forever.
| 0x95 | Call Address: 3 bytes (Add nDataOffset (usually 0x1C) to find out the absolute address.) | CALL. It's like a function call. The SSEQ-sequncer jumps to the address and starts playing at there, until it sees a RETURN event.
| 0xFD | NONE	| RETURN. The SSEQ will return to the caller's address + 4 (a Call event is 4 bytes in size).
| 0xA0 .. 0xBf | See loveemu's sseq2mid for more details.	| Some arithmetic operations / comparions. Affect how SSEQ is to be played.
| 0xC0 | Pan Value: 1 byte [0..127], middle is 64	| PAN
| 0xC1 | Volume Value: 1 byte [0..127] | VOLUME
| 0xC2 | Master Volume Value: 1 byte [0..127]	| MASTER VOLUME
| 0xC3 | Value: 1 byte [0..64] (Add 64 to make it a MIDI value) | TRANSPOSE (Channel Coarse Tuning)
| 0xC4 | Value: 1 byte | PITCH BEND
| 0xC5 | Value: 1 byte | PITCH BEND RANGE
| 0xC6 | Value: 1 byte | TRACK PRIORITY
| 0xC7 | Value: 1 byte [0: Poly, 1: Mono]	| MONO/POLY
| 0xC8 | Value: 1 byte [0: Off, 1: On] | TIE (unknown)
| 0xC9 | Value: 1 byte | PORTAMENTO CONTROL
| 0xCA | Value: 1 byte [0: Off, 1: On] | MODULATION DEPTH
| 0xCB | Value: 1 byte | MODULATION SPEED
| 0xCC | Value: 1 byte [0: Pitch, 1: Volume, 2: Pan] | MODULATION TYPE
| 0xCD | Value: 1 byte | MODULATION RANGE
| 0xCE | Value: 1 byte | PORTAMENTO ON/OFF
| 0xCF | Time: 1 byte	| PORTAMENTO TIME
| 0xD0 | Value: 1 byte | ATTACK RATE
| 0xD1 | Value: 1 byte | DECAY RATE
| 0xD2 | Value: 1 byte | SUSTAIN RATE
| 0xD3 | Value: 1 byte | RELEASE RATE
| 0xD4 | Count: 1 byte (how many times to be looped) | LOOP START MARKER
| 0xFC | NONE	| LOOP END MARKER
| 0xD5 | Value: 1 byte | EXPRESSION
| 0xD6 | Value: 1 byte | PRINT VARIABLE (unknown)
| 0xE0 | Value: 2 byte | MODULATION DELAY
| 0xE1 | BPM: 2 byte | TEMPO
| 0xE3 | Value: 2 byte | SWEEP PITCH
| 0xFF | NONE | EOT: End Of Track
