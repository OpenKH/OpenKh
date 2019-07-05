# [Kingdom Hearts II](ard.md) - ARD

Object disposition in a map. Internally it is a [bar](bar.md) which contains a bunch of information.

## SpawnPoint

Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File type; always 2.
| 04     | int   | 
| 08     | short | 
| 0a     | short | 
| 0c     | int   | Entries count
| 10     | int   | 

Entry

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Model ID
| 04     | float | Position X  
| 08     | float | Position Z
| 0c     | float | Position Y
| 0c     | float | Rotation X
| 10     | float | Rotation Z
| 14     | float | Rotation Y
| 18     | int   | 
| 1c     | short | 
| 1e     | short | 
| 20     | int   | 
| 24     | int   | AI parameter
| 28     | int   | 
| 2c     | int   | 
| 30     | int   | 
| 34     | int   | 
| 38     | int   | 
| 3c     | int   | 
