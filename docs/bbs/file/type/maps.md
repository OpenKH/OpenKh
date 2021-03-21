# [Kingdom Hearts: Birth By Sleep](./index.md) - Maps

Maps in BBS consist of an [ARC](.\arc.md) file which is loaded into memory in it's entirety when the map is loaded. The arc containing the map data is named `{WORLD_CODE}{MAP_NUMBER}.arc`. The contents of this arc can vary but it always contains at least the following 4 files, where `{map}` is short for `{world_code}_{MAP_NUMBER}`:
* [{map}.pmp](./pmp.md) - This contains all the models + textures for the map.
* [[map].mss](./mss.md) - This contains the settings for the map's sound, such as reverberation and other things.
* [[map].pvd](./pvd.md) - This contains effects only applied to maps such as fog or glare.

Optionally, some other files are added for extra features, such as these:

* [{map}a.bcd](./bcd.md) - This contains the collision data for the map.
* {map}e.pst - Its use is unknown.
* [{map}nm0.nmd](./nmd.md) - Still unknown, it has to do with the navimap.

Additionally many maps will contain a .tm2 file. This is the minimap. Note that the minimap texture does not have the red exit markers drawn on it.

`FEP` effects can occasionally be within the ARC. As to how they are used in the level, it will most likely have to do with the `PST` file.

In HD versions of the maps, some contain one additional file:

* [[map].env](./env.md) - This file contains extra sound files to be added to the maps externally.