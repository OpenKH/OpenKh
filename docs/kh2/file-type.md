# [Kingdom Hearts II](index.md) - File Types

| Container                  | Description                                                         | Tools                                      |
|----------------------------|---------------------------------------------------------------------|--------------------------------------------|
| [2DD](file/type/2ld.md#2D-sequence-(2DD)) | A sub-tybe of BAR; usually for menus on the 2D screen buffer | OpenKh.Tools.LayoutEditor
| [2LD](file/type/2ld.md)         | A sub-tybe of BAR; usually for menus on the 3D screen buffer | OpenKh.Tools.LayoutEditor
| [AI](file/ai/index.md)          | Located in BAR files; used as a scripting language | OpenKh.Command.Bdxio
| [ANB](file/anb/anb.md)          | Raw animation data (bone manipulations, positions, etc.) | OpenKh.Tools.Kh2MsetEditorCrazyEdition or OpenKH.Command.ANBMaker
| [ARD](file/type/areadata.md)    | Event files containing all sorts of miscellaneous information | OpenKh.Tools.Kh2MapStudio or OpenKh.Command.Spawnscript
| [BAR](file/type/bar.md)         | Primary file and data container | OpenKh.Tools.BarEditor
| BGM                             | Midi-like file
| [COCT](file/type/coct.md)       | Data to instance collision detection | OpenKh.Command.Mapgen or OpenKh.Command.CoctChanger
| DBG                             | Binary file; likely used for debug menu 
| [DOCT](file/type/doct.md)       | Defines occlusion culling (hiding obstructed objects) | OpenKh.Command.Mapgen or OpenKh.Command.DoctChanger
| DPD                             | Executed by the graphical effects engine; contains images, 3D models and scripts | OpenKh.Tools.Kh2ObjectEditor
| DPX                             | Contains various DPD files | OpenKh.Tools.Kh2ObjectEditor
| [FAC](file/type/image.md#fac)   | A sub-type of IMGD
| GBX                             | Gummi Ship mission map
| [IDX](file//type/idx.md)        | File table for an IMG                                               | [kh.cmd.idximg](../tool/kh.cmd.idximg.md)  |
| [IMG](file//type/idx.md)        | Contains a bunch of un/compressed files; they are indexed by IDX    | [kh.cmd.idximg](../tool/kh.cmd.idximg.md)  |
| [IMGD](file/type/image.md#imgd) | Images rendered on the 2D screen buffer
| [IMGZ](file/type/image.md#imgz) | A container housing multiple IMGDs with separation layers
| MAG                             | A descriptor for magic; contains a PAX inside
| [MAP](file/map.md)              | Game map | OpenKh.Tools.Kh2MapStudio or OpenKh.Command.MapGen
| [MDLX](file/type/mdlx.md)       | Container for VIF packets; model data and files like textures | OpenKh.Tools.KH2MdlxEditor
| [MSET](file/anb/mset.md)        | Moveset; contains effect casters, references ANBs, etc. | OpenKh.Tools.KH2MsetEditor
| [MSG](file/type/msg.md)         | Storing localized HUD textures for worlds | OpenKh.Tools.Kh2TextEditor
| [MSN](file/type/msn.md)         | Mission file; defines how maps behave
| PAX                             | Graphical effects; contains inside DPX entries
| [SEB](file/type/seb.md)         | Sound effect; on PC, these files are pointers to an SCD in the objects remastered folder
| VAG                             | Streamed music or voice (monaural audio)
| VAS                             | Streamed music or voice (stereo audio)
| VSB                             | A sub-type of BAR; contains VAG
| WD                              | Instruments for BGM files; unused in the PC release.
| [SCD](file/type/scd.md)         | Audio container, exclusive to the HD Remaster.
| a.fm                            | A sub-type of BAR; usually used in conjunction with MDLX
| a.fr                            | French localized a.fm
| a.gr                            | German localized a.fm
| a.it                            | Italian localized a.fm
| a.jp                            | Non-Final Mix a.fm; used only in the JP PS2 Version
| a.sp                            | Spanish localized a.fm
| a.uk                            | English localized a.fm (Unused except for spelling differences. [US/UK])
| a.us                            | English localized a.fm (HD releases based on a.fm; PS2 releases based on a.jp)
| apdx                            | Stripped down version of a.jp; used in localization builds of KH2
