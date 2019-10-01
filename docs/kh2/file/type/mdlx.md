# [Kingdom Hearts II](../../index) - MDLX (MoDeL eXtended)
The MDLX format is an extension of the MDLS format from Kingdom Hearts 1. It
is a BAR encapsulating the 3D data of the model(0x04), textures(0x07), object
definition(0x17) and potentially other additional informations such as the AI.
This page will mostly define the 3D data of the file as other informations will
be documented separately.


## 3D Data Structure

Kingdom Hearts II splits models in packets of up to a theoretical limit of 16KB
due to the memory limit of the PS2 CPU VU1, processing each of those parts,
which we will subsequently call subparts.


An MDLX has at least a bone structure and a model(several models can be present
in the case of, ie, a model for the shadows).
Bone entries use an old technique for skeletal animation called
Scale-Rotate-Translate or SRT. All bones SRT values are derived from their
parents.

Each subpart contains one or more VIF packet which contains 
The VIF packets must not be larger than 100 QWC or else the game will begin to
slow down dramatically.


### 3D Data Structure Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | void | A memory buffer used by the game as Lookup Tables |
| 0x90 | uint32_t | The version of the format(currently 3) |
| 0x94 | uint32_t | Reserved |
| 0x98 | uint32_t | Reserved |
| 0x9C | uint32_t | Next model header |
| 0xA0 | uint16_t | Bone count |
| 0xA2 | uint16_t | Unknown |
| 0xA4 | uint32_t | Bone offset |
| 0xA8 | uint32_t | Unknown offset |
| 0xAC | uint16_t | Model Subpart Count |
| 0xAE | uint16_t | Unknown |

### 3D Data Subpart Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | uint32_t | Unknown |
| 4 | uint32_t | Texture index |
| 8 | uint32_t | Unknown |
| 0xC | uint32_t | Unknown |
| 0x10 | uint32_t | DMA Offset |
| 0x14 | uint16_t | MAT Offset |
| 0x18 | uint16_t | DMA Size |
| 0x1C | uint32_t | Unknown |

### 3D Data Bone Entry 

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0      | uint16_t | Index |
| 2 | uint16_t | Reserved |
| 4 | int32_t | Parent |
| 8 | uint32_t | Unknown |
| 0xC | uint32_t | Unknown |
| 0x10 | f32 | Scale X |
| 0x14 | f32 | Scale Y |
| 0x18 | f32 | Scale Z |
| 0x1C | f32 | Scale W |
| 0x20 | f32 | Rotation X |
| 0x24 | f32 | Rotation Y |
| 0x28 | f32 | Rotation Z |
| 0x2C | f32 | Rotation W |
| 0x30 | f32 | Translation X |
| 0x34 | f32 | Translation Y |
| 0x38 | f32 | Translation Z |
| 0x3C | f32 | Translation W |


### VIF Data Structure


### Kaitai file structure

Due to the complexity of the file format a Kaitai parser has not been written
yet. Should you be in need of an emitter/parser you can look at those
[two](https://code.govanify.com/govanify/kh2mdlx)
[tools](https://code.govanify.com/govanify/kh2vif) in the meanwhile.
