# [OpenKh Tool Documentation](../index.md) - TexFooter

## Overview

TexFooter is useful tool to:

- Change texture footer data stored in mdlx or map files.

Note: texture footer data is same thing as described at `Texture Animation Metadata` section of [Raw Texture](../../kh2/file/raw-texture.md).

## Command usage

```
Usage: OpenKh.Command.TexFooter [command] [options]

Options:
  --version     Show version information
  -?|-h|--help  Show help information

Commands:
  bin-to-yml    texture footer bin -> yml
  export        map file: export map or mdlx texture footer. map -> yml
  import        map file: import map or mdlx texture footer. yml -> map
  yml-to-bin    yml -> texture footer bin

Run 'OpenKh.Command.TexFooter [command] -?|-h|--help' for more information about a command.
```

### `bin-to-yml` and `yml-to-bin` command

This is for low level support of footer data generation.

### `export` command

```
map file: export map or mdlx texture footer. map -> yml

Usage: OpenKh.Command.TexFooter export [options] <MapFile> <OutputDir>

Arguments:
  MapFile       Map file
  OutputDir     Output dir

Options:
  -?|-h|--help  Show help information
```

### `import` command

```
map file: import map or mdlx texture footer. yml -> map

Usage: OpenKh.Command.TexFooter import [options] <MapFile> <YmlFile>

Arguments:
  MapFile       Map file (in and out)
  YmlFile       YML file (`P_EX100.footer.yml`)

Options:
  -?|-h|--help  Show help information
```

## Using export and import

```bat
rem export
OpenKh.Command.TexFooter export nm07.map

rem import
OpenKh.Command.TexFooter import nm07.map
```

Some files are exported from `nm07.map`, and saved at the same folder in default behavior.

```
$ ls -lh
total 1.8M
-rwxrwx---+ 1 KU KU 2.4K Jan  4 21:54 nm07.footer-MAP-0.png
-rwxrwx---+ 1 KU KU 3.9K Jan  4 21:54 nm07.footer-MAP-1.png
-rwxrwx---+ 1 KU KU 2.0K Jan  4 21:54 nm07.footer-MAP-2.png
-rwxrwx---+ 1 KU KU  961 Jan  4 21:54 nm07.footer-MAP-3.png
-rwxrwx---+ 1 KU KU  961 Jan  4 21:54 nm07.footer-MAP-4.png
-rwxrwx---+ 1 KU KU  961 Jan  4 21:54 nm07.footer-MAP-5.png
-rwxrwx---+ 1 KU KU  961 Jan  4 21:54 nm07.footer-MAP-6.png
-rwxrwx---+ 1 KU KU  13K Jan  4 21:54 nm07.footer.yml
-rwxrwx---+ 1 KU KU 1.8M Aug  7 19:37 nm07.map
```

`nm07.footer.yml` stores enough information to rebuild Texture Animation Metadata.

## map.footer.yml structure

Notes:

- `Textures` is always required in order to support all of multiple textures contained in a single map/mdlx.
- `ShouldEmitDMYAtFirst` and `ShouldEmitKN5` are simple flags to to make it successful of consistency r/w tests.
- `UnkFooter` is used by some mdlx files, but its working is still unknown.

```yml
Textures:
  MAP:
    UvscList: []
    TextureAnimationList: []
    UnkFooter: []
    ShouldEmitDMYAtFirst: false
    ShouldEmitKN5: true
```

### UvscList

Notes:

- `TextureIndex` is zero based index to targeting `GS Info`.
- U is horizontal-speed
- V is vertical-speed
- _6400000_ will be enough speed to scroll 32 pixels in 1 second.

```yml
Textures:
  MAP:
    UvscList:
    - TextureIndex: 0
      UScrollSpeed: 0
      VScrollSpeed: 2236962.25
    - TextureIndex: 1
      UScrollSpeed: 0
      VScrollSpeed: 2236962.25
```

### TextureAnimationList

Notes:

- `TextureIndex` is zero based index to targeting `GS Info`.
- `SlotTable` is array of int16 ranging from _-32768_ to _32767_.
- Some fields are ommited dut to:
  - `BitsPerPixel` is 4 or 8 acquired from `SpriteImageFile`.
  - `NumAnimations` is count field computed by `FrameGroupList` count.
  - `SpriteWidth` is single sprite width acquired from `SpriteImageFile`.
  - `SpriteHeight` is single sprite height acquired from `SpriteImageFile`, and divided by `NumSpritesInImageData`.
  - `OffsetSlotTable`, `OffsetAnimationTable`, and `OffsetSpriteImage` are offsets automatically computed.

```yml
Textures:
  MAP:
    TextureAnimationList:
    - Unk1: 28
      TextureIndex: 11
      FrameStride: 0
      BaseSlotIndex: 0
      SpriteImageFile: ./nm07.footer0.png
      NumSpritesInImageData: 1
      UOffsetInBaseImage: 192
      VOffsetInBaseImage: 160
      DefaultAnimationIndex: 0
      SlotTable: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0]
      FrameGroupList:
      - IndexedFrameList:
          0:
            MinimumLength: 60
            MaximumLength: 60
            SpriteImageIndex: 0
            FrameControl: DisableSprite
            FrameIndexDelta: 0
          1:
            MinimumLength: 60
            MaximumLength: 60
            SpriteImageIndex: 0
            FrameControl: EnableSprite
            FrameIndexDelta: 0
          2:
            MinimumLength: 0
            MaximumLength: 0
            SpriteImageIndex: 0
            FrameControl: Jump
            FrameIndexDelta: -2
```
