# [OpenKh Tool Documentation](../index.md) - MapGen

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

```yml
# Apply xyz uniform scale to every mesh vertex
scale: 1

# Specify path to image files can be used by `fromFile`
# The path accepts both absolute path, and relative path from folder having `mapdef.yml`.
imageDirs:
- 'images'
- 'images2'
- 'images3'

# `OpenKh.Command.ImgTool.exe` will be used if `.png` file is passed to `fromFile`
imgtoolOptions: '-b 4'

# Declare supplemental info per material of any associated meshes in 3D model data
materials:

# Pattern matching to hit a material. It ignores case.
# Accept like: `*clip`, `*old*`, `material???`
- name: 'Material'

# The mesh having this material is skipped.
- name: 'Material'
  ignore: true

# The mesh having this material is not rendered.
# But collision plane is generated.
- name: 'clip'
  nodraw: true

# The mesh having this material is rendered.
# But collision plane is not generated.
- name: 'falsewall'
  noclip: true

# Specify one image file for this material.
# It uses material name as part of file name if omitted.
# For `one`:
# - `one.imd`
# - `one.png`
- name: 'one'
  fromFile: '1.imd'

# Apply specific `imgtoolOptions` to this material.
- name: `two`
  fromFile: '2.png'
  imgtoolOptions: '-b 8'

# Specify `surfaceFlags` to this material.
# `0x3f1` is one of floor representation.
- name: 'three'
  surfaceFlags: 0x3f1

# Specify maxIntensity to this material.
# Apply to alpha component of vertex color.
- name: 'four'
  maxIntensity: 128

# This is default declaration of each material.
# And also this is used for fallback purpose.
- name: 'fallbackMaterial'
  ignore: false
  fromFile: null
  noclip: false
  nodraw: false
  surfaceFlags: 0x3f1
  maxIntensity: 128
  imgtoolOptions: null

# Customize output bar entries for output `.map` file.
bar:
  model:
    name: "MAP"
  texture:
    name: "MAP"
  coct:
    name: "ID_e"
  doct:
    name: "eh_1"
```

## Example of designer tools usage

- Design a full map model data with [Blender](https://www.blender.org/),
  and then export entire world to `.fbx` file format.

_Note :_ The Y coordinate is up vector in KH2.
