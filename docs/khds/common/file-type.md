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
| nftr | Nitro Font; font data
| nsbmd | Nitro System Binary Model; 3D model data. Often also holds textures and palettes (nsbtx)
| nsbtx | Nitro System Binary Texture; texture and palette data
| nsbca | Nitro Character Animation; 3D model skeletal animations
| nsbtp | Nitro Texture Pattern; texture-swapping animations
| nsbta | Nitro Texture Animation; texture UV-change animations
| nsbma | Nitro Material Animation; material-swap animations
| sdat | Sound Data; contains audio data in sub-files
| sseq | Sound Sequence; an sdat sub-file containing MIDI-like sequenced music. Linked to an sbnk
| sbnk | Sound Bank; an sdat sub-file containing instrument data for an sseq. Can be linked with up to 4 swar files
| swav | Sound Wave; contains a single wave sample. Not found in sdat files
| swar | Sound Wave Archive; an sdat sub-file containing a collection of mono wave samples
| strm | Stream;  an sdat sub-file. It is an individual mono/stereo wave file (PCM8, PCM16 or ADPCM)
