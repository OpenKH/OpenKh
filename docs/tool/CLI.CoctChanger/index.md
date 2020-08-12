# [OpenKh Tool Documentation](../index.md) - CoctChanger

## Overview

CoctChanger is useful tool to:

- Create single room collision data (coct)
- Replace coct inside many map files at once.

## Command usage

```bat
OpenKh.Command.CoctChanger.exe

1.0.0

Usage: OpenKh.Command.CoctChanger [command] [options]

Options:
  --version         Show version information
  -?|-h|--help      Show help information

Commands:
  create-room-coct  coct file: create single closed room
  use-this-coct     map file: replace coct with your coct

Run 'OpenKh.Command.CoctChanger [command] -?|-h|--help' for more information about a command.
```

### `create-room-coct` command

Specify: coctFileOutput

```bat
OpenKh.Command.CoctChanger.exe create-room-coct room.coct --bbox -1800,-500,-1800,1800,1000,1800

```

```bat
OpenKh.Command.CoctChanger.exe create-room-coct -h

coct file: create single closed room

Usage: OpenKh.Command.CoctChanger create-room-coct [options] <CoctOut>

Arguments:
  CoctOut           Output coct

Options:
  -?|-h|--help      Show help information
  -b|--bbox <BBOX>  bbox in model 3D space: minX,Y,Z,maxX,Y,Z (default: ...)

```

### `use-this-coct` command

Batch coct injector for map files.

Specify: inDir, outDir, and coctFile.

```bat
OpenKh.Command.CoctChanger.exe use-this-coct C:\A\eh18 H:\Proj\pcsx2\bin\inject.f266b00b\map\jp room.coct

C:\A\eh18\eh18.map
```

### `show-stats` command

Specify: `.map` file, otherwise treated as `.coct` file.

```bat
OpenKh.Command.CoctChanger.exe show-stats tt05.map

# ID_t:0 (MapCollision)
     352 collision mesh groups.
     306 collision meshes.
   1,076 collisions.
   1,968 vertices.
     869 planes.
     943 bounding boxes.
      11 surface flags.

# CH_t:0 (CameraCollision)
     205 collision mesh groups.
     151 collision meshes.
     250 collisions.
     492 vertices.
     236 planes.
     151 bounding boxes.
       3 surface flags.

# COL_:0 (LightData)
      55 collision mesh groups.
      33 collision meshes.
      41 collisions.
     114 vertices.
      17 planes.
      14 bounding boxes.
       1 surface flags.
```
