// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public partial class Kh2Motion : KaitaiStruct
    {
        public static Kh2Motion FromFile(string fileName)
        {
            return new Kh2Motion(new KaitaiStream(fileName));
        }

        public Kh2Motion(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Motion p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _empty = m_io.ReadBytes(144);
            _version = m_io.ReadU4le();
            _unk04 = m_io.ReadU4le();
            _byteCount = m_io.ReadU4le();
            _unk0c = m_io.ReadU4le();
            switch (Version) {
            case 0: {
                __raw_motion = m_io.ReadBytes((ByteCount - 16));
                var io___raw_motion = new KaitaiStream(__raw_motion);
                _motion = new Interpolated(io___raw_motion, this, m_root);
                break;
            }
            default: {
                _motion = m_io.ReadBytes((ByteCount - 16));
                break;
            }
            }
        }
        public partial class IkHelperTable : KaitaiStruct
        {
            public static IkHelperTable FromFile(string fileName)
            {
                return new IkHelperTable(new KaitaiStream(fileName));
            }

            public IkHelperTable(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _index = m_io.ReadU4le();
                _parentIndex = m_io.ReadU4le();
                _unk08 = m_io.ReadU4le();
                _unk0c = m_io.ReadU4le();
                _scaleX = m_io.ReadF4le();
                _scaleY = m_io.ReadF4le();
                _scaleZ = m_io.ReadF4le();
                _scaleW = m_io.ReadF4le();
                _rotateX = m_io.ReadF4le();
                _rotateY = m_io.ReadF4le();
                _rotateZ = m_io.ReadF4le();
                _rotateW = m_io.ReadF4le();
                _translateX = m_io.ReadF4le();
                _translateY = m_io.ReadF4le();
                _translateZ = m_io.ReadF4le();
                _translateW = m_io.ReadF4le();
            }
            private uint _index;
            private uint _parentIndex;
            private uint _unk08;
            private uint _unk0c;
            private float _scaleX;
            private float _scaleY;
            private float _scaleZ;
            private float _scaleW;
            private float _rotateX;
            private float _rotateY;
            private float _rotateZ;
            private float _rotateW;
            private float _translateX;
            private float _translateY;
            private float _translateZ;
            private float _translateW;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public uint Index { get { return _index; } }
            public uint ParentIndex { get { return _parentIndex; } }
            public uint Unk08 { get { return _unk08; } }
            public uint Unk0c { get { return _unk0c; } }
            public float ScaleX { get { return _scaleX; } }
            public float ScaleY { get { return _scaleY; } }
            public float ScaleZ { get { return _scaleZ; } }
            public float ScaleW { get { return _scaleW; } }
            public float RotateX { get { return _rotateX; } }
            public float RotateY { get { return _rotateY; } }
            public float RotateZ { get { return _rotateZ; } }
            public float RotateW { get { return _rotateW; } }
            public float TranslateX { get { return _translateX; } }
            public float TranslateY { get { return _translateY; } }
            public float TranslateZ { get { return _translateZ; } }
            public float TranslateW { get { return _translateW; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        public partial class UnknownTable8 : KaitaiStruct
        {
            public static UnknownTable8 FromFile(string fileName)
            {
                return new UnknownTable8(new KaitaiStream(fileName));
            }

            public UnknownTable8(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unk00 = m_io.ReadU4le();
                _unk04 = m_io.ReadU4le();
                _unk08 = m_io.ReadF4le();
                _unk0c = m_io.ReadF4le();
                _unk10 = m_io.ReadF4le();
                _unk14 = m_io.ReadF4le();
                _unk18 = m_io.ReadF4le();
                _unk1c = m_io.ReadF4le();
                _unk20 = m_io.ReadF4le();
                _unk24 = m_io.ReadF4le();
                _unk28 = m_io.ReadF4le();
                _unk2c = m_io.ReadF4le();
            }
            private uint _unk00;
            private uint _unk04;
            private float _unk08;
            private float _unk0c;
            private float _unk10;
            private float _unk14;
            private float _unk18;
            private float _unk1c;
            private float _unk20;
            private float _unk24;
            private float _unk28;
            private float _unk2c;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public uint Unk00 { get { return _unk00; } }
            public uint Unk04 { get { return _unk04; } }
            public float Unk08 { get { return _unk08; } }
            public float Unk0c { get { return _unk0c; } }
            public float Unk10 { get { return _unk10; } }
            public float Unk14 { get { return _unk14; } }
            public float Unk18 { get { return _unk18; } }
            public float Unk1c { get { return _unk1c; } }
            public float Unk20 { get { return _unk20; } }
            public float Unk24 { get { return _unk24; } }
            public float Unk28 { get { return _unk28; } }
            public float Unk2c { get { return _unk2c; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        public partial class IkChainTable : KaitaiStruct
        {
            public static IkChainTable FromFile(string fileName)
            {
                return new IkChainTable(new KaitaiStream(fileName));
            }

            public IkChainTable(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unk00 = m_io.ReadU1();
                _unk01 = m_io.ReadU1();
                _modelBoneIndex = m_io.ReadS2le();
                _ikHelperIndex = m_io.ReadS2le();
                _table8Index = m_io.ReadS2le();
                _unk08 = m_io.ReadS2le();
                _unk0a = m_io.ReadS2le();
            }
            private byte _unk00;
            private byte _unk01;
            private short _modelBoneIndex;
            private short _ikHelperIndex;
            private short _table8Index;
            private short _unk08;
            private short _unk0a;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public byte Unk00 { get { return _unk00; } }
            public byte Unk01 { get { return _unk01; } }
            public short ModelBoneIndex { get { return _modelBoneIndex; } }
            public short IkHelperIndex { get { return _ikHelperIndex; } }
            public short Table8Index { get { return _table8Index; } }
            public short Unk08 { get { return _unk08; } }
            public short Unk0a { get { return _unk0a; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        public partial class TimelineTable : KaitaiStruct
        {
            public static TimelineTable FromFile(string fileName)
            {
                return new TimelineTable(new KaitaiStream(fileName));
            }

            public TimelineTable(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _time = m_io.ReadS2le();
                _valueIndex = m_io.ReadS2le();
                _tangentIndexEaseIn = m_io.ReadS2le();
                _tangentIndexEaseOut = m_io.ReadS2le();
            }
            private short _time;
            private short _valueIndex;
            private short _tangentIndexEaseIn;
            private short _tangentIndexEaseOut;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public short Time { get { return _time; } }
            public short ValueIndex { get { return _valueIndex; } }
            public short TangentIndexEaseIn { get { return _tangentIndexEaseIn; } }
            public short TangentIndexEaseOut { get { return _tangentIndexEaseOut; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        public partial class UnknownTable6 : KaitaiStruct
        {
            public static UnknownTable6 FromFile(string fileName)
            {
                return new UnknownTable6(new KaitaiStream(fileName));
            }

            public UnknownTable6(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unk00 = m_io.ReadS2le();
                _unk02 = m_io.ReadS2le();
                _unk04 = m_io.ReadF4le();
                _unk08 = m_io.ReadS2le();
                _unk0a = m_io.ReadS2le();
            }
            private short _unk00;
            private short _unk02;
            private float _unk04;
            private short _unk08;
            private short _unk0a;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public short Unk00 { get { return _unk00; } }
            public short Unk02 { get { return _unk02; } }
            public float Unk04 { get { return _unk04; } }
            public short Unk08 { get { return _unk08; } }
            public short Unk0a { get { return _unk0a; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        public partial class Interpolated : KaitaiStruct
        {
            public static Interpolated FromFile(string fileName)
            {
                return new Interpolated(new KaitaiStream(fileName));
            }

            public Interpolated(KaitaiStream p__io, Kh2Motion p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                f_joints = false;
                f_tangentValues = false;
                f_table8 = false;
                f_staticPose = false;
                f_ikHelperAnimation = false;
                f_ikChains = false;
                f_ikHelpers = false;
                f_rawTimeline = false;
                f_modelBoneAnimation = false;
                f_table7 = false;
                f_keyFrames = false;
                f_transformationValues = false;
                f_table6 = false;
                _read();
            }
            private void _read()
            {
                _boneCount = m_io.ReadU2le();
                _totalBoneCount = m_io.ReadU2le();
                _totalFrameCount = m_io.ReadS4le();
                _ikHelperOffset = m_io.ReadS4le();
                _jointIndexOffset = m_io.ReadS4le();
                _keyFrameCount = m_io.ReadS4le();
                _staticPoseOffset = m_io.ReadU4le();
                _staticPoseCount = m_io.ReadU4le();
                _footerOffset = m_io.ReadU4le();
                _modelBoneAnimationOffset = m_io.ReadU4le();
                _modelBoneAnimationCount = m_io.ReadU4le();
                _ikHelperAnimationOffset = m_io.ReadU4le();
                _ikHelperAnimationCount = m_io.ReadU4le();
                _timelineOffset = m_io.ReadU4le();
                _keyFrameOffset = m_io.ReadU4le();
                _transformationValueOffset = m_io.ReadU4le();
                _tangentOffset = m_io.ReadU4le();
                _ikChainOffset = m_io.ReadU4le();
                _ikChainCount = m_io.ReadU4le();
                _unk48 = m_io.ReadU4le();
                _table8Offset = m_io.ReadU4le();
                _table7Offset = m_io.ReadU4le();
                _table7Count = m_io.ReadU4le();
                _table6Offset = m_io.ReadU4le();
                _table6Count = m_io.ReadU4le();
                _boundingBoxMinX = m_io.ReadF4le();
                _boundingBoxMinY = m_io.ReadF4le();
                _boundingBoxMinZ = m_io.ReadF4le();
                _boundingBoxMinW = m_io.ReadF4le();
                _boundingBoxMaxX = m_io.ReadF4le();
                _boundingBoxMaxY = m_io.ReadF4le();
                _boundingBoxMaxZ = m_io.ReadF4le();
                _boundingBoxMaxW = m_io.ReadF4le();
                _frameLoop = m_io.ReadF4le();
                _frameEnd = m_io.ReadF4le();
                _framePerSecond = m_io.ReadF4le();
                _frameCount = m_io.ReadF4le();
                _unknownTable1Offset = m_io.ReadU4le();
                _unknownTable1Count = m_io.ReadU4le();
                _unk98 = m_io.ReadU4le();
                _unk9c = m_io.ReadU4le();
            }
            private bool f_joints;
            private List<JointTable> _joints;
            public List<JointTable> Joints
            {
                get
                {
                    if (f_joints)
                        return _joints;
                    long _pos = m_io.Pos;
                    m_io.Seek(JointIndexOffset);
                    _joints = new List<JointTable>((int) (TotalBoneCount));
                    for (var i = 0; i < TotalBoneCount; i++)
                    {
                        _joints.Add(new JointTable(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_joints = true;
                    return _joints;
                }
            }
            private bool f_tangentValues;
            private List<float> _tangentValues;
            public List<float> TangentValues
            {
                get
                {
                    if (f_tangentValues)
                        return _tangentValues;
                    long _pos = m_io.Pos;
                    m_io.Seek(TangentOffset);
                    _tangentValues = new List<float>((int) (((IkChainOffset - TangentOffset) / 4)));
                    for (var i = 0; i < ((IkChainOffset - TangentOffset) / 4); i++)
                    {
                        _tangentValues.Add(m_io.ReadF4le());
                    }
                    m_io.Seek(_pos);
                    f_tangentValues = true;
                    return _tangentValues;
                }
            }
            private bool f_table8;
            private List<UnknownTable8> _table8;
            public List<UnknownTable8> Table8
            {
                get
                {
                    if (f_table8)
                        return _table8;
                    long _pos = m_io.Pos;
                    m_io.Seek(Table8Offset);
                    _table8 = new List<UnknownTable8>((int) (((Table7Offset - Table8Offset) / 48)));
                    for (var i = 0; i < ((Table7Offset - Table8Offset) / 48); i++)
                    {
                        _table8.Add(new UnknownTable8(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_table8 = true;
                    return _table8;
                }
            }
            private bool f_staticPose;
            private List<StaticPose> _staticPose;
            public List<StaticPose> StaticPose
            {
                get
                {
                    if (f_staticPose)
                        return _staticPose;
                    long _pos = m_io.Pos;
                    m_io.Seek(StaticPoseOffset);
                    _staticPose = new List<StaticPose>((int) (StaticPoseCount));
                    for (var i = 0; i < StaticPoseCount; i++)
                    {
                        _staticPose.Add(new StaticPose(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_staticPose = true;
                    return _staticPose;
                }
            }
            private bool f_ikHelperAnimation;
            private List<BoneAnimationTable> _ikHelperAnimation;
            public List<BoneAnimationTable> IkHelperAnimation
            {
                get
                {
                    if (f_ikHelperAnimation)
                        return _ikHelperAnimation;
                    long _pos = m_io.Pos;
                    m_io.Seek(IkHelperAnimationOffset);
                    _ikHelperAnimation = new List<BoneAnimationTable>((int) (IkHelperAnimationCount));
                    for (var i = 0; i < IkHelperAnimationCount; i++)
                    {
                        _ikHelperAnimation.Add(new BoneAnimationTable(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_ikHelperAnimation = true;
                    return _ikHelperAnimation;
                }
            }
            private bool f_ikChains;
            private List<IkChainTable> _ikChains;
            public List<IkChainTable> IkChains
            {
                get
                {
                    if (f_ikChains)
                        return _ikChains;
                    long _pos = m_io.Pos;
                    m_io.Seek(IkChainOffset);
                    _ikChains = new List<IkChainTable>((int) (IkChainCount));
                    for (var i = 0; i < IkChainCount; i++)
                    {
                        _ikChains.Add(new IkChainTable(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_ikChains = true;
                    return _ikChains;
                }
            }
            private bool f_ikHelpers;
            private List<IkHelperTable> _ikHelpers;
            public List<IkHelperTable> IkHelpers
            {
                get
                {
                    if (f_ikHelpers)
                        return _ikHelpers;
                    long _pos = m_io.Pos;
                    m_io.Seek(IkHelperOffset);
                    _ikHelpers = new List<IkHelperTable>((int) ((TotalBoneCount - BoneCount)));
                    for (var i = 0; i < (TotalBoneCount - BoneCount); i++)
                    {
                        _ikHelpers.Add(new IkHelperTable(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_ikHelpers = true;
                    return _ikHelpers;
                }
            }
            private bool f_rawTimeline;
            private List<TimelineTable> _rawTimeline;
            public List<TimelineTable> RawTimeline
            {
                get
                {
                    if (f_rawTimeline)
                        return _rawTimeline;
                    long _pos = m_io.Pos;
                    m_io.Seek(TimelineOffset);
                    _rawTimeline = new List<TimelineTable>((int) (((KeyFrameOffset - TimelineOffset) / 8)));
                    for (var i = 0; i < ((KeyFrameOffset - TimelineOffset) / 8); i++)
                    {
                        _rawTimeline.Add(new TimelineTable(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_rawTimeline = true;
                    return _rawTimeline;
                }
            }
            private bool f_modelBoneAnimation;
            private List<BoneAnimationTable> _modelBoneAnimation;
            public List<BoneAnimationTable> ModelBoneAnimation
            {
                get
                {
                    if (f_modelBoneAnimation)
                        return _modelBoneAnimation;
                    long _pos = m_io.Pos;
                    m_io.Seek(ModelBoneAnimationOffset);
                    _modelBoneAnimation = new List<BoneAnimationTable>((int) (ModelBoneAnimationCount));
                    for (var i = 0; i < ModelBoneAnimationCount; i++)
                    {
                        _modelBoneAnimation.Add(new BoneAnimationTable(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_modelBoneAnimation = true;
                    return _modelBoneAnimation;
                }
            }
            private bool f_table7;
            private List<UnknownTable7> _table7;
            public List<UnknownTable7> Table7
            {
                get
                {
                    if (f_table7)
                        return _table7;
                    long _pos = m_io.Pos;
                    m_io.Seek(Table7Offset);
                    _table7 = new List<UnknownTable7>((int) (Table7Count));
                    for (var i = 0; i < Table7Count; i++)
                    {
                        _table7.Add(new UnknownTable7(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_table7 = true;
                    return _table7;
                }
            }
            private bool f_keyFrames;
            private List<float> _keyFrames;
            public List<float> KeyFrames
            {
                get
                {
                    if (f_keyFrames)
                        return _keyFrames;
                    long _pos = m_io.Pos;
                    m_io.Seek(KeyFrameOffset);
                    _keyFrames = new List<float>((int) (KeyFrameCount));
                    for (var i = 0; i < KeyFrameCount; i++)
                    {
                        _keyFrames.Add(m_io.ReadF4le());
                    }
                    m_io.Seek(_pos);
                    f_keyFrames = true;
                    return _keyFrames;
                }
            }
            private bool f_transformationValues;
            private List<float> _transformationValues;
            public List<float> TransformationValues
            {
                get
                {
                    if (f_transformationValues)
                        return _transformationValues;
                    long _pos = m_io.Pos;
                    m_io.Seek(TransformationValueOffset);
                    _transformationValues = new List<float>((int) (((TangentOffset - TransformationValueOffset) / 4)));
                    for (var i = 0; i < ((TangentOffset - TransformationValueOffset) / 4); i++)
                    {
                        _transformationValues.Add(m_io.ReadF4le());
                    }
                    m_io.Seek(_pos);
                    f_transformationValues = true;
                    return _transformationValues;
                }
            }
            private bool f_table6;
            private List<UnknownTable6> _table6;
            public List<UnknownTable6> Table6
            {
                get
                {
                    if (f_table6)
                        return _table6;
                    long _pos = m_io.Pos;
                    m_io.Seek(Table6Count);
                    _table6 = new List<UnknownTable6>((int) (Table6Count));
                    for (var i = 0; i < Table6Count; i++)
                    {
                        _table6.Add(new UnknownTable6(m_io, this, m_root));
                    }
                    m_io.Seek(_pos);
                    f_table6 = true;
                    return _table6;
                }
            }
            private ushort _boneCount;
            private ushort _totalBoneCount;
            private int _totalFrameCount;
            private int _ikHelperOffset;
            private int _jointIndexOffset;
            private int _keyFrameCount;
            private uint _staticPoseOffset;
            private uint _staticPoseCount;
            private uint _footerOffset;
            private uint _modelBoneAnimationOffset;
            private uint _modelBoneAnimationCount;
            private uint _ikHelperAnimationOffset;
            private uint _ikHelperAnimationCount;
            private uint _timelineOffset;
            private uint _keyFrameOffset;
            private uint _transformationValueOffset;
            private uint _tangentOffset;
            private uint _ikChainOffset;
            private uint _ikChainCount;
            private uint _unk48;
            private uint _table8Offset;
            private uint _table7Offset;
            private uint _table7Count;
            private uint _table6Offset;
            private uint _table6Count;
            private float _boundingBoxMinX;
            private float _boundingBoxMinY;
            private float _boundingBoxMinZ;
            private float _boundingBoxMinW;
            private float _boundingBoxMaxX;
            private float _boundingBoxMaxY;
            private float _boundingBoxMaxZ;
            private float _boundingBoxMaxW;
            private float _frameLoop;
            private float _frameEnd;
            private float _framePerSecond;
            private float _frameCount;
            private uint _unknownTable1Offset;
            private uint _unknownTable1Count;
            private uint _unk98;
            private uint _unk9c;
            private Kh2Motion m_root;
            private Kh2Motion m_parent;
            public ushort BoneCount { get { return _boneCount; } }
            public ushort TotalBoneCount { get { return _totalBoneCount; } }
            public int TotalFrameCount { get { return _totalFrameCount; } }
            public int IkHelperOffset { get { return _ikHelperOffset; } }
            public int JointIndexOffset { get { return _jointIndexOffset; } }
            public int KeyFrameCount { get { return _keyFrameCount; } }
            public uint StaticPoseOffset { get { return _staticPoseOffset; } }
            public uint StaticPoseCount { get { return _staticPoseCount; } }
            public uint FooterOffset { get { return _footerOffset; } }
            public uint ModelBoneAnimationOffset { get { return _modelBoneAnimationOffset; } }
            public uint ModelBoneAnimationCount { get { return _modelBoneAnimationCount; } }
            public uint IkHelperAnimationOffset { get { return _ikHelperAnimationOffset; } }
            public uint IkHelperAnimationCount { get { return _ikHelperAnimationCount; } }
            public uint TimelineOffset { get { return _timelineOffset; } }
            public uint KeyFrameOffset { get { return _keyFrameOffset; } }
            public uint TransformationValueOffset { get { return _transformationValueOffset; } }
            public uint TangentOffset { get { return _tangentOffset; } }
            public uint IkChainOffset { get { return _ikChainOffset; } }
            public uint IkChainCount { get { return _ikChainCount; } }
            public uint Unk48 { get { return _unk48; } }
            public uint Table8Offset { get { return _table8Offset; } }
            public uint Table7Offset { get { return _table7Offset; } }
            public uint Table7Count { get { return _table7Count; } }
            public uint Table6Offset { get { return _table6Offset; } }
            public uint Table6Count { get { return _table6Count; } }
            public float BoundingBoxMinX { get { return _boundingBoxMinX; } }
            public float BoundingBoxMinY { get { return _boundingBoxMinY; } }
            public float BoundingBoxMinZ { get { return _boundingBoxMinZ; } }
            public float BoundingBoxMinW { get { return _boundingBoxMinW; } }
            public float BoundingBoxMaxX { get { return _boundingBoxMaxX; } }
            public float BoundingBoxMaxY { get { return _boundingBoxMaxY; } }
            public float BoundingBoxMaxZ { get { return _boundingBoxMaxZ; } }
            public float BoundingBoxMaxW { get { return _boundingBoxMaxW; } }
            public float FrameLoop { get { return _frameLoop; } }
            public float FrameEnd { get { return _frameEnd; } }
            public float FramePerSecond { get { return _framePerSecond; } }
            public float FrameCount { get { return _frameCount; } }
            public uint UnknownTable1Offset { get { return _unknownTable1Offset; } }
            public uint UnknownTable1Count { get { return _unknownTable1Count; } }
            public uint Unk98 { get { return _unk98; } }
            public uint Unk9c { get { return _unk9c; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion M_Parent { get { return m_parent; } }
        }
        public partial class UnknownTable7 : KaitaiStruct
        {
            public static UnknownTable7 FromFile(string fileName)
            {
                return new UnknownTable7(new KaitaiStream(fileName));
            }

            public UnknownTable7(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _unk00 = m_io.ReadS2le();
                _unk02 = m_io.ReadS2le();
                _unk04 = m_io.ReadS2le();
                _unk06 = m_io.ReadS2le();
            }
            private short _unk00;
            private short _unk02;
            private short _unk04;
            private short _unk06;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public short Unk00 { get { return _unk00; } }
            public short Unk02 { get { return _unk02; } }
            public short Unk04 { get { return _unk04; } }
            public short Unk06 { get { return _unk06; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        public partial class JointTable : KaitaiStruct
        {
            public static JointTable FromFile(string fileName)
            {
                return new JointTable(new KaitaiStream(fileName));
            }

            public JointTable(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _jointIndex = m_io.ReadS2le();
                _flag = m_io.ReadS2le();
            }
            private short _jointIndex;
            private short _flag;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public short JointIndex { get { return _jointIndex; } }
            public short Flag { get { return _flag; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        public partial class BoneAnimationTable : KaitaiStruct
        {
            public static BoneAnimationTable FromFile(string fileName)
            {
                return new BoneAnimationTable(new KaitaiStream(fileName));
            }

            public BoneAnimationTable(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _jointIndex = m_io.ReadS2le();
                _channel = m_io.ReadU1();
                _timelineCount = m_io.ReadU1();
                _timelineStartIndex = m_io.ReadS2le();
            }
            private short _jointIndex;
            private byte _channel;
            private byte _timelineCount;
            private short _timelineStartIndex;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public short JointIndex { get { return _jointIndex; } }
            public byte Channel { get { return _channel; } }
            public byte TimelineCount { get { return _timelineCount; } }
            public short TimelineStartIndex { get { return _timelineStartIndex; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        public partial class StaticPose : KaitaiStruct
        {
            public static StaticPose FromFile(string fileName)
            {
                return new StaticPose(new KaitaiStream(fileName));
            }

            public StaticPose(KaitaiStream p__io, Kh2Motion.Interpolated p__parent = null, Kh2Motion p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _boneIndex = m_io.ReadS2le();
                _channel = m_io.ReadS2le();
                _value = m_io.ReadF4le();
            }
            private short _boneIndex;
            private short _channel;
            private float _value;
            private Kh2Motion m_root;
            private Kh2Motion.Interpolated m_parent;
            public short BoneIndex { get { return _boneIndex; } }
            public short Channel { get { return _channel; } }
            public float Value { get { return _value; } }
            public Kh2Motion M_Root { get { return m_root; } }
            public Kh2Motion.Interpolated M_Parent { get { return m_parent; } }
        }
        private byte[] _empty;
        private uint _version;
        private uint _unk04;
        private uint _byteCount;
        private uint _unk0c;
        private object _motion;
        private Kh2Motion m_root;
        private KaitaiStruct m_parent;
        private byte[] __raw_motion;
        public byte[] Empty { get { return _empty; } }
        public uint Version { get { return _version; } }
        public uint Unk04 { get { return _unk04; } }
        public uint ByteCount { get { return _byteCount; } }
        public uint Unk0c { get { return _unk0c; } }
        public object Motion { get { return _motion; } }
        public Kh2Motion M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
        public byte[] M_RawMotion { get { return __raw_motion; } }
    }
}
