# OpenKH Game Engine

The OpenKH Game Engine aims to re-create the same experience for games from Kingdom Hearts 1 to Kingdom Hearts Dream Drop Distance, allowing to mix assets between those games and unleash modding by expanding the source code or from just unchain the games from the hardware limitations of their original hardwares.

As for today, there is only basic support for Kingdom Hearts II and it runs on Windows, Linux and macOS. Even the Raspberry PI is supported.

## Set-up

In its current form the engine needs your original copy of Kingdom Hearts II for PlayStation 2 or PlayStation 4, no matter from which region. OpenKH will never host any of the original game files as they are copyrighted.

Once the assets and configurations are set, run `OpenKh.Game.exe` on Windows or `dotnet OpenKh.Game.dll` on different operating systems.

### From a PlayStation 2 copy

Insert your original game disc in your computer or mount the ISO from your game's dump. Its content will reveal a bunch of files, but we are interested only in `KH2.IMG` and `KH2.IDX`. The choice to copy those files in a convenient folder or to ask the engine to load the files directly from the disc is yours. Change the `config.yml` to let the game engine knows where is the location of the game's data.

You can optionally decide to extract the content of `KH2.IMG` if you are interested on modding the game's data as the engine supports loading extracted asset. Please refer to [this guide](../tool/CLI.IdxImg/index.md) to know more.

### From a PlayStation 4 copy

Using PlayStation 4 assets is more complicated, but it leads to better results such as 16:9 support and native support for all the main 5 languages. You would need a way to access to the game's data, potentially with a jailbroken PlayStation 4, to get the files `kh2_first.psarc` and `kh2_second.psarc`. You need to extract those two files and merge their extracted content into the same directory. There are various PSARC extractors you can find using your favourite search engine.

The PlayStation 4 assets uses high-definition textures, but they are currently unsupported as we miss a GNF image decoder. In this case you would need to strip out the HD assets with the command line tool OpenKh.Command.HdAssets. A `dotnet OpenKh.Command.HdAssets.dll path_to_ps4_assets --recursive` will just do the trick. Finally change the `config.yml` to let the engine know the directory of the game's data.

## Troubleshooting

The engine creates a file called `openkh.log`, where it stores a high degree of debugging information helpful to understand how the game internally works or to understand what caused a crash.

## Configuration

The engine configuration is stored in `config.yml`, stored in the same directory where the engine's executable is. If you can not find the file, run the game engine executable once to create it.

### Game's data

Change `dataPath` according to the location of your Kingdom Hearts II game's assets. The assets can be packed as `KH2.IMG` and `KH2.IDX` or extracted. The default directory is `data`. If you desire to use a different name for your IDX and IMG, please modify `idxFilePath` and `imgFilePath` accordingly.

### Save data support

There is basic save support to load your own Kingdom Hearts II save game. The location of the save data is defined in `savePath` and its default value is `save`. The engine tracks the latest save used, defined in `lastSave` with a default value of `BISLPM-66675FM-00`. You can obtain saves by converting your virutal memory card from PCSX2 into a folder or by using uLaunchELF to extract the memory card's data from your original PlayStation 2 console. Saves form American, European or Final Mix versions of the game are currently supported, not the original Japanese version.

### Mods support

There is a basic mods support that allows the game engine to override the original game assets with the one you defined, leaving the original data folder untouched. Just place the assets following the original game's structure. For instance, `mod/msg/us/sys.bar` will virtually replace the file in `data/msg/us/sys.bar`. This is true also if the data assets are loaded from `KH2.IMG`.

The `modPath` will define the location of the directory that contains the assets to override. The default directory is `mod`.

### Game resolution

The fields `resolutionWidth` and `resolutionHeight` will set the width and height of the window. When their value is `0` the proper resolution is used accordingly to the game assets, 512x416 for PlayStation 2 and 684x416 for PlayStation 4.

The field `resolutionBoost` allows to multiply the internal resolution. It supports floating point values, so for instance you can use a value of `2.5` to boost the resolution from 684x416 to 1710x1040.

The field `isFullScreen` is self-explainatory and can accept `true` or `false` as values. There are no in-game shortcuts to change it.

All the above values takes effect immediately, so you can modify the resolution whiel the game is running.

Currently there is no way to set the framerate as it is locked to your monitor's refresh rate. The VSync will be always on.

### Languages

It is possible to force a specific language by using the field `regionId`. Its default value is `-1` to automatically set the first language found.

| Value | Description
|-------|------------
| -1    | Auto-detect
| 0     | Japanese
| 1     | United States
| 2     | United Kingdom / Australian
| 3     | Italian
| 4     | Spanish
| 5     | German
| 6     | French
| 7     | Japanese Final Mix

If you are using a translated copy of Kingdom Hearts II: Final Mix you will encounter a crash as the game can not distinguish if to use the Japanese or International text encoding. Set `enforceInternationalTextEncoding` to `true` to mitigate the crash.
