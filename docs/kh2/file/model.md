# [Kingdom Hearts II](../index) - Models

Found inside [MDLX](type/mdlx) or MAP files as a [BAR](type/bar) type 4, it contains 3d meshes and texture pointers to render a full 3D model.

There are two type of models, one for [objects](#object-model) and another one for [maps](#map-model). They comes with minor differences and they can be identified by their first byte in the header.

## Map model

The simplest of the two models. Its type identifier is `2`. Every file contains a reserved area of `0x90` bytes, used to store pointers at run-time.

### Header

| Offset | Type | Description |
|--------|------|-------------|
| 0x00   | uint32 | Model identifier. Always `2` for map files.
| 0x04   | uint32 | Unknown. They are often 0.
| 0x08   | uint32 | Unknown. They are often 0.
| 0x0c   | uint32 | Next offset, but it is always 0.
| 0x10   | uint32 | Number of [DMA Chain Maps](#dma-chain-map)
| 0x14   | uint16 | ??? va4
| 0x16   | uint16 | Number of [alb2 (???)](#alb2)
| 0x18   | uint32 | Offset of [alb2 (???)](#alb2)
| 0x1c   | uint32 | Offset of an [unknown portion of data](#map-unknown-data)

### DMA Chain Map

Stored straight after the model [header](#header).

| Offset | Type | Description |
|--------|------|-------------|
| 0x00   | uint32 | [VIF program offset](#vif-program)
| 0x04   | uint32 | Texture Index
| 0x08   | uint32 | Unknown
| 0x0c   | uint32 | Unknown

### alb2

Its use is unknown. The name is given after [Kenjiuno's source code](https://gitlab.com/kenjiuno/khkh_xldM/-/blob/master/khiiMapv/MEForm.cs#L520).

The first 4 bytes stores the offset of `alb2` table. Then the amount of alb2 specified in the [header](#header) is stored as `uint32`. Each of them represents the absolute offset to a single `alb2` entry.

An `alb2` entry is a list of `uint16`, terminating with a `0xffff`.

A map can be successfully rendered without using this information.

### Map unknown data

Another structure of unknown purpose. Its content is an array of `uint16` with a `0xffff` indicating the array termination.

Most of the times, each element is stored sequencially from 0 to n-1, and the number never surpasses the [Dma Chain Map}(#dma-chain-map) count.

## Object model

Type identifier `3`.

## VIF program