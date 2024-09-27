using System;
using Godot;

namespace OpenKh.Godot.Resources;

public partial class KH2ObjectEntry : Resource
{
    [Export] public uint Id;
    [Export] public ObjectEntryType Type;
    [Export] public byte Subtype;
    [Export] public byte DrawPriority;
    [Export] public byte WeaponJoint;
    [Export] public string ModelName;
    [Export] public string AnimationName;
    [Export] public ObjectEntryFlags Flags;
    [Export] public TargetType TargetType;
    [Export] public ushort NeoStatus;
    [Export] public ushort NeoMoveset;
    [Export] public float Weight;
    [Export] public byte SpawnLimiter;
    [Export] public byte Page; //unk
    [Export] public ShadowSize ShadowSize;
    [Export] public Form FormMenu;
    [Export] public ushort Spawn0;
    [Export] public ushort Spawn1;
    [Export] public ushort Spawn2;
    [Export] public ushort Spawn3;
}

[Flags]
public enum ObjectEntryFlags : ushort
{
    NoApdx = 1 << 0,
    Before = 1 << 1,
    FixColor = 1 << 2,
    Fly = 1 << 3,
    Scissoring = 1 << 4,
    Pirate = 1 << 5,
    OCCWall = 1 << 6,
    Hift = 1 << 7,
}
public enum ObjectEntryType : byte
{
    Player,
    PartyMember,
    NPC,
    Boss,
    Enemy,
    Weapon,
    ARDPlaceholder,
    SavePoint,
    Neutral,
    Partner,
    Chest,
    Moogle,
    LargeBoss,
    Unk1,
    PauseMenuDummy,
    Unk2,
    Unk3,
    WorldMap,
    PrizeBox,
    Summon,
    Shop,
    Enemy2,
    CrowdSpawner,
    Unk4,
    PuzzlePiece,
}
public enum TargetType : byte
{
    M,
    S,
    L,
}
public enum ShadowSize : byte
{
    NoShadow,
    SmallShadow,
    MediumShadow,
    LargeShadow,
    SmallMovingShadow,
    MediumMovingShadow,
    LargeMovingShadow,
}
public enum Form : byte
{
    PlayerDefault,
    Valor,
    Wisdom,
    Limit,
    Master,
    Final,
    Anti,
    Lion,
    Atlantica,
    Carpet,
    RoxasDualWield,
    Default,
    CubeCard,
}
