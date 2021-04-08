# [OpenKh Tool Documentation](../index.md) - KH2 Mods Manager

This document will focus on teaching you how to create mods using the OpenKH Mods Manager

## Creating a mod

A well produced mod should contain the following

* mod.yml - The format of which is explained below 
* icon.png - A 128x128 image which will be seen in the mods list
* preview.png - A image of maximum size 512 px wide by 288 px tall, which will be shown in the mod description on the right
* Other files - whatever files are needed for the mod, as defined by the mod.yml

The mod.yml file is a YAML format specification for your mod. It will contain the following fields

* `title` - The title of your mod, as displayed in the mods manager
* `description` - A description of what your mod does
* `originalAuthor` - The name of the original author who created this mod. If you just ported this mod to the modsmanager for someone else, include the original authors name.
* `logo` - The path to the icon.png
* `assets` - A list of assets that will be modified when the mod runs. See `asset types`, for details on creating an asset. Some asset types will work on any game, while others are game specific.

## Asset Methods

* `copy` (any game) - Performs a direct copy to overwrite a file. Works on any file type.

Asset Example: 

```
- method: copy
  name: msn/jp/BB03_MS103.bar
  source:
  - name: files/modified_msn.bar
```

* `binarc` (KH2, BBS, DDD) - Specifies a modification to a subfile within a binarc, using one of the available methods. See `binarc methods` for details on implementing a specific method.

Asset Example

```
- method: binarc
  name: ard/wi03.ard
  source:
  - method: spawnpoint
    name: b_40
    source:
    - name: files/b_40.yml
    type: AreaDataSpawn
```

## Binarc Methods

* `copy` (KH2, BBS) - Performs a copy on a supfile within a Bar. Must be one of the [following](https://github.com/Xeeynamo/OpenKh/blob/master/OpenKh.Tools.BarEditor/Helpers.cs#L14) types

Asset Example

```
- method: binarc
  name: msn/jp/HB01_MS601.bar
  source:
  - method: copy
    name: ms_b
    source:
    - name: he_c.bdx
    type: Bdx
```

* `imgd` (KH2) - Replaces a single imgd found within a binarc

Asset Example

```
  - name: menu/us/title.2ld
    multi:
      - name: menu/jp/title.2ld
    required: true
    method: binarc
    source:
      - name: titl
        type: imgd
        method: imgd
        source:
          - name: title/title1.png
            highdef: title/title1_hd.png
```

* // `imgz` // `fac` (KH2) - Replaces multiple imgd's found within a binarc. 

Asset Example

```
  - name: menu/us/title.2ld
    multi:
      - name: menu/jp/title.2ld
      - name: menu/uk/title.2ld
      - name: menu/it/title.2ld
      - name: menu/sp/title.2ld
      - name: menu/gr/title.2ld
      - name: menu/fr/title.2ld
      - name: menu/fm/title.2ld
    required: true
    method: binarc
    source:
      - name: titl
        type: imgz
        method: imgz
        source:
          - name: title/title1.png
            highdef: title/title1_hd.png
            index: 1
```

* `kh2msg` (KH2) - Replaces text found within a kh2 messages file. Uses a yaml file as an source.

Asset Example

```
  - name: msg/jp/sys.bar
    method: binarc
    source:
      - name: sys
        type: list
        method: kh2msg
        source:
          - name: sys.yml
            language: jp
```

Yaml Source Example

```
- id: 0x432e
  en: OpenKH is awesome!
  it: OpenKH è incredibile!
  sp: ¡OpenKH es increíble!
  gr: OpenKH ist großartig!
  fr: OpenKH est incroyable!
  jp: OPENKHすばらしい!
```

* `areadatascript` (KH2) - Modifies a series programs found within a KH2 Spawnscript subfile (located within ard files), using the text format created by OpenKh.Command.SpawnScript. You can only provide a subset of the programs found within the spawnscript, the others will be taken from the original file.

Asset Example
```
- method: binarc
  name: ard/hb34.ard
  source:
  - method: areadatascript
    name: evt
    source:
    - name: files/hb34/program-87
    - name: files/hb34/program-7c
    - name: files/hb34/program-7d
    - name: files/hb34/program-86
    type: AreaDataScript
```

Text Source Example
```
Program 0x7C
Party DEFAULT
Bgm Default Default
AreaSettings 0 -1
	SetJump Type 2 World HB Area 0 Entrance 0 LocalSet 151 FadeType 1
	SetPartyMenu 0
```

* `areadataspawn` (KH2) - Modifies a KH2 Spawnpoint subfile (located within ard files), using an yaml file created using OpenKh.Command.SpawnScript.

Asset Example

```
- method: binarc
  name: ard/wi03.ard
  source:
  - method: spawnpoint
    name: b_40
    source:
    - name: files/b_40.yml
    type: AreaDataSpawn
```

* `listpatch` (KH2) - Can modify the following different types of list binaries found within KH2.
 * `trsr`
 * `item`
 * `fmlv`
 * `lvup`
 * `bons`
 * `objentry`

Asset Example
```
- name: 00battle.bin
  method: binarc
  source:
  - name: fmlv
    method: listpatch
    type: List
    source:
      - name: FmlvList.yml
        type: fmlv
```

`trsr` Source Example
```
2:
  ItemId: 347
```

`item` Source Example
```
Stats:
- Ability: 412
  AbilityPoints: 0
  Attack: 0
  DarkResistance: 100
  Defense: 0
  FireResistance: 100
  GeneralResistance: 100
  IceResistance: 100
  Id: 116
  LightningResistance: 100
  Magic: 7
  Unknown: 0
  Unknown08: 100
  Unknown0d: 100
Items:
- Id: 1
  Type: Consumable
  Flag0: 0
  Flag1: 40
  Rank: C
  StatEntry: 1
  Name: 33528
  Description: 33529
  ShopBuy: 40
  ShopSell: 10
  Command: 23
  Slot: 0
  Picture: 1
  Icon1: 9
  Icon2: 0
```

`fmlv` Source Example
```
Final:
- Ability: 578
  Experience: 12
  FormId: 5
  FormLevel: 1
  GrowthAbilityLevel: 1
```

`lvup` Source Example
```
Sora:
  2:
    Ap: 0
    Character: Sora
    Defense: 0
    Exp: 100
    Level: 2
    Magic: 0
    Padding: 0
    ShieldAbility: 577
    StaffAbility: 577
    Strength: 0
    SwordAbility: 577
```

`bons` Source Example
```
2:
  Sora:
    AccessorySlotUpgrade: 0
    ArmorSlotUpgrade: 0
    BonusItem1: 99
    BonusItem2: 0
    CharacterId: 1
    Description: ''
    DriveGaugeUpgrade: 0
    HpIncrease: 0
    ItemSlotUpgrade: 0
    MpIncrease: 0
    RewardId: 2
    Unknown0c: 0
```

`objentry` Source Example
```
4:
  ObjectId: 4
  ObjectType: ZAKO
  SubType: 0
  DrawPriority: 0
  WeaponJoint: 0
  ModelName: M_EX520
  AnimationName: M_EX520.mset
  Flag: 8
  TargetType: 1
  Padding: 0
  NeoStatus: 1006
  NeoMoveset: 0
  Weight: 100
  SpawnLimiter: 8
  Page: 1
  ShadowSize: 1
  CommandMenuOption: Default
  SpawnObject1: 0
  SpawnObject2: 0
  SpawnObject3: 0
  SpawnObject4: 0
```

An example of a fully complete mod.yml can be seen below, and the full source of the mod can be seen [here](https://github.com/OpenKH/mod-template)

```
title: OpenKH mod template
originalAuthor: OpenKH open-source assets
description: An example of mod to use as a template
assets:
  - name: menu/us/title.2ld
    multi:
      - name: menu/jp/title.2ld
      - name: menu/uk/title.2ld
      - name: menu/it/title.2ld
      - name: menu/sp/title.2ld
      - name: menu/gr/title.2ld
      - name: menu/fr/title.2ld
      - name: menu/fm/title.2ld
    required: true
    method: binarc
    source:
      - name: titl
        type: imgz
        method: imgz
        source:
          - name: title/title1.png
            highdef: title/title1_hd.png
            index: 1
  - name: msg/jp/sys.bar
    method: binarc
    source:
      - name: sys
        type: list
        method: kh2msg
        source:
          - name: sys.yml
            language: jp
  - name: msg/us/sys.bar
    method: binarc
    source:
      - name: sys
        type: list
        method: kh2msg
        source:
          - name: sys.yml
            language: en
  - name: msg/it/sys.bar
    method: binarc
    source:
      - name: sys
        type: list
        method: kh2msg
        source:
          - name: sys.yml
            language: it
```

## Publishing a mod

Mods should be published to a public github repository, so that users an install the mod just by providing the repository name.

It is recommended to apply the following tags to the repository, in order to make it easily found by searching GitHub for mods manager mods.

`openkh-mods`

`<your games abbreviation>` (ie `kh2` or `bbs`)