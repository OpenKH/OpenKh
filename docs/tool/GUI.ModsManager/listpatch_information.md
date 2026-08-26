# [OpenKh Tool Documentation](../index.md) - KH2 Mod Manager

This document contains detailed information on the various listpatches available to use with the Mod Manager.


# [Table of Contents]()
  * [trsr](#trsr-source-example)
  * [cmd](#cmd-source-example)
  * [item](#item-source-example)
  * [shop](#shop-source-example)
  * [sklt](#sklt-source-example)
  * [arif](#arif-source-example)
  * [memt](#memt-source-example)
  * [went](#went-source-example)
  * [fmab](#fmab-source-example)
  * [prty](#prty-source-example)
  * [sstm](#sstm-source-example)
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
  * [slct](#slct-source-example)


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
  InsertBefore: 7 #This will insert the item ID before the item ID you specify here.
  #Defaults to 0, which will append to the item list instead.
  #You can alternatively use InsertAfter. 
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
### `went` Source Example
```
Sora: #Specify a name to patch a WENT Set. 
  1: 9345 #Specify an index to patch, and an ObjEntry ID to patch it with
  2: 9777
  98: 998
  99: 998 #If expanding a WENT set, you need to expand it sequentially (I.e you cannot skip from entry 98 to 101 right after)
  100: 999
  101: 1000

#Full list of names of WENT sets that are listpatchable:
#Sora, SoraNM, SoraTR, SoraWI
#Donald, DonaldNM, DonaldTR, DonaldWI
#Goofy, GoofyLK, GoofyNM, GoofyTR, GoofyWI
#Aladdin, Auron, Mulan, Ping, Tron, Mickey, Beast
#Jack, Simba, Sparrow, Riku, SparrowHuman
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
### `prty` Source Example
```
Sora:
  WalkSpeed: 2.0
  RunSpeed: 8.0
  JumpHeight: 185
  TurnSpeed: 0.2617994
  HangHeight: 150
  HangMargin: 210
  StunTime: 0
  MpChargeTime: 3000
  UpDownSpeed: 0
  DashSpeed: 0
  Acceleration: 0
  Brake: 0
  Subjective: 150
#The character you wish to modify must match one of these:
#Sora, ValorForm, WisdomForm, MasterForm, FinalForm, AntiForm, SoraLK, SoraLM, LimitForm
#Donald, DonaldLK, DonaldLM, Goofy, GoofyLK
#Aladdin, Auron, Mulan, Ping, Tron, Mickey, Beast, Jack
#Simba, Sparrow, Riku, MagicCarpet, LightCycle
#SoraDie, Unknown1, Unknown2, GummiShip, RedRocket, Neverland, Session
```
### `sstm` Source Example
```
#Note: Not all fields need to be filled out. Fields you don't need to change can be left out, and will remain unmodified.
CeilingStop: 0.49
CeilingDisableCommandTime: 16
HangeRangeH: 30
HangeRangeL: 60
HangRangeXZ: 20
FallMax: 16
BlowBrakeXZ: 0.8
BlowMinXZ: 0.1
BlowBrakeUp: 0.8
BlowUp: 24
BlowSpeed: 40
BlowToHitBack: 8
HitBack: 200
HitBackSmall: 200
HitBackToJump: 0.048
FlyBlowBrake: 0.8
FlyBlowStop: 0.05
FlyBlowUpAdjust: 0.3
MagicJump: 40
LockOnRange: 1200
LockOnReleaseRange: 1200
StunRecov: 0.5
StunRecovHp: 0.5
StunRelax: 8
DriveZako: 0.75
ChangeTimeZako: 0.2
DriveTime: 600
DriveTimeRelax: 0.5
ChangeTimeAddRate: 12
ChangeTimeSubRate: 15
MpDriveRate: 2
MpToMpDrive: 12
SummonTimeRelax: 0.25
SummonPrayTime: 30
SummonPrayTimeSkip: 60
AntiFormDriveCount: 5
AntiFormSubCount: 6
AntiFormDamageRate: 1.5
FinalFormRate: 3
FinalFormMulRate: 3
FinalFormMaxRate: 75
FinalFormSubCount: 10
AttackDistanceToSpeed: 0.1
AlCarpetDashInner: 0.8660254
AlCarpetDashDelay: 60
AlCarpetDashAcceleration: 0.98
AlCarpetDashBrake: 0.98
LkDashDriftInner: 0.8191521
LkDashDriftTime: 60
LkDashAccelerationDrift: 0.95
LkDashAccelerationStop: 0.9
LkDashDriftSpeed: 12
LkMagicJump: 30
MickeyChargeWait: 4
MickeyDownRate: 0.8
MickeyMinRate: 0.5
LmSwimSpeed: 18
LmSwimControl: 6
LmSwimAcceleration: 0.96
LmDolphinAcceleration: 0
LmDolphinSpeedMax: 20
LmDolphinSpeedMin: 8
LmDolphinSpeedMaxDistance: 500
LmDolphinSpeedMinDistance: 300
LmDolphinRotationMax: 1.570796
LmDolphinRotationDist: 300
LmDolphinRotationMaxDistance: 100
LmDolphinDistanceToTime: 0.2
LmDolphinTimeMax: 120
LmDolphinTimeMin: 60
LmDolphinNearSpeed: 4
DriveBerserkAttack: 2
MpHaste: 0.25
MpHastera: 0.5
MpHastega: 0.75
DrawRange: 125
ComboDamageUp: 10
ReactionDamageUp: 50
DamageDrive: 50
DriveBoost: 0.2
FormBoost: 0.8
ExpChance: 0.5
Defender: 3
ElementUp: 20
DamageAspir: 75
HyperHeal: 0.75
CombinationBoost: 0.8
PrizeUp: 0.5
LuckUp: 0.5
ItemUp: 50
AutoHeal: 60
SummonBoost: 0.8
DriveConvert: 0.5
DefenseMaster: 0.25
DefenseMasterRatio: 50
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
### `slct` Source Example
```
- Id: 1
  ChoiceNum: 4 #Amount of options
  ChoiceDefault: 3 #Option to default to
  Choice:
  - Id: 0 #Choice "Function"
    MessageId: 0 #MessageID to assign to the Choice
  - Id: 1
    MessageId: 1
  - Id: 2
    MessageId: 2
  - Id: 3
    MessageId: 3
  BaseSequence: 12 #Signed short, can be negative
  TitleSequence: 13 #Signed short, can be negative
  Information: 14 #Value tends to always be 0?
  EntryId: 15 #
  Task: 16
  PauseMode: 0 #6 possible values. 0 = Null, 1 = Battle, 2 = Form, 3 = Mission, 4 = Event, 5 = Musical
  Flag: 1 #Two possible flags. 0 Allows you to unpause, 1 disables unpausing to get out of the menu.
  SoundPause: 19
  Padding: #There are 25 padding bytes in total
  - 0
```
