# [OpenKh Tool Documentation](../index.md) - Bdxio

## Overview

Bdxio is useful tool to:

- Decode bdscript from binary bdx file.
- Encode binary bdx file from bdscript.

## Command usage

```bat
OpenKh.Command.Bdxio.exe
1.0.0

Usage: OpenKh.Command.Bdxio [command] [options]

Options:
  --version     Show version information.
  -?|-h|--help  Show help information.

Commands:
  decode        decode bdx
  encode        encode bdx

Run 'OpenKh.Command.Bdxio [command] -?|-h|--help' for more information about a command.
```

### `decode` command

```bat
Usage: OpenKh.Command.Bdxio decode [options] <InputFile> <OutputFile>

Arguments:
  InputFile                    Input bdx file
  OutputFile                   Output text file

Options:
  -?|-h|--help                 Show help information.
  -l|--labels <LABELS>         Additional addresses to decode as code
  -r|--code-revealer           Decode unrevealed as code
  -b|--code-revealer-labeling  Mark unrevealed code as label, and then replace pushImm arg```
```

### `encode` command

```bat
Usage: OpenKh.Command.Bdxio encode [options] <InputFile> <OutputFile>

Arguments:
  InputFile     Input text file
  OutputFile    Output bdx file

Options:
  -?|-h|--help  Show help information.
```
