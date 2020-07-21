# [Kingdom Hearts II](../../index.md) - MSG (Message)

Store localized text into a key-value pair, where the key is the Message ID and the value is the text itself.

A MSG is a [BAR](bar.md) file always contains a binary file, representing the [MSG](#msg-structure) itself and a `md_m`, which is an [IMGD](image.md#imgd) that contains the HUD for that specific world.

## MSG Structure

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | Magic Code, always `1`
| 4      | uint32 | Message count
| 8      | LBA[0..n]    | [LBA](#lba)

### LBA

The messages are always stored in ascending order based on the Message ID. The technical reason is that the game engine uses a Binary Search to look-up a message.

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | Message ID
| 4      | uint32 | Message offset

## Encoder

The game uses a custom text encoding and it varies based on the font the game wants to use. There are 4 encodings:

* International System, used by the non-japanese versions that uses the menu font.
* International Event, used by the non-japanese versions that uses the cutscene font.
* Japanese System, used by the japanese versions that uses the menu font.
* Japanese Event, used by the japanese versions that uses the cutscene font.

Regardless the encoder used, the bytes from `00` to `1F` are used for [special behaviours](#commands).

## Commands

| Id | Bytes | Description
|---------|--------|------------
| 00 | 1 | Message terminator
| 01 | 1 | Prints a space
| 02 | 1 | New line
| 03 | 1 | Reset any modifier
| 04 | 1 | Use the color defined in the [world palette](./03system.md#ftst)
| 05 | 7 | 
| 06 | 1 | 
| 07 | 5 | Define a BGRA color
| 08 | 4 | 
| 09 | 2 | Prints an icon
| 0A | 2 | Scale the text. 100% value is 
| 0B | 2 | Scale the text horizontally. 100% value is 
| 0C | 2 | Define the space in pixels between characters
| 0D | 1 |
| 0E | 2 | 
| 0F | 6 | 
| 10 | 1 | Clear the text, used in baloon text 
| 11 | 5 | Define the text position
| 12 | 3 | 
| 13 | 5 | 
| 14 | 3 | Define the delay, in frames, before printing the next character
| 15 | 3 | Define the delay, in frame, for every character 
| 16 | 2 | 
| 17 | 3 | Set a delay and make the text fades away
| 18 | 3 | 
| 19 | 1 | Use the 2nd font table for the next character
| 1A | 1 | Use the 3rd font table for the next character
| 1B | 1 | Use the 4th font table for the next character
| 1C | 1 | Use the 5th font table for the next character
| 1D | 1 | Use the 6th font table for the next character
| 1E | 1 | Use the 7th font table for the next character
| 1F | 1 | Use the 8th font table for the next character
