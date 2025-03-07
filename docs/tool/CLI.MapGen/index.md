# [OpenKh Tool Documentation](../index.md) - MapGen <!-- omit in toc -->

TOC

- [Overview](#overview)
- [Command usage](#command-usage)
  - [Supported 3D model data formats](#supported-3d-model-data-formats)
  - [Supported 3D model data concepts](#supported-3d-model-data-concepts)
  - [Example: fbx to map](#example-fbx-to-map)
- [mapdef.yml](#mapdefyml)
  - [Material definition](#material-definition)
  - [scale](#scale)
  - [applyMatrix](#applymatrix)
  - [imageDirs](#imagedirs)
  - [imgtoolOptions](#imgtooloptions)
  - [textureOptions](#textureoptions)
  - [uvscList](#uvsclist)
  - [disableTriangleStripsOptimization](#disabletrianglestripsoptimization)
  - [disableBSPCollisionBuilder](#disablebspcollisionbuilder)
  - [disableBSPCollisionBuilder2](#disablebspcollisionbuilder2)
  - [ignore](#ignore)
  - [nodraw](#nodraw)
  - [noclip](#noclip)
  - [fromFile](#fromfile)
  - [surfaceFlags](#surfaceflags)
  - [group](#group)
  - [maxColorIntensity](#maxcolorintensity)
  - [maxAlpha](#maxalpha)
  - [transparentFlag](#transparentflag)
  - [skipConversionIfExists](#skipconversionifexists)
  - [reuseImd](#reuseimd)
  - [uvscIndex](#uvscindex)
  - [ground](#ground)
  - [floorLevel](#floorlevel)
  - [cameraClip](#cameraclip)
  - [cameraFlags](#cameraflags)
  - [lightClip](#lightclip)
  - [lightFlags](#lightflags)
  - [noShadow](#noshadow)
  - [alphaAdd](#alphaadd)
  - [alphaSubtract](#alphasubtract)
  - [normal](#normal)
  - [bar](#bar)
  - [nococt](#nococt)
  - [nodoct](#nodoct)
  - [collisionPartitionSize](#collisionpartitionsize)
  - [doctPartitionSize](#doctpartitionsize)
  - [fog](#fog)
  - [bgColor](#bgcolor)
  - [onColorTable](#oncolortable)
  - [default](#default)
  - [imageDirs and fromFile](#imagedirs-and-fromfile)
- [Example of designer tools usage](#example-of-designer-tools-usage)

## Overview

MapGen is a tool to create `.map` file from 3D model data.

## Command usage

```bat
OpenKh.Command.MapGen.exe -h
1.0.0

Usage: OpenKh.Command.MapGen [options] <InputFile> <OutputMap>

Arguments:
  InputFile     Input file: mapdef.yml or model.{fbx,dae}
  OutputMap     Output map file

Options:
  --version     Show version information
  -?|-h|--help  Show help information
```

### Supported 3D model data formats

MapGen utilizes Assimp (Open Asset Import Library) to import 3D model data.

Thus Assimp supported formats will be usable.

See also: https://github.com/assimp/assimp/blob/master/doc/Fileformats.md

### Supported 3D model data concepts

- Mesh
- Face (vertex indices)
- Vertex position (x, y, z)
- Vertex UV component (s, t)
- Vertex color (r, g, b, a)
- Material name
- Material's diffuse texture file path

### Example: fbx to map

Command line:

```bat
OpenKh.Command.MapGen.exe terrain.fbx
```

Output:

```
DEBUG ymlFile is "H:\Proj\khkh_xldM\newMap\terrain\mapdef.yml"
DEBUG baseDir is "H:\Proj\khkh_xldM\newMap\terrain"
DEBUG Loading 3D model file "H:\Proj\khkh_xldM\newMap\terrain\terrain.fbx" using Assimp.
DEBUG Starting triangle strip conversion for 1 meshes.
DEBUG Mesh: Plane (256 faces, 1,024 vertices)
DEBUG Output: 289 vertices, 36 triangle strips.
DEBUG The conversion has done.
DEBUG Starting mesh splitter and vif packets builder.
DEBUG Output: 10 vif packets.
DEBUG The builder has done.
DEBUG Starting vifPacketRenderingGroup builder.
DEBUG Output: 1 groups.
DEBUG Going to load material "geo".
DEBUG Load image from "H:\Proj\khkh_xldM\newMap\terrain\images\geo.imd"
DEBUG Building map file structure.
DEBUG Writing to "H:\Proj\khkh_xldM\newMap\terrain\terrain.map".
DEBUG Done
```

## mapdef.yml

3D model data cannot hold all of extra data needed for map conversion.
Thus write `mapdef.yml` file as needed, and place it to the same folder having 3D model data.

### Material definition

```yml
# Declare supplemental info per material of any associated meshes in 3D model data
materials:
# Pattern matching to hit a material. It ignores case.
# Accept like: `*clip`, `*old*`, `material???`
# It respects only first matched material. If there are 2 or more hits, later ones will be ignored.
- name: '*'
  fromFile: 'floor.imd'

# This never reach.
- name: 'ceil'
  fromFile: 'ceil.imd'
```

### scale

```yml
# Apply xyz uniform scale to every mesh vertex. Skipped if applyMatrix exists.
scale: 1
```

### applyMatrix

```yml
# Apply matrix for each input vertex position.
# Default is null and assume identity matrix.
applyMatrix: [
    1, 0, 0, 0, 
    0,-1, 0, 0, 
    0, 0,-1, 0, 
    0, 0, 0, 1
]

# This matrix is useful for maps exported from KH1.
```

### imageDirs

```yml
# Specify parent directory path to image files that can be used by `fromFile`
# The path accepts both absolute path, and relative path from folder having `mapdef.yml`.
# Default is `images`. If you define imageDirs, The default `images` will be lost.
imageDirs:
- 'images'
- 'images2'
- 'images3'
```

### imgtoolOptions

`OpenKh.Command.ImgTool.exe` will be used if `.png` file is passed to `fromFile`.

```yml
# Apply globally
imgtoolOptions: '-b 8'

# Apply to specific material, and override global settings.
- name: `floor`
  imgtoolOptions: '-b 8 -p'
```

### textureOptions

```yml
# Apply textureOptions globally. The individual property of textureOptions will be adopted.
textureOptions:
  # addressU
  # addressV are one of: 'Repeat', 'Clamp', 'RegionClamp', 'RegionRepeat'
  addressU: 'Repeat'
  addressV: 'Repeat'

# Apply to specific material. This overrides global setting.
- name: `floor`
  textureOptions:
    addressU: 'Repeat'
    addressV: 'Repeat'
```

### uvscList
```yml
# Values to use to control the speed of horizontal and vertical UV scrolling.
# Mapgen doesn't seem to append these values, so they'll need to be added manually.
# Additionally, looping will need to be fixed in the GSInfo portion of the texture that has UV Scrolling.
# The 129th-132nd bytes in that texture control this.
uvscList:
  - index: 0
    u: 64000000
    v: 64000000
```

### disableTriangleStripsOptimization

```yml
# Disable triangleStrips optimization. Every output shape becomes triangle.
disableTriangleStripsOptimization: true
```

### disableBSPCollisionBuilder

```yml
# Disable BSP collision builder. Compose single huge collision table.
disableBSPCollisionBuilder: true
```

### disableBSPCollisionBuilder2

```yml
# Alternate version of Disable BSP collision builder. Splits faces into their own mesh.
disableBSPCollisionBuilder2: true
```

### ignore

```yml
# The mesh having this material is skipped.
- name: 'Material'
  ignore: true
```

### nodraw

```yml
# The mesh having this material is not rendered.
# But collision plane is generated.
- name: 'clip'
  nodraw: true
```

### noclip

```yml
# The mesh having this material is rendered.
# But collision plane is not generated.
- name: 'falsewall'
  noclip: true
```

### fromFile

```yml
# Specify one image file for this material.
# It uses material name as part of file name if omitted.
# For `floor`:
# - `floor.imd`
# - `floor.png`
- name: 'floor'
  fromFile: 'floor_texture.imd'
```

### surfaceFlags
```yml
# Specify `surfaceFlags` to this material.
# `0x3f1` is one of floor representation.
# This value is made up of 4 bitflags.
# They control things like whether Inverse Kinematics should apply, if a player can grab the ledge, etc.
# Grabbable Ledges should only be placed on top of surfaces, some buggier interactions with ledges may occur otherwise.
- name: 'floor'
  surfaceFlags: 0x3f1
```
Bit 1
```
Kind: 0x1, 0x2, 0x3, 0x4, may go up to 0x0F?
Hit_Player: 0x10
Hit_Enemy: 0x20
Hit_Flyenemy: 0x40
Hit_Attack: 0x80
```
Bit 2
```
Hit_Safety: 0x1
IK: 0x2
Grabbable Ledge: 0x4
Barrier: 0x8
MSG_Wall: 0x10
Callback: 0x20
Carrib_Disp: 0x40 or 0x80
```
Bit 3 (Unknown uses an additional one?)
```
Belt: 0x1
Polygon_SE: 0x2 or 0x4
Hit_RTN: 0x8
Nohit_Floor: 0x10
Unk: 0x20, 0x40, 0x60, 0x80, 0xA0, 0xC0, 0xE0, 0x100, 0x120, 0x140, 0x160
```

### group
```yml
# Specify `group` to this material.
# Only usable on collision meshes, when using the disableBSPCollisionBuilder2 option.
# When used, this will assign a group value to faces with this material.
# This can turn the collision on or off depending on the value used for MapVisibility in the ARD.
- name: 'floor'
  group: 1
```


### maxColorIntensity

maxColorIntensity: from 0 to 255

```yml
# Set maxColorIntensity globally.
maxColorIntensity: 128

# Specify maxColorIntensity to the material for overriding global settings.
# Apply max value to each RGB color component of vertex color.
- name: 'floor'
  maxColorIntensity: 128
```

### maxAlpha

maxAlpha: from 0 to 255

```yml
# Set maxAlpha globally.
maxAlpha: 128

# Specify maxAlpha to the material for overriding global settings.
# Apply max value to each Alpha color component of vertex color.
- name: 'floor'
  maxAlpha: 128
```

### transparentFlag

```yml
# Specify maxIntensity to this material.
# Set `transparentFlag: 1` to use transparency texture.
- name: 'five'
  transparentFlag: 0
```

### skipConversionIfExists

```yml
# Specify true if you want to skip png to imd conversion.
skipConversionIfExists: true
```

### reuseImd

```yml
# Specify true if you want to reuse `.imd` file converted by ImgTool.
# This is useful if diffuse texture file path is `images/tex.png`, and omit re-conversion.
reuseImd: true
```

### uvscIndex
```yml
# Index of UV Scrolling values to use.
# These are found at the end of the texture file.
# "_KN5" denotes the end of the number of entries.
- name: 'six'
  uvscIndex: 0
```


### ground
Ground value in collision. </br>
These values are used to determine which footsteps to play when walking or running across the collision with that materials name. </br>
Most sound effects will not play without modifying [12soundinfo.bar](/docs/kh2/file/type/12soundinfo.md) for that rooms world.</br>
PAX like water splashes and footprints also depend on the collision having specific values. </br> 
For light collision, this is index to `onColorTable[]`.
```yml
- name: 'floor'
  ground: 1
```
What each value corresponds to is shown below.
```
1: Stone
2: Stone_SP
3: Dirt*
4: Dirt_SP
5: Sand
6: Sand_SP
7: Wood
8: Board
9: Creakwood
10: Lawn*
11: Grass
12: Drygrass
13: Water*
14: Deepwater
15: Snow*
16: Metal
17: Ironwire
18: Carpet
19: Cloth
20: Sofa
21: Roof
22: Roof_SP
23: Electric
24: Electric_SP
25: Glass
26: Rubber
27: Bone
28: Paper
29: Fragment
30: Last
*This will play PAX Effects if available.
```

### floorLevel

FloorLevel value in collision

```yml
- name: 'floor'
  floorLevel: 1
```

### cameraClip

Setting cameraClip to true will generate camera collision plane.

```yml
- name: 'camera'
  nodraw: true
  noclip: true
  cameraClip: true
  cameraFlags: 0x000003F0
```

### cameraFlags

This is Collision.Attributes (surfaceFlags) of camera collision data.

```yml
- name: 'camera'
  nodraw: true
  noclip: true
  cameraClip: true
  cameraFlags: 0x000003F0
```

### lightClip

Setting lightClip to true will generate light collision plane.

```yml
- name: 'light'
  nodraw: true
  noclip: true
  lightClip: true
  lightFlags: 0x000803F1
```

### lightFlags

This is Collision.Attributes (surfaceFlags) of light collision data.

```yml
- name: 'light'
  nodraw: true
  noclip: true
  lightClip: true
  lightFlags: 0x000803F1
```

### noShadow

```yml
- name: 'floor'
  noShadow: true
```

### alphaAdd

```yml
- name: 'floor'
  alphaAdd: true
```

### alphaSubtract

```yml
- name: 'floor'
  alphaSubtract: true
```

### normal

Set `IsSpecular` to true, and then attach normal vector.

```yml
- name: 'lens_flare'
  normal: true
```

### bar

```yml
# Customize output bar entries for output `.map` file.
# toFile is option in case of needs to save raw data individually.
bar:
  model:
    name: "MAP"
    toFile: 'MAP_0.model'
  texture:
    name: "MAP"
    toFile: 'MAP_0.modeltexture'
  coct:
    name: "ID_e"
    toFile: 'ID_e_0.coct'
  doct:
    name: "eh_1"
    toFile: 'eh_1_0.doct'
  camera:
    name: "CH_e"
    toFile: 'CH_e.coct'
  light:
    name: "COL_"
    toFile: 'COL_.coct'
```

### nococt

Applying `nococt: true` will skip COCT generation. default is `nococt: false`.

```yml
nococt: true
```

This is a mandatory option if we build SK0 model, because the bounding box of COCT has limitation of INT16 range (-32768 ~ 32767).

### nodoct

Applying `nodoct: true` will skip DOCT generation. default is `nodoct: false`.

```yml
nodoct: true
```

This is a mandatory option if we build SK0 model, because the bounding box of DOCT has limitation of INT16 range (-32768 ~ 32767).

### collisionPartitionSize

Split nodes until single node has up to specified mesh face count.

```yml
collisionPartitionSize: 300
```

### doctPartitionSize

Split nodes until single node has up to specified mesh face count.

```yml
doctPartitionSize: 500
```

### fog

Fog effect. Color is AABBGGRR format.

```yml
fog:
  color: 0x80402010
  min: 100
  max: 700
  near: 0
  far: 255
```

### bgColor

Background color in AABBGGRR format.

```yml
bgColor: 0x80804010
```

### onColorTable

Map colors up to 16 colors. `0x80808080` are appended on lack. Excess is dropped. Color is in AABBGGRR format.

```yml
onColorTable:
  - 0x80808080
  - 0x80808080
  - 0x80808080
```

### default

```yml
# This is default declaration of each material.
# And also this is used for fallback purpose.
- name: 'fallbackMaterial'
  ignore: false
  fromFile: null
  fromFile2: null
  noclip: false
  nodraw: false
  surfaceFlags: 0x3f1
  maxColorIntensity: null
  maxAlpha: null
  imgtoolOptions: null
  textureOptions:
    addressU: null
    addressV: null
```

### imageDirs and fromFile

You can specify imageDirs like:

```
imageDirs:
- 'images'
- 'images2'
- 'images3'
```

The image file associated with material will be located in this rule:

`{baseDir}/{eachInputDir}/{eachFileName}`

baseDir:

- The folder having `mapdef.yml` file.

eachInputDir:

- `images` (from `imageDirs:` section)
- `images2` (from `imageDirs:` section)
- `images3` (from `imageDirs:` section)
- The folder having `mapdef.yml` file.

eachFileName:

- `fromFile` of material (you can specify)
- `fromFile2` of material (extracted from material's diffuse texture file path as imd extension, if reuseImd set)
- `fromFile3` of material (extracted from material's diffuse texture file path)
- material name + ".imd"
- material name + ".png"


## Example of designer tools usage

- Design a full map model data with [Blender](https://www.blender.org/),
  and then export entire world to `.fbx` file format.
- Ideally, you'll want to generate the regular map model without collision, then create a lower-poly collision mesh, where you select specific vertices and define each set as materials as needed.
- _Note :_ The Y coordinate is up vector in KH2.

