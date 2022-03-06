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
| 14135       | Keep the look of the original command menu. | {:scale 24}{:color D3D971FF}Use this if you prefer the classic Command Menu.

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


The HD Collection, the PC port specifically, repurposes unused and PS2 button icons and adds in brand new ones for its multiple supported control options.
Below is a list of each icon name sorted by button type.

Xbox:
| Icon-Name				| Icon
|-----------------------|---------
| button-cross          | ![image](../../kh2/images/icons/button-cross(PC).png)
| button-circle         | ![image](../../kh2/images/icons/button-circle(PC).png)
| button-square         | ![image](../../kh2/images/icons/button-square(PC).png)
| button-triangle       | ![image](../../kh2/images/icons/button-triangle(PC).png)
| button-select         | ![image](../../kh2/images/icons/button-select(PC).png)
| button-start          | ![image](../../kh2/images/icons/button-start(PC).png)
| button-l1             | ![image](../../kh2/images/icons/button-l1(PC).png)
| button-l2             | ![image](../../kh2/images/icons/button-l2(PC).png)
| weapon-keyblade-equip	| ![image](../../kh2/images/icons/weapon-keyblade-equip(PC).png)
| button-r1             | ![image](../../kh2/images/icons/button-r1(PC).png)
| button-r2             | ![image](../../kh2/images/icons/button-r2(PC).png)
| weapon-staff-equip    | ![image](../../kh2/images/icons/weapon-staff-equip(PC).png)
| xb-analog             | ![image](../../kh2/images/icons/xb-analog.png)
| form                  | ![image](../../kh2/images/icons/form(PC).png)
| xb-left-analog-up     | ![image](../../kh2/images/icons/xb-left-analog-up.png)
| xb-left-analog-down   | ![image](../../kh2/images/icons/xb-left-analog-down.png)
| xb-left-analog-left   | ![image](../../kh2/images/icons/xb-left-analog-left.png)
| xb-left-analog-right  | ![image](../../kh2/images/icons/xb-left-analog-right.png)
| weapon-shield-equip   | ![image](../../kh2/images/icons/weapon-shield-equip(PC).png)
| xb-right-analog-up    | ![image](../../kh2/images/icons/xb-right-analog-up.png)
| xb-right-analog-down  | ![image](../../kh2/images/icons/xb-right-analog-down.png)
| xb-right-analog-left  | ![image](../../kh2/images/icons/xb-right-analog-left.png)
| xb-right-analog-right | ![image](../../kh2/images/icons/xb-right-analog-right.png)
| button-dpad           | ![image](../../kh2/images/icons/button-dpad(PC).png)
| xb-dpad-up            | ![image](../../kh2/images/icons/xb-dpad-up.png)
| xb-dpad-down          | ![image](../../kh2/images/icons/xb-dpad-down.png)
| xb-dpad-left          | ![image](../../kh2/images/icons/xb-dpad-left.png)
| xb-dpad-right         | ![image](../../kh2/images/icons/xb-dpad-right.png)
| xb-dpad-updown        | ![image](../../kh2/images/icons/xb-dpad-updown.png)
| xb-dpad-leftright     | ![image](../../kh2/images/icons/xb-dpad-leftright.png)

Playstation:
| Icon-Name				| Icon
|-----------------------|---------
| ps-cross              | ![image](../../kh2/images/icons/button-cross.png)
| ps-circle             | ![image](../../kh2/images/icons/button-circle.png)
| ps-square             | ![image](../../kh2/images/icons/button-square.png)
| ps-triangle           | ![image](../../kh2/images/icons/button-triangle.png)
| ps-touchpad           | ![image](../../kh2/images/icons/ps-touchpad.png)
| ps-options            | ![image](../../kh2/images/icons/ps-options.png)
| ps-l1                 | ![image](../../kh2/images/icons/button-l1.png)
| ps-l2                 | ![image](../../kh2/images/icons/button-l2.png)
| ps-r3                 | ![image](../../kh2/images/icons/ps-r3.png)
| ps-r1                 | ![image](../../kh2/images/icons/button-r1.png)
| ps-r2                 | ![image](../../kh2/images/icons/button-r2.png)
| ps-l3                 | ![image](../../kh2/images/icons/ps-l3.png)
| ps-analog             | ![image](../../kh2/images/icons/ps-analog.png)
| ps-analog-left        | ![image](../../kh2/images/icons/ps-analog-left.png)
| ps-left-analog-up     | ![image](../../kh2/images/icons/ps-left-analog-up.png)
| ps-left-analog-down   | ![image](../../kh2/images/icons/ps-left-analog-down.png)
| ps-left-analog-left   | ![image](../../kh2/images/icons/ps-left-analog-left.png)
| ps-left-analog-right  | ![image](../../kh2/images/icons/ps-left-analog-right.png)
| ps-analog-right       | ![image](../../kh2/images/icons/ps-analog-right.png)
| ps-right-analog-up    | ![image](../../kh2/images/icons/ps-right-analog-up.png)
| ps-right-analog-down  | ![image](../../kh2/images/icons/ps-right-analog-down.png)
| ps-right-analog-left  | ![image](../../kh2/images/icons/ps-right-analog-left.png)
| ps-right-analog-right	| ![image](../../kh2/images/icons/ps-right-analog-right.png)
| ps-dpad               | ![image](../../kh2/images/icons/button-dpad.png)
| ps-dpad-up            | ![image](../../kh2/images/icons/ps-dpad-up.png)
| ps-dpad-down          | ![image](../../kh2/images/icons/ps-dpad-down.png)
| ps-dpad-left          | ![image](../../kh2/images/icons/ps-dpad-left.png)
| ps-dpad-right         | ![image](../../kh2/images/icons/ps-dpad-right.png)
| ps-dpad-updown        | ![image](../../kh2/images/icons/ps-dpad-updown.png)
| ps-dpad-leftright     | ![image](../../kh2/images/icons/ps-dpad-leftright.png)

Generic:
| Icon-Name				| Icon
|-----------------------|---------
| gen-1                 | ![image](../../kh2/images/icons/gen-1.png)
| gen-2                 | ![image](../../kh2/images/icons/gen-2.png)
| gen-3                 | ![image](../../kh2/images/icons/gen-3.png)
| gen-4                 | ![image](../../kh2/images/icons/gen-4.png)
| gen-r1                | ![image](../../kh2/images/icons/gen-r1.png)
| gen-r2                | ![image](../../kh2/images/icons/gen-r2.png)
| gen-l1                | ![image](../../kh2/images/icons/gen-l1.png)
| gen-l2                | ![image](../../kh2/images/icons/gen-l2.png)
| gen-select            | ![image](../../kh2/images/icons/gen-select.png)
| gen-start             | ![image](../../kh2/images/icons/gen-start.png)

Keyboard & Mouse:
| Icon-Name				| Icon
|-----------------------|---------
| kb-a                  | ![image](../../kh2/images/icons/kb-a.png)
| kb-b                  | ![image](../../kh2/images/icons/kb-b.png)
| kb-c                  | ![image](../../kh2/images/icons/kb-c.png)
| kb-d                  | ![image](../../kh2/images/icons/kb-d.png)
| kb-e                  | ![image](../../kh2/images/icons/kb-e.png)
| kb-f                  | ![image](../../kh2/images/icons/kb-f.png)
| kb-g                  | ![image](../../kh2/images/icons/kb-g.png)
| kb-h                  | ![image](../../kh2/images/icons/kb-h.png)
| kb-i                  | ![image](../../kh2/images/icons/kb-i.png)
| kb-j                  | ![image](../../kh2/images/icons/kb-j.png)
| kb-k                  | ![image](../../kh2/images/icons/kb-k.png)
| kb-l                  | ![image](../../kh2/images/icons/kb-l.png)
| kb-m                  | ![image](../../kh2/images/icons/kb-m.png)
| kb-n                  | ![image](../../kh2/images/icons/kb-n.png)
| kb-o                  | ![image](../../kh2/images/icons/kb-o.png)
| kb-p                  | ![image](../../kh2/images/icons/kb-p.png)
| kb-q                  | ![image](../../kh2/images/icons/kb-q.png)
| kb-r                  | ![image](../../kh2/images/icons/kb-r.png)
| kb-s                  | ![image](../../kh2/images/icons/kb-s.png)
| kb-t                  | ![image](../../kh2/images/icons/kb-t.png)
| kb-u                  | ![image](../../kh2/images/icons/kb-u.png)
| kb-v                  | ![image](../../kh2/images/icons/kb-v.png)
| kb-w                  | ![image](../../kh2/images/icons/kb-w.png)
| kb-x                  | ![image](../../kh2/images/icons/kb-x.png)
| kb-y                  | ![image](../../kh2/images/icons/kb-y.png)
| kb-z                  | ![image](../../kh2/images/icons/kb-z.png)
| kb-left-shift         | ![image](../../kh2/images/icons/kb-left-shift.png)
| kb-right-shift        | ![image](../../kh2/images/icons/kb-right-shift.png)
| kb-left-ctrl          | ![image](../../kh2/images/icons/kb-left-ctrl.png)
| kb-right-crtl         | ![image](../../kh2/images/icons/kb-right-crtl.png)
| kb-left-alt           | ![image](../../kh2/images/icons/kb-left-alt.png)
| kb-right-alt          | ![image](../../kh2/images/icons/kb-right-alt.png)
| kb-enter              | ![image](../../kh2/images/icons/kb-enter.png)
| kb-backspace          | ![image](../../kh2/images/icons/kb-backspace.png)
| kb-space              | ![image](../../kh2/images/icons/kb-space.png)
| kb-esc                | ![image](../../kh2/images/icons/kb-esc.png)
| kb-insert             | ![image](../../kh2/images/icons/kb-insert.png)
| kb-delete             | ![image](../../kh2/images/icons/kb-delete.png)
| kb-1                  | ![image](../../kh2/images/icons/kb-1.png)
| kb-2                  | ![image](../../kh2/images/icons/kb-2.png)
| kb-3                  | ![image](../../kh2/images/icons/kb-3.png)
| kb-4                  | ![image](../../kh2/images/icons/kb-4.png)
| kb-5                  | ![image](../../kh2/images/icons/kb-5.png)
| kb-6                  | ![image](../../kh2/images/icons/kb-6.png)
| kb-7                  | ![image](../../kh2/images/icons/kb-7.png)
| kb-8                  | ![image](../../kh2/images/icons/kb-8.png)
| kb-9                  | ![image](../../kh2/images/icons/kb-9.png)
| kb-0                  | ![image](../../kh2/images/icons/kb-0.png)
| kb-numpad-1           | ![image](../../kh2/images/icons/kb-numpad-1.png)
| kb-numpad-2           | ![image](../../kh2/images/icons/kb-numpad-2.png)
| kb-numpad-3           | ![image](../../kh2/images/icons/kb-numpad-3.png)
| kb-numpad-4           | ![image](../../kh2/images/icons/kb-numpad-4.png)
| kb-numpad-5           | ![image](../../kh2/images/icons/kb-numpad-5.png)
| kb-numpad-6           | ![image](../../kh2/images/icons/kb-numpad-6.png)
| kb-numpad-7           | ![image](../../kh2/images/icons/kb-numpad-7.png)
| kb-numpad-8           | ![image](../../kh2/images/icons/kb-numpad-8.png)
| kb-numpad-9           | ![image](../../kh2/images/icons/kb-numpad-9.png)
| kb-numpad-0           | ![image](../../kh2/images/icons/kb-numpad-0.png)
| kb-numpad-divide      | ![image](../../kh2/images/icons/kb-numpad-divide.png)
| kb-numpad-multiply    | ![image](../../kh2/images/icons/kb-numpad-multiply.png)
| kb-numpad-minus       | ![image](../../kh2/images/icons/kb-numpad-minus.png)
| kb-numpad-plus        | ![image](../../kh2/images/icons/kb-numpad-plus.png)
| kb-numpad-period      | ![image](../../kh2/images/icons/kb-numpad-period.png)
| kb-up                 | ![image](../../kh2/images/icons/kb-up.png)
| kb-down               | ![image](../../kh2/images/icons/kb-down.png)
| kb-left               | ![image](../../kh2/images/icons/kb-left.png)
| kb-right              | ![image](../../kh2/images/icons/kb-right.png)
| kb-f1                 | ![image](../../kh2/images/icons/kb-f1.png)
| kb-f2                 | ![image](../../kh2/images/icons/kb-f2.png)
| kb-f3                 | ![image](../../kh2/images/icons/kb-f3.png)
| kb-f4                 | ![image](../../kh2/images/icons/kb-f4.png)
| kb-f5                 | ![image](../../kh2/images/icons/kb-f5.png)
| kb-f6                 | ![image](../../kh2/images/icons/kb-f6.png)
| kb-f7                 | ![image](../../kh2/images/icons/kb-f7.png)
| kb-f8                 | ![image](../../kh2/images/icons/kb-f8.png)
| kb-f9                 | ![image](../../kh2/images/icons/kb-f9.png)
| kb-f10                | ![image](../../kh2/images/icons/kb-f10.png)
| kb-f11                | ![image](../../kh2/images/icons/kb-f11.png)
| kb-f12                | ![image](../../kh2/images/icons/kb-f12.png)
| mouse-left-click      | ![image](../../kh2/images/icons/mouse-left-click.png)
| mouse-right-click     | ![image](../../kh2/images/icons/mouse-right-click.png)
| mouse-middle-click    | ![image](../../kh2/images/icons/mouse-middle-click.png)
| mouse-extra-click1    | ![image](../../kh2/images/icons/mouse-extra-click1.png)
| mouse-extra-click2    | ![image](../../kh2/images/icons/mouse-extra-click2.png)
| mouse-up              | ![image](../../kh2/images/icons/mouse-up.png)
| mouse-down            | ![image](../../kh2/images/icons/mouse-down.png)
| mouse-left            | ![image](../../kh2/images/icons/mouse-left.png)
| mouse-right           | ![image](../../kh2/images/icons/mouse-right.png)
| mouse-scroll-up       | ![image](../../kh2/images/icons/mouse-scroll-up.png)
| mouse-scroll-down     | ![image](../../kh2/images/icons/mouse-scroll-down.png)
| mouse-neutral         | ![image](../../kh2/images/icons/mouse-neutral.png)
| hash                  | ![image](../../kh2/images/icons/hash.png)


These final icons don't have a static image but instead change what they look like based on the current active controller. 
As an example `dynamic-cross` will look like the Playstation Cross input when using a Playstation controller then dynamically change to the Xbox A input if the cotroller was changed to an Xbox controller during gameplay.
`dynamic-circle-jp` and `dynamic-cross-jp` are used indead of `dynamic-cross` and `dynamic-circle` respectively if the user changed their their settings to swap the comfim/cancel input.

| Icon-Name				
|-----------------------
| dynamic-cross
| dynamic-circle-jp
| dynamic-circle
| dynamic-cross-jp
| dynamic-square
| dynamic-triangle
| dynamic-select
| dynamic-start
| dynamic-l1
| dynamic-l3
| dynamic-l2
| dynamic-r1
| dynamic-r2
| dynamic-r3
| dynamic-analog
| dynamic-left-analog
| dynamic-left-analog-up
| dynamic-left-analog-down
| dynamic-left-analog-left
| dynamic-left-analog-right
| dynamic-right-analog
| dynamic-right-analog-up
| dynamic-right-analog-down
| dynamic-right-analog-left
| dynamic-right-analog-right
| dynamic-dpad
| dynamic-dpad-up
| dynamic-dpad-down
| dynamic-dpad-left
| dynamic-dpad-right
| dynamic-dpad-up-down
| dynamic-dpad-left-right
