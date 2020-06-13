# [Kingdom Hearts II](../../index.md) - MSN

Mission file. They are located in `msn/{language}/` and describe how a certain map should behave. Internally they are just [bar](bar.md) files.

## Mission

This is the entry point of the file. It is a binary-type file and it always have as a name `{WORLD_ID}{MAP_INDEX}`

eg. for the file `EH21_MS101`, where `EH` is the [world](../../worlds.md) and `21` is the map index, the mission name is called `EH21`.

How this file is read is currently unknown.

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
