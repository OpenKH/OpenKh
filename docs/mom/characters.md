# [Kingdom Hearts Melody of Memory](index.md) Characters

Most of the character and party data can be manipulated using a SQLite database located in: `StreamingAssets\SQLite\Table.db`

## Documented IDs

Format: `Name - AssetBundle Name / PartyTable ID`

* Sora (KH1) - character0000000 / 110000000
* Donald - character0000001 / 110000001
* Goofy - character0000002 / 110000002
* Riku (DDD) - character0000003 / 110000003
* Meow Wow - character0000004 / 110000004
* Komory Bat - character0000005 /  110000005
* Roxas - character0000006 / 110000006
* Axel - character0000007 / 110000007
* Xion - character0000008 / 110000008
* Ventus - character0000009 / 110000009
* Terra - character0000010 / 110000010
* Aqua - character0000011 / 110000011
* Mickey (KH2/DDD) - character0000012 / 110000012
* Aladdin - character0000014 / 110000014
* Ariel - character0000015 / 110000015
* Peter Pan - character0000016 /  110000016
* Beast - character0000017 / 110000017
* Hercules - character0000018 / 110000018
* Mulan - character0000019 / 110000019
* Simba - character0000020 / 110000020
* Experiment 626 - character0000029 / 110000029
* Mickey (DAYS) - character0000030 / 110000030
* Mickey (BBS) - character0000031 / 110000031
* Riku (KH1) - character0000032 / 110000032
* Sora (KH3) - character0000033 / 110000033
* Kairi (KH3) - character0000034 / 110000034

## Issues with Characters

* [All default party leads show large headshot image when put in non-lead position](https://i.imgur.com/z7n5PUx.jpg)
* Multiple of same character causes weird jumping physics, and only one copy visible/exists
* Characters without abilities cause infinite level loading screen as party lead.
* Characters without a DeformedChara assigned will crash the game in the level loading screen. Can be worked around by assigning one in CharacterTable.