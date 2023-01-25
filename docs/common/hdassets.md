# HD assets - Back to [Index](../index.md)

PS4 versions of Kingdom Hearts (and probably other PS3/PS4 remasters) add a special header to every file to override a specific texture or sound file.

For example, the file `arc_en/boss/b01dc00.arc` is the original file found in Final Mix version, wrapped with the remastered header that specifies the files `US_b01dc00_arc0.gnf`, `US_b01dc00_arc1.gnf`, `US_b01dc00_arc2.gnf`, `US_b01dc00_arc3.gnf` and `US_b01dc00_arc4.gnf`. The `GNF` files (PS4 textures) will override the loading of the original assets.

If a file does not contain any remastered asset (like raw binary files that contains only gameplay data) have just the minimum header.

## Tools (Under Construction)

[OpenKh.Command.HdAssets](../tool/CLI.HdAssets.md)