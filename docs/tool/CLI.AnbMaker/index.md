# [OpenKh Tool Documentation](../index.md) - AnbMaker

## Overview

Create anb file.

## Command usage

```bat
OpenKh.Command.AnbMaker.exe --help
1.0.0

Usage: OpenKh.Command.AnbMaker [command] [options]

Options:
  --version     Show version information.
  -?|-h|--help  Show help information.

Commands:
  anb           fbx file: fbx to raw anb
  anb-ex        fbx file: fbx to interpolated motion anb
  export-raw    raw anb file: bone and animation to fbx

Run 'OpenKh.Command.AnbMaker [command] -?|-h|--help' for more information about a command.
```

## `anb` command

Create new anb file from fbx, using raw (matrices) motion format.
Optionally this can update motion inside existing mset file.

```
fbx file: fbx to raw anb

Usage: OpenKh.Command.AnbMaker anb [options] <InputModel> <Output>

Arguments:
  InputModel                            fbx input
  Output                                anb output

Options:
  -?|-h|--help                          Show help information.
  -r|--root-name <ROOT_NAME>            specify root armature node name
  -m|--mesh-name <MESH_NAME>            specify mesh name to read bone data
  -x|--node-scaling <NODE_SCALING>      apply scaling to each source node
                                        Default value is: 1.
  -a|--animation-name <ANIMATION_NAME>  specify animation name to read bone data
  -w|--mset-file <MSET_FILE>            optionally inject new motion into mset directly
  -i|--mset-index <MSET_INDEX>          zero based target index of bar entry in mset file
                                        Default value is: 0.
```

## `anb-ex` command

Create new anb file from fbx, using interpolated motion format.
Optionally this can update motion inside existing mset file.

```
fbx file: fbx to interpolated motion anb

Usage: OpenKh.Command.AnbMaker anb-ex [options] <InputModel> <Output>

Arguments:
  InputModel                            fbx input
  Output                                anb output

Options:
  -?|-h|--help                          Show help information.
  -r|--root-name <ROOT_NAME>            specify root armature node name
  -m|--mesh-name <MESH_NAME>            specify mesh name to read bone data
  -a|--animation-name <ANIMATION_NAME>  specify animation name to read bone data
  -x|--node-scaling <NODE_SCALING>      apply scaling to each source node
                                        Default value is: 1.
  -w|--mset-file <MSET_FILE>            optionally inject new motion into mset directly
  -i|--mset-index <MSET_INDEX>          zero based target index of bar entry in mset file
                                        Default value is: 0.
```

## `export-raw` command

This is a debug command. Export skeleton animation from raw motion anb file to new fbx file.

```
raw anb file: bone and animation to fbx

Usage: OpenKh.Command.AnbMaker export-raw [options] <InputMotion> <OutputFbx>

Arguments:
  InputMotion   anb input
  OutputFbx     fbx output

Options:
  -?|-h|--help  Show help information.
```
