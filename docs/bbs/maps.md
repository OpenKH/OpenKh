# [Kingdom Hearts: Birth By Sleep](../index.md) - Maps

Maps in BBS consist of an [ARC](.\arc.md) file which is loaded into memory in it's entirety when the map is loaded. The arc containing the map data is named `{WORLD_CODE}{MAP_NUMBER}.arc`. The contents of this arc can vary but it always contains at least the following 4 files, where [map] is short for `{world_code}_{MAP_NUMBER}`:
* [map].pmp - This contains all the models + textures for the map
* [map]a.bcd - This contains the collision data for the map and other, currently unknown stuff
* [map].mss - Unknown
* [map].pvd - Unknown

Additionally many maps will contain a .tm2 file. This is the minimap. Note that the minimap texture does not have the red exit markers drawn on it.