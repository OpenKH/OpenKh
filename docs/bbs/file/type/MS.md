# MS

Some special files, usually starting with *MS* contain several parameters for specific events. All of them will be listed here.

|    Arc File   | Internal File      | Description
|---------------|--------------------|------------
| cd03ms201.arc | [MS201.bin](#CD-MS201)              | Jaq with Key
| cd08ms302.arc | [MS302.bin](#CD-MS302)              | Cinderella 1
| cd08ms303.arc | [MS302.bin](#CD-MS302)              | Cinderella 2
| he01ms105.arc | [MS101.bin](#HE-MS101)              | Hercules
| he01ms201.arc | [MS101.bin](#HE-MS101)              | Hercules
| he03ms102.arc | [PotBkHe.bin](#Pot-Break-Hercules)    | 
| he03ms102.arc | [MS102.bin](#HE-MS102)    | Unknown
| he03ms102.arc | [PotBreak.bin](#Pot-Break)    | 
| he05ms101.arc | [MS101Zack.bin](#HE-MS101-ZACK)            |
| he05ms101.arc | [MS101.bin](#HE-MS101)              | Hercules
| kg01ms101.arc | [MissionD.bin](#KG-MissionD)              | Unknown
| kg01ms102.arc | [MS201King.bin](#KG-MS201-King)              | King Mickey
| kg01ms301.arc | [MissionD.bin](#KG-MissionD)              | Unknown
| kg12ms202.arc | [MissionD.bin](#KG-MissionD)              | Unknown
| ls13ms101.arc | [MissionD.bin](#LS-MissionD)              | Unknown
| pp13ms302.arc | [MissionD.bin](#PP-MissionD)              | Unknown
| rg10ms201.arc | [MS201King.bin](#RG-MS201-King)              | King Mickey
| sb02ms202.arc | [MS201.bin](#SB-MS201)              | Prince Phillip (Battle)
| sb02ms203.arc | [MS203.bin](#SB-MS203)              | Prince Phillip (Escort)
| sw08ms103.arc | [MS103.bin](#SW-MS103)              | Snowwhite

---

## CD MS201

Jaq (Escort)

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Movement Speed
| 0x4    | float | Frightened Distance
| 0x8    | float | Re Frightened Distance
| 0xC    | float | Check FOV
| 0x10   | float | Near PC Distance
| 0x14   | float | Maximum Rotation Distance
| 0x18   | float | Wait Frame Judgement
| 0x1C   | float | Wait Frame Frightened
| 0x20   | float | Wait Frame PC Look
| 0x24   | float | Damage Rate Time

## CD MS301

Cinderella (Escort)

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Movement Speed
| 0x4    | float | Frightened Distance
| 0x8    | float | Re Frightened Distance
| 0xC    | float | Running Speed
| 0x10   | float | Run Judge Distance
| 0x14   | float | Avoidance Distance
| 0x18   | float | Stop Movement Near PC Distance
| 0x1C   | float | Wait Check PC Angle
| 0x20   | float | Avoidance Direction Check
| 0x24   | float | Avoidance Direction Range
| 0x28   | float | Avoidance Maximum Rotation
| 0x2C   | float | Damage Rate Time
| 0x30   | float | Help Me Damage Rate
| 0x34   | float | Add Maximum Rotation
| 0x38   | float | Stop Move Change Walk

---

## HE MS101

Hercules Ally

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Run Speed
| 0x4    | float | Run Max Speed
| 0x8    | float | Rotation Speed
| 0xC    | float | Attack Range
| 0x10   | float | Follow Suit Angle
| 0x14   | float | Follow Start Distance
| 0x18   | float | Follow Position Distance
| 0x1C   | float | Goal to PC Distance
| 0x20   | float | Run to Maximum Run Distance
| 0x24   | float | Maximum Run to Run Distance
| 0x28   | float | Damage Rate Time
| 0x2C   | float | Faint Frames
| 0x30   | float | Swing Slash Time
| 0x34   | float | Attack Gimmick Rate
| 0x38   | float | Enemy Gimmick Distance
| 0x3C   | float | Attack Miss Rate

## Pot Break Hercules

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Run Speed
| 0x4    | float | Idling Frame Minimum
| 0x8    | float | Idling Frame Maximum
| 0xC    | float | Idling Big Barrel Frame Minimum
| 0x10   | float | Idling Big Barrel Frame Maximum
| 0x14   | float | Gimmick Distance Minimum
| 0x18   | float | Gimmick Distance Maximum
| 0x1C   | float | Move To Gimmick Rate
| 0x20   | float | Attack Miss Rate
| 0x24   | float | Faint Time

## HE MS102

0x28 length

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | 


## Pot Break

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Maximum Gimmick
| 0x4    | float | Minimum Gimmick
| 0x8    | float | Gimmick Create Delay
| 0xC    | float | Big Barrel Create Rate 1
| 0x10   | float | Big Barrel Create Rate 2
| 0x14   | float | Bomb Barrel Create Rate 2
| 0x18   | float | Big Barrel Create Rate 3
| 0x1C   | float | Bomb Barrel Create Rate 3
| 0x20   | float | Big Pot Create Rate
| 0x24   | float | Big Pot Create Maximum
| 0x28   | float | Big Barrel Create Maximum
| 0x2C   | float | Bomb Barrel Create Maximum
| 0x30   | float | Tag Bomb Barrel Rate
| 0x34   | float | Tag Big Barrel Rate
| 0x38   | float | Tag Big Pot Rate
| 0x3C   | float | Battle Time 1
| 0x40   | float | Battle Time 2
| 0x44   | float | Battle Time 3
| 0x48   | float | Clear Points

## HE MS101 ZACK

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Run Speed
| 0x4    | float | Run Maximum Speed
| 0x8    | float | Rotation Speed
| 0xC    | float | Attack Range
| 0x10   | float | Attack Medium Range
| 0x14   | float | Attack Medium Rate
| 0x18   | float | Follow Suit Angle
| 0x1C   | float | Follow Start Distance
| 0x20   | float | Follow Position Distance
| 0x24   | float | Goal to PC Distance
| 0x28   | float | Run to Maximum Distance
| 0x2C   | float | Max Run to Run Distance
| 0x30   | float | Damage Rate Time
| 0x34   | float | Faint Frames

---

## KG MissionD

0xC length

## KG MS201 King

King Mickey

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Movement Walking Speed
| 0x4    | float | Movement Running Speed
| 0x8    | float | Movement Running Max Speed
| 0xC    | float | Walking Distance
| 0x10   | float | Rotation Speed
| 0x14   | float | Attack Range
| 0x18   | float | Attack Medium Range
| 0x1C   | float | Attack Medium Rate
| 0x20   | float | Attack Long Range
| 0x24   | float | Attack Long Rate
| 0x28   | float | Holy Ball Speed
| 0x2C   | float | Player Follow Suit Angle
| 0x30   | float | Player Follow Start Distance
| 0x34   | float | Player Follow Position Distance
| 0x38   | float | Player Goal to PC Distance
| 0x3C   | float | Player Maximum Runnning to Maximum Running Distance
| 0x40   | float | Player Maximum Runnning to Running Distance
| 0x44   | float | Damage Rate Time
| 0x48   | float | Player Faint Frames
| 0x4C   | float | Holy Flood Point 1
| 0x50   | float | Holy Flood Point 2

---

## KG MissionD

0xC length

---

## PP MissionD

0xC length

---

## RG MS201 King

King Mickey

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Movement Walking Speed
| 0x4    | float | Movement Running Speed
| 0x8    | float | Movement Running Max Speed
| 0xC    | float | Walking Distance
| 0x10   | float | Rotation Speed
| 0x14   | float | Attack Range
| 0x18   | float | Attack Medium Range
| 0x1C   | float | Attack Medium Rate
| 0x20   | float | Attack Long Range
| 0x24   | float | Attack Long Rate
| 0x28   | float | Holy Ball Speed
| 0x2C   | float | Player Follow Suit Angle
| 0x30   | float | Player Follow Start Distance
| 0x34   | float | Player Follow Position Distance
| 0x38   | float | Player Goal to PC Distance
| 0x3C   | float | Player Maximum Runnning to Maximum Running Distance
| 0x40   | float | Player Maximum Runnning to Running Distance
| 0x44   | float | Damage Rate Time
| 0x48   | float | Player Faint Frames
| 0x4C   | float | Holy Flood Point 1
| 0x50   | float | Holy Flood Point 2

---

## SB MS201

Prince Phillip (Battle)

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Movement Walking Speed
| 0x4    | float | Movement Running Speed
| 0x8    | float | Movement Running Max Speed
| 0xC    | float | Rotation Player
| 0x10   | float | Player Rotation Speed
| 0x14   | float | Attack Range
| 0x18   | float | Player Follow Suit Angle
| 0x1C   | float | Player Maximum Rotation Power
| 0x20   | float | Player Follow Start Distance
| 0x24   | float | Player Follow Position Distance
| 0x28   | float | Player Goal to PC Distance
| 0x2C   | float | Player Walk Distance
| 0x30   | float | Player Walk to Max Run Distance
| 0x34   | float | Player Max Run to Run Distance
| 0x38   | float | Player Faint Frames
| 0x3C   | float | Player Guard Rate
| 0x40   | float | Player Help Rate

## SB MS203

Prince Phillip (Escort)

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Movement Running Speed
| 0x4    | float | Rotation Player
| 0x8    | float | Player Rotation Speed
| 0xC    | float | Attack Range
| 0x10   | float | Player Faint Frames
| 0x14   | float | Player Guard Rate
| 0x18   | float | Player Boulder Wait Time
| 0x1C   | float | Player Guard to Boulder Frames

---

## SW MS103

Snowwhite (Escort)

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | float | Movement Running Speed
| 0x4    | float | White Silence Wait Frames
| 0x8    | float | Tree FOV Rotation
| 0xC    | float | Tree Distance
| 0x10   | float | Enemy Rotation
| 0x14   | float | Enemy Distance
| 0x18   | float | Player Rotation
| 0x1C   | float | White Distance
| 0x20   | float | Tree Attack Points
| 0x24   | float | Tree Attack Wait Frames
| 0x28   | float | Damage Rate Time
| 0x2C   | float | Player Distance
| 0x30   | float | Player Distance Max
| 0x34   | float | Enemy Distance Max
| 0x38   | float | Act Judgement Wait Frames