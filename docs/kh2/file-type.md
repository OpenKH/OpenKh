# [Kingdom Hearts II](index.md) - file types

| Extension | Description | Tool | 
|-----------|-------------|------|
| [IDX](file//type/idx.md) | File table for an IMG | [kh.cmd.idximg](../tool/kh.cmd.idximg.md)
| [IMG](file//type/idx.md) | Contains a bunch of un/compressed files; they are indexed by IDX | [kh.cmd.idximg](../tool/kh.cmd.idximg.md)
| DBG | Binary file; probably used by a debug menu
| [BAR](file/type/bar.md) | Primary file and data container
| 2DD | A sub-tybe of BAR; usually for menus on the 2D screen buffer
| 2LD | A sub-tybe of BAR; usually for menus on the 3D screen buffer
| ANB | Raw animation data (bone manipulations, positions, etc.)
| DPD | Excuted by the graphical effects engine; contains images, 3D models and scripts
| DPX | Contains various DPD files
| PAX | Graphical effect; contains inside DPX entries
| GBX | Gummiship mission map
| IMGD | Images rendered on the 2D screen buffer
| FAC | A sub-type of IMGD
| IMGZ | A container housing multiple IMGDs with separation layers
| ARD | Event files containing all sorts of miscellaneous information
| MAP | Game map
| MAG | A descriptor for magic; contains a PAX inside
| MDLX | Container for VIF packets; model data and files like textures
| MSET | Moveset; contains effect casters, references ANBs, etc.
| a.fm | A sub-type of BAR; usually used in conjunction with MDLX
| a.us | English localized a.fm (HD releases based on a.fm; PS2 releases based on a.jp)
| a.uk | English localized a.fm (Unused except for spelling differences. [US/UK])
| a.gr | German localized a.fm
| a.sp | Spanish localized a.fm
| a.fr | French localized a.fm
| a.it | Italian localized a.fm
| a.jp | Non-Final Mix a.fm; used only in the JP PS2 Version
| apdx | stripped down version of the a.jp; used in localization builds of KH2 
| SEB | Sound effect
| VAG | Streamed music or voice (monaural audio)
| VAS | Streamed music or voice (stereo audio)
| VSB | A sub-type of BAR; contains VAG
| BGM | Midi-like file
| WD  | Instruents for BGM files
