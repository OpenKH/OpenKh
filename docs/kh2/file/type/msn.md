# [Kingdom Hearts II](../../index.md) - MSN

Mission file. They are located in `msn/{language}/` and describe how a certain map should behave. Internally they are just [bar](bar.md) files.

## Mission

This is the entry point of the file. It is a binary-type file and it always have as a name `{WORLD_ID}{MAP_INDEX}`

eg. for the file `EH21_MS101`, where `EH` is the [world](../../worlds.md) and `21` is the map index, the mission name is called `EH21`.

How this file is read is not completely understood.

What is known about this file is as follows:

| Offset | Type   | Description |
|--------|--------|-------------|
| 0x00   | uint16 | Magic Code, always `2`
| 0x02   | BitArray | Boolean Flag Array [1]
| 0x03   | BitArray | Boolean Flag Array [2]
| 0x04   | BitArray | Boolean Flag Array [3]
| 0x05   | BitArray | Boolean Flag Array [4]
| 0x06   | uint16 | Unknown. Probably actually a bit array.
| 0x08   | Unknown (Either uint16 or BitArray) | Pause Menu Controller
| 0x0A   | uint16 | Pause Menu Information Text
| 0x0C   | BitArray | Boolean Flag Array [5]
| 0x0D   | byte | [Bonus Reward](00battle.md#bons)
| 0x0E   | uint16 | Antiform Multiplier
| - | - | -
| 0x1C | uint16 | Intro Camera Controller


### Boolean Flag Arrays

This file uses Bit Arrays to store boolean flags for use during the missions. Known arrays/flags are as follows:

#### Boolean Flag Array 3

| Bit | Description |
|-----|-------------|
| 1 | Is Boss Battle?
| 2 | Is Drive Disabled?
| 4 | ???
| 8 | ???
| 16 | ???
| 32 | ???
| 64 | ???
| 128 | ???

#### Boolean Flag Array 4

| Bit | Description |
|-----|-------------|
| 1 | ???
| 2 | Can Mickey save Sora?
| 4 | ???
| 8 | Is Magic Disabled?
| 16 | Does the "Continue" option on the Game Over screen retry the mission?
| 32 | Are Summons Enabled?
| 64 | ???
| 128 | ???

### Pause Menu Controller
Defines the type of Pause Menu Available when pressing start. Known values are as follows:

| Value | Description |
|-------|-------------|
| 00 | Only pause menu
| 01 | Only pause menu
| 02 | 4 button pause menu with text (cont, retry, help, quit)
| 03 | Only pause menu
| 04 | Only pause menu
| 05 | Pause menu with text
| 06 | Pause menu with text
| 07 | Only pause menu
| 08 | 3 button pause menu (retry, help, quit)
| 09 | Jiminy's journal
| 0A | Only pause menu
| 0B | Only pause menu
| 0C | Only pause menu
| 0D | Only pause menu
| 0E | Only pause menu
| 0F | Only pause menu
| -|-
| 1C | Pause Menu with Text and 3 Options (Continue, Jiminy's Journal, Save)
| 1E | Pause Menu with Text and 4 Options (Continue, Retry, Help, Quit)
| 1F | ???

This is most likely a BitArray and needs to be explored further.

### Pause Menu Information Text
Loads a string of ID uint16 from `.\msg\{LANGUAGE}\{WORLD_ID}.bar` to be displayed in the information box while the game is paused.

### Antiform Multiplier
Multiplies the chance of getting Antiform in a battle. Seems to affect the frequency with which Mickey can save Sora, too.

### Intro Camera Controller
Decides whether or not the game will do the intro camera zooms on the character at the beginning of the battle. The only known value is '01'.

## The `miss` entry

Present for most of the mission files, it is a PAX file that it is always named `miss`.

Its purpose is currently unknown.

## Animation loader

A mission file can contain between 0 and multiple animation loaders. Their names are usually `0a`, `1a`.

Its purpose is currently unknonw.

## Sequences

Most of the missions have a pair of IMGD and SEQD. The most common file names is `ct_e`, `st_h`, `ed_h`, `gh_h`, `cb_e`, `ed_t`, `ti_e`, `st_t` and many others.

How they are used is unknown and the meaning of their file names is currently unknonw.

## Boss script

A mission that hosts a boss battle has a `ms_b` file, which is an AI script.

## Unknown scripts

`ms_d`, `ms_m`, `ms_a`, `ms_g`, `kino` are some of the script names where their purposes is still unknown.
