# PRIZEBOXDATA

PRIZEBOXDATA is a single file that contains every prize box dropped by every enemy in the game.

Located in the `ITEM` folder or inside `arc/system/commongame.arc`.

The file lacks a header. You keep reading data until the `name` field is *`null`*.

## Prize Box Structure

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[8]   | Name of the associated file
| 0x8     | [PrizeBoxData](#Prize-Box-Data)[8][3]   | Array of Prize Box Data

## Prize Box Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | [Index Type](#IDX)
| 0x2     | uint8   | Type
| 0x3     | uint8   | Percent Rate

## IDX

| Value | Name  | Description
|--------|-------|------------
| 0     | IDX_SCORE_PLAYER   | 
| 1     | IDX_SCORE_ENEMY   | 
| 2     | IDX_SCORE_RECORD   | 