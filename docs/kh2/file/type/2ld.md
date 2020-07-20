# [Kingdom Hearts II](../../index.md) - 2D animation system

## 2D layout (2LD)

A [BAR](./bar.md) container that usually only holds [IMGZ](./image.md#imgz) and [Layout](#layout) files.

## 2D sequence (2DD)

A [BAR](./bar.md) container that usually only holds [IMGD](./image.md#imgd) and [Sequence](#sequence) files.

## Layout (LAD)

Used to draw window animations, like menus and the title screen. Layout files are responsible to fill the entire screen and they uses [sequences](#sequence) by repositioning them to give a proper graphics feedback.

Since a layout includes embeds sequences, they can be used to also draw GUI elements on the menu. Although, those information are not directly referenced by the layout and the speculation is that are rendered by code.

The logic of a layout is to render multiple sequences by associating them a texture from a [IMGZ](./image.md#imgz), move them to a specific screen location and start to play them from a specific frame.

According to Kingdom Hearts RE: Chain of Memories, the extension of this file is LAD.

## Sequence (SED)

Used to draw element animations, like HUD elements, fonts and floating menu. This is the core of the entire 2D animation system.

The three essential parts of a sequence are [animation group](#animation-group), [sprite group](#sprite-group) and [sprite](#sprite). Since a sequence is always linked to a specific texture, sprites uses it when drawing on screen.

Following the pattern of other file formats such as LAD and IMD, it is safe to assume that the extension of this file format might be SED.

### Animation group

Contains a list of animations and define, for each of them, their starting frame and loop.

An animation is capable to take a specific sprite group to perform transformations such as translation, rotation, change the pivot, scaling, color masking, color blending, bouncing. Each of those elements have a start keyframe and end keyframe, where the interpolation can be linear or cubic.

### Sprite group

A collection of sprites, where each sprite is located to a specific location and scaled, when needed. The reason why this is a powerful tool is because you can easily re-use sprites. For a GUI element, for example, it might just need to use a single corner to draw it four times by mirroring it horizontally and vertically.

### Sprite

A sprite is more than just cropping part of the texture atlas. Each corner have its own color mask, so it is possible to re-use a single portion of the atlas texture to obtain sprite of different colors, a shadowed version of it, or even gradients from 4bpp textures.

Last, but not least, it is even possible to perform UV animations by moving the sprite horizontally and vertically inside the LTRB box of a sprite. This is extensively used by the title screen menu or to create 2D animations like smog or fire.
