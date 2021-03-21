# [Kingdom Hearts II](index.md) - Editions

The game internally uses a field called _Edition Id_, which dictates the following logic:

* What game files to load
* The ID used for the save data
* The distance between letters when using a space
* Circle and Cross swap button

|Code|Edition|Confirm Button|Space width|Save header|Save (JP build)|Save (US build)|Save (EU build)|Save (FM build)|
|----|-------|--------------|-----------|-----------|---------------|---------------|---------------|---------------|
|1   |us     |cross         |12         |KH2U       |SLPS-99999     |SLUS-21005     |SLUS-21005     |SLUS-21005FM   |
|2   |jp     |circle        |18         |KH2J       |SLPM-66233     |SLPM-66233     |SLPM-66233     |SLPM-66233FM   |
|3   |uk     |cross         |12         |KH2E       |SLPS-99999     |SLPS-99999     |SLES-54114     |SLES-54114FM   |
|4   |it     |cross         |12         |KH2E       |SLPS-99999     |SLPS-99999     |SLES-54234     |SLES-54234FM   |
|5   |sp     |cross         |12         |KH2E       |SLPS-99999     |SLPS-99999     |SLES-54235     |SLES-54235FM   |
|6   |gr     |cross         |12         |KH2E       |SLPS-99999     |SLPS-99999     |SLES-54233     |SLES-54233FM   |
|7   |fr     |cross         |12         |KH2E       |SLPS-99999     |SLPS-99999     |SLES-54232     |SLES-54232FM   |
|8   |fm     |circle        |18         |KH2J       |-              |-              |-              |SLPM-66675FM   |

The _Edition Id_ variable can be found in the following offset:

|Game code|Offset  |Set-up function|
|---------|--------|---------------|
|SLPM66233|00349510|sub_105ca0     |
|SLUS21005|00349D44|sub_105cb0     |
|SLPM66675|0033CAFC|sub_105af8     |

While the vanilla japanese version have hard-coded the edition as `jp`, the Final Mix version checks the content of `SYSTEM.CNF` to establish which edition to set.
