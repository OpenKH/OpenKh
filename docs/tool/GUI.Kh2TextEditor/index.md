# [OpenKh Tool Documentation](../index.md) - KH2 Text Editor

Welcome to the official OpenKh tool documentation!
This document assumes you are already familiar with either compiling or acquiring the released version of the KH2 Text Editor bundled with OpenKH and have a dumped copy of the ISO. 
If you have neither, you can download the release builds of OpenKH [here](https://github.com/Xeeynamo/OpenKh/releases) and then dump your ISO using [this tutorial](../CLI.IdxImg/index.md).

This document will focus on teaching you how to use the first fully functional text editor for KH2.

## Navigating the Tool

First and foremost, one you have opened the program, you need to open both the fontimage.bar and fontinfo.bar files (located in ./KH2/msg/jp). To get these, you must extract your game with the help of our ImgIdx CLI tool. If you need help, with this step, [here is another document to get you started](../CLI/IdxImg/index.md). When ready, open your files, like so:

<img src="./images/gif01.gif" width="640">

Afterwards, open the desired .bar file containing the text in question that you would like to edit. These can be located at `./KH2/msg/jp`, just like the fontimage and fontinfo files. For this example, we'll go with sys.bar, which contains all frequently referenced, important, and miscellaneous text, such as menus.

<img src="./images/gif02.gif" width="640">

## Basic Text Editing

Now that we've got our sys.bar open, let's say we want to edit some of the Command Menu text. It should be easy enough. Let's change its text output from the classic "Attack" to something more fierce, like "Slash".

<img src="./images/gif03.gif" width="640">

There is a search bar at yor disposal in the bottom left which will greatly help you narrow down your search for specific strings of text! As long as you know what it is your looking for, type in a keyword or phrase and the editor will narrow down the selection for you. This way, you don't have to browse for text by string numbers or approximate by hand where they're located sequentially in the file. Once we've changed all of our basic single line text, such as "Attack", "Magic", and whatnot, let's save the file to a new location so we don't accidentaly overwrite the original.

<img src="./images/gif04.gif" width="640">

After saving, we can finally patch in our simple edit to see if the changes are reflected in-game!

<img src="./images/image01.png" width="320">

Uh-oh! Some of our changed text is going a little too far and is popping out of the text box. We'll fix that and do some other neat adjustments in the next section!

## Advanced Text Editing

Before we get started, here's just how fancy text editing can get with this powerful tool!

<img src="./images/image02.png" width="320">

As far as we know, this is just the tip of the iceberg. There are many more powerful functions built into this very tool, and the best part is it's utilizing functions in the game engine itself! There's a somewhat lengthy list of commands that can be called for various purposes, from width-scaling to forcing specific colors and transparency, and more! We'll now be going over how exactly to utilize these string maps we have at our disposal.

For now, let's list all the primary functions you will be likely to use, their type indicator, and detail exactly what they do:

| Type | Human-Readable Format 	| Description
|------|------------------------|------------------------
| 02   | {:newline}             | Feeds a new line to the current selected text string. This argument becomes invisible upon reopening the string, but that is intended; it is still working.
| 03   | {:reset}               | Resets all text afterwards to be argument-less.
| 07   | {:color #RRGGBBAA}     | Forces all text after this argument to appear as the specified color in Hex. An AA value higher than 80 (default) will make your text appear bold, while values lower than 80 will make it appear less bold. The default color value for most text in the game is #F0F0F080.
| 09   | {:icon icon-name}      | Displays the named icon within the text string. A list of all icons resides at the end of this document.
| 0A   | {:scale Value}         | Forces all text after this argument to scale proportionately to the original size. (16 is normal.)
| 0B   | {:width Value}         | Forces all text after this argument to scale only in width, leaving height untouched. (100 is normal, 72 for 16:9 widescreen fixed.)
| 10   | {:clear}               | Makes all text after this argument null, meaning it does not show, regardless of what you type. (There is probably no real pracical use for this function, but it is there nonetheless.)
| 11   | {:position X,Y}        | Relocates text's (X,Y) pixel coordinates from the origin point on screen. (Text is rendered on a 2D screen buffer approximately the size of 512x416. Because of this, you will likely never want to use values higher than X:512, X:-512, Y:416, or Y:-416.)

While there are more types, as of writing these are probably the only arguments you will ever likely need to use. They should have all of your bases covered. With that, let's see what exactly your typed out text might look like in the editor after you make some adjustments. For this example, we'll be looking at the (overly complicated) "Transformations" text that replaced "Drive". While the example itself is a ridiculous setup and you would never use the tool like this practically, we'll fix its width spacing anyway, just to show how to alleviate this problem.
```
{:width 50}{:color #75FFFFFF}T{:color #FF75FFFF}r{:color #FFFF75FF}a{:color #C3FFFFFF}n{:color #FFC3FFFF}s{:color #FFFFC3FF}f{:color #00C3FFFF}o{:color #FF00C3FF}r{:color #FF00C3FF}m{:color #C3FF00FF}a{:color #C325FFFF}t{:color #FFC325FF}i{:color #25FFC3FF}o{:color #C325FFFF}n{:color #FFFFFFFF}s
```

It's certainly not practical to make your text look like this, but it's cool nonetheless that we can do such a thing! It really doesn't get any simpler than this. To start, I needed to determine that all the text should scale in width proportionately, so I made sure to place my {:width 50} argument at the very beginning so I didn't have to do it for every individual letter, as the arguments apply to everything that come after it. Placing something like `{:width 80}` halfway through the text would've made the second half much wider than the `{:width 50}` half. So you can mix and match your arguments as you please, though again, you will likely never need to do such a thing.

**In addition to the aforementioned arguments, certain special characters such as Roman numberals can be called at any point by using arguments such as {I}, {III}, {XIII}, and so forth.**

Let's test some more arguments for various texts in our sys.bar.

| Line Number | Original Text                          | New Text
|-------------|----------------------------------------|-----------------------
| 480         | Attack                                 | {:color 952121FF}Slash
| 481         | Magic                                  | {:color 214D95FF}Mana
| 482         | Items                                  | {:width 64}{:color 219542FF}Consumables
| 483         | Drive                                  | {:width 50}{:color 218995FF}Transformations
| 14133       | Kingdom Hearts                         | {:icon form}{:color D3D971FF}Classic Menu{:icon form}
| 14135       | Keep the look of the original command menu. | {:scale 24}{:color D3D971FF}Use this if you prefer the classic{:newline}Command Menu.

<img src="./images/image03.png" width="640">

And how it all appears in-game:

<img src="./images/image04.png" width="320">
<img src="./images/image05.png" width="540">

## Closing Notes

Now that you've got a pretty good idea of how the text works, feel free to experiment and see what all you can come up with! As mentioned earlier, there are more arguments that can be used, but for the sake of preventing this documentation from being too complicated, for now it's just covering the core functions that the majority of people will use.

As a farewell, here's a list of all the {:icon} arguments you can use to spice up your text as you see fit!

| Icon-Name (Alphabetical)  | Icon
|---------------------------|---------
| ability-equip             | ![image](../../kh2/images/icons/ability-equip.png)
| ability-unequip           | ![image](../../kh2/images/icons/ability-unequip.png)
| accessory                 | ![image](../../kh2/images/icons/accessory.png)
| ai-mode-frequent          | ![image](../../kh2/images/icons/ai-mode-frequent.png)
| ai-mode-moderate          | ![image](../../kh2/images/icons/ai-mode-moderate.png)
| ai-mode-rare              | ![image](../../kh2/images/icons/ai-mode-rare.png)
| ai-settings               | ![image](../../kh2/images/icons/ai-settings.png)
| armor                     | ![image](../../kh2/images/icons/armor.png)
| auto-equip                | ![image](../../kh2/images/icons/auto-equip.png)
| button-circle             | ![image](../../kh2/images/icons/button-circle.png)
| button-cross              | ![image](../../kh2/images/icons/button-cross.png)
| button-dpad               | ![image](../../kh2/images/icons/button-dpad.png)
| button-l1                 | ![image](../../kh2/images/icons/button-l1.png)
| button-l2                 | ![image](../../kh2/images/icons/button-l2.png)
| button-r1                 | ![image](../../kh2/images/icons/button-r1.png)
| button-r2                 | ![image](../../kh2/images/icons/button-r2.png)
| button-select             | ![image](../../kh2/images/icons/button-select.png)
| button-square             | ![image](../../kh2/images/icons/button-square.png)
| button-start              | ![image](../../kh2/images/icons/button-start.png)
| button-triangle           | ![image](../../kh2/images/icons/button-triangle.png)
| exclamation-mark          | ![image](../../kh2/images/icons/exclamation-mark.png)
| form                      | ![image](../../kh2/images/icons/form.png)
| gem-blazing               | ![image](../../kh2/images/icons/gem-blazing.png)
| gem-bright                | ![image](../../kh2/images/icons/gem-bright.png)
| gem-dark                  | ![image](../../kh2/images/icons/gem-dark.png)
| gem-dense                 | ![image](../../kh2/images/icons/gem-dense.png)
| gem-energy                | ![image](../../kh2/images/icons/gem-energy.png)
| gem-frost                 | ![image](../../kh2/images/icons/gem-frost.png)
| gem-lightning             | ![image](../../kh2/images/icons/gem-lightning.png)
| gem-lucid                 | ![image](../../kh2/images/icons/gem-lucid.png)
| gem-mythril               | ![image](../../kh2/images/icons/gem-mythril.png)
| gem-orichalcum            | ![image](../../kh2/images/icons/gem-orichalcum.png)
| gem-power                 | ![image](../../kh2/images/icons/gem-power.png)
| gem-serenity              | ![image](../../kh2/images/icons/gem-serenity.png)
| gem-twilight              | ![image](../../kh2/images/icons/gem-twilight.png)
| gumi-block                | ![image](../../kh2/images/icons/gumi-block.png)
| gumi-blueprint            | ![image](../../kh2/images/icons/gumi-blueprint.png)
| gumi-brush                | ![image](../../kh2/images/icons/gumi-brush.png)
| gumi-gear                 | ![image](../../kh2/images/icons/gumi-gear.png)
| gumi-ship                 | ![image](../../kh2/images/icons/gumi-ship.png)
| item-consumable           | ![image](../../kh2/images/icons/item-consumable.png)
| item-key                  | ![image](../../kh2/images/icons/item-key.png)
| item-tent                 | ![image](../../kh2/images/icons/item-tent.png)
| magic                     | ![image](../../kh2/images/icons/magic.png)
| magic-nocharge            | ![image](../../kh2/images/icons/magic-nocharge.png)
| material                  | ![image](../../kh2/images/icons/material.png)
| party                     | ![image](../../kh2/images/icons/party.png)
| question-mark             | ![image](../../kh2/images/icons/question-mark.png)
| rank-a                    | ![image](../../kh2/images/icons/rank-a.png)
| rank-b                    | ![image](../../kh2/images/icons/rank-b.png)
| rank-c                    | ![image](../../kh2/images/icons/rank-c.png)
| rank-s                    | ![image](../../kh2/images/icons/rank-s.png)
| remembrance               | ![image](../../kh2/images/icons/remembrance.png)
| tranquil                  | ![image](../../kh2/images/icons/tranquil.png)
| weapon-keyblade           | ![image](../../kh2/images/icons/weapon-keyblade.png)
| weapon-keyblade-equip     | ![image](../../kh2/images/icons/weapon-keyblade-equip.png)
| weapon-shield             | ![image](../../kh2/images/icons/weapon-shield.png)
| weapon-shield-equip       | ![image](../../kh2/images/icons/weapon-shield-equip.png)
| weapon-staff              | ![image](../../kh2/images/icons/weapon-staff.png)
| weapon-staff-equip        | ![image](../../kh2/images/icons/weapon-staff-equip.png)
