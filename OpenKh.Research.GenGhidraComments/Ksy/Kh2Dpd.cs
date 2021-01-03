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
                M_Tracer.BeginMember(nameof(Ofs));
                _ofs = m_io.ReadU4le();
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
                    m_io.Seek(Ofs);
                    M_Tracer.Seek(Ofs);
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
            private uint _ofs;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Ofs { get { return _ofs; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd M_Parent { get { return m_parent; } }
            public byte[] M_RawItem { get { return __raw_item; } }
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
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Skip));
                _skip = m_io.ReadBytes(16);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Item));
                _item = new DpdEffect(m_io, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
            }
            private byte[] _skip;
            private DpdEffect _item;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsGroup m_parent;
            public byte[] Skip { get { return _skip; } }
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
                M_Tracer.BeginMember(nameof(Ofs));
                _ofs = m_io.ReadU4le();
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
                    m_io.Seek(Ofs);
                    M_Tracer.Seek(Ofs);
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
            private uint _ofs;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Ofs { get { return _ofs; } }
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
                M_Tracer.BeginMember(nameof(Ofs));
                _ofs = m_io.ReadU4le();
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
                    m_io.Seek(Ofs);
                    M_Tracer.Seek(Ofs);
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
            private uint _ofs;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Ofs { get { return _ofs; } }
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
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Mark));
                _mark = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private uint _mark;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab3Parent m_parent;
            public uint Mark { get { return _mark; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd.EffectsTab3Parent M_Parent { get { return m_parent; } }
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
                M_Tracer.BeginMember(nameof(Ofs));
                _ofs = m_io.ReadU4le();
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
                    m_io.Seek(Ofs);
                    M_Tracer.Seek(Ofs);
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
            private uint _ofs;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Ofs { get { return _ofs; } }
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
                M_Tracer.BeginMember(nameof(SkipUnkData));
                _skipUnkData = m_io.ReadBytes(80);
                M_Tracer.EndMember();
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
            private byte[] _skipUnkData;
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
            public byte[] SkipUnkData { get { return _skipUnkData; } }
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
                M_Tracer.BeginMember(nameof(Ofs));
                _ofs = m_io.ReadU4le();
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
                    m_io.Seek(Ofs);
                    M_Tracer.Seek(Ofs);
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
            private uint _ofs;
            private Kh2Dpd m_root;
            private Kh2Dpd m_parent;
            private byte[] __raw_item;
            public uint Ofs { get { return _ofs; } }
            public Kh2Dpd M_Root { get { return m_root; } }
            public Kh2Dpd M_Parent { get { return m_parent; } }
            public byte[] M_RawItem { get { return __raw_item; } }
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
            }
            private uint _mark;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab5Parent m_parent;
            public uint Mark { get { return _mark; } }
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
            private bool f_next;
            private DpdEffect _next;
            public DpdEffect Next
            {
                get
                {
                    if (f_next)
                        return _next;
                    if (OffsetNext != 0) {
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
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Mark));
                _mark = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private uint _mark;
            private Kh2Dpd m_root;
            private Kh2Dpd.EffectsTab4Parent m_parent;
            public uint Mark { get { return _mark; } }
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
