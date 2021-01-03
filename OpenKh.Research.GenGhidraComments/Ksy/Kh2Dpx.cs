// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public partial class Kh2Dpx : KaitaiStruct
    {
        public Tracer M_Tracer;

        public static Kh2Dpx FromFile(string fileName)
        {
            return new Kh2Dpx(new KaitaiStream(fileName));
        }

        public Kh2Dpx(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Dpx p__root = null, Tracer tracer = null) : base(p__io)
        {
            M_Tracer = tracer;
            var entityName = nameof(Kh2Dpx);
            m_parent = p__parent;
            m_root = p__root ?? this;
            M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
            _read();
            M_Tracer.EndRead();
        }
        private void _read()
        {
            M_Tracer.BeginMember(nameof(MagicCode82));
            _magicCode82 = m_io.ReadU4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(Unk04));
            _unk04 = m_io.ReadU4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(Unk08));
            _unk08 = m_io.ReadU4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(DpxEntries));
            _dpxEntries = m_io.ReadU4le();
            M_Tracer.EndMember();
            _dpxEnts = new List<DpxEntry>((int) (DpxEntries));
            for (var i = 0; i < DpxEntries; i++)
            {
                M_Tracer.BeginArrayMember(nameof(DpxEnts));
                _dpxEnts.Add(new DpxEntry(m_io, this, m_root, tracer: M_Tracer));
                M_Tracer.EndArrayMember();
            }
        }
        public partial class DpxEntry : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DpxEntry FromFile(string fileName)
            {
                return new DpxEntry(new KaitaiStream(fileName));
            }

            public DpxEntry(KaitaiStream p__io, Kh2Dpx p__parent = null, Kh2Dpx p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DpxEntry);
                m_parent = p__parent;
                m_root = p__root;
                f_dpd = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(DpdOffset));
                _dpdOffset = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Index));
                _index = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Id));
                _id = m_io.ReadU4le();
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
            }
            private bool f_dpd;
            private Kh2Dpd _dpd;
            public Kh2Dpd Dpd
            {
                get
                {
                    if (f_dpd)
                        return _dpd;
                    long _pos = m_io.Pos;
                    m_io.Seek(DpdOffset);
                    M_Tracer.Seek(DpdOffset);
                    __raw_dpd = m_io.ReadBytesFull();
                    var io___raw_dpd = new KaitaiStream(__raw_dpd);
                    M_Tracer.DeclareNewIo();
                    M_Tracer.BeginMember(nameof(Dpd));
                    _dpd = new Kh2Dpd(io___raw_dpd, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_dpd = true;
                    return _dpd;
                }
            }
            private uint _dpdOffset;
            private uint _index;
            private uint _id;
            private uint _unk0C;
            private uint _unk10;
            private uint _unk14;
            private uint _unk18;
            private uint _unk1C;
            private Kh2Dpx m_root;
            private Kh2Dpx m_parent;
            private byte[] __raw_dpd;
            public uint DpdOffset { get { return _dpdOffset; } }
            public uint Index { get { return _index; } }
            public uint Id { get { return _id; } }
            public uint Unk0C { get { return _unk0C; } }
            public uint Unk10 { get { return _unk10; } }
            public uint Unk14 { get { return _unk14; } }
            public uint Unk18 { get { return _unk18; } }
            public uint Unk1C { get { return _unk1C; } }
            public Kh2Dpx M_Root { get { return m_root; } }
            public Kh2Dpx M_Parent { get { return m_parent; } }
            public byte[] M_RawDpd { get { return __raw_dpd; } }
        }
        private uint _magicCode82;
        private uint _unk04;
        private uint _unk08;
        private uint _dpxEntries;
        private List<DpxEntry> _dpxEnts;
        private Kh2Dpx m_root;
        private KaitaiStruct m_parent;
        public uint MagicCode82 { get { return _magicCode82; } }
        public uint Unk04 { get { return _unk04; } }
        public uint Unk08 { get { return _unk08; } }
        public uint DpxEntries { get { return _dpxEntries; } }
        public List<DpxEntry> DpxEnts { get { return _dpxEnts; } }
        public Kh2Dpx M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}
