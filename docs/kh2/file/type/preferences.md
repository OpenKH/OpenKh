# [Kingdom Hearts II](../../index.md) - Preferences

Pref file inside [03system](./03system.md).

Defines preferences. It is a [BAR](bar.md) file containing the following subfiles:

| File | Description |
|--------|---------------|
| plyr 	 | [Player](#Plyr)
| fmab 	 | [Form Abilities](#Fmab)
| prty 	 | [Party](#Prty)
| sstm 	 | [System](#Sstm)
| magi 	 | [Magic](#Magi)

### Plyr

Each pointer leads to a specific entry's offset.

### Plyr Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer Count [uint]
| 59 	 | Pointers [uint]
| 10 	 | Plyr Entries

### Plyr Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | float | Attack Y Offset
| 4 | float | Attack Radius
| 8 | float | Attack Min H
| 12 | float | Attack Max H
| 16 | float | Attack V Angle
| 20 | float | Attack V Range
| 24 | float | Attack S Range
| 28 | float | Attack H Angle
| 32 | float | Attack U Min H
| 36 | float | Attack U Max H
| 40 | float | Attack U Range
| 44 | float | Attack J Front
| 48 | float | Attack Air Min H
| 52 | float | Attack Air Max H
| 56 | float | Attack Air Big H Offset
| 60 | float | Attack UV0
| 64 | float | Attack JV0
| 68 | float | Attack First V0
| 72 | float | Attack Combo V0
| 76 | float | Attack Finish V0
| 80 | float | Blow Recovery H
| 84 | float | Blow Recovery V
| 88 | float | Blow Recovery Time
| 92 | float | Auto Lock On Range
| 96 | float | Auto Lock On Min H
| 100 | float | Auto Lock On Max H
| 104 | float | Auto Lock On Time
| 108 | float | Auto Lock On H Adjust
| 112 | float | Auto Lock On Inner Adjust


### Fmab

Each pointer leads to a specific entry's offset.

### Fmab Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer Count [uint]
| 5 	 | Pointers [uint]
| 5 	 | Fmab Entries

### Fmab Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | float | High Jump Height
| 4 | float | Air Dodge Height
| 8 | float | Air Dodge Speed
| 12 | float | Air Slide Time
| 16 | float | Air Slide Speed
| 20 | float | Air Slide Brake
| 24 | float | Air Slide Stop Brake
| 28 | float | Air Slide Star Time
| 32 | float | Glide Speed
| 36 | float | Glide Fall Ratio
| 40 | float | Glide Fall Height
| 44 | float | Glide Fall Max
| 48 | float | Glide Acceleration
| 52 | float | Glide Start Height
| 56 | float | Glide End Height
| 60 | float | Glide Turn Speed
| 64 | float | Dodge Roll Star Time

### Prty

Each pointer leads to a specific entry's offset.

### Prty Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer Count [uint]
| 70 	 | Pointers [uint]
| 5 	 | Prty Entries

### Prty Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | float | Walk Speed
| 4 | float | Run Speed
| 8 | float | Jump Height
| 12 | float | Turn Speed
| 16 | float | Hang Height
| 20 | float | Hang Margin
| 24 | float | Stun Time
| 28 | float | Mp Drive
| 32 | float | Up Down Speed
| 36 | float | Dash Speed
| 40 | float | Acceleration
| 44 | float | Brake
| 48 | float | Subjective Offset

### Sstm

Each pointer leads to a specific entry's offset.

### Sstm Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer Count [uint]
| 1 	 | Pointers [uint]
| 1 	 | Sstm Entries

### Sstm Entry

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | float | Ceiling Stop
| 4 | float | Ceiling Disable Command Time
| 8 | float | Hang Range H
| 12 | float | Hang Range L
| 16 | float | Hang Range XZ
| 20 | float | Fall Max
| 24 | float | Blow Brake XZ
| 28 | float | Blow Min XZ
| 32 | float | Blow Brake Up
| 36 | float | Blow Up
| 40 | float | Blow Speed
| 44 | float | Blow To Hit Back
| 48 | float | Hit Back
| 52 | float | Hit Back Small
| 56 | float | Hit Back To Jump
| 60 | float | Fly Blow Brake
| 64 | float | Fly Blow Stop
| 68 | float | Fly Blow Up Adjust
| 72 | float | Magic Jump
| 76 | float | Lock On Range
| 80 | float | Lock On Release Range
| 84 | float | Stun Recovery
| 88 | float | Stun Recovery Hp
| 92 | float | Stun Relax
| 96 | float | Drive Enemy
| 100 | float | Change Time Enemy
| 104 | float | Drive Time
| 108 | float | Drive Time Relax
| 112 | float | Change Time Add Rate
| 116 | float | Change Time Sub Rate
| 120 | float | Mp Drive Rate
| 124 | float | Mp To Mp Drive
| 128 | float | Summon Time Relax
| 132 | float | Summon Pray Time
| 136 | float | Summon Pray Time Skip
| 140 | int | Anti Form Drive Count
| 144 | int | Anti Form Sub Count
| 148 | float | Anti Form Damage Rate
| 152 | float | Final Form Rate
| 156 | float | Final Form Mul Rate
| 160 | float | Final Form Max Rate
| 164 | int | Final Form Sub Count
| 168 | float | Attack Distance To Speed
| 172 | float | Al Carpet Dash Inner
| 176 | float | Al Carpet Dash Delay
| 180 | float | Al Carpet Dash Acceleration
| 184 | float | Al Carpet Dash Brake
| 188 | float | Lk Dash Drift Inner
| 192 | float | Lk Dash Drift Time
| 196 | float | Lk Dash Acceleration Drift
| 200 | float | Lk Dash Acceleration Stop
| 204 | float | Lk Dash Drift Speed
| 208 | float | Lk Magic Jump
| 212 | float | Mickey Charge Wait
| 216 | float | Mickey Down Rate
| 220 | float | Mickey Min Rate
| 224 | float | Lm Swim Speed
| 228 | float | Lm Swim Control
| 232 | float | Lm Swim Acceleration
| 236 | float | Lm Dolphin Acceleration
| 240 | float | Lm Dolphin Speed Max
| 244 | float | Lm Dolphin Speed Min
| 248 | float | Lm Dolphin Speed Max Distance
| 252 | float | Lm Dolphin Speed Min Distance
| 256 | float | Lm Dolphin Rotation Max
| 260 | float | Lm Dolphin Rotation Distance
| 264 | float | Lm Dolphin Rotation Max Distance
| 268 | float | Lm Dolphin Distance To Time
| 272 | float | Lm Dolphin Time Max
| 276 | float | Lm Dolphin Time Min
| 280 | float | Lm Dolphin Near Speed
| 284 | int | Drive Berserk Attack
| 288 | float | Mp Haste
| 292 | float | Mp Hastera
| 296 | float | Mp Hastega
| 300 | float | Draw Range
| 304 | int | Combo Damage Up
| 308 | int | Reaction Damage Up
| 312 | float | Damage Drive
| 316 | float | Drive Boost
| 320 | float | Form Boost
| 324 | float | Exp Chance
| 328 | int | Defender
| 332 | int | Element Up
| 336 | float | Damage Aspir
| 340 | float | Hyper Heal
| 344 | float | Combination Boost
| 348 | float | Prize Up
| 352 | float | Luck Up
| 356 | int | Item Up
| 360 | float | Auto Heal
| 364 | float | Summon Boost
| 368 | float | Drive Convert
| 372 | float | Defense Master
| 376 | int | Defense Master Ratio

### Magi

Each pointer leads to a specific entry's offset.

### Magi Structure

| Amount | Description |
|--------|---------------|
| 1 	 | Pointer Count [uint]
| 36 	 | Pointers [uint]
| 5 	 | Magi Entries

### Magi Entry

\* NOTE: Each entry is 124 bytes long, so the info for the fields is wrong somewhere.

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | float | Fire Radius
| 4 | float | Fire Height
| 8 | float | Fire Time
| 12 | float | Blizzard Fade Time
| 16 | float | Blizzard Time
| 20 | float | Blizzard Speed
| 24 | float | Blizzard Radius
| 28 | float | Blizzard Hit Back
| 32 | float | Thunder No Target Distance
| 36 | float | Thunder Border Height
| 40 | float | Thunder No Target Height
| 44 | float | Thunder Check Height
| 48 | float | Thunder Burst Radius
| 52 | float | Thunder Height
| 56 | float | Thunder Radius
| 60 | float | Thunder Attack Wait
| 64 | float | Thunder Time
| 68 | float | Cure Range
| 72 | float | Magnet Min Y Offset
| 76 | float | Magnet large Time
| 80 | float | Magnet Stay Time
| 84 | float | Magnet Small Time
| 88 | float | Magnet Radius
| 92 | float | Reflect Radius
| 96 | float | Reflect Laser Time
| 100 | float | Reflect Finish Time
| 104 | float | Reflect Lv1 Radius
| 108 | float | Reflect Lv1 Height
| 112 | int | Reflect Lv2 Count
| 116 | float | Reflect Lv2 Radius
| 120 | float | Reflect Lv2 Height
| 124 | int | Reflect Lv3 Count
| 128 | float | Reflect Lv3 Radius
| 132 | float | Reflect Lv3 Height