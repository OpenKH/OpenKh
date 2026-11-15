# [Kingdom Hearts Dream Drop Distance](../index.md) - shop.bin

Location: `/item/jp/bin/`

Contains the data for command prices and availability in Moogle Shops.

Click a link below to jump to a specific section.
- [File Structure](#file-structure)
- [Entry Structure](#entry-structure)
	- [Moogle Shop](#moogle-shop)
	- [Medal Shop](#medal-shop)

## File Structure

| Amount | Description						|
|--------|----------------------------------|
| 1      | Header (12 bytes)				|
| X      | Entries (8 bytes each)			|
| 1      | EOF (CD CD CD CD)				|

## Entry Structure

`0x00` is the Moogle Shop's required level for a given command. The Shop technically contains entries for practically all items, but only sell items based on the player's current shop level. There are 8 shop levels, and the shop levels up as you complete a World. Some items are marked higher than Level 8 (`0x00` = `63`); these are assumed to be unpurchasable.

`0x01` is the required Flick Rush level. Currently the only items gated by this requirement are the Meowjesty, Sudo Neku, and Ursa Circus recipes. 

`0x03` is not entirely known, but may be used as a distinction between the Moogle and Medal Shops.

`0x06` is each command's buying price. Items are sold back to the Moogle Shop for 50% of `0x06`. During Bargain Flurry, items can be bought for 80% of `0x06`. 

`0x02` is a flag for Bargain Flurry-exclusive items. These items are marked as full price; the 20% sale does not apply. 

| Position | Type	|	Description								|
|----------|--------|-------------------------------------------|
| 0x04     | int8	|	[Item Id](../dictionary/items)			|
| 0x05     | int8	|	?										|
| 0x06     | int16	|	Buying Price							|
| 0x00     | int8	|	Shop Level								|
| 0x01     | int8	|	Rush Level								|
| 0x02     | int8	|	Bargain-Exclusive flag					|
| 0x03     | int8	|	Shop Mode?								|

### Moogle Shop

The PC version includes recipes for Frootz Cat, Kab Kannon, R & R Seal, Beatalike, and Tubguin Ace. The 3DS file does not include these entries.

| Item ID<br>(0x04) | Item<br>Name         | ???<br>(0x05) | Price<br>(0x06) | Shop Level<br>(0x00) | Rush Level<br>(0x01) | Shop-Special<br>(0x02) | Shop Mode<br>(0x03) |
|-------------------|----------------------|---------------|-----------------|----------------------|----------------------|------------------------|---------------------|
| 28                | Quick Blitz          | 00            | 100             | 01                   | 00                   | 00                     | 03                  |
| 29                | Blizzard Edge        | 00            | 400             | 04                   | 00                   | 00                     | 03                  |
| 2A                | Dark Break           | 00            | 1500            | 63                   | 00                   | 00                     | 01                  |
| 2B                | Slot Edge            | 00            | 1200            | 63                   | 00                   | 00                     | 01                  |
| 2C                | Blitz                | 00            | 1000            | 07                   | 00                   | 00                     | 03                  |
| 2D                | Meteor Crash         | 00            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 2E                | Spark Dive           | 00            | 200             | 03                   | 00                   | 00                     | 03                  |
| 2F                | Poison Dive          | 00            | 900             | 63                   | 00                   | 00                     | 01                  |
| 30                | Drain Dive           | 00            | 100             | 63                   | 00                   | 00                     | 01                  |
| 31                | Sliding Dash         | 00            | 150             | 01                   | 00                   | 01                     | 03                  |
| 32                | Thunder Dash         | 00            | 600             | 05                   | 00                   | 01                     | 03                  |
| 33                | Sonic Blade          | 00            | 2200            | 08                   | 00                   | 00                     | 03                  |
| 34                | Dark Aura            | 00            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 35                | Zantetsuken          | 00            | 2000            | 63                   | 00                   | 00                     | 03                  |
| 36                | Strike Raid          | 00            | 200             | 03                   | 00                   | 00                     | 03                  |
| 37                | Spark Raid           | 00            | 120             | 63                   | 00                   | 00                     | 01                  |
| 38                | Circle Raid          | 00            | 1200            | 05                   | 00                   | 01                     | 03                  |
| 39                | Aerial Slam          | 00            | 800             | 06                   | 00                   | 00                     | 03                  |
| 3A                | Ars Arcanum          | 00            | 2400            | 63                   | 00                   | 00                     | 01                  |
| 3B                | Dark Splicer         | 00            | 2400            | 63                   | 00                   | 00                     | 01                  |
| 3C                | Gravity Strike       | 00            | 600             | 05                   | 00                   | 00                     | 03                  |
| 3D                | Confusing Strike     | 00            | 400             | 04                   | 00                   | 00                     | 03                  |
| 3E                | Tornado Strike       | 00            | 1500            | 07                   | 00                   | 00                     | 03                  |
| 3F                | Prism Windmill       | 00            | 1500            | 08                   | 00                   | 01                     | 03                  |
| 40                | Timestorm            | 00            | 800             | 06                   | 00                   | 00                     | 03                  |
| 41                | Fire Windmill        | 00            | 600             | 63                   | 00                   | 00                     | 01                  |
| 42                | Icebreaker           | 00            | 1000            | 08                   | 00                   | 00                     | 03                  |
| 43                | Shadowbreaker        | 00            | 120             | 07                   | 00                   | 00                     | 01                  |
| 44                | Magnet Spiral        | 00            | 120             | 08                   | 00                   | 01                     | 03                  |
| 45                | Salvation            | 00            | 2400            | 08                   | 00                   | 00                     | 01                  |
| 46                | Limit Storm          | 00            | 1500            | 63                   | 00                   | 00                     | 01                  |
| 47                | Collision Magnet     | 00            | 1000            | 07                   | 00                   | 00                     | 03                  |
| 48                | Sacrifice            | 00            | 120             | 63                   | 00                   | 00                     | 01                  |
| 49                | Break Time           | 00            | 1200            | 63                   | 00                   | 00                     | 01                  |
| 4A                | Fire                 | 00            | 100             | 01                   | 00                   | 00                     | 03                  |
| 4B                | Fira                 | 00            | 400             | 05                   | 00                   | 00                     | 03                  |
| 4C                | Firaga               | 00            | 1000            | 07                   | 00                   | 00                     | 03                  |
| 4D                | Dark Firaga          | 00            | 120             | 63                   | 00                   | 00                     | 01                  |
| 4E                | Firaga Burst         | 00            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 4F                | Mega Flare           | 00            | 2600            | 63                   | 00                   | 00                     | 01                  |
| 50                | Blizzard             | 00            | 100             | 01                   | 00                   | 00                     | 03                  |
| 51                | Blizzara             | 00            | 400             | 05                   | 00                   | 00                     | 03                  |
| 52                | Blizzaga             | 00            | 1000            | 07                   | 00                   | 00                     | 03                  |
| 53                | Icicle Splitter      | 00            | 700             | 63                   | 00                   | 00                     | 01                  |
| 54                | Deep Freeze          | 00            | 120             | 63                   | 00                   | 00                     | 01                  |
| 55                | Ice Barrage          | 00            | 1500            | 63                   | 00                   | 00                     | 01                  |
| 56                | Thunder              | 00            | 150             | 01                   | 00                   | 01                     | 03                  |
| 57                | Thundara             | 00            | 600             | 05                   | 00                   | 01                     | 03                  |
| 58                | Thundaga             | 00            | 1500            | 07                   | 00                   | 01                     | 03                  |
| 59                | Triple Plasma        | 00            | 1500            | 63                   | 00                   | 00                     | 01                  |
| 5A                | Cure                 | 00            | 100             | 02                   | 00                   | 00                     | 03                  |
| 5B                | Cura                 | 00            | 600             | 06                   | 00                   | 00                     | 03                  |
| 5C                | Curaga               | 00            | 1000            | 08                   | 00                   | 00                     | 03                  |
| 5D                | Esuna                | 00            | 200             | 04                   | 00                   | 00                     | 03                  |
| 5E                | Zero Gravity         | 00            | 200             | 03                   | 00                   | 00                     | 03                  |
| 5F                | Zero Gravira         | 00            | 600             | 06                   | 00                   | 00                     | 03                  |
| 60                | Zero Graviga         | 00            | 1000            | 08                   | 00                   | 00                     | 03                  |
| 61                | Zero Graviza         | 00            | 120             | 08                   | 00                   | 01                     | 03                  |
| 62                | Balloon              | 00            | 300             | 01                   | 00                   | 01                     | 03                  |
| 63                | Balloonra            | 00            | 900             | 05                   | 00                   | 01                     | 03                  |
| 64                | Balloonga            | 00            | 120             | 63                   | 00                   | 00                     | 01                  |
| 65                | Spark                | 00            | 200             | 03                   | 00                   | 00                     | 03                  |
| 66                | Sparkra              | 00            | 800             | 06                   | 00                   | 00                     | 03                  |
| 67                | Sparkga              | 00            | 2000            | 08                   | 00                   | 01                     | 03                  |
| 68                | Faith                | 00            | 2400            | 63                   | 00                   | 00                     | 01                  |
| 69                | Tornado              | 00            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 6A                | Meteor               | 00            | 2600            | 63                   | 00                   | 00                     | 01                  |
| 6B                | Mini                 | 00            | 600             | 63                   | 00                   | 00                     | 01                  |
| 6C                | Blackout             | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 6D                | Time Bomb            | 00            | 450             | 01                   | 00                   | 01                     | 03                  |
| 6E                | Confuse              | 00            | 200             | 02                   | 00                   | 00                     | 03                  |
| 6F                | Bind                 | 00            | 400             | 04                   | 00                   | 00                     | 03                  |
| 70                | Poison               | 00            | 400             | 03                   | 00                   | 00                     | 03                  |
| 71                | Slow                 | 00            | 200             | 02                   | 00                   | 00                     | 03                  |
| 72                | Sleep                | 00            | 100             | 01                   | 00                   | 00                     | 03                  |
| 73                | Sleepra              | 00            | 300             | 05                   | 00                   | 00                     | 03                  |
| 74                | Sleepga              | 00            | 600             | 63                   | 00                   | 00                     | 01                  |
| 75                | Stop                 | 00            | 600             | 63                   | 00                   | 00                     | 01                  |
| 76                | Vanish               | 00            | 120             | 63                   | 00                   | 00                     | 01                  |
| 77                | Potion               | 00            | 20              | 01                   | 00                   | 00                     | 03                  |
| 78                | Hi-Potion            | 00            | 40              | 02                   | 00                   | 00                     | 03                  |
| 79                | Mega-Potion          | 00            | 100             | 05                   | 00                   | 00                     | 03                  |
| 7A                | Drop-Me-Not          | 00            | 40              | 02                   | 00                   | 00                     | 03                  |
| 7B                | Drop-Me-Never        | 00            | 240             | 63                   | 00                   | 00                     | 01                  |
| 7C                | Panacea              | 00            | 50              | 02                   | 00                   | 00                     | 03                  |
| 7D                | Elixir               | 00            | 3000            | 01                   | 00                   | 01                     | 03                  |
| 7E                | Megalixir            | 00            | 4000            | 63                   | 00                   | 00                     | 01                  |
| 7F                | Dream Candy          | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 02                | High Jump            | 00            | 100             | 63                   | 00                   | 00                     | 01                  |
| 04                | Slide Roll           | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 05                | Dark Roll            | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 07                | Sonic Impact         | 00            | 300             | 05                   | 00                   | 00                     | 03                  |
| 08                | Double Impact        | 00            | 300             | 63                   | 00                   | 00                     | 01                  |
| 0A                | Super Glide          | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 0B                | Shadow Slide         | 00            | 500             | 63                   | 00                   | 00                     | 01                  |
| 0C                | Doubleflight         | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 0E                | Wake-Up Block        | 00            | 400             | 06                   | 00                   | 00                     | 03                  |
| 0F                | Link Block           | 00            | 500             | 08                   | 00                   | 00                     | 03                  |
| 10                | Sliding Block        | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 11                | Dark Barrier         | 00            | 500             | 63                   | 00                   | 00                     | 01                  |
| 12                | Counter Rush         | 00            | 300             | 63                   | 00                   | 00                     | 01                  |
| 13                | Counter Aura         | 00            | 400             | 06                   | 00                   | 00                     | 03                  |
| 14                | Shadow Strike        | 00            | 500             | 63                   | 00                   | 00                     | 01                  |
| 15                | Payback Raid         | 00            | 400             | 07                   | 00                   | 00                     | 03                  |
| 16                | Payback Blast        | 00            | 400             | 07                   | 00                   | 00                     | 03                  |
| 17                | Aerial Recovery      | 00            | 400             | 06                   | 00                   | 00                     | 03                  |
| 18                | Steep Climb          | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 19                | Rapid Descent        | 00            | 400             | 63                   | 00                   | 00                     | 01                  |
| 1A                | Sliding Sidewinder   | 00            | 400             | 07                   | 00                   | 00                     | 03                  |
| 1B                | Sliding Crescent     | 00            | 400             | 07                   | 00                   | 00                     | 03                  |
| 00                | Fleeting Figment     | 06            | 150             | 04                   | 00                   | 00                     | 03                  |
| 01                | Fleeting Fancy       | 06            | 300             | 07                   | 00                   | 00                     | 03                  |
| 02                | Fleeting Fantasy     | 06            | 1000            | 07                   | 00                   | 01                     | 03                  |
| 03                | Lofty Figment        | 06            | 100             | 03                   | 00                   | 00                     | 03                  |
| 04                | Lofty Fancy          | 06            | 400             | 08                   | 00                   | 00                     | 03                  |
| 05                | Lofty Fantasy        | 06            | 800             | 63                   | 00                   | 00                     | 01                  |
| 06                | Rampant Figment      | 06            | 50              | 01                   | 00                   | 00                     | 03                  |
| 07                | Rampant Fancy        | 06            | 200             | 05                   | 00                   | 00                     | 03                  |
| 08                | Rampant Fantasy      | 06            | 500             | 01                   | 00                   | 01                     | 03                  |
| 09                | Dulcet Figment       | 06            | 100             | 03                   | 00                   | 00                     | 03                  |
| 0A                | Dulcet Fancy         | 06            | 400             | 08                   | 00                   | 00                     | 03                  |
| 0B                | Dulcet Fantasy       | 06            | 1000            | 08                   | 00                   | 01                     | 03                  |
| 0C                | Malleable Fantasy    | 06            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 0D                | Prickly Fantasy      | 06            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 0E                | Wild Fantasy         | 06            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 0F                | Epic Fantasy         | 06            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 10                | Charming Fantasy     | 06            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 11                | Brilliant Fantasy    | 06            | 2000            | 63                   | 00                   | 00                     | 01                  |
| 12                | Intrepid Figment     | 06            | 50              | 02                   | 00                   | 00                     | 03                  |
| 13                | Intrepid Fancy       | 06            | 400             | 08                   | 00                   | 00                     | 03                  |
| 14                | Intrepid Fantasy     | 06            | 1000            | 08                   | 00                   | 01                     | 03                  |
| 15                | Savage Fantasy       | 06            | 2000            | 08                   | 00                   | 00                     | 03                  |
| 16                | Noble Figment        | 06            | 100             | 03                   | 00                   | 00                     | 03                  |
| 17                | Noble Fancy          | 06            | 200             | 05                   | 00                   | 00                     | 03                  |
| 18                | Noble Fantasy        | 06            | 800             | 03                   | 00                   | 01                     | 03                  |
| 19                | Grim Figment         | 06            | 50              | 02                   | 00                   | 00                     | 03                  |
| 1A                | Grim Fancy           | 06            | 300             | 07                   | 00                   | 00                     | 03                  |
| 1B                | Grim Fantasy         | 06            | 300             | 07                   | 00                   | 00                     | 03                  |
| 1C                | Vibrant Figment      | 06            | 50              | 01                   | 00                   | 00                     | 03                  |
| 1D                | Vibrant Fancy        | 06            | 200             | 05                   | 00                   | 00                     | 03                  |
| 1E                | Vibrant Fantasy      | 06            | 500             | 02                   | 00                   | 01                     | 03                  |
| 1F                | Troubling Figment    | 06            | 50              | 01                   | 00                   | 00                     | 03                  |
| 20                | Troubling Fancy      | 06            | 100             | 03                   | 00                   | 00                     | 03                  |
| 21                | Troubling Fantasy    | 06            | 500             | 02                   | 00                   | 01                     | 03                  |
| 22                | Wondrous Figment     | 06            | 150             | 04                   | 00                   | 00                     | 03                  |
| 23                | Wondrous Fancy       | 06            | 300             | 07                   | 00                   | 00                     | 03                  |
| 24                | Wondrous Fantasy     | 06            | 400             | 08                   | 00                   | 00                     | 03                  |
| 01                | Tama Sheep Recipe    | 03            | 50              | 03                   | 00                   | 00                     | 02                  |
| 04                | Pricklemane Recipe   | 03            | 50              | 01                   | 00                   | 00                     | 02                  |
| 05                | Hebby Repp Recipe    | 03            | 50              | 01                   | 00                   | 00                     | 02                  |
| 08                | Fin Fatale Recipe    | 03            | 100             | 02                   | 00                   | 00                     | 02                  |
| 0A                | Necho Cat Recipe     | 03            | 200             | 04                   | 00                   | 00                     | 02                  |
| 0E                | Iceguin Ace Recipe   | 03            | 200             | 06                   | 00                   | 00                     | 02                  |
| 10                | Escarglow Recipe     | 03            | 100             | 03                   | 00                   | 00                     | 02                  |
| 11                | KO Kabuto Recipe     | 03            | 200             | 04                   | 00                   | 00                     | 02                  |
| 16                | Halbird Recipe       | 03            | 300             | 07                   | 00                   | 00                     | 02                  |
| 18                | Fishboné Recipe      | 03            | 100             | 05                   | 00                   | 00                     | 02                  |
| 19                | Flowbermeow Recipe   | 03            | 1000            | 63                   | 00                   | 01                     | 02                  |
| 1D                | Tatsu Blaze Recipe   | 03            | 100             | 05                   | 00                   | 00                     | 02                  |
| 1F                | Woeflower Recipe     | 03            | 200             | 03                   | 00                   | 00                     | 02                  |
| 22                | Me Me Bunny Recipe   | 03            | 100             | 06                   | 00                   | 00                     | 02                  |
| 29                | Aura Lion Recipe     | 03            | 500             | 08                   | 00                   | 00                     | 02                  |
| 2F                | Frootz Cat Recipe    | 03            | 500             | 07                   | 00                   | 00                     | 02                  |
| 31                | Kab Kannon Recipe    | 03            | 500             | 07                   | 00                   | 00                     | 02                  |
| 32                | R & R Seal Recipe    | 03            | 200             | 02                   | 00                   | 00                     | 02                  |
| 34                | Beatalike Recipe     | 03            | 1000            | 08                   | 00                   | 00                     | 02                  |
| 35                | Tubguin Ace Recipe   | 03            | 200             | 02                   | 00                   | 00                     | 02                  |
| 00                | Confetti Candy       | 07            | 30              | 01                   | 00                   | 00                     | 03                  |
| 01                | Shield Cookie        | 07            | 30              | 01                   | 00                   | 00                     | 03                  |
| 02                | Block-It Chocolate   | 07            | 30              | 01                   | 00                   | 00                     | 03                  |
| 03                | Ice Dream Cone       | 07            | 30              | 01                   | 00                   | 00                     | 03                  |
| 04                | Royal Cake           | 07            | 240             | 01                   | 00                   | 00                     | 01                  |
| 05                | Confetti Candy 2     | 07            | 120             | 04                   | 00                   | 00                     | 03                  |
| 06                | Shield Cookie 2      | 07            | 120             | 04                   | 00                   | 00                     | 03                  |
| 07                | Block-It Chocolate 2 | 07            | 120             | 04                   | 00                   | 00                     | 03                  |
| 08                | Ice Dream Cone 2     | 07            | 120             | 04                   | 00                   | 00                     | 03                  |
| 0A                | Confetti Candy 3     | 07            | 240             | 07                   | 00                   | 01                     | 03                  |
| 0B                | Shield Cookie 3      | 07            | 240             | 07                   | 00                   | 01                     | 03                  |
| 0C                | Block-It Chocolate 3 | 07            | 240             | 07                   | 00                   | 01                     | 03                  |
| 0D                | Ice Dream Cone 3     | 07            | 240             | 07                   | 00                   | 01                     | 03                  |
| 00                | Balloon              | 08            | 80              | 01                   | 00                   | 00                     | 03                  |
| 03                | Candy Goggles        | 08            | 80              | 01                   | 00                   | 00                     | 03                  |
| 04                | Water Barrel         | 08            | 80              | 01                   | 00                   | 00                     | 03                  |
| 05                | Paint Gun: Red       | 08            | 10              | 01                   | 00                   | 00                     | 03                  |
| 06                | Paint Gun: Blue      | 08            | 10              | 01                   | 00                   | 00                     | 03                  |
| 07                | Paint Gun: Green     | 08            | 10              | 01                   | 00                   | 00                     | 03                  |
| 08                | Paint Gun: Yellow    | 08            | 10              | 01                   | 00                   | 00                     | 03                  |
| 09                | Paint Gun: White     | 08            | 10              | 01                   | 00                   | 00                     | 03                  |
| 0A                | Paint Gun: Black     | 08            | 10              | 01                   | 00                   | 00                     | 03                  |
| 0B                | Paint Gun: Purple    | 08            | 10              | 01                   | 00                   | 00                     | 03                  |
| 0C                | Paint Gun: Sky Blue  | 08            | 10              | 01                   | 00                   | 00                     | 03                  |

### Medal Shop

Medal Shops begin at offset `05B0` on PC, and can otherwise be distinguished by `0x07` = `00`.

| Item ID<br>(0x04) | Item<br>Name         | ???<br>(0x05) | Price<br>(0x06) | Shop Level<br>(0x00) | Rush Level<br>(0x01) | Shop-Special<br>(0x02) | Shop Mode<br>(0x03) |
|-------------------|----------------------|---------------|-----------------|----------------------|----------------------|------------------------|---------------------|
| 2B                | Slot Edge            | 00            | 150             | 01                   | 00                   | 00                     | 00                  |
| 30                | Drain Dive           | 00            | 120             | 01                   | 00                   | 00                     | 00                  |
| 41                | Fire Windmill        | 00            | 250             | 01                   | 00                   | 00                     | 00                  |
| 46                | Limit Storm          | 00            | 1000            | 05                   | 00                   | 00                     | 00                  |
| 48                | Sacrifice            | 00            | 1000            | 05                   | 00                   | 00                     | 00                  |
| 4E                | Firaga Burst         | 00            | 2000            | 08                   | 00                   | 00                     | 00                  |
| 54                | Deep Freeze          | 00            | 1200            | 05                   | 00                   | 00                     | 00                  |
| 59                | Triple Plasma        | 00            | 1500            | 08                   | 00                   | 00                     | 00                  |
| 64                | Balloonga            | 00            | 1200            | 05                   | 00                   | 00                     | 00                  |
| 6B                | Mini                 | 00            | 200             | 01                   | 00                   | 00                     | 00                  |
| 75                | Stop                 | 00            | 150             | 01                   | 00                   | 00                     | 00                  |
| 7B                | Drop-Me-Never        | 00            | 100             | 05                   | 00                   | 00                     | 00                  |
| 7E                | Megalixir            | 00            | 4000            | 08                   | 00                   | 00                     | 00                  |
| 7F                | Dream Candy          | 00            | 200             | 01                   | 00                   | 00                     | 00                  |
| 05                | Dark Roll            | 00            | 700             | 08                   | 00                   | 00                     | 00                  |
| 11                | Dark Barrier         | 00            | 750             | 08                   | 00                   | 00                     | 00                  |
| 0B                | Thunderaffe Recipe   | 03            | 280             | 04                   | 00                   | 00                     | 00                  |
| 0C                | Kooma Panda Recipe   | 03            | 80              | 00                   | 00                   | 00                     | 00                  |
| 0D                | Pegaslick Recipe     | 03            | 330             | 05                   | 00                   | 00                     | 00                  |
| 15                | Juggle Pup Recipe    | 03            | 180             | 02                   | 00                   | 00                     | 00                  |
| 23                | Drill Sye Recipe     | 03            | 150             | 01                   | 00                   | 00                     | 00                  |
| 25                | Majik Lapin Recipe   | 03            | 400             | 06                   | 00                   | 00                     | 00                  |
| 2D                | Meowjesty Recipe     | 03            | 2000            | 05                   | 14                   | 00                     | 00                  |
| 2E                | Sudo Neku Recipe     | 03            | 2000            | 04                   | 14                   | 00                     | 00                  |
| 30                | Ursa Circus Recipe   | 03            | 2000            | 07                   | 14                   | 00                     | 00                  |
| 00                | Confetti Candy       | 07            | 40              | 01                   | 00                   | 00                     | 00                  |
| 01                | Shield Cookie        | 07            | 40              | 01                   | 00                   | 00                     | 00                  |
| 02                | Block-It Chocolate   | 07            | 40              | 01                   | 00                   | 00                     | 00                  |
| 03                | Ice Dream Cone       | 07            | 40              | 01                   | 00                   | 00                     | 00                  |
| 04                | Royal Cake           | 07            | 300             | 08                   | 00                   | 00                     | 00                  |
| 05                | Confetti Candy 2     | 07            | 100             | 04                   | 00                   | 00                     | 00                  |
| 06                | Shield Cookie 2      | 07            | 100             | 04                   | 00                   | 00                     | 00                  |
| 07                | Block-It Chocolate 2 | 07            | 100             | 04                   | 00                   | 00                     | 00                  |
| 08                | Ice Dream Cone 2     | 07            | 100             | 04                   | 00                   | 00                     | 00                  |
| 0A                | Confetti Candy 3     | 07            | 200             | 07                   | 00                   | 00                     | 00                  |
| 0B                | Shield Cookie 3      | 07            | 200             | 07                   | 00                   | 00                     | 00                  |
| 0C                | Block-It Chocolate 3 | 07            | 200             | 07                   | 00                   | 00                     | 00                  |
| 0D                | Ice Dream Cone 3     | 07            | 200             | 07                   | 00                   | 00                     | 00                  |
| 00                | Balloon              | 08            | 50              | 01                   | 00                   | 00                     | 00                  |
| 03                | Candy Goggles        | 08            | 50              | 01                   | 00                   | 00                     | 00                  |
| 04                | Water Barrel         | 08            | 50              | 01                   | 00                   | 00                     | 00                  |
| 05                | Paint Gun: Red       | 08            | 05              | 01                   | 00                   | 00                     | 00                  |
| 06                | Paint Gun: Blue      | 08            | 05              | 01                   | 00                   | 00                     | 00                  |
| 07                | Paint Gun: Green     | 08            | 05              | 01                   | 00                   | 00                     | 00                  |
| 08                | Paint Gun: Yellow    | 08            | 05              | 01                   | 00                   | 00                     | 00                  |
| 09                | Paint Gun: White     | 08            | 05              | 01                   | 00                   | 00                     | 00                  |
| 0A                | Paint Gun: Black     | 08            | 05              | 01                   | 00                   | 00                     | 00                  |
| 0B                | Paint Gun: Purple    | 08            | 05              | 01                   | 00                   | 00                     | 00                  |
| 0C                | Paint Gun: Sky Blue  | 08            | 05              | 01                   | 00                   | 00                     | 00                  |
