# [Kingdom Hearts DS Games](index.md) - file types

These file formats are found in many Nintendo DS games, not just Kingdom Hearts.

All information was referenced from:
* [Nintendo DS - The VG Resource Wiki](https://wiki.vg-resource.com/Nintendo_DS)
* [Nitro Files - The VG Resource Wiki](https://wiki.vg-resource.com/Nitro_Files#1._NSBMD_Format)
* [Nitro Composer File (*.sdat) Specification](http://www.feshrine.net/hacking/doc/nds-sdat.html#sdat)

| Extension | Description | Tool | 
|-----------|-------------|------|
| arm9.bin | ARM9 executable code; can be either encrypted or decrypted
| arm7.bin | ARM7 executable code
| NFTR | Nitro Font; font data
| NSBMD | Nitro System Binary Model; 3D model data. Often also holds textures and palettes (nsbtx)
| NSBTX | Nitro System Binary Texture; texture and palette data
| NSBCA | Nitro Character Animation; 3D model skeletal animations
| NSBTP | Nitro Texture Pattern; texture-swapping animations
| NSBTA | Nitro Texture Animation; texture UV-change animations
| NSBMA | Nitro Material Animation; material-swap animations
| [SDAT](file//type/sdat.md) | Sound Data; contains audio data in sub-files
| [SSEQ](file//type/sseq.md) | Sound Sequence; an sdat sub-file containing MIDI-like sequenced music. Linked to an sbnk
| [SBNK](file//type/sbnk.md) | Sound Bank; an sdat sub-file containing instrument data for an sseq. Can be linked with up to 4 swar files
| [SWAV](file//type/swav.md) | Sound Wave; contains a single wave sample. Not found in sdat files
| [SWAR](file//type/swar.md) | Sound Wave Archive; an sdat sub-file containing a collection of mono wave samples
| [STRM](file//type/strm.md) | Stream;  an sdat sub-file. It is an individual mono/stereo wave file (PCM8, PCM16 or ADPCM)
