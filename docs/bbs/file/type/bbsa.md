# [Kingdom Hearts Birth By Sleep](index.md) - Archive format

The game loads all the game files from Birth By Sleep Archive, in short `BBSA`.

All the BBSA are located in `PSP_GAME/USRDIR/` and they have the name of `BBS0.DAT`, `BBS1.DAT`, `BBS2.DAT`, `BBS3.DAT` and `BBS4.DAT`.

Birth By Sleep comes with a Data Install option, where based on the level of installation it will copy `BBS1`, `BBS2` and `BBS3`. The files are stored in those archives in a way where `BBS1` contains the most used files, `BBS0` contains the common files loaded only once and `BBS4` just few PMF movies that is useless to store in a installation file.

Since `BBS1`, `BBS2` and `BBS3` are copied to the PSP Memory Stick, those are encrypted to prevent modification. The encryption system used is [PDG](#pdg-keys) and it is the one that PSP firmware provides to game developers.

## BBSA format

When referred as `sector`, it means ISO sector. Each sector is 2048 bytes long.

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File identifier, always `bbsa`
| 04     | int   | Version, `5` for JP and `6` for other builds
| 08     | short | [Partition](#partition) count
| 0a     | short | Unknown
| 0c     | short | Unknown
| 0e     | short | [Directory](#directory) count
| 10     | int   | [Partition](#partition) offset
| 14     | int   | [Directory](#directory) offset
| 18     | short | [Archive Partition](#archive-partition) sector
| 1a     | short | Archive 0 start sector
| 1c     | int  | Total sector count
| 20     | int | Archive 1 start sector
| 24     | int | Archive 2 start sector
| 28     | int | Archive 3 start sector
| 2c     | int | Archive 4 start sector

### Partition

A typical directory name for a partition is `arc/boss`.

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Directory name [hash](#name-hashing)
| 04     | short | Files count
| 06     | short | [Partition file entries](#partition-file-entry) offset

### Partition file entry

All the file names are stored without extension, but officially they have [`.arc`](arc.md) extension (source: Birth By Sleep remastered for PS3/PS4).

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File name [hash](#name-hashing)
| 04     | bit 0-11 | Sector count
| 04     | bit 12-31 | Start sector

### Directory

A directory entry contains two name hashes, where one is the full directory path and the second one is the file name without extension. The two combined gives the full path. If the `Sector Count` is 0xFFF (the maximum possible value) it means that the file is meant to be streamed and not loaded in memory.

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File name [hash](#name-hashing)
| 04     | bit 0-11 | Sector count
| 04     | bit 12-31 | Start sector
| 08     | int   | Directory path [hash](#name-hashing)

### Archive partition

The purpose of this structure is still unknown. It seems to contain some information related to the file content from the [partition file entries](#partition-file-entry).

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | [partition file](#partition-file-entry) name [hash](#name-hashing)
| 04     | short | [archive partition entry](#archive-partition-entry) offset
| 06     | byte  | [archive partition entry](#archive-partition-entry) count
| 07     | byte  | Unknown

### Archive partition entry

The purpose of this structure is still unknown. Each name represents an existing file entry in one of the [partition file entries](#partition-file-entry).

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Unknown
| 02     | int   | Name [hash](#name-hashing)

## Name hashing

The hash is calculate using a non-modified version of the CRC32 algorithm with `0xEDB88320` as polynomial.

The following subroutines are used to calculate the hash:

| Game      | Subroutine |
|-----------|-------------|
| ULJM05600 | sub_8AC7580 |

## PDG keys

The following keys are used from the game to decrypt, at runtime, the BBSA files:

| Game version | Key |
|--------------|-----|
| Japanese     | `9A88ED5C33D95313320C3BC997FF10E7 A931E3B557A16F5B98A6E2195D07D4AF 18E597E96C559AD378DED05F3C25AB9C`
| USA/European | `7F0067C280626625276E8C3EB8307345 8F67981EACF0717434B1A5F98A0CD18E 77B9DE64CD1FC39279D190564728A378`
| Final Mix    | `2A7069ED492539395AD9A8616C060B57 749B1E1F547E8A7043E4BA807D7E3D4E C111DA2CF00E66AAEADD609EEEA6FC8A`
