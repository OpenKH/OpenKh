// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public partial class Kh2Pax : KaitaiStruct
    {
        public Tracer M_Tracer;

        public static Kh2Pax FromFile(string fileName)
        {
            return new Kh2Pax(new KaitaiStream(fileName));
        }

        public Kh2Pax(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Pax p__root = null, Tracer tracer = null) : base(p__io)
        {
            M_Tracer = tracer;
            var entityName = nameof(Kh2Pax);
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_name = false;
            f_dpx = false;
            M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
            _read();
            M_Tracer.EndRead();
        }
        private void _read()
        {
            M_Tracer.BeginMember(nameof(Magic));
            _magic = m_io.ReadU4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(OffsetName));
            _offsetName = m_io.ReadU4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(EntriesCount));
            _entriesCount = m_io.ReadU4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(OffsetDpx));
            _offsetDpx = m_io.ReadU4le();
            M_Tracer.EndMember();
            _paxEnts = new List<PaxEntry>((int) (EntriesCount));
            for (var i = 0; i < EntriesCount; i++)
            {
                M_Tracer.BeginArrayMember(nameof(PaxEnts));
                _paxEnts.Add(new PaxEntry(m_io, this, m_root, tracer: M_Tracer));
                M_Tracer.EndArrayMember();
            }
        }
        public partial class PaxEntry : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static PaxEntry FromFile(string fileName)
            {
                return new PaxEntry(new KaitaiStream(fileName));
            }

            public PaxEntry(KaitaiStream p__io, Kh2Pax p__parent = null, Kh2Pax p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(PaxEntry);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Effect));
                _effect = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Caster));
                _caster = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk04));
                _unk04 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk06));
                _unk06 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk08));
                _unk08 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk0c));
                _unk0c = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk10));
                _unk10 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk14));
                _unk14 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(SoundEffect));
                _soundEffect = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PosX));
                _posX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PosZ));
                _posZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(PosY));
                _posY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotX));
                _rotX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotZ));
                _rotZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(RotY));
                _rotY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ScaleX));
                _scaleX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ScaleZ));
                _scaleZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ScaleY));
                _scaleY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk40));
                _unk40 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk44));
                _unk44 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk48));
                _unk48 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk4c));
                _unk4c = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private ushort _effect;
            private ushort _caster;
            private ushort _unk04;
            private ushort _unk06;
            private uint _unk08;
            private uint _unk0c;
            private uint _unk10;
            private uint _unk14;
            private uint _soundEffect;
            private float _posX;
            private float _posZ;
            private float _posY;
            private float _rotX;
            private float _rotZ;
            private float _rotY;
            private float _scaleX;
            private float _scaleZ;
            private float _scaleY;
            private uint _unk40;
            private uint _unk44;
            private uint _unk48;
            private uint _unk4c;
            private Kh2Pax m_root;
            private Kh2Pax m_parent;
            public ushort Effect { get { return _effect; } }
            public ushort Caster { get { return _caster; } }
            public ushort Unk04 { get { return _unk04; } }
            public ushort Unk06 { get { return _unk06; } }
            public uint Unk08 { get { return _unk08; } }
            public uint Unk0c { get { return _unk0c; } }
            public uint Unk10 { get { return _unk10; } }
            public uint Unk14 { get { return _unk14; } }
            public uint SoundEffect { get { return _soundEffect; } }
            public float PosX { get { return _posX; } }
            public float PosZ { get { return _posZ; } }
            public float PosY { get { return _posY; } }
            public float RotX { get { return _rotX; } }
            public float RotZ { get { return _rotZ; } }
            public float RotY { get { return _rotY; } }
            public float ScaleX { get { return _scaleX; } }
            public float ScaleZ { get { return _scaleZ; } }
            public float ScaleY { get { return _scaleY; } }
            public uint Unk40 { get { return _unk40; } }
            public uint Unk44 { get { return _unk44; } }
            public uint Unk48 { get { return _unk48; } }
            public uint Unk4c { get { return _unk4c; } }
            public Kh2Pax M_Root { get { return m_root; } }
            public Kh2Pax M_Parent { get { return m_parent; } }
        }
        private bool f_name;
        private byte[] _name;
        public byte[] Name
        {
            get
            {
                if (f_name)
                    return _name;
                long _pos = m_io.Pos;
                m_io.Seek(OffsetName);
                M_Tracer.Seek(OffsetName);
                M_Tracer.BeginMember(nameof(Name));
                _name = m_io.ReadBytes(128);
                M_Tracer.EndMember();
                m_io.Seek(_pos);
                f_name = true;
                return _name;
            }
        }
        private bool f_dpx;
        private Kh2Dpx _dpx;
        public Kh2Dpx Dpx
        {
            get
            {
                if (f_dpx)
                    return _dpx;
                long _pos = m_io.Pos;
                m_io.Seek(OffsetDpx);
                M_Tracer.Seek(OffsetDpx);
                __raw_dpx = m_io.ReadBytesFull();
                var io___raw_dpx = new KaitaiStream(__raw_dpx);
                M_Tracer.DeclareNewIo();
                M_Tracer.BeginMember(nameof(Dpx));
                _dpx = new Kh2Dpx(io___raw_dpx, tracer: M_Tracer);
                M_Tracer.EndMember();
                m_io.Seek(_pos);
                f_dpx = true;
                return _dpx;
            }
        }
        private uint _magic;
        private uint _offsetName;
        private uint _entriesCount;
        private uint _offsetDpx;
        private List<PaxEntry> _paxEnts;
        private Kh2Pax m_root;
        private KaitaiStruct m_parent;
        private byte[] __raw_dpx;
        public uint Magic { get { return _magic; } }
        public uint OffsetName { get { return _offsetName; } }
        public uint EntriesCount { get { return _entriesCount; } }
        public uint OffsetDpx { get { return _offsetDpx; } }
        public List<PaxEntry> PaxEnts { get { return _paxEnts; } }
        public Kh2Pax M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
        public byte[] M_RawDpx { get { return __raw_dpx; } }
    }
}
