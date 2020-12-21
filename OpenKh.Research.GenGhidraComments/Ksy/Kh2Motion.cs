// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;

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
            private byte[] _joints;
            public byte[] Joints
            {
                get
                {
                    if (f_joints)
                        return _joints;
                    long _pos = m_io.Pos;
                    m_io.Seek(JointIndexOffset);
                    _joints = m_io.ReadBytes((4 * TotalBoneCount));
                    m_io.Seek(_pos);
                    f_joints = true;
                    return _joints;
                }
            }
            private bool f_tangentValues;
            private byte[] _tangentValues;
            public byte[] TangentValues
            {
                get
                {
                    if (f_tangentValues)
                        return _tangentValues;
                    long _pos = m_io.Pos;
                    m_io.Seek(TangentOffset);
                    _tangentValues = m_io.ReadBytes((IkChainOffset - TangentOffset));
                    m_io.Seek(_pos);
                    f_tangentValues = true;
                    return _tangentValues;
                }
            }
            private bool f_table8;
            private byte[] _table8;
            public byte[] Table8
            {
                get
                {
                    if (f_table8)
                        return _table8;
                    long _pos = m_io.Pos;
                    m_io.Seek(Table8Offset);
                    _table8 = m_io.ReadBytes((Table7Offset - Table8Offset));
                    m_io.Seek(_pos);
                    f_table8 = true;
                    return _table8;
                }
            }
            private bool f_staticPose;
            private byte[] _staticPose;
            public byte[] StaticPose
            {
                get
                {
                    if (f_staticPose)
                        return _staticPose;
                    long _pos = m_io.Pos;
                    m_io.Seek(StaticPoseOffset);
                    _staticPose = m_io.ReadBytes((8 * StaticPoseCount));
                    m_io.Seek(_pos);
                    f_staticPose = true;
                    return _staticPose;
                }
            }
            private bool f_ikHelperAnimation;
            private byte[] _ikHelperAnimation;
            public byte[] IkHelperAnimation
            {
                get
                {
                    if (f_ikHelperAnimation)
                        return _ikHelperAnimation;
                    long _pos = m_io.Pos;
                    m_io.Seek(IkHelperAnimationOffset);
                    _ikHelperAnimation = m_io.ReadBytes((4 * IkHelperAnimationCount));
                    m_io.Seek(_pos);
                    f_ikHelperAnimation = true;
                    return _ikHelperAnimation;
                }
            }
            private bool f_ikChains;
            private byte[] _ikChains;
            public byte[] IkChains
            {
                get
                {
                    if (f_ikChains)
                        return _ikChains;
                    long _pos = m_io.Pos;
                    m_io.Seek(IkChainOffset);
                    _ikChains = m_io.ReadBytes((12 * IkChainCount));
                    m_io.Seek(_pos);
                    f_ikChains = true;
                    return _ikChains;
                }
            }
            private bool f_ikHelpers;
            private byte[] _ikHelpers;
            public byte[] IkHelpers
            {
                get
                {
                    if (f_ikHelpers)
                        return _ikHelpers;
                    long _pos = m_io.Pos;
                    m_io.Seek(IkHelperOffset);
                    _ikHelpers = m_io.ReadBytes((64 * (TotalBoneCount - BoneCount)));
                    m_io.Seek(_pos);
                    f_ikHelpers = true;
                    return _ikHelpers;
                }
            }
            private bool f_rawTimeline;
            private byte[] _rawTimeline;
            public byte[] RawTimeline
            {
                get
                {
                    if (f_rawTimeline)
                        return _rawTimeline;
                    long _pos = m_io.Pos;
                    m_io.Seek(TimelineOffset);
                    _rawTimeline = m_io.ReadBytes((KeyFrameOffset - TimelineOffset));
                    m_io.Seek(_pos);
                    f_rawTimeline = true;
                    return _rawTimeline;
                }
            }
            private bool f_modelBoneAnimation;
            private byte[] _modelBoneAnimation;
            public byte[] ModelBoneAnimation
            {
                get
                {
                    if (f_modelBoneAnimation)
                        return _modelBoneAnimation;
                    long _pos = m_io.Pos;
                    m_io.Seek(ModelBoneAnimationOffset);
                    _modelBoneAnimation = m_io.ReadBytes((6 * ModelBoneAnimationCount));
                    m_io.Seek(_pos);
                    f_modelBoneAnimation = true;
                    return _modelBoneAnimation;
                }
            }
            private bool f_table7;
            private byte[] _table7;
            public byte[] Table7
            {
                get
                {
                    if (f_table7)
                        return _table7;
                    long _pos = m_io.Pos;
                    m_io.Seek(Table7Offset);
                    _table7 = m_io.ReadBytes((8 * Table7Count));
                    m_io.Seek(_pos);
                    f_table7 = true;
                    return _table7;
                }
            }
            private bool f_keyFrames;
            private byte[] _keyFrames;
            public byte[] KeyFrames
            {
                get
                {
                    if (f_keyFrames)
                        return _keyFrames;
                    long _pos = m_io.Pos;
                    m_io.Seek(KeyFrameOffset);
                    _keyFrames = m_io.ReadBytes((4 * KeyFrameCount));
                    m_io.Seek(_pos);
                    f_keyFrames = true;
                    return _keyFrames;
                }
            }
            private bool f_transformationValues;
            private byte[] _transformationValues;
            public byte[] TransformationValues
            {
                get
                {
                    if (f_transformationValues)
                        return _transformationValues;
                    long _pos = m_io.Pos;
                    m_io.Seek(TransformationValueOffset);
                    _transformationValues = m_io.ReadBytes((TangentOffset - TransformationValueOffset));
                    m_io.Seek(_pos);
                    f_transformationValues = true;
                    return _transformationValues;
                }
            }
            private bool f_table6;
            private byte[] _table6;
            public byte[] Table6
            {
                get
                {
                    if (f_table6)
                        return _table6;
                    long _pos = m_io.Pos;
                    m_io.Seek(Table6Count);
                    _table6 = m_io.ReadBytes((12 * Table6Count));
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
