# [Kingdom Hearts II](../../index) - mixdata.bar

This file contains informations about the moogle shop. Internally it is a [bar](bar.md) file.

* [Subfile Header](#header)
* Entries
    * [RECI](#reci) - Recipes
    * [COND](#cond) - Conditions
    * [LEVE](#leve) - Levels
    * [EXP](#exp) - Experience

---

## Headers

Each subfile starts with a header. The structure for all is the same, aside from the Magic Code.

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | int    | Magic Code
| 04     | int    | Unknown. Seems to be related to the game version
| 08     | int    | Entries count
| 0C     | int    | Padding

---

## Subfiles

---

### Reci

Contains the moogle recipes.

Magic Code `MIRE`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | [Item ID of Recipe](./03system.md#item)
| 02     | byte   | Unlock (0,1,2,3 => recipe, free development 1,2,3)
| 03     | byte   | Rank (0,1,2,3 => C,B,A,S)
| 04     | ushort | Obtained Item
| 06     | ushort | Upgraded Item (e.g Plus Accessory)
| 08     | ushort | Ingredient 1
| 0A     | ushort | Needed amount of Ingredient 1
| 0C     | ushort | Ingredient 2
| 0E     | ushort | Needed amount of Ingredient 2
| 10     | ushort | Ingredient 3
| 12     | ushort | Needed amount of Ingredient 3
| 14     | ushort | Ingredient 4
| 16     | ushort | Needed amount of Ingredient 4
| 18     | ushort | Ingredient 5
| 1A     | ushort | Needed amount of Ingredient 5
| 1C     | ushort | Ingredient 6
| 1E     | ushort | Needed amount of Ingredient 6

---

### Cond

Contains the table from Synthesize -> Lists (Get n types of materials etc.)

Magic Code `MICO`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | Id
| 02     | short  | Reward (either an Item or a shop upgrade) - [Item LIST](../../dictionary/inventory.md)
| 04     | byte   | Reward type (0 for Item, 1 for Shop upgrade)
| 05     | byte   | Type of material (Check [Items](./03system.md#item))
| 06     | byte   | Rank of material (Check [Items](./03system.md#item))
| 07     | byte   | Condition Type (0 for stack, 1 for collect uniques)
| 08     | short  | Count of needed Materials
| 0A     | short  | Unlock event for the shop (Same used in shops [Shops](./03system.md#shop))

---

### Leve

Contains moogle level up informations.

Magic Code `MILV`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | Id
| 02     | ushort |
| 04     | ushort | 
| 06     | ushort |
| 08     | int    | EXP needed

---

### Exp

Contains exp values for the materials used in recipes. The total exp from a recipe is the sum of all of its materials' exp values.

Magic Code `MIEX`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | Rank C mat exp
| 02     | ushort | Rank B mat exp
| 04     | ushort | Rank A mat exp
| 06     | ushort | Rank S mat exp
| 08     | byte[8]    | 