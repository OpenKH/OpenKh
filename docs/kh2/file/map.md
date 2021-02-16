# [Kingdom Hearts II](../index.md) - MAP files

As the name suggest, this is the file that contains everything about maps but [object spawn](type/ard.md). A `.map` file is nothing more than a [BAR](type/bar.md) that contains specific entries.

## MAP

Always paired with a [models](model.md) and [textures](raw-texture.md) to represent the geometry to render a map. Those two files are required or the game would just crash.

## SK0

Always paired with a [models](model.md) and [textures](raw-texture.md), used to represent the skybox. The skybox is optional. When no skybox files are found, a plain black background will be drawn. Maps like `hb38` will not use a skybox at all as they rely to [MAP](#map) to render a background.

## SK1

Similar to [SK0](#sk0), but from early tests it just render a brighter skybox. It can be found in maps like `tt00`.

## BOB

Object without logic that are embedded into a map, like the train in Twilight Town or the columns in `hb38`. Often multiple BOB are found in a map and they will be stored sequencially in an array of entities.

Always paired with a [models](model.md), [textures](raw-texture.md) and a stripped version of an [ANB](anb/anb.md). If there are no animations for the BOB model, the animation file will just be 0KB long.

## BOP

Better known as Background Object Placement, it is found in every map that contains [BOB](#bob) files names as `out.bop`. It is responsible to let the game know how to place BOB objects to the map.

### Header

| Offset | Type | Description |
|--------|------|-------------|
| 0x00   | uint | File identifier. Always `8`.
| 0x04   | uint | Total length of the file, minus the 8 bytes of the header. Divide by `0x68` to find how many [descriptor](#bob-entry-descriptor) are stored.

### BOB entry descriptor

| Offset | Type  | Description |
|--------|-------|-------------|
| 0x00   | float | Position X
| 0x04   | float | Position Y
| 0x08   | float | Position Z
| 0x0c   | float | Rotation X
| 0x10   | float | Rotation Y
| 0x14   | float | Rotation Z
| 0x18   | float | Scale X
| 0x1c   | float | Scale Y
| 0x20   | float | Scale Z
| 0x24   | uint  | BOB index to show
| 0x28   | uint  |
| 0x2c   | uint  |
| 0x30   | uint  |
| 0x34   | uint  |
| 0x38   | float |
| 0x3c   | float |
| 0x40   | float |
| 0x44   | float |
| 0x48   | float |
| 0x4c   | float |
| 0x50   | float |
| 0x54   | float |
| 0x58   | float |
| 0x5c   | float |
| 0x60   | float |
| 0x64   | float |

## rada

A [tim2](../../common/tm2.md) file used to show the mini-map on the top-right angle of the screen.

## xx_0
