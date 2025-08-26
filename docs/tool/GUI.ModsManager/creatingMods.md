# [OpenKh Tool Documentation](../index.md) - KH2 Mods Manager

This document will focus on teaching you how to create mods using the OpenKH Mod Manager.

# [Table of Contents]()
* [Creating a Mod for Use With OpenKH](#creating-a-mod-for-use-with-openkh)
* [Asset Types](#asset-types)
* [Asset Methods](#asset-methods)
  * [copy](#copy-any-game---performs-a-direct-copy-to-overwrite-a-file-works-on-any-file-type)
  * [binarc (KH2)](#binarc-kh2---specifies-a-modification-to-a-subfile-within-a-binarc-using-one-of-the-available-methods-see-binarc-methods-for-details-on-implementing-a-specific-method)
    * [index](#index-kh2---specifies-a-modification-to-a-specific-indexed-subfile-within-a-binarc-using-one-of-the-available-methods-instead-of-searching-for-a-matching-filename-to-replace)
    * [copy](#copy-kh2---performs-a-copy-on-a-supfile-within-a-bar-must-be-one-of-the-following-types)
    * [imgd](#imgd-kh2---replaces-a-single-imgd-found-within-a-binarc)
    * [imgz / fac](#imgz--fac-kh2---replaces-multiple-imgds-found-within-a-binarc)
    * [kh2msg](#kh2msg-kh2---replaces-text-found-within-a-kh2-messages-file-uses-a-yaml-file-as-an-source)
    * [areadatascript](#areadatascript-kh2---modifies-a-series-programs-found-within-a-kh2-spawnscript-subfile-located-within-ard-files-using-the-text-format-created-by-openkhcommandspawnscript-you-can-only-provide-a-subset-of-the-programs-found-within-the-spawnscript-the-others-will-be-taken-from-the-original-file)
    * [areadataspawn](#areadataspawn-kh2---modifies-a-kh2-spawnpoint-subfile-located-within-ard-files-using-an-yaml-file-created-using-openkhcommandspawnscript)
    * [listpatch](#listpatch-kh2---can-modify-the-following-different-types-of-list-binaries-found-within-kh2)
      * [trsr](#trsr-source-example)
      * [cmd](#cmd-source-example)
      * [item](#item-source-example)
      * [shop](#shop-source-example)
      * [sklt](#sklt-source-example)
      * [arif](#arif-source-example)
      * [memt](#memt-source-example)
      * [fmab](#fmab-source-example)
      * [enmp](#enmp-source-example)
      * [fmlv](#fmlv-source-example)
      * [lvpm](#lvpm-source-example)
      * [lvup](#lvup-source-example)
      * [bons](#bons-source-example)
      * [atkp](#atkp-source-example)
      * [przt](#przt-source-example)
      * [magc](#magc-source-example)
      * [limt](#limt-source-example)
      * [vtbl](#vtbl-source-example)
      * [btlv](#btlv-source-example)
      * [objentry](#objentry-source-example)
      * [libretto](#libretto-source-example)
      * [localset](#localset-source-example)
      * [soundinfo](#soundinfo-source-example)
      * [place](#place-source-example)
      * [jigsaw](#jigsaw-source-example)
  * [bbsarc](#bbsarc-bbs)
  * [Example of a Fully Complete `mod.yml` File](#an-example-of-a-fully-complete-modyml-can-be-seen-below-and-the-full-source-of-the-mod-can-be-seen-here)
* [Generating a Simple `mod.yml` for New Mod Authors](#generating-a-simple-modyml-for-new-mod-authors)
* [Publishing a Mod on GitHub](#publishing-a-mod-on-github)

## Creating a Mod for Use With OpenKH

A well produced mod should contain the following

* `mod.yml` - The format of which is explained below 
* `icon.png` - A 128x128 image which will be seen in the mods list
* `preview.png` - A image of maximum size 512 px wide by 288 px tall, which will be shown in the mod description on the right
* Other files - whatever files are needed for the mod, as defined by the `mod.yml`

The mod.yml file is a YAML format specification for your mod. It will contain the following fields:

* `title` - The title of your mod, as displayed in the mods manager
* `description` - A description of what your mod does
* `originalAuthor` - The name of the original author who created this mod. If you just ported this mod to the modsmanager for someone else, include the original author's name. If you do not, this is considered plagiarism, and people tend to not like that.
* `logo` - The path to the icon.png
* `assets` - A list of assets that will be modified when the mod runs. 
  * See [`asset types`](#asset-types), for details on creating an asset. Some asset types will work on any game, while others are game specific.

Additional optional fields for the mod.yml include:
* `game` - If this mod is for a single specific game you can specify it here to ensure safe install
* `speciications` -
* `dependencies` -
* `isCollection` - Specifies that this is a collection of mods, potentially cross game
* `collectionGames` - A list of the short hand titles of the games the mod collection covers (accepted values: `kh1`, `kh2`, `bbs`, `Recom`, `kh3d`)

Additional optional fields for Assets:
* `game` - The game this specific asset belongs to when part of a mod collection
* `collectionOptional` - Marks the asset as optionally installable, and sets it to show in the collecion settings pane

While you are developing a mod you can create a folder inside the `mods` directory of the mod manager release. I.e.:

`openkh/mods/<authorname>/<modname>`

## Asset Types

The truth of the matter is there are seemingly innumerable "asset types" in these games. Each game has several that are exclusive to itself, with little to none shared between any two or more. Some (but not all!) examples of the types of assets you may work with as a mod author are as follows:

* Models
  * .mdls
  * .mdlx
  * .cvbl
  * .wpn
  * .pmo
  * .mdl
* Binary archives
  * .bar
  * .arc
  * .ard
  * .bin
* Sound archives
  * .wd
  * .seb
  * .scd
  * .snd
* Texture formats
  * .img
  * .imd / .imgd
  * .imz / .imgz
  * .dds / .png (PC only)
  * .tm2
* Movesets & animation
  * .mset
  * .anb
  * .arc
* Menu & UI layouts
  * .seqd
  * .lad
  * .l2d
  * .2ld
  * .2dd
* Maps & scripting
  * .map
  * .bar
  * .arc
  * .ard
* Many, many, many more...

However, there is a point to knowing the many asset types the games have on offer. With enough know-how, custom events can be made, called, and integrated into their respective games. Not all of the formats above are from any one single game either; some are shared, some are specific to only one game. Some formats are not even the same between two games, despite having the same file extension. ~~I'm looking at you, `.arc`, `.ard`, and `.pmo`.~~

There are many tools at your disposal to edit a vast array of these formats, thanks to the large contributions over the years to the OpenKH project. We're far from finished yet, though, and would love more help looking into formats and making even more tools!

Additionally, the type of format you are working with will work better with certain [asset methods](#asset-methods) as shown below. Many of the games' files are just archive formats with many other files inside. For example, KH2's `title.2ld` UI layout file for the title screen is an archive that contains a `.imz`, which is another archive with multiple images inside. It also contains `layout data` (primarily called `sequence data` in KH2) in the form of `.seqd` files, which help manage the intro logos and title screen assets contained within the aforementioned `.imz` image archive.

In that example, if you were to simply replace one image of the title screen, you would only need to look as far as one of the several images within, which would best utilize the [`imgz`](#imgz--fac-kh2---replaces-multiple-imgds-found-within-a-binarc) asset method shown below.


# Asset Methods

## `copy` (any game) - Performs a direct copy to overwrite a file. Works on any file type.

Asset Example: 

```
- method: copy
  name: msn/jp/BB03_MS103.bar
  source:
  - name: files/modified_msn.bar
```

## `binarc` (KH2) - Specifies a modification to a subfile within a binarc, using one of the available methods. See `binarc methods` for details on implementing a specific method.

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
### Binarc Methods

## `index` (KH2) - Specifies a modification to a specific, indexed subfile within a binarc using one of the available methods, instead of searching for a matching filename to replace.

Asset Example

```
- name: 07localset.bin
  method: binarc
  source:
  - index: 1 #Patch the file at index 1, instead of replacing the first file with a name.
    method: listpatch
    type: List
    source:
    - name: LocalsetList.yml
      type: localset
```

## `copy` (KH2) - Performs a copy on a supfile within a Bar. Must be one of the [following](https://github.com/Xeeynamo/OpenKh/blob/master/OpenKh.Tools.BarEditor/Helpers.cs#L14) types

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

## `imgd` (KH2) - Replaces a single imgd found within a binarc

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

## `imgz` // `fac` (KH2) - Replaces multiple imgd's found within a binarc. 

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

## `kh2msg` (KH2) - Replaces text found within a kh2 messages file. Uses a yaml file as an source.

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

## `areadatascript` (KH2) - Modifies a series programs found within a KH2 Spawnscript subfile (located within ard files), using the text format created by OpenKh.Command.SpawnScript. You can only provide a subset of the programs found within the spawnscript, the others will be taken from the original file.

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

## `areadataspawn` (KH2) - Modifies a KH2 Spawnpoint subfile (located within ard files), using an yaml file created using OpenKh.Command.SpawnScript.

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

## `listpatch` (KH2) - Can modify the following different types of list binaries found within KH2:
 * `trsr`
 * `cmd`
 * `item`
 * `shop`
 * `sklt`
 * `arif`
 * `memt`
 * `fmab`
 * `enmp`
 * `fmlv`
 * `lvup`
 * `bons`
 * `atkp`
 * `przt`
 * `magc`
 * `limt`
 * `vtbl`
 * `btlv`
 * `objentry`
 * `libretto`
 * `localset`
 * `soundinfo`
 * `place`
 * `jigsaw`

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
  - name: atkp
    method: listpatch
    type: List
    source:
      - name: AtkpList.yml
        type: atkp
```

### `trsr` Source Example
```
2:
  ItemId: 347
```
### `cmd` Source Example
```
- Id: 1
  Execute: 3
  Argument: 3
  SubMenu: 1
  CmdIcon: 3
  MessageId: 33249
  Flags: Cursor, InBattleOnly
  Range: -1
  Dir: 0
  DirRange: -1
  Cost: 0
  CmdCamera: 0
  Priority: 100
  CmdReceiver: 0
  Time: 0
  Require: 0
  Mark: 1
  CmdAction: 0
  ReactionCount: 0
  DistRange: 0
  Score: 0
  DisableForm: 63552
  Group: 2
  Reserve: 0
```
### `item` Source Example
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
  InsertBefore: 7 #This will insert the item ID before the item ID you specify here. Defaults to 0, which will append to the item list instead. You can alternatively use InsertAfter. 
```
### `shop` Source Example
```
ShopEntryHelpers:
- CommandArgument: 104
  UnlockMenuFlag: 42
  NameID: 35513
  ShopKeeperEntityID: 1865
  PosX: 134
  PosY: 150
  PosZ: -591
  ExtraInventoryBitMask: 130
  SoundID: 1
  InventoryCount: 1
  ShopID: 0
  Unk19: 2
  InventoryStartIndex: 0
InventoryEntryHelpers:
- InventoryIndex: 0
  UnlockEventID: 65535
  ProductCount: 2
  ProductStartIndex: 0
ProductEntryHelpers:
- ProductIndex: 0
  ItemID: 67
- ProductIndex: 1
  ItemID: 296
ValidProductEntryHelpers:
- ProductIndex: 0
  ItemID: 67
- ProductIndex: 1
  ItemID: 296
```
### `sklt` Source Example
```
- CharacterId: 1
  Bone1: 178
  Bone2: 86
```
### `arif` Source Example
```
End of Sea: #End of Sea. Names are taken from worlds.md
  2:
    Flags: IsKnownArea, IndoorArea, Monochrome #Other acceptable flags are NoShadow and HasGlow.
    Reverb: 10
    SoundEffectBank1: 20
    SoundEffectBank2: 30
    Bgms:
      - BgmField: 2000
        BgmBattle: 2000
      - BgmField: 600
        BgmBattle: 600
      - BgmField: 1200
        BgmBattle: 1200
      - BgmField: 1200
        BgmBattle: 1200
      - BgmField: 1000
        BgmBattle: 1000
      - BgmField: 1000
        BgmBattle: 1000
      - BgmField: 1500
        BgmBattle: 1500
      - BgmField: 1500
        BgmBattle: 1500
    Voice: 40
    NavigationMapItem: 50
    Command: 60
    Reserved: []
```
### `memt` Source Example
```
MemtEntries: 
  - Index: 0 #Index to edit. Specify new indices to append new entries
    WorldId: 2
    CheckStoryFlag: 209
    FlagForWorld: The World That Never Was #Specify world name
    CheckStoryFlagNegation: 0
    NegationFlagForWorld: Twilight Town #Specify world name
    CheckArea: 2
    Padding: 0
    PlayerSize: 4299264
    FriendSize: 1625344
    Members: [84, 92, 93, 2073, 85, 86, 2397, 87, 88, 89, 91, 264, 200, 1529, 2431, 1530, 1531, 264]
  - Index: 37
    WorldId: 2
    CheckStoryFlag: 209
    FlagForWorld: Twilight Town #Specify world name
    CheckStoryFlagNegation: 0
    FlagNegationForWorld: Twilight Town #Specify world name
    CheckArea: 2
    Padding: 0
    PlayerSize: 4299264
    FriendSize: 1625344
    Members: [84, 92, 93, 2073, 85, 86, 2397, 87, 88, 89, 91, 264, 200, 1529, 2431, 1530, 1531, 264]


MemberIndices:
  - Index: 0
    Player: 15
    Friend1: 20
    Friend2: 32
    FriendWorld: 42
```

### `fmab` Source Example
```
Entries:
  1: #This is "Growth Ability Level"; so this edits the second entry in the list, or, LV 2.
    HighJumpHeight: 999
    AirDodgeHeight: 999
    AirDodgeSpeed: 3.0
    AirSlideTime: 1.0
    AirSlideSpeed: 2.0
    AirSlideBrake: 1.0
    AirSlideStopBrake: 1.0
    AirSlideInvulnerableFrames: 0.5
    GlideSpeed: 2.0
    GlideFallRatio: 0.8
    GlideFallHeight: 1.2
    GlideFallMax: 1.5
    GlideAcceleration: 2.5
    GlideStartHeight: 1.0
    GlideEndHeight: 0.8
    GlideTurnSpeed: 1.5
    DodgeRollInvulnerableFrames: 0.7
  2:
    HighJumpHeight: 9999
    AirDodgeHeight: 999
    AirDodgeSpeed: 3.0
    AirSlideTime: 1.0
    AirSlideSpeed: 2.0
    AirSlideBrake: 1.0
    AirSlideStopBrake: 1.0
    AirSlideInvulnerableFrames: 0.5
    GlideSpeed: 2.0
    GlideFallRatio: 0.8
    GlideFallHeight: 1.2
    GlideFallMax: 1.5
    GlideAcceleration: 2.5
    GlideStartHeight: 1.0
    GlideEndHeight: 0.8
    GlideTurnSpeed: 1.5
    DodgeRollInvulnerableFrames: 0.7
```

### `enmp` Source Example
```
- Id: 0
  Level: 1
  Health: 
  - 1
  - 1
  - 1
  - 1
  - 1
  - 1
  - 1
  MaxDamage: 1
  MinDamage: 1
  PhysicalWeakness: 1
  FireWeakness: 1
  IceWeakness: 1
  ThunderWeakness: 1
  DarkWeakness: 1
  LightWeakness: 1
  GeneralWeakness: 1
  Experience: 1
  Prize: 1
  BonusLevel: 1  
```

### `fmlv` Source Example
```
Final:
- Ability: 578
  Experience: 12
  FormId: 5
  FormLevel: 1
  GrowthAbilityLevel: 1
```

### `lvpm` Source Example
```
- Level: 0
  HpMultiplier: 100
  Strength: 45
  Defense: 26
  MaxStrength: 16
  MinStrength: 5
  Experience: 3212
```

### `lvup` Source Example
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

### `bons` Source Example
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

### `atkp` Source Example
```
- Id: 0 #Hitbox 0
  SubId: 3
  Type: 1
  CriticalAdjust: 0
  Power: 25
  Team: 0
  Element: 0
  EnemyReaction: 0
  EffectOnHit: 2
  KnockbackStrength1: 32767
  KnockbackStrength2: 0
  Unknown: 0000
  Flags: BGHit, LimitPAX, Land, CapturePAX, ThankYou, KillBoss #Every possible AttackFlag shown
  RefactSelf: 0
  RefactOther: 0
  ReflectedMotion: 0
  ReflectHitBack: 0
  ReflectAction: 0
  ReflectHitSound: 0
  ReflectRC: 0
  ReflectRange: 0
  ReflectAngle: 0
  DamageEffect: 0
  Switch: 1
  Interval: 1
  FloorCheck: 1
  DriveDrain: 1
  RevengeDamage: 1
  AttackTrReaction: 1
  ComboGroup: 1
  RandomEffect: 1
  Kind: ComboFinisher
  HpDrain: 15
```

### `przt` Source Example
```
- Id: 1
  SmallHpOrbs: 0
  BigHpOrbs: 1
  BigMoneyOrbs: 1
  MediumMoneyOrbs: 1
  SmallMoneyOrbs: 1
  SmallMpOrbs: 1
  BigMpOrbs: 1
  SmallDriveOrbs: 0
  BigDriveOrbs: 1
  Item1: 1
  Item1Percentage: 1
  Item2: 0
  Item2Percentage: 0
  Item3: 0
  Item3Percentage: 0
```

### `magc` Source Example
```
- Id: 0 
  Level: 3
  World: 1
  FileName: magic/FIRE_3.mag
  Item: 21
  Command: 120
  GroundMotion: 56
  GroundBlend: 2
  FinishMotion: 57
  FinishBlend: 2
  AirMotion: 58
  AirBlend: 2
  Voice: 7
  VoiceFinisher: 11
  VoiceSelf: -1
```
### `limt` Source Example
```
- Id: 1
  Character: Sora
  Summon: None
  Group: 0
  FileName: TESTLIMIT
  SpawnId: 0
  Command: 100
  Limit: 0
  World: 00
  Padding: []
- Id: 30
  Character: Donald
  Summon: Goofy
  Group: 0
  FileName: TESTLIMIT
  SpawnId: 0
  Command: 100
  Limit: 0
  World: 00
  Padding: []
```

### `vtbl` Source Example
```
- Id: 26
  CharacterId: 1
  Priority: 01
  Voices:
    - VsbIndex: 5
      Weight: 100
    - VsbIndex: -1
      Weight: 0
    - VsbIndex: -1
      Weight: 0
    - VsbIndex: -1
      Weight: 0
    - VsbIndex: 6
      Weight: 5
  Reserved: 0
```

### `btlv` Source Example
```
- Id: 0
  ProgressFlag: 0x1099
  WorldZZ: 1
  WorldOfDarkness: 1
  TwilightTown: 1
  DestinyIslands: 1
  HollowBastion: 1
  BeastCastle: 1
  OlympusColiseum: 1
  Agrabah: 1
  LandOfDragons: 1
  HundredAcreWoods: 1
  PrideLands: 1
  Atlantica: 1
  DisneyCastle: 1
  TimelessRiver: 1
  HalloweenTown: 1
  PortRoyal: 1
  SpaceParanoids: 1
  TheWorldThatNeverWas: 1
  Padding: []
```

### `objentry` Source Example
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

### `libretto` Source Example
```
- TalkMessageId: 752 #Id to update. This is used as "ReactionCommand" in ARDs.
  Type: 3 #Specify the type of Message. 1, 2, and 3 seem functionally identical, while 0 does nothing.
  Contents: #Contents to update. Will insert additional Contents as necessary. When no additional are detected, terminates with 0.
    - CodeType: 0x0001 #CodeType. Most worlds use a value of 1, while the World Map uses 0x100 to 0x113.
      Unknown: 0x0001 #Unknown what this is used for. 
      TextId: 0x3DEB #TextID in that worlds .bar file in the msg folder to reference.
    - CodeType: 0x0001
      Unknown: 0x0001
      TextId: 0x183C
```
### `localset` Source Example
```
- ProgramId: 999
  MapNumber: 25
```
### `soundinfo` Source Example
```
- Index: 0			#Specify an index to modify; otherwise if the index doesn't exist it will be created.
  Reverb: -1
  Rate: 1
  EnvironmentWAV: 99
  EnvironmentSEB: 99
  EnvironmentNUMBER: 99
  FootstepWAV: 99
  FootstepSORA: 99
  FootstepDONALD: 99
  FootstepGOOFY: 99
  FootstepWORLDFRIEND: 99
  FootstepOTHER: 99
```
### `place` Source Example
```
- Index: 0			#Index should match the ID of the room in the world; i.e, Index 0 = al00 if you were modifying Agrabah.
  MessageId: 1234
  Padding: 0
```
### `jigsaw` Source Example
```
- Picture: 2
  Part: 4
  Text: 1500
  World: 2
  Room: 1
  JigsawIdWorld: 99
  Unk07: 0
  Unk08: 0
```

* `synthpatch` (KH2) - Modifies Mixdata.bar, a file used for various properties related to synthesis in KH2. 

 * `recipe`
 * `level`
 * `condition`


Asset Example

```
- name: menu/us/mixdata.bar
  method: binarc
  source:
  - name: reci
    method: synthpatch
    type: Synthesis
    source:
      - name: ReciList.yml
        type: recipe
```

### `recipe` Source Example
```
- Id: 1
  Unlock: 0
  Rank: 5
  Item: 100
  UpgradedItem: 101
  Ingredient1: 200
  Ingredient1Amount: 1
  Ingredient2: 201
  Ingredient2Amount: 2
  Ingredient3: 202
  Ingredient3Amount: 3
  Ingredient4: 203
  Ingredient4Amount: 4
  Ingredient5: 204
  Ingredient5Amount: 5
  Ingredient6: 205
  Ingredient6Amount: 6
```

### `level` Source Example
```
- Title: 48338 #TextID to use for Moogle Level "Title", pulls from Sys.Bar.
  Stat: 48740
  Enable: -1
  Padding: 0
  Exp: 0
```

### `condition` Source Example
```
- TextId: 151
  RewardId: 0
  Type: 5
  MaterialType: 100
  MaterialRank: 101
  ItemCollect: 200
  Count: 1
  ShopUnlock: 201
```

### `bbsarc` (BBS)
Allows you to add/patch files inside a bbs `.arc` container without having to `copy` the entire arc file into your mod. You can use any method to patch those files, although at time of writing the only one that works for BBS files (other than `bbsarc`) is `copy`.

Asset example:

```
- name: arc/map/SW10.arc
  method: bbsarc
  source:
  - name: sw_10.pvd
    method: copy
    source:
    - name: sw_10.pvd
```

### An example of a fully complete mod.yml can be seen below, and the full source of the mod can be seen [here](https://github.com/OpenKH/mod-template)

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

## Generating a Simple `mod.yml` for New Mod Authors

This method is recommended exclusively for mod authors, as the YAML Generator built into the Mod Manager is considered fully functional, yet lacks all of the aforementioned [asset methods](#asset-methods), with the exception of a couple. Those being:
* [`copy`](#copy-any-game---performs-a-direct-copy-to-overwrite-a-file-works-on-any-file-type) (game agnostic)
* [`binarc`](#binarc-kh2---specifies-a-modification-to-a-subfile-within-a-binarc-using-one-of-the-available-methods-see-binarc-methods-for-details-on-implementing-a-specific-method) (KH2 only so far)

To start, here are the steps:

1. Create an isolated folder where you will be working on your mod. Ideally, to make it easy for testing in-game, this would be in `openkh/mods/Your Name/Your Mod Name`.
2. To keep things simple, recreate the game's folder structure. I.e., if you were to edit KH2's title screen, Sora's HUD sprites, and replacing the font in KH2 with something goofy like Comic Sans, you would have a file tree that looks like this:
```
├───menu
│   └───us
│       └───title.2ld
│
└───remastered
    └───msg
    │   └───us
    │       └───fontimage.bar
    └───obj
        └───P_EX100.a.us
            └─── ~8.dds
```

3. Open the `Creator` menu on the top of Mod Manager, and select `YamlGenerator`.
4. Make the path to your `mod.yml` where your root mod folder is (i.e., where the example file structure would be located at).
5. Click `Generate or update mod.yml`.
6. Make the `GameDataPath` the same folder as where your new `mod.yml` has been generated.
7. Click `Begin`.
8. Click `Search` in the upper right.
9. Select all your files (**not** `mod.yml`).
10. Click `Copy each`.
11. Make sure the `[ ]Exists` box is checked. If it's not, you did something wrong. Retrace your steps.
12. Click `Proceed` at the bottom.
13. Answer yes, you would like to commit the change to the `mod.yml` file.
14. Close the creator and edit your `mod.yml` file with a text editor to change the following fields as necessary:
  * `title`
  * `originalAuthor`
  * `description`
15. Save your changes and upload the entirety of this folder, including the `mod.yml` file to a new GitHub repository.
16. (Optional) Archive everything into a `.zip` archive and upload to Nexusmods under the appropriate game section.
17. 
18. Profit!

## Publishing a Mod on GitHub

Mods should be published to a public GitHub repository, so that users can install the mod just by providing the repository name.

It is recommended to apply the following tags to the repository, in order to make it easily found by searching GitHub for mods manager mods:

* `openkh-mods`

* `<Your game's abbreviation>`
  * `kh1`
  * `kh2`
  * `bbs`
  * `ddd`
  * `recom`
