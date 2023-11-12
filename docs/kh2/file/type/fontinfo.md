# [Kingdom Hearts II](../../index.md) - Font and icon spacing

fontinfo.bar:

Tag   | Type      | Purpose
------|-----------|--------------------------
`sys` | List      | System text font spacing
`evt` | List      | Event text font spacing
`icon`| List      | Icon sprite spacing

The entire of spacing data is array of `byte`.

Each character unit size is 24x24 or such (size is square).

The spacing declares each character size in pixel unit spreading from bottom left origin point to top right end.

![](images/spacing.png)
