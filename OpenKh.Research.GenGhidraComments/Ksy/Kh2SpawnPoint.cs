// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public partial class Kh2SpawnPoint : KaitaiStruct
    {
        public Tracer M_Tracer;

        public static Kh2SpawnPoint FromFile(string fileName)
        {
            return new Kh2SpawnPoint(new KaitaiStream(fileName));
        }

        public Kh2SpawnPoint(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2SpawnPoint p__root = null, Tracer tracer = null) : base(p__io)
        {
            M_Tracer = tracer;
            var entityName = nameof(Kh2SpawnPoint);
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_spawnPointDesc = false;
            M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
            _read();
            M_Tracer.EndRead();
        }
        private void _read()
        {
            M_Tracer.BeginMember(nameof(TypeId));
            _typeId = m_io.ReadS4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(ItemCount));
            _itemCount = m_io.ReadS4le();
            M_Tracer.EndMember();
        }
        public partial class Unknown0a : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static Unknown0a FromFile(string fileName)
            {
                return new Unknown0a(new KaitaiStream(fileName));
            }

            public Unknown0a(KaitaiStream p__io, Kh2SpawnPoint.SpawnPoint p__parent = null, Kh2SpawnPoint p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(Unknown0a);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk00));
                _unk00 = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk01));
                _unk01 = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk02));
                _unk02 = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk03));
                _unk03 = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk04));
                _unk04 = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk05));
                _unk05 = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk06));
                _unk06 = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk08));
                _unk08 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk0c));
                _unk0c = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private byte _unk00;
            private byte _unk01;
            private byte _unk02;
            private byte _unk03;
            private byte _unk04;
            private byte _unk05;
            private short _unk06;
            private uint _unk08;
            private uint _unk0c;
            private Kh2SpawnPoint m_root;
            private Kh2SpawnPoint.SpawnPoint m_parent;
            public byte Unk00 { get { return _unk00; } }
            public byte Unk01 { get { return _unk01; } }
            public byte Unk02 { get { return _unk02; } }
            public byte Unk03 { get { return _unk03; } }
            public byte Unk04 { get { return _unk04; } }
            public byte Unk05 { get { return _unk05; } }
            public short Unk06 { get { return _unk06; } }
            public uint Unk08 { get { return _unk08; } }
            public uint Unk0c { get { return _unk0c; } }
            public Kh2SpawnPoint M_Root { get { return m_root; } }
            public Kh2SpawnPoint.SpawnPoint M_Parent { get { return m_parent; } }
        }
        public partial class WalkPathDesc : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static WalkPathDesc FromFile(string fileName)
            {
                return new WalkPathDesc(new KaitaiStream(fileName));
            }

            public WalkPathDesc(KaitaiStream p__io, Kh2SpawnPoint.SpawnPoint p__parent = null, Kh2SpawnPoint p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(WalkPathDesc);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk00));
                _unk00 = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Count));
                _count = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk04));
                _unk04 = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk06));
                _unk06 = m_io.ReadS2le();
                M_Tracer.EndMember();
                _positions = new List<Position>((int) (Count));
                for (var i = 0; i < Count; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Positions));
                    _positions.Add(new Position(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
            }
            private short _unk00;
            private short _count;
            private short _unk04;
            private short _unk06;
            private List<Position> _positions;
            private Kh2SpawnPoint m_root;
            private Kh2SpawnPoint.SpawnPoint m_parent;
            public short Unk00 { get { return _unk00; } }
            public short Count { get { return _count; } }
            public short Unk04 { get { return _unk04; } }
            public short Unk06 { get { return _unk06; } }
            public List<Position> Positions { get { return _positions; } }
            public Kh2SpawnPoint M_Root { get { return m_root; } }
            public Kh2SpawnPoint.SpawnPoint M_Parent { get { return m_parent; } }
        }
        public partial class EventActivator : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EventActivator FromFile(string fileName)
            {
                return new EventActivator(new KaitaiStream(fileName));
            }

            public EventActivator(KaitaiStream p__io, Kh2SpawnPoint.SpawnPoint p__parent = null, Kh2SpawnPoint p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EventActivator);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk00));
                _unk00 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PositionX));
                _positionX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PositionY));
                _positionY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PositionZ));
                _positionZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ScaleX));
                _scaleX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ScaleY));
                _scaleY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ScaleZ));
                _scaleZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotationX));
                _rotationX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotationY));
                _rotationY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotationZ));
                _rotationZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk28));
                _unk28 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2c));
                _unk2c = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk30));
                _unk30 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk34));
                _unk34 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk38));
                _unk38 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk3c));
                _unk3c = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private uint _unk00;
            private float _positionX;
            private float _positionY;
            private float _positionZ;
            private float _scaleX;
            private float _scaleY;
            private float _scaleZ;
            private float _rotationX;
            private float _rotationY;
            private float _rotationZ;
            private uint _unk28;
            private uint _unk2c;
            private uint _unk30;
            private uint _unk34;
            private uint _unk38;
            private uint _unk3c;
            private Kh2SpawnPoint m_root;
            private Kh2SpawnPoint.SpawnPoint m_parent;
            public uint Unk00 { get { return _unk00; } }
            public float PositionX { get { return _positionX; } }
            public float PositionY { get { return _positionY; } }
            public float PositionZ { get { return _positionZ; } }
            public float ScaleX { get { return _scaleX; } }
            public float ScaleY { get { return _scaleY; } }
            public float ScaleZ { get { return _scaleZ; } }
            public float RotationX { get { return _rotationX; } }
            public float RotationY { get { return _rotationY; } }
            public float RotationZ { get { return _rotationZ; } }
            public uint Unk28 { get { return _unk28; } }
            public uint Unk2c { get { return _unk2c; } }
            public uint Unk30 { get { return _unk30; } }
            public uint Unk34 { get { return _unk34; } }
            public uint Unk38 { get { return _unk38; } }
            public uint Unk3c { get { return _unk3c; } }
            public Kh2SpawnPoint M_Root { get { return m_root; } }
            public Kh2SpawnPoint.SpawnPoint M_Parent { get { return m_parent; } }
        }
        public partial class SpawnPoint : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static SpawnPoint FromFile(string fileName)
            {
                return new SpawnPoint(new KaitaiStream(fileName));
            }

            public SpawnPoint(KaitaiStream p__io, Kh2SpawnPoint p__parent = null, Kh2SpawnPoint p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(SpawnPoint);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk00));
                _unk00 = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk02));
                _unk02 = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(EntityCount));
                _entityCount = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(EventActivatorCount));
                _eventActivatorCount = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk08Count));
                _unk08Count = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk0aCount));
                _unk0aCount = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk0cCount));
                _unk0cCount = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk10));
                _unk10 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk14));
                _unk14 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk18));
                _unk18 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Place));
                _place = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Door));
                _door = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(World));
                _world = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk1f));
                _unk1f = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk20));
                _unk20 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk24));
                _unk24 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk28));
                _unk28 = m_io.ReadU4le();
                M_Tracer.EndMember();
                _entities = new List<Entity>((int) (EntityCount));
                for (var i = 0; i < EntityCount; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Entities));
                    _entities.Add(new Entity(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
                _eventActivators = new List<EventActivator>((int) (EventActivatorCount));
                for (var i = 0; i < EventActivatorCount; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(EventActivators));
                    _eventActivators.Add(new EventActivator(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
                _walkPath = new List<WalkPathDesc>((int) (Unk08Count));
                for (var i = 0; i < Unk08Count; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(WalkPath));
                    _walkPath.Add(new WalkPathDesc(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
                _unknown0aTable = new List<Unknown0a>((int) (Unk0aCount));
                for (var i = 0; i < Unk0aCount; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Unknown0aTable));
                    _unknown0aTable.Add(new Unknown0a(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
                _unknown0cTable = new List<Unknown0c>((int) (Unk0cCount));
                for (var i = 0; i < Unk0cCount; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Unknown0cTable));
                    _unknown0cTable.Add(new Unknown0c(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
            }
            private short _unk00;
            private short _unk02;
            private short _entityCount;
            private short _eventActivatorCount;
            private short _unk08Count;
            private short _unk0aCount;
            private uint _unk0cCount;
            private uint _unk10;
            private uint _unk14;
            private uint _unk18;
            private byte _place;
            private byte _door;
            private byte _world;
            private byte _unk1f;
            private uint _unk20;
            private uint _unk24;
            private uint _unk28;
            private List<Entity> _entities;
            private List<EventActivator> _eventActivators;
            private List<WalkPathDesc> _walkPath;
            private List<Unknown0a> _unknown0aTable;
            private List<Unknown0c> _unknown0cTable;
            private Kh2SpawnPoint m_root;
            private Kh2SpawnPoint m_parent;
            public short Unk00 { get { return _unk00; } }
            public short Unk02 { get { return _unk02; } }
            public short EntityCount { get { return _entityCount; } }
            public short EventActivatorCount { get { return _eventActivatorCount; } }
            public short Unk08Count { get { return _unk08Count; } }
            public short Unk0aCount { get { return _unk0aCount; } }
            public uint Unk0cCount { get { return _unk0cCount; } }
            public uint Unk10 { get { return _unk10; } }
            public uint Unk14 { get { return _unk14; } }
            public uint Unk18 { get { return _unk18; } }
            public byte Place { get { return _place; } }
            public byte Door { get { return _door; } }
            public byte World { get { return _world; } }
            public byte Unk1f { get { return _unk1f; } }
            public uint Unk20 { get { return _unk20; } }
            public uint Unk24 { get { return _unk24; } }
            public uint Unk28 { get { return _unk28; } }
            public List<Entity> Entities { get { return _entities; } }
            public List<EventActivator> EventActivators { get { return _eventActivators; } }
            public List<WalkPathDesc> WalkPath { get { return _walkPath; } }
            public List<Unknown0a> Unknown0aTable { get { return _unknown0aTable; } }
            public List<Unknown0c> Unknown0cTable { get { return _unknown0cTable; } }
            public Kh2SpawnPoint M_Root { get { return m_root; } }
            public Kh2SpawnPoint M_Parent { get { return m_parent; } }
        }
        public partial class Entity : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static Entity FromFile(string fileName)
            {
                return new Entity(new KaitaiStream(fileName));
            }

            public Entity(KaitaiStream p__io, Kh2SpawnPoint.SpawnPoint p__parent = null, Kh2SpawnPoint p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(Entity);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(ObjectId));
                _objectId = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PositionX));
                _positionX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PositionY));
                _positionY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PositionZ));
                _positionZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotationX));
                _rotationX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotationY));
                _rotationY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotationZ));
                _rotationZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk1c));
                _unk1c = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk1e));
                _unk1e = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk20));
                _unk20 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(AiParameter));
                _aiParameter = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(TalkMessage));
                _talkMessage = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ReactionCommand));
                _reactionCommand = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk30));
                _unk30 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk34));
                _unk34 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk38));
                _unk38 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk3c));
                _unk3c = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private uint _objectId;
            private float _positionX;
            private float _positionY;
            private float _positionZ;
            private float _rotationX;
            private float _rotationY;
            private float _rotationZ;
            private short _unk1c;
            private short _unk1e;
            private uint _unk20;
            private uint _aiParameter;
            private uint _talkMessage;
            private uint _reactionCommand;
            private uint _unk30;
            private uint _unk34;
            private uint _unk38;
            private uint _unk3c;
            private Kh2SpawnPoint m_root;
            private Kh2SpawnPoint.SpawnPoint m_parent;
            public uint ObjectId { get { return _objectId; } }
            public float PositionX { get { return _positionX; } }
            public float PositionY { get { return _positionY; } }
            public float PositionZ { get { return _positionZ; } }
            public float RotationX { get { return _rotationX; } }
            public float RotationY { get { return _rotationY; } }
            public float RotationZ { get { return _rotationZ; } }
            public short Unk1c { get { return _unk1c; } }
            public short Unk1e { get { return _unk1e; } }
            public uint Unk20 { get { return _unk20; } }
            public uint AiParameter { get { return _aiParameter; } }
            public uint TalkMessage { get { return _talkMessage; } }
            public uint ReactionCommand { get { return _reactionCommand; } }
            public uint Unk30 { get { return _unk30; } }
            public uint Unk34 { get { return _unk34; } }
            public uint Unk38 { get { return _unk38; } }
            public uint Unk3c { get { return _unk3c; } }
            public Kh2SpawnPoint M_Root { get { return m_root; } }
            public Kh2SpawnPoint.SpawnPoint M_Parent { get { return m_parent; } }
        }
        public partial class Position : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static Position FromFile(string fileName)
            {
                return new Position(new KaitaiStream(fileName));
            }

            public Position(KaitaiStream p__io, Kh2SpawnPoint.WalkPathDesc p__parent = null, Kh2SpawnPoint p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(Position);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(X));
                _x = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Y));
                _y = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Z));
                _z = m_io.ReadF4le();
                M_Tracer.EndMember();
            }
            private float _x;
            private float _y;
            private float _z;
            private Kh2SpawnPoint m_root;
            private Kh2SpawnPoint.WalkPathDesc m_parent;
            public float X { get { return _x; } }
            public float Y { get { return _y; } }
            public float Z { get { return _z; } }
            public Kh2SpawnPoint M_Root { get { return m_root; } }
            public Kh2SpawnPoint.WalkPathDesc M_Parent { get { return m_parent; } }
        }
        public partial class Unknown0c : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static Unknown0c FromFile(string fileName)
            {
                return new Unknown0c(new KaitaiStream(fileName));
            }

            public Unknown0c(KaitaiStream p__io, Kh2SpawnPoint.SpawnPoint p__parent = null, Kh2SpawnPoint p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(Unknown0c);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk00));
                _unk00 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk04));
                _unk04 = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private uint _unk00;
            private uint _unk04;
            private Kh2SpawnPoint m_root;
            private Kh2SpawnPoint.SpawnPoint m_parent;
            public uint Unk00 { get { return _unk00; } }
            public uint Unk04 { get { return _unk04; } }
            public Kh2SpawnPoint M_Root { get { return m_root; } }
            public Kh2SpawnPoint.SpawnPoint M_Parent { get { return m_parent; } }
        }
        private bool f_spawnPointDesc;
        private List<SpawnPoint> _spawnPointDesc;
        public List<SpawnPoint> SpawnPointDesc
        {
            get
            {
                if (f_spawnPointDesc)
                    return _spawnPointDesc;
                _spawnPointDesc = new List<SpawnPoint>((int) (ItemCount));
                for (var i = 0; i < ItemCount; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(SpawnPointDesc));
                    _spawnPointDesc.Add(new SpawnPoint(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
                f_spawnPointDesc = true;
                return _spawnPointDesc;
            }
        }
        private int _typeId;
        private int _itemCount;
        private Kh2SpawnPoint m_root;
        private KaitaiStruct m_parent;
        public int TypeId { get { return _typeId; } }
        public int ItemCount { get { return _itemCount; } }
        public Kh2SpawnPoint M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
