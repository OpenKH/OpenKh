# [Kingdom Hearts Melody of Memory](index.md) Trigger Maps (Enemy Layout)

Music trigger maps are located within Unity AssetBundles under `StreamingAssets\AssetBundles\regioncommon\languagecommon\music` that corresponds with the music ID/filename.

Each AssetBundle contains 3 different trigger maps labeled `trigger_std_0#01`, with the `#` being a number from 1-3 representing the Beginner, Standard and Proud difficulties respectively.

Each entry in a trigger map represents one object, either an enemy, box, jump, ability, glide note or performer target, with metadata such as defining their horizontal position or if they are "linked" with other triggers requiring multiple buttons be pressed at once to activated and possibly more.

Enemy Hex Layout:
