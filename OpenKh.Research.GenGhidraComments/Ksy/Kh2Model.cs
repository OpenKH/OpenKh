// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public partial class Kh2Model : KaitaiStruct
    {
        public Tracer M_Tracer;

        public static Kh2Model FromFile(string fileName)
        {
            return new Kh2Model(new KaitaiStream(fileName));
        }


        public enum ModelType
        {
            Map = 2,
            Object = 3,
            Shadow = 4,
        }

        public enum VifCmd
        {
            Nop = 0,
            Stcycl = 1,
            Offset = 2,
            Base = 3,
            Itop = 4,
            Stmod = 5,
            Mskpath3 = 6,
            Mark = 7,
            Flushe = 16,
            Flush = 17,
            Flusha = 19,
            Mscal = 20,
            Mscalf = 21,
            Mscnt = 23,
            Stmask = 32,
            Strow = 48,
            Stcol = 49,
            Mpg = 74,
            Direct = 80,
            Directhl = 81,
            UnmaskedS32 = 96,
            UnmaskedS16 = 97,
            UnmaskedS8 = 98,
            UnmaskedV232 = 100,
            UnmaskedV216 = 101,
            UnmaskedV28 = 102,
            UnmaskedV232Alt = 104,
            UnmaskedV316 = 105,
            UnmaskedV38 = 106,
            UnmaskedV432 = 108,
            UnmaskedV416 = 109,
            UnmaskedV48 = 110,
            UnmaskedV45 = 111,
            MaskedS32 = 112,
            MaskedS16 = 113,
            MaskedS8 = 114,
            MaskedV232 = 116,
            MaskedV216 = 117,
            MaskedV28 = 118,
            MaskedV232Alt = 120,
            MaskedV316 = 121,
            MaskedV38 = 122,
            MaskedV432 = 124,
            MaskedV416 = 125,
            MaskedV48 = 126,
            MaskedV45 = 127,
        }

        public enum DmaTagId
        {
            Refe = 0,
            Cnt = 1,
            Next = 2,
            Ref = 3,
            Refs = 4,
            Call = 5,
            Ret = 6,
            End = 7,
        }
        public Kh2Model(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
        {
            M_Tracer = tracer;
            var entityName = nameof(Kh2Model);
            m_parent = p__parent;
            m_root = p__root ?? this;
            f_modelInst = false;
            M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
            _read();
            M_Tracer.EndRead();
        }
        private void _read()
        {
            M_Tracer.BeginMember(nameof(Hw));
            _hw = m_io.ReadBytes(144);
            M_Tracer.EndMember();
        }
        public partial class GifTag : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static GifTag FromFile(string fileName)
            {
                return new GifTag(new KaitaiStream(fileName));
            }

            public GifTag(KaitaiStream p__io, Kh2Model.SourceChainDmaTag p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(GifTag);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Nloop));
                _nloop = m_io.ReadBitsIntBe(15);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Eop));
                _eop = m_io.ReadBitsIntBe(1) != 0;
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Skip));
                _skip = m_io.ReadBitsIntBe(31);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Pre));
                _pre = m_io.ReadBitsIntBe(1) != 0;
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Prim));
                _prim = m_io.ReadBitsIntBe(10);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Flg));
                _flg = m_io.ReadBitsIntBe(2);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Nreg));
                _nreg = m_io.ReadBitsIntBe(4);
                M_Tracer.EndMember();
                m_io.AlignToByte();
                M_Tracer.BeginMember(nameof(Regs));
                _regs = m_io.ReadU8le();
                M_Tracer.EndMember();
            }
            private ulong _nloop;
            private bool _eop;
            private ulong _skip;
            private bool _pre;
            private ulong _prim;
            private ulong _flg;
            private ulong _nreg;
            private ulong _regs;
            private Kh2Model m_root;
            private Kh2Model.SourceChainDmaTag m_parent;
            public ulong Nloop { get { return _nloop; } }
            public bool Eop { get { return _eop; } }
            public ulong Skip { get { return _skip; } }
            public bool Pre { get { return _pre; } }
            public ulong Prim { get { return _prim; } }
            public ulong Flg { get { return _flg; } }
            public ulong Nreg { get { return _nreg; } }
            public ulong Regs { get { return _regs; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.SourceChainDmaTag M_Parent { get { return m_parent; } }
        }
        public partial class DmaChainMap : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DmaChainMap FromFile(string fileName)
            {
                return new DmaChainMap(new KaitaiStream(fileName));
            }

            public DmaChainMap(KaitaiStream p__io, Kh2Model.MapDesc p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DmaChainMap);
                m_parent = p__parent;
                m_root = p__root;
                f_dmaTags = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(DmaTagOff));
                _dmaTagOff = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(TextureIdx));
                _textureIdx = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk1));
                _unk1 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2));
                _unk2 = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_dmaTags;
            private DmaTagArrayMap _dmaTags;
            public DmaTagArrayMap DmaTags
            {
                get
                {
                    if (f_dmaTags)
                        return _dmaTags;
                    long _pos = m_io.Pos;
                    m_io.Seek(DmaTagOff);
                    M_Tracer.Seek(DmaTagOff);
                    M_Tracer.BeginMember(nameof(DmaTags));
                    _dmaTags = new DmaTagArrayMap(m_io, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_dmaTags = true;
                    return _dmaTags;
                }
            }
            private uint _dmaTagOff;
            private uint _textureIdx;
            private uint _unk1;
            private uint _unk2;
            private Kh2Model m_root;
            private Kh2Model.MapDesc m_parent;
            public uint DmaTagOff { get { return _dmaTagOff; } }
            public uint TextureIdx { get { return _textureIdx; } }
            public uint Unk1 { get { return _unk1; } }
            public uint Unk2 { get { return _unk2; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.MapDesc M_Parent { get { return m_parent; } }
        }
        public partial class Model : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static Model FromFile(string fileName)
            {
                return new Model(new KaitaiStream(fileName));
            }

            public Model(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(Model);
                m_parent = p__parent;
                m_root = p__root;
                f_subModel = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Type));
                _type = ((Kh2Model.ModelType) m_io.ReadU4le());
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk1));
                _unk1 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2));
                _unk2 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(NextOff));
                _nextOff = m_io.ReadU4le();
                M_Tracer.EndMember();
                if (Type == Kh2Model.ModelType.Map) {
                    M_Tracer.BeginMember(nameof(MapDesc));
                    _mapDesc = new MapDesc(m_io, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                }
                if ( ((Type == Kh2Model.ModelType.Object) || (Type == Kh2Model.ModelType.Shadow)) ) {
                    M_Tracer.BeginMember(nameof(ObjectDesc));
                    _objectDesc = new ObjectDesc(m_io, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                }
            }
            private bool f_subModel;
            private Model _subModel;
            public Model SubModel
            {
                get
                {
                    if (f_subModel)
                        return _subModel;
                    if (NextOff != 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(NextOff);
                        M_Tracer.Seek(NextOff);
                        __raw_subModel = m_io.ReadBytesFull();
                        var io___raw_subModel = new KaitaiStream(__raw_subModel);
                        M_Tracer.DeclareNewIo();
                        M_Tracer.BeginMember(nameof(SubModel));
                        _subModel = new Model(io___raw_subModel, this, m_root, tracer: M_Tracer);
                        M_Tracer.EndMember();
                        m_io.Seek(_pos);
                        f_subModel = true;
                    }
                    return _subModel;
                }
            }
            private ModelType _type;
            private uint _unk1;
            private uint _unk2;
            private uint _nextOff;
            private MapDesc _mapDesc;
            private ObjectDesc _objectDesc;
            private Kh2Model m_root;
            private KaitaiStruct m_parent;
            private byte[] __raw_subModel;
            public ModelType Type { get { return _type; } }
            public uint Unk1 { get { return _unk1; } }
            public uint Unk2 { get { return _unk2; } }
            public uint NextOff { get { return _nextOff; } }
            public MapDesc MapDesc { get { return _mapDesc; } }
            public ObjectDesc ObjectDesc { get { return _objectDesc; } }
            public Kh2Model M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
            public byte[] M_RawSubModel { get { return __raw_subModel; } }
        }
        public partial class DmaChainIndexRemapTable : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DmaChainIndexRemapTable FromFile(string fileName)
            {
                return new DmaChainIndexRemapTable(new KaitaiStream(fileName));
            }

            public DmaChainIndexRemapTable(KaitaiStream p__io, Kh2Model.MapDesc p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DmaChainIndexRemapTable);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(NextOff));
                _nextOff = m_io.ReadU4le();
                M_Tracer.EndMember();
                _dmaChainIndex = new List<ushort>();
                {
                    var i = 0;
                    ushort M_;
                    do {
                        M_Tracer.BeginArrayMember(nameof(DmaChainIndex));
                        M_ = m_io.ReadU2le();
                        _dmaChainIndex.Add(M_);
                        M_Tracer.EndArrayMember();
                        i++;
                    } while (!(M_ == 65535));
                }
            }
            private uint _nextOff;
            private List<ushort> _dmaChainIndex;
            private Kh2Model m_root;
            private Kh2Model.MapDesc m_parent;
            public uint NextOff { get { return _nextOff; } }
            public List<ushort> DmaChainIndex { get { return _dmaChainIndex; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.MapDesc M_Parent { get { return m_parent; } }
        }
        public partial class AxBone : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static AxBone FromFile(string fileName)
            {
                return new AxBone(new KaitaiStream(fileName));
            }

            public AxBone(KaitaiStream p__io, Kh2Model.ObjectDesc p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(AxBone);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(ThisIdx));
                _thisIdx = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ThisReverseIdx));
                _thisReverseIdx = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ParentIdx));
                _parentIdx = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(ParentReverseIdx));
                _parentReverseIdx = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk1));
                _unk1 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2));
                _unk2 = m_io.ReadU4le();
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
                M_Tracer.BeginMember(nameof(Unk3));
                _unk3 = m_io.ReadF4le();
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
                M_Tracer.BeginMember(nameof(Unk4));
                _unk4 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(TranslateX));
                _translateX = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(TranslateY));
                _translateY = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(TranslateZ));
                _translateZ = m_io.ReadF4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk5));
                _unk5 = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private ushort _thisIdx;
            private ushort _thisReverseIdx;
            private ushort _parentIdx;
            private ushort _parentReverseIdx;
            private uint _unk1;
            private uint _unk2;
            private float _scaleX;
            private float _scaleY;
            private float _scaleZ;
            private float _unk3;
            private float _rotationX;
            private float _rotationY;
            private float _rotationZ;
            private uint _unk4;
            private float _translateX;
            private float _translateY;
            private float _translateZ;
            private uint _unk5;
            private Kh2Model m_root;
            private Kh2Model.ObjectDesc m_parent;
            public ushort ThisIdx { get { return _thisIdx; } }
            public ushort ThisReverseIdx { get { return _thisReverseIdx; } }
            public ushort ParentIdx { get { return _parentIdx; } }
            public ushort ParentReverseIdx { get { return _parentReverseIdx; } }
            public uint Unk1 { get { return _unk1; } }
            public uint Unk2 { get { return _unk2; } }
            public float ScaleX { get { return _scaleX; } }
            public float ScaleY { get { return _scaleY; } }
            public float ScaleZ { get { return _scaleZ; } }
            public float Unk3 { get { return _unk3; } }
            public float RotationX { get { return _rotationX; } }
            public float RotationY { get { return _rotationY; } }
            public float RotationZ { get { return _rotationZ; } }
            public uint Unk4 { get { return _unk4; } }
            public float TranslateX { get { return _translateX; } }
            public float TranslateY { get { return _translateY; } }
            public float TranslateZ { get { return _translateZ; } }
            public uint Unk5 { get { return _unk5; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.ObjectDesc M_Parent { get { return m_parent; } }
        }
        public partial class SourceChainDmaTag : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static SourceChainDmaTag FromFile(string fileName)
            {
                return new SourceChainDmaTag(new KaitaiStream(fileName));
            }

            public SourceChainDmaTag(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(SourceChainDmaTag);
                m_parent = p__parent;
                m_root = p__root;
                f_endTransfer = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Qwc));
                _qwc = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Pad));
                _pad = m_io.ReadBitsIntBe(8);
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Irq));
                _irq = m_io.ReadBitsIntBe(1) != 0;
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(TagId));
                _tagId = ((Kh2Model.DmaTagId) m_io.ReadBitsIntBe(3));
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Pce));
                _pce = m_io.ReadBitsIntBe(2);
                M_Tracer.EndMember();
                m_io.AlignToByte();
                M_Tracer.BeginMember(nameof(Addr));
                _addr = m_io.ReadU4le();
                M_Tracer.EndMember();
                _vifTag = new List<VifTag>((int) (2));
                for (var i = 0; i < 2; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(VifTag));
                    _vifTag.Add(new VifTag(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
                if ( (( ((TagId == Kh2Model.DmaTagId.Cnt) || (TagId == Kh2Model.DmaTagId.Next) || (TagId == Kh2Model.DmaTagId.Refe) || (TagId == Kh2Model.DmaTagId.Call) || (TagId == Kh2Model.DmaTagId.Ret)) ) && (VifTag[1].Cmd == Kh2Model.VifCmd.Direct)) ) {
                    _gifTag = new List<GifTag>((int) (Qwc));
                    for (var i = 0; i < Qwc; i++)
                    {
                        M_Tracer.BeginArrayMember(nameof(GifTag));
                        _gifTag.Add(new GifTag(m_io, this, m_root, tracer: M_Tracer));
                        M_Tracer.EndArrayMember();
                    }
                }
                if ( (( ((TagId == Kh2Model.DmaTagId.Cnt) || (TagId == Kh2Model.DmaTagId.Next) || (TagId == Kh2Model.DmaTagId.Refe) || (TagId == Kh2Model.DmaTagId.Call) || (TagId == Kh2Model.DmaTagId.Ret)) ) && (VifTag[1].Cmd != Kh2Model.VifCmd.Direct)) ) {
                    _rawData = new List<byte[]>((int) (Qwc));
                    for (var i = 0; i < Qwc; i++)
                    {
                        M_Tracer.BeginArrayMember(nameof(RawData));
                        _rawData.Add(m_io.ReadBytes(16));
                        M_Tracer.EndArrayMember();
                    }
                }
            }
            private bool f_endTransfer;
            private bool _endTransfer;
            public bool EndTransfer
            {
                get
                {
                    if (f_endTransfer)
                        return _endTransfer;
                    M_Tracer.BeginMember(nameof(EndTransfer));
                    _endTransfer = (bool) ( ((TagId == Kh2Model.DmaTagId.Refe) || (TagId == Kh2Model.DmaTagId.End) || (TagId == Kh2Model.DmaTagId.Ret)) );
                    M_Tracer.EndMember();
                    f_endTransfer = true;
                    return _endTransfer;
                }
            }
            private ushort _qwc;
            private ulong _pad;
            private bool _irq;
            private DmaTagId _tagId;
            private ulong _pce;
            private uint _addr;
            private List<VifTag> _vifTag;
            private List<GifTag> _gifTag;
            private List<byte[]> _rawData;
            private Kh2Model m_root;
            private KaitaiStruct m_parent;
            public ushort Qwc { get { return _qwc; } }
            public ulong Pad { get { return _pad; } }
            public bool Irq { get { return _irq; } }
            public DmaTagId TagId { get { return _tagId; } }
            public ulong Pce { get { return _pce; } }
            public uint Addr { get { return _addr; } }
            public List<VifTag> VifTag { get { return _vifTag; } }
            public List<GifTag> GifTag { get { return _gifTag; } }
            public List<byte[]> RawData { get { return _rawData; } }
            public Kh2Model M_Root { get { return m_root; } }
            public KaitaiStruct M_Parent { get { return m_parent; } }
        }
        public partial class ObjectDesc : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static ObjectDesc FromFile(string fileName)
            {
                return new ObjectDesc(new KaitaiStream(fileName));
            }

            public ObjectDesc(KaitaiStream p__io, Kh2Model.Model p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(ObjectDesc);
                m_parent = p__parent;
                m_root = p__root;
                f_axBone = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(NumAxbone));
                _numAxbone = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk1));
                _unk1 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(OffAxbone));
                _offAxbone = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2));
                _unk2 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(CntModelParts));
                _cntModelParts = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk3));
                _unk3 = m_io.ReadU2le();
                M_Tracer.EndMember();
                _modelParts = new List<ModelPart>((int) (CntModelParts));
                for (var i = 0; i < CntModelParts; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(ModelParts));
                    _modelParts.Add(new ModelPart(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
            }
            private bool f_axBone;
            private List<AxBone> _axBone;
            public List<AxBone> AxBone
            {
                get
                {
                    if (f_axBone)
                        return _axBone;
                    if (OffAxbone != 0) {
                        long _pos = m_io.Pos;
                        m_io.Seek(OffAxbone);
                        M_Tracer.Seek(OffAxbone);
                        _axBone = new List<AxBone>((int) (NumAxbone));
                        for (var i = 0; i < NumAxbone; i++)
                        {
                            M_Tracer.BeginArrayMember(nameof(AxBone));
                            _axBone.Add(new AxBone(m_io, this, m_root, tracer: M_Tracer));
                            M_Tracer.EndArrayMember();
                        }
                        m_io.Seek(_pos);
                        f_axBone = true;
                    }
                    return _axBone;
                }
            }
            private ushort _numAxbone;
            private ushort _unk1;
            private uint _offAxbone;
            private uint _unk2;
            private ushort _cntModelParts;
            private ushort _unk3;
            private List<ModelPart> _modelParts;
            private Kh2Model m_root;
            private Kh2Model.Model m_parent;
            public ushort NumAxbone { get { return _numAxbone; } }
            public ushort Unk1 { get { return _unk1; } }
            public uint OffAxbone { get { return _offAxbone; } }
            public uint Unk2 { get { return _unk2; } }
            public ushort CntModelParts { get { return _cntModelParts; } }
            public ushort Unk3 { get { return _unk3; } }
            public List<ModelPart> ModelParts { get { return _modelParts; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.Model M_Parent { get { return m_parent; } }
        }
        public partial class IndicesOfAxbone : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static IndicesOfAxbone FromFile(string fileName)
            {
                return new IndicesOfAxbone(new KaitaiStream(fileName));
            }

            public IndicesOfAxbone(KaitaiStream p__io, Kh2Model.ModelPart p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(IndicesOfAxbone);
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
                _indexOfAxbone = new List<uint>((int) (Count));
                for (var i = 0; i < Count; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(IndexOfAxbone));
                    _indexOfAxbone.Add(m_io.ReadU4le());
                    M_Tracer.EndArrayMember();
                }
            }
            private uint _count;
            private List<uint> _indexOfAxbone;
            private Kh2Model m_root;
            private Kh2Model.ModelPart m_parent;
            public uint Count { get { return _count; } }
            public List<uint> IndexOfAxbone { get { return _indexOfAxbone; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.ModelPart M_Parent { get { return m_parent; } }
        }
        public partial class ModelPart : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static ModelPart FromFile(string fileName)
            {
                return new ModelPart(new KaitaiStream(fileName));
            }

            public ModelPart(KaitaiStream p__io, Kh2Model.ObjectDesc p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(ModelPart);
                m_parent = p__parent;
                m_root = p__root;
                f_dmaTags = false;
                f_indicesOfAxbone = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Unk1));
                _unk1 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(TextureIndex));
                _textureIndex = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk2));
                _unk2 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk3));
                _unk3 = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(OffFirstDmaTag));
                _offFirstDmaTag = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(OffIndicesOfAxbone));
                _offIndicesOfAxbone = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(NumDmaQwcPackets));
                _numDmaQwcPackets = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk5));
                _unk5 = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_dmaTags;
            private DmaTagArrayObject _dmaTags;
            public DmaTagArrayObject DmaTags
            {
                get
                {
                    if (f_dmaTags)
                        return _dmaTags;
                    long _pos = m_io.Pos;
                    m_io.Seek(OffFirstDmaTag);
                    M_Tracer.Seek(OffFirstDmaTag);
                    __raw_dmaTags = m_io.ReadBytes((16 * NumDmaQwcPackets));
                    var io___raw_dmaTags = new KaitaiStream(__raw_dmaTags);
                    M_Tracer.DeclareNewIo();
                    M_Tracer.BeginMember(nameof(DmaTags));
                    _dmaTags = new DmaTagArrayObject(io___raw_dmaTags, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_dmaTags = true;
                    return _dmaTags;
                }
            }
            private bool f_indicesOfAxbone;
            private IndicesOfAxbone _indicesOfAxbone;
            public IndicesOfAxbone IndicesOfAxbone
            {
                get
                {
                    if (f_indicesOfAxbone)
                        return _indicesOfAxbone;
                    long _pos = m_io.Pos;
                    m_io.Seek(OffIndicesOfAxbone);
                    M_Tracer.Seek(OffIndicesOfAxbone);
                    M_Tracer.BeginMember(nameof(IndicesOfAxbone));
                    _indicesOfAxbone = new IndicesOfAxbone(m_io, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_indicesOfAxbone = true;
                    return _indicesOfAxbone;
                }
            }
            private uint _unk1;
            private uint _textureIndex;
            private uint _unk2;
            private uint _unk3;
            private uint _offFirstDmaTag;
            private uint _offIndicesOfAxbone;
            private uint _numDmaQwcPackets;
            private uint _unk5;
            private Kh2Model m_root;
            private Kh2Model.ObjectDesc m_parent;
            private byte[] __raw_dmaTags;
            public uint Unk1 { get { return _unk1; } }
            public uint TextureIndex { get { return _textureIndex; } }
            public uint Unk2 { get { return _unk2; } }
            public uint Unk3 { get { return _unk3; } }
            public uint OffFirstDmaTag { get { return _offFirstDmaTag; } }
            public uint OffIndicesOfAxbone { get { return _offIndicesOfAxbone; } }
            public uint NumDmaQwcPackets { get { return _numDmaQwcPackets; } }
            public uint Unk5 { get { return _unk5; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.ObjectDesc M_Parent { get { return m_parent; } }
            public byte[] M_RawDmaTags { get { return __raw_dmaTags; } }
        }
        public partial class VifPacketRenderingGroup : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static VifPacketRenderingGroup FromFile(string fileName)
            {
                return new VifPacketRenderingGroup(new KaitaiStream(fileName));
            }

            public VifPacketRenderingGroup(KaitaiStream p__io, Kh2Model.MapDesc p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(VifPacketRenderingGroup);
                m_parent = p__parent;
                m_root = p__root;
                f_list = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(OffsetToGroup));
                _offsetToGroup = m_io.ReadU4le();
                M_Tracer.EndMember();
            }
            private bool f_list;
            private List<ushort> _list;
            public List<ushort> List
            {
                get
                {
                    if (f_list)
                        return _list;
                    KaitaiStream io = M_Parent.M_Io;
                    long _pos = io.Pos;
                    io.Seek(OffsetToGroup);
                    M_Tracer.Seek(OffsetToGroup);
                    _list = new List<ushort>();
                    {
                        var i = 0;
                        ushort M_;
                        do {
                            M_Tracer.BeginArrayMember(nameof(List));
                            M_ = io.ReadU2le();
                            _list.Add(M_);
                            M_Tracer.EndArrayMember();
                            i++;
                        } while (!(M_ == 65535));
                    }
                    io.Seek(_pos);
                    f_list = true;
                    return _list;
                }
            }
            private uint _offsetToGroup;
            private Kh2Model m_root;
            private Kh2Model.MapDesc m_parent;
            public uint OffsetToGroup { get { return _offsetToGroup; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.MapDesc M_Parent { get { return m_parent; } }
        }
        public partial class DmaTagArrayMap : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DmaTagArrayMap FromFile(string fileName)
            {
                return new DmaTagArrayMap(new KaitaiStream(fileName));
            }

            public DmaTagArrayMap(KaitaiStream p__io, Kh2Model.DmaChainMap p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DmaTagArrayMap);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                _dmaTag = new List<SourceChainDmaTag>();
                {
                    var i = 0;
                    SourceChainDmaTag M_;
                    do {
                        M_Tracer.BeginArrayMember(nameof(DmaTag));
                        M_ = new SourceChainDmaTag(m_io, this, m_root, tracer: M_Tracer);
                        _dmaTag.Add(M_);
                        M_Tracer.EndArrayMember();
                        i++;
                    } while (!(M_.EndTransfer));
                }
            }
            private List<SourceChainDmaTag> _dmaTag;
            private Kh2Model m_root;
            private Kh2Model.DmaChainMap m_parent;
            public List<SourceChainDmaTag> DmaTag { get { return _dmaTag; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.DmaChainMap M_Parent { get { return m_parent; } }
        }
        public partial class MapDesc : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static MapDesc FromFile(string fileName)
            {
                return new MapDesc(new KaitaiStream(fileName));
            }

            public MapDesc(KaitaiStream p__io, Kh2Model.Model p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(MapDesc);
                m_parent = p__parent;
                m_root = p__root;
                f_vifPacketRenderingGroup = false;
                f_dmaChainIndexRemapTable = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(NumDmaChainMaps));
                _numDmaChainMaps = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Unk3));
                _unk3 = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(NumVifPacketRenderingGroup));
                _numVifPacketRenderingGroup = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(OffVifPacketRenderingGroup));
                _offVifPacketRenderingGroup = m_io.ReadU4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(OffDmaChainIndexRemapTable));
                _offDmaChainIndexRemapTable = m_io.ReadU4le();
                M_Tracer.EndMember();
                _dmaChainMap = new List<DmaChainMap>((int) (NumDmaChainMaps));
                for (var i = 0; i < NumDmaChainMaps; i++)
                {
                    M_Tracer.BeginArrayMember(nameof(DmaChainMap));
                    _dmaChainMap.Add(new DmaChainMap(m_io, this, m_root, tracer: M_Tracer));
                    M_Tracer.EndArrayMember();
                }
            }
            private bool f_vifPacketRenderingGroup;
            private List<VifPacketRenderingGroup> _vifPacketRenderingGroup;
            public List<VifPacketRenderingGroup> VifPacketRenderingGroup
            {
                get
                {
                    if (f_vifPacketRenderingGroup)
                        return _vifPacketRenderingGroup;
                    long _pos = m_io.Pos;
                    m_io.Seek(OffVifPacketRenderingGroup);
                    M_Tracer.Seek(OffVifPacketRenderingGroup);
                    __raw_vifPacketRenderingGroup = new List<byte[]>((int) (NumVifPacketRenderingGroup));
                    _vifPacketRenderingGroup = new List<VifPacketRenderingGroup>((int) (NumVifPacketRenderingGroup));
                    for (var i = 0; i < NumVifPacketRenderingGroup; i++)
                    {
                        M_Tracer.BeginArrayMember(nameof(M_RawVifPacketRenderingGroup));
                        __raw_vifPacketRenderingGroup.Add(m_io.ReadBytes(4));
                        M_Tracer.EndArrayMember();
                        var io___raw_vifPacketRenderingGroup = new KaitaiStream(__raw_vifPacketRenderingGroup[__raw_vifPacketRenderingGroup.Count - 1]);
                        M_Tracer.DeclareNewIo();
                        M_Tracer.BeginArrayMember(nameof(VifPacketRenderingGroup));
                        _vifPacketRenderingGroup.Add(new VifPacketRenderingGroup(io___raw_vifPacketRenderingGroup, this, m_root, tracer: M_Tracer));
                        M_Tracer.EndArrayMember();
                    }
                    m_io.Seek(_pos);
                    f_vifPacketRenderingGroup = true;
                    return _vifPacketRenderingGroup;
                }
            }
            private bool f_dmaChainIndexRemapTable;
            private DmaChainIndexRemapTable _dmaChainIndexRemapTable;
            public DmaChainIndexRemapTable DmaChainIndexRemapTable
            {
                get
                {
                    if (f_dmaChainIndexRemapTable)
                        return _dmaChainIndexRemapTable;
                    long _pos = m_io.Pos;
                    m_io.Seek(OffDmaChainIndexRemapTable);
                    M_Tracer.Seek(OffDmaChainIndexRemapTable);
                    M_Tracer.BeginMember(nameof(DmaChainIndexRemapTable));
                    _dmaChainIndexRemapTable = new DmaChainIndexRemapTable(m_io, this, m_root, tracer: M_Tracer);
                    M_Tracer.EndMember();
                    m_io.Seek(_pos);
                    f_dmaChainIndexRemapTable = true;
                    return _dmaChainIndexRemapTable;
                }
            }
            private uint _numDmaChainMaps;
            private ushort _unk3;
            private ushort _numVifPacketRenderingGroup;
            private uint _offVifPacketRenderingGroup;
            private uint _offDmaChainIndexRemapTable;
            private List<DmaChainMap> _dmaChainMap;
            private Kh2Model m_root;
            private Kh2Model.Model m_parent;
            private List<byte[]> __raw_vifPacketRenderingGroup;
            public uint NumDmaChainMaps { get { return _numDmaChainMaps; } }
            public ushort Unk3 { get { return _unk3; } }
            public ushort NumVifPacketRenderingGroup { get { return _numVifPacketRenderingGroup; } }
            public uint OffVifPacketRenderingGroup { get { return _offVifPacketRenderingGroup; } }
            public uint OffDmaChainIndexRemapTable { get { return _offDmaChainIndexRemapTable; } }
            public List<DmaChainMap> DmaChainMap { get { return _dmaChainMap; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.Model M_Parent { get { return m_parent; } }
            public List<byte[]> M_RawVifPacketRenderingGroup { get { return __raw_vifPacketRenderingGroup; } }
        }
        public partial class VifTag : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static VifTag FromFile(string fileName)
            {
                return new VifTag(new KaitaiStream(fileName));
            }

            public VifTag(KaitaiStream p__io, Kh2Model.SourceChainDmaTag p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(VifTag);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Immediate));
                _immediate = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Num));
                _num = m_io.ReadU1();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Cmd));
                _cmd = ((Kh2Model.VifCmd) m_io.ReadU1());
                M_Tracer.EndMember();
            }
            private ushort _immediate;
            private byte _num;
            private VifCmd _cmd;
            private Kh2Model m_root;
            private Kh2Model.SourceChainDmaTag m_parent;
            public ushort Immediate { get { return _immediate; } }
            public byte Num { get { return _num; } }
            public VifCmd Cmd { get { return _cmd; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.SourceChainDmaTag M_Parent { get { return m_parent; } }
        }
        public partial class DmaTagArrayObject : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static DmaTagArrayObject FromFile(string fileName)
            {
                return new DmaTagArrayObject(new KaitaiStream(fileName));
            }

            public DmaTagArrayObject(KaitaiStream p__io, Kh2Model.ModelPart p__parent = null, Kh2Model p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(DmaTagArrayObject);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                _dmaTag = new List<SourceChainDmaTag>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        M_Tracer.BeginArrayMember(nameof(DmaTag));
                        _dmaTag.Add(new SourceChainDmaTag(m_io, this, m_root, tracer: M_Tracer));
                        M_Tracer.EndArrayMember();
                        i++;
                    }
                }
            }
            private List<SourceChainDmaTag> _dmaTag;
            private Kh2Model m_root;
            private Kh2Model.ModelPart m_parent;
            public List<SourceChainDmaTag> DmaTag { get { return _dmaTag; } }
            public Kh2Model M_Root { get { return m_root; } }
            public Kh2Model.ModelPart M_Parent { get { return m_parent; } }
        }
        private bool f_modelInst;
        private Model _modelInst;
        public Model ModelInst
        {
            get
            {
                if (f_modelInst)
                    return _modelInst;
                long _pos = m_io.Pos;
                m_io.Seek(144);
                M_Tracer.Seek(144);
                __raw_modelInst = m_io.ReadBytesFull();
                var io___raw_modelInst = new KaitaiStream(__raw_modelInst);
                M_Tracer.DeclareNewIo();
                M_Tracer.BeginMember(nameof(ModelInst));
                _modelInst = new Model(io___raw_modelInst, this, m_root, tracer: M_Tracer);
                M_Tracer.EndMember();
                m_io.Seek(_pos);
                f_modelInst = true;
                return _modelInst;
            }
        }
        private byte[] _hw;
        private Kh2Model m_root;
        private KaitaiStruct m_parent;
        private byte[] __raw_modelInst;
        public byte[] Hw { get { return _hw; } }
        public Kh2Model M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
        public byte[] M_RawModelInst { get { return __raw_modelInst; } }
    }
}
