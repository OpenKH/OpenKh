// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public partial class Kh2Dpd : KaitaiStruct
    {
        public Tracer M_Tracer;

        public static Kh2Dpd FromFile(string fileName)
        {
            return new Kh2Dpd(new KaitaiStream(fileName));
        }

        public Kh2Dpd(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
        {
            M_Tracer = tracer;
            var entityName = nameof(Kh2Dpd);
            m_parent = p__parent;
            m_root = p__root ?? this;
            M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
            _read();
            M_Tracer.EndRead();
        }
        private void _read()
        {
            M_Tracer.BeginMember(nameof(MagicCode96));
            _magicCode96 = m_io.ReadU4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(NumEffectsGroupList));
            _numEffectsGroupList = m_io.ReadU4le();
            M_Tracer.EndMember();
            _offEffectsGroupList = new List<EffectsGroupParent>((int) (NumEffectsGroupList));
            for (var i = 0; i < NumEffectsGroupList; i++)
            {
                M_Tracer.BeginArrayMember(nameof(OffEffectsGroupList));
                _offEffectsGroupList.Add(new EffectsGroupParent(m_io, this, m_root, tracer: M_Tracer));
                M_Tracer.EndArrayMember();
            }
            M_Tracer.BeginMember(nameof(NumTextures));
            _numTextures = m_io.ReadU4le();
            M_Tracer.EndMember();
            _offTextures = new List<EffectsTextureParent>((int) (NumTextures));
            for (var i = 0; i < NumTextures; i++)
            {
                M_Tracer.BeginArrayMember(nameof(OffTextures));
                _offTextures.Add(new EffectsTextureParent(m_io, this, m_root, tracer: M_Tracer));
                M_Tracer.EndArrayMember();
            }
            M_Tracer.BeginMember(nameof(NumTab3));
            _numTab3 = m_io.ReadU4le();
            M_Tracer.EndMember();
            _offTab3 = new List<EffectsTab3Parent>((int) (NumTab3));
            for (var i = 0; i < NumTab3; i++)
            {
                M_Tracer.BeginArrayMember(nameof(OffTab3));
                _offTab3.Add(new EffectsTab3Parent(m_io, this, m_root, tracer: M_Tracer));
                M_Tracer.EndArrayMember();
            }
            M_Tracer.BeginMember(nameof(NumTab4));
            _numTab4 = m_io.ReadU4le();
            M_Tracer.EndMember();
            _offTab4 = new List<EffectsTab4Parent>((int) (NumTab4));
            for (var i = 0; i < NumTab4; i++)
            {
                M_Tracer.BeginArrayMember(nameof(OffTab4));
                _offTab4.Add(new EffectsTab4Parent(m_io, this, m_root, tracer: M_Tracer));
                M_Tracer.EndArrayMember();
            }
            M_Tracer.BeginMember(nameof(NumTab5));
            _numTab5 = m_io.ReadU4le();
            M_Tracer.EndMember();
            _offTab5 = new List<EffectsTab5Parent>((int) (NumTab5));
            for (var i = 0; i < NumTab5; i++)
            {
                M_Tracer.BeginArrayMember(nameof(OffTab5));
                _offTab5.Add(new EffectsTab5Parent(m_io, this, m_root, tracer: M_Tracer));
                M_Tracer.EndArrayMember();
            }
        }
        public partial class DpdEffectSub : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DpdEffectSub FromFile(string fileName)
            {
                return new DpdEffectSub(new KaitaiStream(fileName));
            }

            public DpdEffectSub(KaitaiStream p__io, Kh2Dpd.DpdEffectParent p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DpdEffectSub);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Count));
                _count = m_io.ReadU4le();
                M_Tracer.EndMember();
                _items = new List<uint>((int) (Count));
                for (var i = 0; i < Count; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Items));
                    _items.Add(m_io.ReadU4le());
                    M_Tracer.EndArrayMember();
                }
            }
            private uint _count;
            private List<uint> _items;
            private Kh2Dpd m_root;
            private Kh2Dpd.DpdEffectParent m_parent;
            public uint Count { get { return _count; } }
            public List<uint> Items { get { return _items; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.DpdEffectParent M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab4Vtx6 : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab4Vtx6 FromFile(string fileName)
            {
                return new EffectsTab4Vtx6(new KaitaiStream(fileName));
            }

            public EffectsTab4Vtx6(KaitaiStream p__io, Kh2Dpd.EffectsTab4VertSet p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab4Vtx6);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Clr0));
                _clr0 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Clr1));
                _clr1 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Clr2));
                _clr2 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Clr3));
                _clr3 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert0));
                _vert0 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert1));
                _vert1 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert2));
                _vert2 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert3));
                _vert3 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Uv0));
                _uv0 = new EffectUv(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Uv1));
                _uv1 = new EffectUv(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Uv2));
                _uv2 = new EffectUv(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Uv3));
                _uv3 = new EffectUv(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
            }
            private EffectRgba _clr0;
            private EffectRgba _clr1;
            private EffectRgba _clr2;
            private EffectRgba _clr3;
            private ushort _vert0;
            private ushort _vert1;
            private ushort _vert2;
            private ushort _vert3;
            private EffectUv _uv0;
            private EffectUv _uv1;
            private EffectUv _uv2;
            private EffectUv _uv3;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab4VertSet m_parent;
            public EffectRgba Clr0 { get { return _clr0; } }
            public EffectRgba Clr1 { get { return _clr1; } }
            public EffectRgba Clr2 { get { return _clr2; } }
            public EffectRgba Clr3 { get { return _clr3; } }
            public ushort Vert0 { get { return _vert0; } }
            public ushort Vert1 { get { return _vert1; } }
            public ushort Vert2 { get { return _vert2; } }
            public ushort Vert3 { get { return _vert3; } }
            public EffectUv Uv0 { get { return _uv0; } }
            public EffectUv Uv1 { get { return _uv1; } }
            public EffectUv Uv2 { get { return _uv2; } }
            public EffectUv Uv3 { get { return _uv3; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab4VertSet M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab4Parent : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab4Parent FromFile(string fileName)
            {
                return new EffectsTab4Parent(new KaitaiStream(fileName));
            }

            public EffectsTab4Parent(KaitaiStream p__io, Kh2Dpd p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab4Parent);
                m_parent = p__parent;
                m_root = p__root;
                f_item = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Offset));
                _offset = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_item;
            private EffectsTab4 _item;
            public EffectsTab4 Item
            {
                get
                {
                    if (f_item)
                        return _item;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offset);
                    M_Tracer.Seek(Offset);
                    __raw_item = m_io.ReadBytesFull();
                    var io___raw_item = new KaitaiStream(__raw_item);
                    M_Tracer.DeclareNewIo();
                    M_Tracer.BeginMember(nameof(Item));
                    _item = new EffectsTab4(io___raw_item, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_item = true;
                    return _item;
                }
            }
            private uint _offset;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Offset { get { return _offset; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd M_Parent { get { return m_parent; } }
            public byte[] M_RawItem { get { return __raw_item; } }
        }
        public partial class EffectsTab3A : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab3A FromFile(string fileName)
            {
                return new EffectsTab3A(new KaitaiStream(fileName));
            }

            public EffectsTab3A(KaitaiStream p__io, Kh2Dpd.EffectsTab3Sub p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab3A);
                m_parent = p__parent;
                m_root = p__root;
                f_b = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Offset));
                _offset = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Flags));
                _flags = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk));
                _unk = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_b;
            private EffectsTab3B _b;
            public EffectsTab3B B
            {
                get
                {
                    if (f_b)
                        return _b;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offset);
                    M_Tracer.Seek(Offset);
                    M_Tracer.BeginMember(nameof(B));
                    _b = new EffectsTab3B(m_io, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_b = true;
                    return _b;
                }
            }
            private ushort _offset;
            private ushort _flags;
            private uint _unk;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab3Sub m_parent;
            public ushort Offset { get { return _offset; } }
            public ushort Flags { get { return _flags; } }
            public uint Unk { get { return _unk; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab3Sub M_Parent { get { return m_parent; } }
        }
        public partial class DpdEffectParent : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DpdEffectParent FromFile(string fileName)
            {
                return new DpdEffectParent(new KaitaiStream(fileName));
            }

            public DpdEffectParent(KaitaiStream p__io, Kh2Dpd.EffectsGroup p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DpdEffectParent);
                m_parent = p__parent;
                m_root = p__root;
                f_sub8 = false;
                f_subc = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk0));
                _unk0 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk4));
                _unk4 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Offset8));
                _offset8 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Offsetc));
                _offsetc = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Item));
                _item = new DpdEffect(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
            }
            private bool f_sub8;
            private DpdEffectSub _sub8;
            public DpdEffectSub Sub8
            {
                get
                {
                    if (f_sub8)
                        return _sub8;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offset8);
                    M_Tracer.Seek(Offset8);
                    M_Tracer.BeginMember(nameof(Sub8));
                    _sub8 = new DpdEffectSub(m_io, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_sub8 = true;
                    return _sub8;
                }
            }
            private bool f_subc;
            private DpdEffectSub _subc;
            public DpdEffectSub Subc
            {
                get
                {
                    if (f_subc)
                        return _subc;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offsetc);
                    M_Tracer.Seek(Offsetc);
                    M_Tracer.BeginMember(nameof(Subc));
                    _subc = new DpdEffectSub(m_io, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_subc = true;
                    return _subc;
                }
            }
            private uint _unk0;
            private uint _unk4;
            private uint _offset8;
            private uint _offsetc;
            private DpdEffect _item;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsGroup m_parent;
            public uint Unk0 { get { return _unk0; } }
            public uint Unk4 { get { return _unk4; } }
            public uint Offset8 { get { return _offset8; } }
            public uint Offsetc { get { return _offsetc; } }
            public DpdEffect Item { get { return _item; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsGroup M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab5Parent : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab5Parent FromFile(string fileName)
            {
                return new EffectsTab5Parent(new KaitaiStream(fileName));
            }

            public EffectsTab5Parent(KaitaiStream p__io, Kh2Dpd p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab5Parent);
                m_parent = p__parent;
                m_root = p__root;
                f_item = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Offset));
                _offset = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_item;
            private EffectsTab5 _item;
            public EffectsTab5 Item
            {
                get
                {
                    if (f_item)
                        return _item;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offset);
                    M_Tracer.Seek(Offset);
                    __raw_item = m_io.ReadBytesFull();
                    var io___raw_item = new KaitaiStream(__raw_item);
                    M_Tracer.DeclareNewIo();
                    M_Tracer.BeginMember(nameof(Item));
                    _item = new EffectsTab5(io___raw_item, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_item = true;
                    return _item;
                }
            }
            private uint _offset;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Offset { get { return _offset; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd M_Parent { get { return m_parent; } }
            public byte[] M_RawItem { get { return __raw_item; } }
        }
        public partial class EffectsTextureParent : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTextureParent FromFile(string fileName)
            {
                return new EffectsTextureParent(new KaitaiStream(fileName));
            }

            public EffectsTextureParent(KaitaiStream p__io, Kh2Dpd p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTextureParent);
                m_parent = p__parent;
                m_root = p__root;
                f_item = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Offset));
                _offset = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_item;
            private EffectsTexture _item;
            public EffectsTexture Item
            {
                get
                {
                    if (f_item)
                        return _item;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offset);
                    M_Tracer.Seek(Offset);
                    __raw_item = m_io.ReadBytesFull();
                    var io___raw_item = new KaitaiStream(__raw_item);
                    M_Tracer.DeclareNewIo();
                    M_Tracer.BeginMember(nameof(Item));
                    _item = new EffectsTexture(io___raw_item, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_item = true;
                    return _item;
                }
            }
            private uint _offset;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Offset { get { return _offset; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd M_Parent { get { return m_parent; } }
            public byte[] M_RawItem { get { return __raw_item; } }
        }
        public partial class EffectsTab3 : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab3 FromFile(string fileName)
            {
                return new EffectsTab3(new KaitaiStream(fileName));
            }

            public EffectsTab3(KaitaiStream p__io, Kh2Dpd.EffectsTab3Parent p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab3);
                m_parent = p__parent;
                m_root = p__root;
                f_sub = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Mark));
                _mark = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk04));
                _unk04 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk08));
                _unk08 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk0c));
                _unk0c = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_sub;
            private EffectsTab3Sub _sub;
            public EffectsTab3Sub Sub
            {
                get
                {
                    if (f_sub)
                        return _sub;
                    __raw_sub = m_io.ReadBytesFull();
                    var io___raw_sub = new KaitaiStream(__raw_sub);
                    M_Tracer.DeclareNewIo();
                    M_Tracer.BeginMember(nameof(Sub));
                    _sub = new EffectsTab3Sub(io___raw_sub, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    f_sub = true;
                    return _sub;
                }
            }
            private uint _mark;
            private uint _unk04;
            private uint _unk08;
            private uint _unk0c;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab3Parent m_parent;
            private byte[] __raw_sub;
            public uint Mark { get { return _mark; } }
            public uint Unk04 { get { return _unk04; } }
            public uint Unk08 { get { return _unk08; } }
            public uint Unk0c { get { return _unk0c; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab3Parent M_Parent { get { return m_parent; } }
            public byte[] M_RawSub { get { return __raw_sub; } }
        }
        public partial class EffectsGroupParent : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsGroupParent FromFile(string fileName)
            {
                return new EffectsGroupParent(new KaitaiStream(fileName));
            }

            public EffectsGroupParent(KaitaiStream p__io, Kh2Dpd p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsGroupParent);
                m_parent = p__parent;
                m_root = p__root;
                f_item = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Offset));
                _offset = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_item;
            private EffectsGroup _item;
            public EffectsGroup Item
            {
                get
                {
                    if (f_item)
                        return _item;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offset);
                    M_Tracer.Seek(Offset);
                    __raw_item = m_io.ReadBytesFull();
                    var io___raw_item = new KaitaiStream(__raw_item);
                    M_Tracer.DeclareNewIo();
                    M_Tracer.BeginMember(nameof(Item));
                    _item = new EffectsGroup(io___raw_item, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_item = true;
                    return _item;
                }
            }
            private uint _offset;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Offset { get { return _offset; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd M_Parent { get { return m_parent; } }
            public byte[] M_RawItem { get { return __raw_item; } }
        }
        public partial class EffectsGroup : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsGroup FromFile(string fileName)
            {
                return new EffectsGroup(new KaitaiStream(fileName));
            }

            public EffectsGroup(KaitaiStream p__io, Kh2Dpd.EffectsGroupParent p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsGroup);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                _matrix1 = new List<float>((int) (16));
                for (var i = 0; i < 16; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Matrix1));
                    _matrix1.Add(m_io.ReadF4le());
                    M_Tracer.EndArrayMember();
                }
                _matrix2 = new List<float>((int) (16));
                for (var i = 0; i < 16; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Matrix2));
                    _matrix2.Add(m_io.ReadF4le());
                    M_Tracer.EndArrayMember();
                }
                _position = new List<float>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Position));
                    _position.Add(m_io.ReadF4le());
                    M_Tracer.EndArrayMember();
                }
                _rotation = new List<float>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Rotation));
                    _rotation.Add(m_io.ReadF4le());
                    M_Tracer.EndArrayMember();
                }
                _scaling = new List<float>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Scaling));
                    _scaling.Add(m_io.ReadF4le());
                    M_Tracer.EndArrayMember();
                }
                _dummy = new List<float>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Dummy));
                    _dummy.Add(m_io.ReadF4le());
                    M_Tracer.EndArrayMember();
                }
                _dummy1 = new List<uint>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Dummy1));
                    _dummy1.Add(m_io.ReadU4le());
                    M_Tracer.EndArrayMember();
                }
                _dummy2 = new List<uint>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Dummy2));
                    _dummy2.Add(m_io.ReadU4le());
                    M_Tracer.EndArrayMember();
                }
                _dummy3 = new List<uint>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Dummy3));
                    _dummy3.Add(m_io.ReadU4le());
                    M_Tracer.EndArrayMember();
                }
                _dummy4 = new List<uint>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Dummy4));
                    _dummy4.Add(m_io.ReadU4le());
                    M_Tracer.EndArrayMember();
                }
                _dummy5 = new List<uint>((int) (4));
                for (var i = 0; i < 4; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Dummy5));
                    _dummy5.Add(m_io.ReadU4le());
                    M_Tracer.EndArrayMember();
                }
                __raw_dpdEffect = m_io.ReadBytesFull();
                var io___raw_dpdEffect = new KaitaiStream(__raw_dpdEffect);
                M_Tracer.DeclareNewIo();
                M_Tracer.BeginMember(nameof(DpdEffect));
                _dpdEffect = new DpdEffectParent(io___raw_dpdEffect, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
            }
            private List<float> _matrix1;
            private List<float> _matrix2;
            private List<float> _position;
            private List<float> _rotation;
            private List<float> _scaling;
            private List<float> _dummy;
            private List<uint> _dummy1;
            private List<uint> _dummy2;
            private List<uint> _dummy3;
            private List<uint> _dummy4;
            private List<uint> _dummy5;
            private DpdEffectParent _dpdEffect;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsGroupParent m_parent;
            private byte[] __raw_dpdEffect;
            public List<float> Matrix1 { get { return _matrix1; } }
            public List<float> Matrix2 { get { return _matrix2; } }
            public List<float> Position { get { return _position; } }
            public List<float> Rotation { get { return _rotation; } }
            public List<float> Scaling { get { return _scaling; } }
            public List<float> Dummy { get { return _dummy; } }
            public List<uint> Dummy1 { get { return _dummy1; } }
            public List<uint> Dummy2 { get { return _dummy2; } }
            public List<uint> Dummy3 { get { return _dummy3; } }
            public List<uint> Dummy4 { get { return _dummy4; } }
            public List<uint> Dummy5 { get { return _dummy5; } }
            public DpdEffectParent DpdEffect { get { return _dpdEffect; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsGroupParent M_Parent { get { return m_parent; } }
            public byte[] M_RawDpdEffect { get { return __raw_dpdEffect; } }
        }
        public partial class EffectsTab3Parent : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab3Parent FromFile(string fileName)
            {
                return new EffectsTab3Parent(new KaitaiStream(fileName));
            }

            public EffectsTab3Parent(KaitaiStream p__io, Kh2Dpd p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab3Parent);
                m_parent = p__parent;
                m_root = p__root;
                f_item = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Offset));
                _offset = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_item;
            private EffectsTab3 _item;
            public EffectsTab3 Item
            {
                get
                {
                    if (f_item)
                        return _item;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offset);
                    M_Tracer.Seek(Offset);
                    __raw_item = m_io.ReadBytesFull();
                    var io___raw_item = new KaitaiStream(__raw_item);
                    M_Tracer.DeclareNewIo();
                    M_Tracer.BeginMember(nameof(Item));
                    _item = new EffectsTab3(io___raw_item, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_item = true;
                    return _item;
                }
            }
            private uint _offset;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Offset { get { return _offset; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd M_Parent { get { return m_parent; } }
            public byte[] M_RawItem { get { return __raw_item; } }
        }
        public partial class EffectsTab3Sub : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab3Sub FromFile(string fileName)
            {
                return new EffectsTab3Sub(new KaitaiStream(fileName));
            }

            public EffectsTab3Sub(KaitaiStream p__io, Kh2Dpd.EffectsTab3 p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab3Sub);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk10));
                _unk10 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Cnt1));
                _cnt1 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Cnt2));
                _cnt2 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk18));
                _unk18 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk1c));
                _unk1c = m_io.ReadU4le();
                M_Tracer.EndMember();
                _a = new List<EffectsTab3A>((int) (Cnt1));
                for (var i = 0; i < Cnt1; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(A));
                    _a.Add(new EffectsTab3A(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
            }
            private uint _unk10;
            private ushort _cnt1;
            private ushort _cnt2;
            private uint _unk18;
            private uint _unk1c;
            private List<EffectsTab3A> _a;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab3 m_parent;
            public uint Unk10 { get { return _unk10; } }
            public ushort Cnt1 { get { return _cnt1; } }
            public ushort Cnt2 { get { return _cnt2; } }
            public uint Unk18 { get { return _unk18; } }
            public uint Unk1c { get { return _unk1c; } }
            public List<EffectsTab3A> A { get { return _a; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab3 M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTexture : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTexture FromFile(string fileName)
            {
                return new EffectsTexture(new KaitaiStream(fileName));
            }

            public EffectsTexture(KaitaiStream p__io, Kh2Dpd.EffectsTextureParent p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTexture);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk0));
                _unk0 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk4));
                _unk4 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Fmt));
                _fmt = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk8));
                _unk8 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Width));
                _width = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Height));
                _height = m_io.ReadU2le();
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
                M_Tracer.BeginMember(nameof(Unk1c));
                _unk1c = m_io.ReadU4le();
                M_Tracer.EndMember();
                if (Fmt == 19) {
                    M_Tracer.BeginMember(nameof(Bitmap));
                    _bitmap = m_io.ReadBytes((Width * Height));
                    M_Tracer.EndMember();
                }
                if (Fmt == 19) {
                    M_Tracer.BeginMember(nameof(Palette));
                    _palette = m_io.ReadBytes(1024);
                    M_Tracer.EndMember();
                }
            }
            private uint _unk0;
            private ushort _unk4;
            private ushort _fmt;
            private uint _unk8;
            private ushort _width;
            private ushort _height;
            private uint _unk10;
            private uint _unk14;
            private uint _unk18;
            private uint _unk1c;
            private byte[] _bitmap;
            private byte[] _palette;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTextureParent m_parent;
            public uint Unk0 { get { return _unk0; } }
            public ushort Unk4 { get { return _unk4; } }
            public ushort Fmt { get { return _fmt; } }
            public uint Unk8 { get { return _unk8; } }
            public ushort Width { get { return _width; } }
            public ushort Height { get { return _height; } }
            public uint Unk10 { get { return _unk10; } }
            public uint Unk14 { get { return _unk14; } }
            public uint Unk18 { get { return _unk18; } }
            public uint Unk1c { get { return _unk1c; } }
            public byte[] Bitmap { get { return _bitmap; } }
            public byte[] Palette { get { return _palette; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTextureParent M_Parent { get { return m_parent; } }
        }
        public partial class EffectUv : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectUv FromFile(string fileName)
            {
                return new EffectUv(new KaitaiStream(fileName));
            }

            public EffectUv(KaitaiStream p__io, Kh2Dpd.EffectsTab4Vtx6 p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectUv);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(U));
                _u = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(V));
                _v = m_io.ReadU2le();
                M_Tracer.EndMember();
            }
            private ushort _u;
            private ushort _v;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab4Vtx6 m_parent;
            public ushort U { get { return _u; } }
            public ushort V { get { return _v; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab4Vtx6 M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab4VertSet : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab4VertSet FromFile(string fileName)
            {
                return new EffectsTab4VertSet(new KaitaiStream(fileName));
            }

            public EffectsTab4VertSet(KaitaiStream p__io, Kh2Dpd.EffectsTab4 p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab4VertSet);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(VertFormat));
                _vertFormat = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(NumVerts));
                _numVerts = m_io.ReadU2le();
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
                _verts = new List<KaitaiStruct>((int) (NumVerts));
                for (var i = 0; i < NumVerts; i++)
                {
                    switch (VertFormat) {
                    case 1536: {
                        M_Tracer.BeginArrayMember(nameof(Verts));
                        _verts.Add(new EffectsTab4Vtx6(m_io, this, m_root, tracer: M_Tracer));
                        M_Tracer.EndArrayMember();
                        break;
                    }
                    case 1024: {
                        M_Tracer.BeginArrayMember(nameof(Verts));
                        _verts.Add(new EffectsTab4Vtx4(m_io, this, m_root, tracer: M_Tracer));
                        M_Tracer.EndArrayMember();
                        break;
                    }
                    case 0: {
                        M_Tracer.BeginArrayMember(nameof(Verts));
                        _verts.Add(new EffectsTab4Vtx0(m_io, this, m_root, tracer: M_Tracer));
                        M_Tracer.EndArrayMember();
                        break;
                    }
                    }
                }
                if (VertFormat == 1536) {
                    M_Tracer.BeginMember(nameof(Pad6));
                    _pad6 = m_io.ReadBytes(((16 - (40 * NumVerts)) & 15));
                    M_Tracer.EndMember();
                }
                if (VertFormat == 1024) {
                    M_Tracer.BeginMember(nameof(Pad4));
                    _pad4 = m_io.ReadBytes(((16 - (24 * NumVerts)) & 15));
                    M_Tracer.EndMember();
                }
                if (VertFormat == 0) {
                    M_Tracer.BeginMember(nameof(Pad0));
                    _pad0 = m_io.ReadBytes(((16 - (20 * NumVerts)) & 15));
                    M_Tracer.EndMember();
                }
            }
            private ushort _vertFormat;
            private ushort _numVerts;
            private uint _unk34;
            private uint _unk38;
            private uint _unk3c;
            private List<KaitaiStruct> _verts;
            private byte[] _pad6;
            private byte[] _pad4;
            private byte[] _pad0;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab4 m_parent;
            public ushort VertFormat { get { return _vertFormat; } }
            public ushort NumVerts { get { return _numVerts; } }
            public uint Unk34 { get { return _unk34; } }
            public uint Unk38 { get { return _unk38; } }
            public uint Unk3c { get { return _unk3c; } }
            public List<KaitaiStruct> Verts { get { return _verts; } }
            public byte[] Pad6 { get { return _pad6; } }
            public byte[] Pad4 { get { return _pad4; } }
            public byte[] Pad0 { get { return _pad0; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab4 M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab4Vtx4 : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab4Vtx4 FromFile(string fileName)
            {
                return new EffectsTab4Vtx4(new KaitaiStream(fileName));
            }

            public EffectsTab4Vtx4(KaitaiStream p__io, Kh2Dpd.EffectsTab4VertSet p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab4Vtx4);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Clr0));
                _clr0 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Clr1));
                _clr1 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Clr2));
                _clr2 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Clr3));
                _clr3 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert0));
                _vert0 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert1));
                _vert1 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert2));
                _vert2 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert3));
                _vert3 = m_io.ReadU2le();
                M_Tracer.EndMember();
            }
            private EffectRgba _clr0;
            private EffectRgba _clr1;
            private EffectRgba _clr2;
            private EffectRgba _clr3;
            private ushort _vert0;
            private ushort _vert1;
            private ushort _vert2;
            private ushort _vert3;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab4VertSet m_parent;
            public EffectRgba Clr0 { get { return _clr0; } }
            public EffectRgba Clr1 { get { return _clr1; } }
            public EffectRgba Clr2 { get { return _clr2; } }
            public EffectRgba Clr3 { get { return _clr3; } }
            public ushort Vert0 { get { return _vert0; } }
            public ushort Vert1 { get { return _vert1; } }
            public ushort Vert2 { get { return _vert2; } }
            public ushort Vert3 { get { return _vert3; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab4VertSet M_Parent { get { return m_parent; } }
        }
        public partial class EffectRgba : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectRgba FromFile(string fileName)
            {
                return new EffectRgba(new KaitaiStream(fileName));
            }

            public EffectRgba(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectRgba);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Red));
                _red = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Green));
                _green = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Blue));
                _blue = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Alpha));
                _alpha = m_io.ReadU1();
                M_Tracer.EndMember();
            }
            private byte _red;
            private byte _green;
            private byte _blue;
            private byte _alpha;
            private Kh2Dpd m_root;
            private KaitaiStruct m_parent;
            public byte Red { get { return _red; } }
            public byte Green { get { return _green; } }
            public byte Blue { get { return _blue; } }
            public byte Alpha { get { return _alpha; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class DpdEffectCommand : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DpdEffectCommand FromFile(string fileName)
            {
                return new DpdEffectCommand(new KaitaiStream(fileName));
            }

            public DpdEffectCommand(KaitaiStream p__io, Kh2Dpd.DpdEffect p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DpdEffectCommand);
                m_parent = p__parent;
                m_root = p__root;
                f_raw1 = false;
                f_raw2 = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Command));
                _command = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ParamLength));
                _paramLength = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ParamCount));
                _paramCount = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(OffsetParameters));
                _offsetParameters = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Offset2));
                _offset2 = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_raw1;
            private byte[] _raw1;
            public byte[] Raw1
            {
                get
                {
                    if (f_raw1)
                        return _raw1;
                    long _pos = m_io.Pos;
                    m_io.Seek(OffsetParameters);
                    M_Tracer.Seek(OffsetParameters);
                    M_Tracer.BeginMember(nameof(Raw1));
                    _raw1 = m_io.ReadBytes((ParamLength * ParamCount));
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_raw1 = true;
                    return _raw1;
                }
            }
            private bool f_raw2;
            private byte[] _raw2;
            public byte[] Raw2
            {
                get
                {
                    if (f_raw2)
                        return _raw2;
                    long _pos = m_io.Pos;
                    m_io.Seek(Offset2);
                    M_Tracer.Seek(Offset2);
                    M_Tracer.BeginMember(nameof(Raw2));
                    _raw2 = m_io.ReadBytes(4);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_raw2 = true;
                    return _raw2;
                }
            }
            private uint _command;
            private ushort _paramLength;
            private ushort _paramCount;
            private uint _offsetParameters;
            private uint _offset2;
            private Kh2Dpd m_root;
            private Kh2Dpd.DpdEffect m_parent;
            public uint Command { get { return _command; } }
            public ushort ParamLength { get { return _paramLength; } }
            public ushort ParamCount { get { return _paramCount; } }
            public uint OffsetParameters { get { return _offsetParameters; } }
            public uint Offset2 { get { return _offset2; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.DpdEffect M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab3C : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab3C FromFile(string fileName)
            {
                return new EffectsTab3C(new KaitaiStream(fileName));
            }

            public EffectsTab3C(KaitaiStream p__io, Kh2Dpd.EffectsTab3B p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab3C);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk0));
                _unk0 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2));
                _unk2 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk4));
                _unk4 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk6));
                _unk6 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk8));
                _unk8 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unka));
                _unka = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unkc));
                _unkc = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unke));
                _unke = m_io.ReadU2le();
                M_Tracer.EndMember();
            }
            private ushort _unk0;
            private ushort _unk2;
            private ushort _unk4;
            private ushort _unk6;
            private ushort _unk8;
            private ushort _unka;
            private ushort _unkc;
            private ushort _unke;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab3B m_parent;
            public ushort Unk0 { get { return _unk0; } }
            public ushort Unk2 { get { return _unk2; } }
            public ushort Unk4 { get { return _unk4; } }
            public ushort Unk6 { get { return _unk6; } }
            public ushort Unk8 { get { return _unk8; } }
            public ushort Unka { get { return _unka; } }
            public ushort Unkc { get { return _unkc; } }
            public ushort Unke { get { return _unke; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab3B M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab5 : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab5 FromFile(string fileName)
            {
                return new EffectsTab5(new KaitaiStream(fileName));
            }

            public EffectsTab5(KaitaiStream p__io, Kh2Dpd.EffectsTab5Parent p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab5);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Mark));
                _mark = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk4));
                _unk4 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk8));
                _unk8 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unkc));
                _unkc = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unke));
                _unke = m_io.ReadU2le();
                M_Tracer.EndMember();
            }
            private uint _mark;
            private uint _unk4;
            private uint _unk8;
            private ushort _unkc;
            private ushort _unke;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab5Parent m_parent;
            public uint Mark { get { return _mark; } }
            public uint Unk4 { get { return _unk4; } }
            public uint Unk8 { get { return _unk8; } }
            public ushort Unkc { get { return _unkc; } }
            public ushort Unke { get { return _unke; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab5Parent M_Parent { get { return m_parent; } }
        }
        public partial class DpdEffect : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DpdEffect FromFile(string fileName)
            {
                return new DpdEffect(new KaitaiStream(fileName));
            }

            public DpdEffect(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DpdEffect);
                m_parent = p__parent;
                m_root = p__root;
                f_checkNextExistence = false;
                f_next = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(OffsetNext));
                _offsetNext = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk04));
                _unk04 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk08));
                _unk08 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk0C));
                _unk0C = m_io.ReadU4le();
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
                M_Tracer.BeginMember(nameof(Unk1C));
                _unk1C = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk20));
                _unk20 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk24));
                _unk24 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(CommandsCount));
                _commandsCount = m_io.ReadU2le();
                M_Tracer.EndMember();
                _commands = new List<DpdEffectCommand>((int) (CommandsCount));
                for (var i = 0; i < CommandsCount; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Commands));
                    _commands.Add(new DpdEffectCommand(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
            }
            private bool f_checkNextExistence;
            private uint _checkNextExistence;
            public uint CheckNextExistence
            {
                get
                {
                    if (f_checkNextExistence)
                        return _checkNextExistence;
                    long _pos = m_io.Pos;
                    m_io.Seek(OffsetNext);
                    M_Tracer.Seek(OffsetNext);
                    M_Tracer.BeginMember(nameof(CheckNextExistence));
                    _checkNextExistence = m_io.ReadU4le();
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_checkNextExistence = true;
                    return _checkNextExistence;
                }
            }
            private bool f_next;
            private DpdEffect _next;
            public DpdEffect Next
            {
                get
                {
                    if (f_next)
                        return _next;
                    if (CheckNextExistence != 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(OffsetNext);
                        M_Tracer.Seek(OffsetNext);
                        M_Tracer.BeginMember(nameof(Next));
                        _next = new DpdEffect(m_io, this, m_root, tracer: M_Tracer);
                        M_Tracer.EndMember();
                        m_io.Seek(_pos);
                        f_next = true;
                    }
                    return _next;
                }
            }
            private uint _offsetNext;
            private uint _unk04;
            private uint _unk08;
            private uint _unk0C;
            private uint _unk10;
            private uint _unk14;
            private uint _unk18;
            private uint _unk1C;
            private uint _unk20;
            private ushort _unk24;
            private ushort _commandsCount;
            private List<DpdEffectCommand> _commands;
            private Kh2Dpd m_root;
            private KaitaiStruct m_parent;
            public uint OffsetNext { get { return _offsetNext; } }
            public uint Unk04 { get { return _unk04; } }
            public uint Unk08 { get { return _unk08; } }
            public uint Unk0C { get { return _unk0C; } }
            public uint Unk10 { get { return _unk10; } }
            public uint Unk14 { get { return _unk14; } }
            public uint Unk18 { get { return _unk18; } }
            public uint Unk1C { get { return _unk1C; } }
            public uint Unk20 { get { return _unk20; } }
            public ushort Unk24 { get { return _unk24; } }
            public ushort CommandsCount { get { return _commandsCount; } }
            public List<DpdEffectCommand> Commands { get { return _commands; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab3B : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab3B FromFile(string fileName)
            {
                return new EffectsTab3B(new KaitaiStream(fileName));
            }

            public EffectsTab3B(KaitaiStream p__io, Kh2Dpd.EffectsTab3A p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab3B);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk0));
                _unk0 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2));
                _unk2 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Size));
                _size = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk8));
                _unk8 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unkc));
                _unkc = m_io.ReadU4le();
                M_Tracer.EndMember();
                _data = new List<EffectsTab3C>((int) ((Size / 16)));
                for (var i = 0; i < (Size / 16); i++)
                {
                    M_Tracer.BeginArrayMember(nameof(Data));
                    _data.Add(new EffectsTab3C(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
            }
            private ushort _unk0;
            private ushort _unk2;
            private uint _size;
            private uint _unk8;
            private uint _unkc;
            private List<EffectsTab3C> _data;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab3A m_parent;
            public ushort Unk0 { get { return _unk0; } }
            public ushort Unk2 { get { return _unk2; } }
            public uint Size { get { return _size; } }
            public uint Unk8 { get { return _unk8; } }
            public uint Unkc { get { return _unkc; } }
            public List<EffectsTab3C> Data { get { return _data; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab3A M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab4Vtx0 : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab4Vtx0 FromFile(string fileName)
            {
                return new EffectsTab4Vtx0(new KaitaiStream(fileName));
            }

            public EffectsTab4Vtx0(KaitaiStream p__io, Kh2Dpd.EffectsTab4VertSet p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab4Vtx0);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Clr0));
                _clr0 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Clr1));
                _clr1 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Clr2));
                _clr2 = new EffectRgba(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert0));
                _vert0 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert1));
                _vert1 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Vert2));
                _vert2 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Pad));
                _pad = m_io.ReadU2le();
                M_Tracer.EndMember();
            }
            private EffectRgba _clr0;
            private EffectRgba _clr1;
            private EffectRgba _clr2;
            private ushort _vert0;
            private ushort _vert1;
            private ushort _vert2;
            private ushort _pad;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab4VertSet m_parent;
            public EffectRgba Clr0 { get { return _clr0; } }
            public EffectRgba Clr1 { get { return _clr1; } }
            public EffectRgba Clr2 { get { return _clr2; } }
            public ushort Vert0 { get { return _vert0; } }
            public ushort Vert1 { get { return _vert1; } }
            public ushort Vert2 { get { return _vert2; } }
            public ushort Pad { get { return _pad; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab4VertSet M_Parent { get { return m_parent; } }
        }
        public partial class EffectsTab4 : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static EffectsTab4 FromFile(string fileName)
            {
                return new EffectsTab4(new KaitaiStream(fileName));
            }

            public EffectsTab4(KaitaiStream p__io, Kh2Dpd.EffectsTab4Parent p__parent = null, Kh2Dpd p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(EffectsTab4);
                m_parent = p__parent;
                m_root = p__root;
                f_norms = false;
                f_points = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Mark));
                _mark = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk04));
                _unk04 = m_io.ReadU4le();
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
                M_Tracer.BeginMember(nameof(OffPoints));
                _offPoints = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(OffNorms));
                _offNorms = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(TotalVerts));
                _totalVerts = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(NumPoints));
                _numPoints = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk24));
                _unk24 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk26));
                _unk26 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk28));
                _unk28 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2c));
                _unk2c = m_io.ReadU4le();
                M_Tracer.EndMember();
                _set = new List<EffectsTab4VertSet>();
                {
                    var i = 0;
                    EffectsTab4VertSet M_;
                    do {
                        M_Tracer.BeginArrayMember(nameof(Set));
                        M_ = new EffectsTab4VertSet(m_io, this, m_root, tracer: M_Tracer);
                        _set.Add(M_);
                        M_Tracer.EndArrayMember();
                        i++;
                    } while (!((i == 0 ? false : (i == 1 ? TotalVerts == M_.NumVerts : true))));
                }
            }
            private bool f_norms;
            private List<short> _norms;
            public List<short> Norms
            {
                get
                {
                    if (f_norms)
                        return _norms;
                    long _pos = m_io.Pos;
                    m_io.Seek(OffPoints);
                    M_Tracer.Seek(OffPoints);
                    _norms = new List<short>((int) ((3 * NumPoints)));
                    for (var i = 0; i < (3 * NumPoints); i++)
                    {
                        M_Tracer.BeginArrayMember(nameof(Norms));
                        _norms.Add(m_io.ReadS2le());
                        M_Tracer.EndArrayMember();
                    }
                    m_io.Seek(_pos);
                    f_norms = true;
                    return _norms;
                }
            }
            private bool f_points;
            private List<short> _points;
            public List<short> Points
            {
                get
                {
                    if (f_points)
                        return _points;
                    long _pos = m_io.Pos;
                    m_io.Seek(OffNorms);
                    M_Tracer.Seek(OffNorms);
                    _points = new List<short>((int) ((3 * NumPoints)));
                    for (var i = 0; i < (3 * NumPoints); i++)
                    {
                        M_Tracer.BeginArrayMember(nameof(Points));
                        _points.Add(m_io.ReadS2le());
                        M_Tracer.EndArrayMember();
                    }
                    m_io.Seek(_pos);
                    f_points = true;
                    return _points;
                }
            }
            private uint _mark;
            private uint _unk04;
            private uint _unk08;
            private uint _unk0c;
            private uint _unk10;
            private uint _unk14;
            private uint _offPoints;
            private uint _offNorms;
            private ushort _totalVerts;
            private ushort _numPoints;
            private ushort _unk24;
            private ushort _unk26;
            private uint _unk28;
            private uint _unk2c;
            private List<EffectsTab4VertSet> _set;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab4Parent m_parent;
            public uint Mark { get { return _mark; } }
            public uint Unk04 { get { return _unk04; } }
            public uint Unk08 { get { return _unk08; } }
            public uint Unk0c { get { return _unk0c; } }
            public uint Unk10 { get { return _unk10; } }
            public uint Unk14 { get { return _unk14; } }
            public uint OffPoints { get { return _offPoints; } }
            public uint OffNorms { get { return _offNorms; } }
            public ushort TotalVerts { get { return _totalVerts; } }
            public ushort NumPoints { get { return _numPoints; } }
            public ushort Unk24 { get { return _unk24; } }
            public ushort Unk26 { get { return _unk26; } }
            public uint Unk28 { get { return _unk28; } }
            public uint Unk2c { get { return _unk2c; } }
            public List<EffectsTab4VertSet> Set { get { return _set; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab4Parent M_Parent { get { return m_parent; } }
        }
        private uint _magicCode96;
        private uint _numEffectsGroupList;
        private List<EffectsGroupParent> _offEffectsGroupList;
        private uint _numTextures;
        private List<EffectsTextureParent> _offTextures;
        private uint _numTab3;
        private List<EffectsTab3Parent> _offTab3;
        private uint _numTab4;
        private List<EffectsTab4Parent> _offTab4;
        private uint _numTab5;
        private List<EffectsTab5Parent> _offTab5;
        private Kh2Dpd m_root;
        private KaitaiStruct m_parent;
        public uint MagicCode96 { get { return _magicCode96; } }
        public uint NumEffectsGroupList { get { return _numEffectsGroupList; } }
        public List<EffectsGroupParent> OffEffectsGroupList { get { return _offEffectsGroupList; } }
        public uint NumTextures { get { return _numTextures; } }
        public List<EffectsTextureParent> OffTextures { get { return _offTextures; } }
        public uint NumTab3 { get { return _numTab3; } }
        public List<EffectsTab3Parent> OffTab3 { get { return _offTab3; } }
        public uint NumTab4 { get { return _numTab4; } }
        public List<EffectsTab4Parent> OffTab4 { get { return _offTab4; } }
        public uint NumTab5 { get { return _numTab5; } }
        public List<EffectsTab5Parent> OffTab5 { get { return _offTab5; } }
        public Kh2Dpd M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
