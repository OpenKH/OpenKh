# [Kingdom Hearts II](../../index.md) - Font and icon spacing

fontinfo.bar:

Tag   | Type      | Purpose
------|-----------|--------------------------
`sys` | List      | System text font spacing
`evt` | List      | Event text font spacing
`icon`| List      | Icon sprite spacing

The entire of spacing data is array of `byte`.

Each character unit size is fixed.

The spacing declares each character width in pixel unit spreading from left to right.

![](images/spacing.png)

The ordering of spacing data is a little bit difficult.
Basically it advances left to right, and then top to bottom.
However if there is a 2 blocks in vertical direction,
at first, the index advances from front plane to back plane,
and then advance to front plane of next block.
