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
# Apply xyz uniform scale to every mesh vertex. Skipped if applyMatrix exists.
scale: 1

# Apply matrix for each input vertex position. This is useful for exported map from KH1.
# Default is null and assume identity matrix.
applyMatrix: [
    1, 0, 0, 0, 
    0,-1, 0, 0, 
    0, 0,-1, 0, 
    0, 0, 0, 1
]

# Specify parent directory path to image files that can be used by `fromFile`
# The path accepts both absolute path, and relative path from folder having `mapdef.yml`.
# Default is `images`. If you define imageDirs, The default `images` will be lost.
imageDirs:
- 'images'
- 'images2'
- 'images3'

# `OpenKh.Command.ImgTool.exe` will be used if `.png` file is passed to `fromFile`
imgtoolOptions: '-b 8'

# Apply textureOptions globally. The individual property of textureOptions will be adopted.
textureOptions:
  # addressU
  # addressV are one of: 'Repeat', 'Clamp', 'RegionClamp', 'RegionRepeat'
  addressU: null
  addressV: null

# Disable triangleStrips optimization. Every output shape becomes triangle.
disableTriangleStripsOptimization: true

# Disable BSP collision builder. Compose single huge collision table.
disableBSPCollisionBuilder: true

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
# Apply max value to each rgba color component of vertex color.
- name: 'four'
  maxIntensity: 128

# Specify maxIntensity to this material.
# Set `transparentFlag: 1` to use transparency texture.
- name: 'five'
  transparentFlag: 0

# This is default declaration of each material.
# And also this is used for fallback purpose.
- name: 'fallbackMaterial'
  ignore: false
  fromFile: null
  fromFile2: null
  noclip: false
  nodraw: false
  surfaceFlags: 0x3f1
  maxIntensity: 128
  imgtoolOptions: null
  textureOptions:
    addressU: null
    addressV: null

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
- `fromFile2` of material (extracted from material's diffuse texture file path)
- material name + ".imd"
- material name + ".png"


## Example of designer tools usage

- Design a full map model data with [Blender](https://www.blender.org/),
  and then export entire world to `.fbx` file format.

_Note :_ The Y coordinate is up vector in KH2.
