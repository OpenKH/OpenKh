using OpenKh.Common;
using OpenKh.Common.Utils;
using OpenKh.Kh2.SystemData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public partial class Motion
    {
        /*
         * Motions animate character models. They can be either Raw or Interpolated. They always start with 0x90 reserved bytes and the Motion Header, which defines the type of the motion.
         * Animations use Straight Ahead animation.
         * Raw motions start from the base pose and contain a matrix of bone animations.
         * Interpolated start from a defined initial pose and count with Inverse Kinematic tech. To help with IK, it creates a series of extra "bones".
         * Bones refers to the original skeleton bones.
         * IK Helpers refers to the extra "bones".
         * Joints refers to the list of bones and IK Helpers.
         */

        public static int baseOffset = 0x90;

        /**************************************
         * STRUCTURES
         **************************************/

        /*
         * COMMON
         */

        public class Header
        {
            [Data] public int Type { get; set; } // ENUM
            [Data] public int SubType { get; set; } // ENUM
            [Data] public int ExtraOffset { get; set; }
            [Data] public int ExtraSize { get; set; }
        }

        // Unknown
        public class Extra
        {
            [Data] public int Type { get; set; } // ENUM
            [Data] public float QWC { get; set; }
            [Data(Count = 2)] public int Param { get; set; }
        }

        // Defines the limits of the animation
        public class BoundingBox
        {
            [Data] public float BoundingBoxMinX { get; set; }
            [Data] public float BoundingBoxMinY { get; set; }
            [Data] public float BoundingBoxMinZ { get; set; }
            [Data] public float BoundingBoxMinW { get; set; }
            [Data] public float BoundingBoxMaxX { get; set; }
            [Data] public float BoundingBoxMaxY { get; set; }
            [Data] public float BoundingBoxMaxZ { get; set; }
            [Data] public float BoundingBoxMaxW { get; set; }
        }
        // Defines when the animation starts, ends and loops
        public class FrameData
        {
            [Data] public float FrameStart { get; set; }
            [Data] public float FrameEnd { get; set; }
            [Data] public float FramesPerSecond { get; set; }
            [Data] public float FrameReturn { get; set; }
        }

        /*
         * RAW
         */

        public class RawMotionHeader
        {
            [Data] public int BoneCount { get; set; }
            [Data(Count = 3)] public uint[] Reserved { get; set; }
            [Data] public int FrameCount { get; set; }
            [Data] public int TotalFrameCount { get; set; }
            [Data] public uint AnimationMatrixOffset { get; set; }
            [Data] public uint PositionMatrixOffset { get; set; }
            [Data] public BoundingBox BoundingBox { get; set; }
            [Data] public FrameData FrameData { get; set; }
        }

        public class RawMotion
        {
            [Data] public Header MotionHeader { get; set; }
            [Data] public RawMotionHeader RawMotionHeader { get; set; }
            public List<Matrix4x4> AnimationMatrices { get; set; }
            public List<Matrix4x4> PositionMatrices { get; set; }

            public RawMotion(Stream stream)
            {
                stream.Position = baseOffset;

                MotionHeader = BinaryMapping.ReadObject<Header>(stream);
                RawMotionHeader = BinaryMapping.ReadObject<RawMotionHeader>(stream);

                stream.Position = baseOffset + RawMotionHeader.AnimationMatrixOffset;
                AnimationMatrices = new List<Matrix4x4>();
                foreach (var _ in Enumerable.Repeat(0, RawMotionHeader.BoneCount * RawMotionHeader.TotalFrameCount))
                {
                    AnimationMatrices.Add(stream.ReadMatrix4x4());
                }

                stream.Position = baseOffset + RawMotionHeader.PositionMatrixOffset;
                PositionMatrices = new List<Matrix4x4>();
                while (stream.Position < MotionHeader.ExtraOffset)
                {
                    PositionMatrices.Add(stream.ReadMatrix4x4());
                }
            }

            private RawMotion()
            {

            }

            public static RawMotion CreateEmpty()
            {
                return new RawMotion
                {
                    MotionHeader = new Header
                    {
                        Type = 1, // RAW
                    },
                    RawMotionHeader = new RawMotionHeader
                    {
                        Reserved = new uint[3],
                        BoundingBox = new BoundingBox(),
                        FrameData = new FrameData(),
                    },
                    AnimationMatrices = new List<Matrix4x4>(),
                    PositionMatrices = new List<Matrix4x4>(),
                };
            }

            public static void Write(Stream stream, RawMotion motion)
            {
                stream.Write(new byte[baseOffset]);

                var topOffset = stream.Position;

                var headerPos = stream.Position;
                BinaryMapping.WriteObject(stream, motion.MotionHeader);
                BinaryMapping.WriteObject(stream, motion.RawMotionHeader);

                motion.RawMotionHeader.AnimationMatrixOffset = Convert.ToUInt32(stream.Position - topOffset);

                foreach (var it in motion.AnimationMatrices)
                {
                    stream.Write(it);
                }

                if (motion.PositionMatrices.Any())
                {
                    motion.RawMotionHeader.PositionMatrixOffset = Convert.ToUInt32(stream.Position - topOffset);

                    foreach (var it in motion.PositionMatrices)
                    {
                        stream.Write(it);
                    }
                }

                motion.MotionHeader.ExtraOffset = Convert.ToInt32(stream.Position - topOffset);

                var endPos = stream.Position;

                stream.Position = headerPos;
                BinaryMapping.WriteObject(stream, motion.MotionHeader);
                BinaryMapping.WriteObject(stream, motion.RawMotionHeader);

                stream.Position = endPos;
            }
        }

        /*
         * INTERPOLATED
         */

        public class InterpolatedMotionHeader
        {
            [Data] public short BoneCount { get; set; }
            [Data] public short TotalBoneCount { get; set; } // Bones + IK Helper bones
            [Data] public int FrameCount { get; set; }
            [Data] public int IKHelperOffset { get; set; }
            [Data] public int JointOffset { get; set; }
            [Data] public int KeyTimeCount { get; set; }
            [Data] public int InitialPoseOffset { get; set; }
            [Data] public int InitialPoseCount { get; set; }
            [Data] public int RootPositionOffset { get; set; }
            [Data] public int FCurveForwardOffset { get; set; }
            [Data] public int FCurveForwardCount { get; set; }
            [Data] public int FCurveInverseOffset { get; set; }
            [Data] public int FCurveInverseCount { get; set; }
            [Data] public int FCurveKeyOffset { get; set; }
            [Data] public int KeyTimeOffset { get; set; }
            [Data] public int KeyValueOffset { get; set; }
            [Data] public int KeyTangentOffset { get; set; }
            [Data] public int ConstraintOffset { get; set; }
            [Data] public int ConstraintCount { get; set; }
            [Data] public int ConstraintActivationOffset { get; set; }
            [Data] public int LimiterOffset { get; set; }
            [Data] public int ExpressionOffset { get; set; }
            [Data] public int ExpressionCount { get; set; }
            [Data] public int ExpressionNodeOffset { get; set; }
            [Data] public int ExpressionNodeCount { get; set; }
            [Data] public BoundingBox BoundingBox { get; set; }
            [Data] public FrameData FrameData { get; set; }
            [Data] public int ExternalEffectorOffset { get; set; }
            [Data(Count = 3)] public int[] Reserved { get; set; }
        }

        public class InterpolatedMotion
        {
            [Data] public Header MotionHeader { get; set; }
            [Data] public InterpolatedMotionHeader InterpolatedMotionHeader { get; set; }
            public List<InitialPose> InitialPoses { get; set; }
            public List<FCurve> FCurvesForward { get; set; }
            public List<FCurve> FCurvesInverse { get; set; }
            public List<Key> FCurveKeys { get; set; }
            public List<float> KeyTimes { get; set; }
            public List<float> KeyValues { get; set; }
            public List<float> KeyTangents { get; set; }
            public List<Constraint> Constraints { get; set; }
            public List<ConstraintActivation> ConstraintActivations { get; set; }
            public List<Limiter> Limiters { get; set; }
            public List<Expression> Expressions { get; set; }
            public List<ExpressionNode> ExpressionNodes { get; set; }
            public List<IKHelper> IKHelpers { get; set; }
            public List<Joint> Joints { get; set; }
            public RootPosition RootPosition { get; set; }
            public List<ExternalEffector> ExternalEffectors { get; set; }

            /**************************************
             * CONSTRUCTORS
             **************************************/

            public InterpolatedMotion(Stream stream)
            {
                stream.Position = baseOffset;

                MotionHeader = BinaryMapping.ReadObject<Header>(stream);
                InterpolatedMotionHeader = BinaryMapping.ReadObject<InterpolatedMotionHeader>(stream);

                stream.Position = baseOffset + InterpolatedMotionHeader.InitialPoseOffset;
                InitialPoses = new List<InitialPose>();
                foreach (int i in Enumerable.Range(0, InterpolatedMotionHeader.InitialPoseCount))
                {
                    InitialPoses.Add(BinaryMapping.ReadObject<InitialPose>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.FCurveForwardOffset;
                FCurvesForward = new List<FCurve>();
                foreach (int i in Enumerable.Range(0, InterpolatedMotionHeader.FCurveForwardCount))
                {
                    FCurvesForward.Add(BinaryMapping.ReadObject<FCurve>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.FCurveInverseOffset;
                FCurvesInverse = new List<FCurve>();
                foreach (int i in Enumerable.Range(0, InterpolatedMotionHeader.FCurveInverseCount))
                {
                    FCurvesInverse.Add(BinaryMapping.ReadObject<FCurve>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.FCurveKeyOffset;
                FCurveKeys = new List<Key>();
                int calcKeyCount = (InterpolatedMotionHeader.KeyTimeOffset - InterpolatedMotionHeader.FCurveKeyOffset) / 8;
                foreach (int i in Enumerable.Range(0, calcKeyCount))
                {
                    FCurveKeys.Add(BinaryMapping.ReadObject<Key>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.KeyTimeOffset;
                KeyTimes = new List<float>();
                foreach (int i in Enumerable.Range(0, InterpolatedMotionHeader.KeyTimeCount))
                {
                    KeyTimes.Add(stream.ReadSingle());
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.KeyValueOffset;
                KeyValues = new List<float>();
                while (stream.Position < baseOffset + InterpolatedMotionHeader.KeyTangentOffset)
                {
                    KeyValues.Add(stream.ReadSingle());
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.KeyTangentOffset;
                KeyTangents = new List<float>();
                while (stream.Position < baseOffset + InterpolatedMotionHeader.ConstraintOffset)
                {
                    KeyTangents.Add(stream.ReadSingle());
                }

                int activationCount = 0;
                short limiterCount = -1;
                stream.Position = baseOffset + InterpolatedMotionHeader.ConstraintOffset;
                Constraints = new List<Constraint>();
                foreach (int i in Enumerable.Range(0, InterpolatedMotionHeader.ConstraintCount))
                {
                    Constraints.Add(BinaryMapping.ReadObject<Constraint>(stream));
                    if (Constraints[i].ActivationCount != 0)
                    {
                        activationCount += Constraints[i].ActivationCount;
                    }
                    if (Constraints[i].LimiterId > limiterCount)
                    {
                        limiterCount = Constraints[i].LimiterId;
                    }
                }
                limiterCount++;

                stream.Position = baseOffset + InterpolatedMotionHeader.ConstraintActivationOffset;
                ConstraintActivations = new List<ConstraintActivation>();
                foreach (int i in Enumerable.Range(0, activationCount))
                {
                    ConstraintActivations.Add(BinaryMapping.ReadObject<ConstraintActivation>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.LimiterOffset;
                Limiters = new List<Limiter>();
                foreach (int i in Enumerable.Range(0, limiterCount))
                {
                    Limiters.Add(BinaryMapping.ReadObject<Limiter>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.ExpressionOffset;
                Expressions = new List<Expression>();
                foreach (int i in Enumerable.Range(0, InterpolatedMotionHeader.ExpressionCount))
                {
                    Expressions.Add(BinaryMapping.ReadObject<Expression>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.ExpressionNodeOffset;
                ExpressionNodes = new List<ExpressionNode>();
                foreach (int i in Enumerable.Range(0, InterpolatedMotionHeader.ExpressionNodeCount))
                {
                    ExpressionNodes.Add(BinaryMapping.ReadObject<ExpressionNode>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.IKHelperOffset;
                IKHelpers = new List<IKHelper>();
                foreach (int i in Enumerable.Range(0, InterpolatedMotionHeader.TotalBoneCount - InterpolatedMotionHeader.BoneCount))
                {
                    IKHelpers.Add(BinaryMapping.ReadObject<IKHelper>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.JointOffset;
                Joints = new List<Joint>();
                for (int i = 0; i < InterpolatedMotionHeader.TotalBoneCount; i++)
                {
                    Joints.Add(BinaryMapping.ReadObject<Joint>(stream));
                }

                stream.Position = baseOffset + InterpolatedMotionHeader.RootPositionOffset;
                RootPosition = BinaryMapping.ReadObject<RootPosition>(stream);

                stream.Position = baseOffset + InterpolatedMotionHeader.ExternalEffectorOffset;
                ExternalEffectors = new List<ExternalEffector>();
                while (stream.Position < baseOffset + MotionHeader.ExtraOffset && stream.PeekInt16() != 0)
                {
                    ExternalEffectors.Add(BinaryMapping.ReadObject<ExternalEffector>(stream));
                }
                // Padding to 16 byte align
            }

            public Stream toStream()
            {
                Stream stream = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(stream);

                stream.Write(new byte[baseOffset]);

                var headerPos = stream.Position;
                BinaryMapping.WriteObject(stream, MotionHeader);
                BinaryMapping.WriteObject(stream, InterpolatedMotionHeader);

                int EnsureOffset() => Convert.ToInt32(stream.Position - baseOffset);

                InterpolatedMotionHeader.TotalBoneCount = (short)(InterpolatedMotionHeader.BoneCount + IKHelpers.Count);

                InterpolatedMotionHeader.InitialPoseOffset = EnsureOffset();
                InterpolatedMotionHeader.InitialPoseCount = InitialPoses.Count;

                foreach (InitialPose item in InitialPoses)
                    BinaryMapping.WriteObject(stream, item);

                InterpolatedMotionHeader.FCurveForwardOffset = EnsureOffset();
                InterpolatedMotionHeader.FCurveForwardCount = FCurvesForward.Count;

                foreach (FCurve item in FCurvesForward)
                    BinaryMapping.WriteObject(stream, item);

                InterpolatedMotionHeader.FCurveInverseOffset = EnsureOffset();
                InterpolatedMotionHeader.FCurveInverseCount = FCurvesInverse.Count;

                foreach (FCurve item in FCurvesInverse)
                    BinaryMapping.WriteObject(stream, item);

                InterpolatedMotionHeader.FCurveKeyOffset = EnsureOffset();

                foreach (Key item in FCurveKeys)
                    BinaryMapping.WriteObject(stream, item);

                alignStreamBytes(stream, 4);

                InterpolatedMotionHeader.KeyTimeOffset = EnsureOffset();
                InterpolatedMotionHeader.KeyTimeCount = KeyTimes.Count;

                foreach (float item in KeyTimes)
                    writer.Write(item);

                InterpolatedMotionHeader.KeyValueOffset = EnsureOffset();

                foreach (float item in KeyValues)
                    writer.Write(item);

                InterpolatedMotionHeader.KeyTangentOffset = EnsureOffset();

                foreach (float item in KeyTangents)
                    writer.Write(item);

                InterpolatedMotionHeader.ConstraintCount = Constraints.Count;
                InterpolatedMotionHeader.ConstraintOffset = EnsureOffset();

                foreach (Constraint item in Constraints)
                    BinaryMapping.WriteObject(stream, item);

                InterpolatedMotionHeader.ConstraintActivationOffset = EnsureOffset();

                foreach (ConstraintActivation item in ConstraintActivations)
                    BinaryMapping.WriteObject(stream, item);

                alignStreamBytes(stream, 16);

                InterpolatedMotionHeader.LimiterOffset = EnsureOffset();

                foreach (Limiter item in Limiters)
                    BinaryMapping.WriteObject(stream, item);

                InterpolatedMotionHeader.ExpressionOffset = EnsureOffset();
                InterpolatedMotionHeader.ExpressionCount = Expressions.Count;

                foreach (Expression item in Expressions)
                    BinaryMapping.WriteObject(stream, item);

                InterpolatedMotionHeader.ExpressionNodeOffset = EnsureOffset();
                InterpolatedMotionHeader.ExpressionNodeCount = ExpressionNodes.Count;

                foreach (ExpressionNode item in ExpressionNodes)
                    BinaryMapping.WriteObject(stream, item);

                alignStreamBytes(stream, 16);

                InterpolatedMotionHeader.IKHelperOffset = EnsureOffset();

                foreach (IKHelper item in IKHelpers)
                    BinaryMapping.WriteObject(stream, item);

                InterpolatedMotionHeader.JointOffset = EnsureOffset();

                foreach (Joint item in Joints)
                    BinaryMapping.WriteObject(stream, item);

                alignStreamBytes(stream, 16);

                InterpolatedMotionHeader.RootPositionOffset = EnsureOffset();

                BinaryMapping.WriteObject(stream, RootPosition);

                InterpolatedMotionHeader.ExternalEffectorOffset = EnsureOffset();

                foreach (ExternalEffector item in ExternalEffectors)
                    BinaryMapping.WriteObject(stream, item);
                
                alignStreamBytes(stream, 16);

                MotionHeader.ExtraOffset = EnsureOffset();

                stream.Position = headerPos;
                BinaryMapping.WriteObject(stream, MotionHeader);
                BinaryMapping.WriteObject(stream, InterpolatedMotionHeader);

                stream.Position = 0;
                return stream;
            }

            private InterpolatedMotion()
            {
            }

            public static InterpolatedMotion CreateEmpty()
            {
                return new InterpolatedMotion
                {
                    MotionHeader = new Header
                    {
                        Type = 0, // interpolated version
                        SubType = 0, // normal
                    },
                    InterpolatedMotionHeader = new InterpolatedMotionHeader
                    {
                        BoundingBox = new BoundingBox(),
                        FrameData = new FrameData
                        {

                        },
                        Reserved = new int[3],
                    },
                    InitialPoses = new List<InitialPose>(),
                    FCurvesForward = new List<FCurve>(),
                    FCurvesInverse = new List<FCurve>(),
                    FCurveKeys = new List<Key>(),
                    KeyTimes = new List<float>(),
                    KeyValues = new List<float>(),
                    KeyTangents = new List<float>(),
                    Constraints = new List<Constraint>(),
                    ConstraintActivations = new List<ConstraintActivation>(),
                    Limiters = new List<Limiter>(),
                    Expressions = new List<Expression>(),
                    ExpressionNodes = new List<ExpressionNode>(),
                    IKHelpers = new List<IKHelper>(),
                    Joints = new List<Joint>(),
                    RootPosition = new RootPosition
                    {
                        ScaleX = 1,
                        ScaleY = 1,
                        ScaleZ = 1,
                        TranslateW = 1,
                        FCurveId = Enumerable.Repeat(-1, 9).ToArray(),
                    },
                    ExternalEffectors = new List<ExternalEffector>(),
                };
            }

            public void alignStreamBytes(Stream stream, int alignToByte)
            {
                while (stream.Length % alignToByte != 0)
                {
                    stream.WriteByte(0);
                }
            }

            public int alignOffsetBytes(int offset, int alignToByte)
            {
                while (offset % alignToByte != 0)
                {
                    offset++;
                }
                return offset;
            }
        }

        public partial class InitialPose
        {
            [Data] public short BoneId { get; set; }
            [Data] public short Channel { get; set; } // ENUM
            [Data] public float Value { get; set; }

            public override string ToString() =>
                $"{BoneId} {Transforms[Channel]} {Value}";
        }
        public partial class FCurve
        {
            [Data] public short JointId { get; set; } // Either Bone ID or IK Helper ID starting from 0 for both types
            [Data] public byte Channel { get; set; } // ENUM. 4b channel + 2b pre + 2b post
            [Data] public byte KeyCount { get; set; }
            [Data] public short KeyStartId { get; set; }

            public override string ToString() =>
                $"{JointId} {Transforms[Channel]} ({KeyStartId} - {KeyStartId + KeyCount - 1})";
        }
        public partial class Key
        {
            [Data] public short Type_Time { get; set; }
            [Data] public short ValueId { get; set; }
            [Data] public short LeftTangentId { get; set; }
            [Data] public short RightTangentId { get; set; }
        }
        public class Constraint
        {
            [Data] public byte Type { get; set; } // ENUM
            [Data] public byte TemporaryActiveFlag { get; set; }
            [Data] public short ConstrainedJointId { get; set; } // Joint being constrained. Either Bone ID or IK Helper ID starting from last bone
            [Data] public short SourceJointId { get; set; } // Source joint of the constrain. Either Bone ID or IK Helper ID starting from last bone
            [Data] public short LimiterId { get; set; }
            [Data] public short ActivationCount { get; set; }
            [Data] public short ActivationStartId { get; set; }
        }

        public class ConstraintActivation
        {
            [Data] public float Time { get; set; }
            [Data] public int Active { get; set; }
        }

        public partial class Limiter
        {
            [Data] public int Flags { get; set; }
            [Data] public int Padding { get; set; }
            [Data] public float DampingWidth { get; set; }
            [Data] public float DampingStrength { get; set; }
            [Data] public float MinX { get; set; }
            [Data] public float MinY { get; set; }
            [Data] public float MinZ { get; set; }
            [Data] public float MinW { get; set; }
            [Data] public float MaxX { get; set; }
            [Data] public float MaxY { get; set; }
            [Data] public float MaxZ { get; set; }
            [Data] public float MaxW { get; set; }

            /*
             * NOT WORTH PARSING
             * FLAGS:
             * type 3b
             * global 1b
             * xmin/xmax/ymin/ymax/zmin/zmax 1b each
             * reserved 22b
             * 
             * TYPES:
             * ROT = 0, SPHERE = 1, BOX = 2
             */
        }

        public class Expression
        {
            [Data] public short TargetId { get; set; } // IK Helper ID starting from last bone
            [Data] public short TargetChannel { get; set; } // ENUM
            [Data] public short Reserved { get; set; }
            [Data] public short NodeId { get; set; }
        }

        public partial class ExpressionNode
        {
            [Data] public int Data { get; set; }
            [Data] public float Value { get; set; }
            [Data] public short CAR { get; set; }
            [Data] public short CDR { get; set; }
            public int type { get { return (int)(Data & 0xF000); } set { Data = (int)(((Data - type) + (value & 0xF000))); } }  // ENUM
            /*
             * NOT WORTH PARSING
             * DATA:
             * type 8b
             * isGlobal 1b
             * reserved 7b
             * element 16b
             */
        }

        public partial class IKHelper
        {
            [Data] public short Index { get; set; }
            [Data] public short SiblingId { get; set; }
            [Data] public short ParentId { get; set; }
            [Data] public short ChildId { get; set; }
            [Data] public int Reserved { get; set; }
            [Data] public int Flags { get; set; }
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public float ScaleW { get; set; }
            [Data] public float RotateX { get; set; }
            [Data] public float RotateY { get; set; }
            [Data] public float RotateZ { get; set; }
            [Data] public float RotateW { get; set; }
            [Data] public float TranslateX { get; set; }
            [Data] public float TranslateY { get; set; }
            [Data] public float TranslateZ { get; set; }
            [Data] public float TranslateW { get; set; }
            /*
             * NOT WORTH PARSING
             * FLAGS:
             * unknown 10b
             * reserved 19b
             * enableBias 1b
             * below 1b
             * terminate 1b
             */
        }
        public partial class Joint
        {
            [Data] public short JointId { get; set; } // Either Bone ID or IK Helper ID starting from last bone
            [Data] public byte Flags { get; set; }
            [Data] public byte Reserved { get; set; }
            /*
             * NOT WORTH PARSING
             * FLAGS:
             * ik 2b
             * trans 1b
             * rotation 1b
             * fixed 1b
             * calculated 1b
             * calcMatrix2Rot 1b
             * extEffector 1b
             */

            public Joint() { }
            public Joint(short jointId)
            {
                this.JointId = jointId;
                Flags = 0;
                Reserved = 0;
            }

            public override string ToString() =>
                $"{JointId} Flag:{Flags} IK:{IK}";
        }
        public class RootPosition
        {
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public int NotUnit { get; set; }
            [Data] public float RotateX { get; set; }
            [Data] public float RotateY { get; set; }
            [Data] public float RotateZ { get; set; }
            [Data] public float RotateW { get; set; }
            [Data] public float TranslateX { get; set; }
            [Data] public float TranslateY { get; set; }
            [Data] public float TranslateZ { get; set; }
            [Data] public float TranslateW { get; set; }
            [Data(Count = 9)] public int[] FCurveId { get; set; } // F Curve Id from both forward and inverse
        }
        public class ExternalEffector
        {
            [Data] public short JointId { get; set; }
        }

        /**************************************
         * ENUMS
         **************************************/

        public enum MotionType
        {
            INTERPOLATED = 0,
            RAW = 1,
        }
        public enum MotionSubType
        {
            NORMAL = 0,
            IGNORE_SCALE = 1,
        }
        public enum ExtraType
        {
            WEAPON_CNS = 0,
            TERMINATE = 1,
        }
        public enum KeyType
        {
            CONSTANT = 0,
            LINEAR = 1,
            HERMITE = 2,
        }
        public enum Channel
        {
            SCALE_X = 0,
            SCALE_Y = 1,
            SCALE_Z = 2,
            ROTATION_X = 3,
            ROTATION_Y = 4,
            ROTATION_Z = 5,
            TRANSLATION_X = 6,
            TRANSLATION_Y = 7,
            TRANSLATION_Z = 8,
            UNKNOWN = -1,
        }
        public enum ConstraintType
        {
            POS = 0,
            PATH = 1,
            ORI = 2,
            DIR = 3,
            UPVCT = 4,
            TWO_PNTS = 5,
            SCL = 6,
            CAM = 7,
            CAM_PATH = 8,
            INT_PATH = 9,
            INT = 10,
            CAM_UPVCT = 11,
            POS_LIM = 12,
            ROT_LIM = 13,
        }
        public enum ExpressionType
        {
            FUNC_SIN = 0,
            FUNC_COS = 1,
            FUNC_TAN = 2,
            FUNC_ASIN = 3,
            FUNC_ACOS = 4,
            FUNC_ATAN = 5,
            FUNC_LOG = 6,
            FUNC_EXP = 7,
            FUNC_ABS = 8,
            FUNC_POW = 9,
            FUNC_SQRT = 10,
            FUNC_MIN = 11,
            FUNC_MAX = 12,
            FUNC_AV = 13,
            FUNC_COND = 14,
            FUNC_AT_FRAME = 15,
            FUNC_CTR_DIST = 16,
            FUNC_FMOD = 17,
            OP_PLUS = 18,
            OP_MINUS = 19,
            OP_MUL = 20,
            OP_DIV = 21,
            OP_MOD = 22,
            OP_EQ = 23,
            OP_GT = 24,
            OP_GE = 25,
            OP_LT = 26,
            OP_LE = 27,
            OP_AND = 28,
            OP_OR = 29,
            VARIABLE_FC = 30,
            CONSTANT_NUM = 31,
            FCURVE_ETRNX = 32,
            FCURVE_ETRNY = 33,
            FCURVE_ETRNZ = 34,
            FCURVE_ROTX = 35,
            FCURVE_ROTY = 36,
            FCURVE_ROTZ = 37,
            FCURVE_SCALX = 38,
            FCURVE_SCALY = 39,
            FCURVE_SCALZ = 40,
            LIST = 41,
            ELEMENT_NAME = 42,
            FUNC_AT_FRAME_ROT = 43,
            EXPR_UNKNOWN = -1,
        }
        public enum CycleType
        {
            FirstLastKey = 0,
            SubtractiveAdditive = 1,
            Repeat = 2,
            Zero = 3,
        }
        [Flags]
        public enum LimiterType
        {
            Rot = 1,
            Sphere = 1,
            Box = 2,
        }

        public static bool isInterpolated(Stream stream)
        {
            stream.Position = baseOffset;
            int type = stream.ReadByte();
            stream.Position = 0;

            if (type == 0)
                return true;
            else
                return false;
        }

        /**************************************
         * OLD VERSION
         * Kept for multiple reasons.
         * - The new version has a complete parsing of the files, however the old version has some good ideas that ideally would be implemented in the new version to keep only 1 version.
         * - Compatibility with older tools (Engine) which parses a motion wether it is raw or interpolated. In the new version the type of motion has to be checked before parsing to one or the other.
         * - The old version stores the times/values/tangents directly in the keys, which makes it easier to work with but harder to parse, since in the original structure they are stored in their own tables.
         **************************************/

        private class PoolValues<T>
        {
            private readonly Dictionary<T, int> _valuePool = new Dictionary<T, int>();
            private readonly List<T> _valueList = new List<T>();

            public List<T> Values => _valueList;

            public PoolValues()
            {

            }

            public PoolValues(IEnumerable<T> values)
            {
                var index = 0;
                foreach (var value in values)
                {
                    _valuePool.Add(value, index++);
                    _valueList.Add(value);
                }
            }

            public int GetIndex(T value)
            {
                if (_valuePool.TryGetValue(value, out var index))
                    return index;

                index = _valueList.Count;
                _valuePool.Add(value, index);
                _valueList.Add(value);

                return index;
            }
        }

        private const int ReservedSize = 0x90;
        private const int Matrix4x4Size = 0x40;
        private static readonly string[] Transforms = new[]
        {
            "Scale.X", "Scale.Y", "Scale.Z",
            "Rotation.X", "Rotation.Y", "Rotation.Z",
            "Translation.X", "Translation.Y", "Translation.Z",
        };

        public enum Interpolation
        {
            Zero = -1, // default
            Nearest,
            Linear,
            Hermite,
            Hermite3, // unused?
            Hermite4, // unused?
        }

        private class Header_Old
        {
            [Data] public int Version { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int ByteCount { get; set; }
            [Data] public int Unk0c { get; set; }
        }

        private class RawMotionInternal
        {
            [Data] public int BoneCount { get; set; }
            [Data] public int Unk14 { get; set; }
            [Data] public int Unk18 { get; set; }
            [Data] public int Unk1c { get; set; }
            [Data] public int FrameCountPerLoop { get; set; }
            [Data] public int TotalFrameCount { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public int Unk2c { get; set; }
            [Data] public float BoundingBoxMinX { get; set; }
            [Data] public float BoundingBoxMinY { get; set; }
            [Data] public float BoundingBoxMinZ { get; set; }
            [Data] public float BoundingBoxMinW { get; set; }
            [Data] public float BoundingBoxMaxX { get; set; }
            [Data] public float BoundingBoxMaxY { get; set; }
            [Data] public float BoundingBoxMaxZ { get; set; }
            [Data] public float BoundingBoxMaxW { get; set; }
            [Data] public float FrameLoop { get; set; }
            [Data] public float FrameEnd { get; set; }
            [Data] public float FramePerSecond { get; set; }
            [Data] public float FrameCount { get; set; }

            public List<Matrix4x4[]> Matrices { get; set; }
        }

        public class RawMotion_OLD
        {
            public int BoneCount { get; set; }
            public int Unk28 { get; set; }
            public int Unk2c { get; set; }
            public float BoundingBoxMinX { get; set; }
            public float BoundingBoxMinY { get; set; }
            public float BoundingBoxMinZ { get; set; }
            public float BoundingBoxMinW { get; set; }
            public float BoundingBoxMaxX { get; set; }
            public float BoundingBoxMaxY { get; set; }
            public float BoundingBoxMaxZ { get; set; }
            public float BoundingBoxMaxW { get; set; }
            public float FrameLoop { get; set; }
            public float FrameEnd { get; set; }
            public float FramePerSecond { get; set; }
            public float FrameCount { get; set; }

            public List<Matrix4x4[]> Matrices { get; set; }
            public Matrix4x4[] Matrices2 { get; set; }
        }

        private class InterpolatedMotionInternal
        {
            [Data] public short BoneCount { get; set; }
            [Data] public short TotalBoneCount { get; set; }
            [Data] public int TotalFrameCount { get; set; }
            [Data] public int IKHelperOffset { get; set; }
            [Data] public int JointIndexOffset { get; set; }
            [Data] public int KeyFrameCount { get; set; }
            [Data] public int StaticPoseOffset { get; set; }
            [Data] public int StaticPoseCount { get; set; }
            [Data] public int FooterOffset { get; set; }
            [Data] public int ModelBoneAnimationOffset { get; set; }
            [Data] public int ModelBoneAnimationCount { get; set; }
            [Data] public int IKHelperAnimationOffset { get; set; }
            [Data] public int IKHelperAnimationCount { get; set; }
            [Data] public int TimelineOffset { get; set; }
            [Data] public int KeyFrameOffset { get; set; }
            [Data] public int TransformationValueOffset { get; set; }
            [Data] public int TangentOffset { get; set; }
            [Data] public int IKChainOffset { get; set; }
            [Data] public int IKChainCount { get; set; }
            [Data] public int Unk48 { get; set; }
            [Data] public int Table8Offset { get; set; }
            [Data] public int Table7Offset { get; set; }
            [Data] public int Table7Count { get; set; }
            [Data] public int Table6Offset { get; set; }
            [Data] public int Table6Count { get; set; }
            [Data] public float BoundingBoxMinX { get; set; }
            [Data] public float BoundingBoxMinY { get; set; }
            [Data] public float BoundingBoxMinZ { get; set; }
            [Data] public float BoundingBoxMinW { get; set; }
            [Data] public float BoundingBoxMaxX { get; set; }
            [Data] public float BoundingBoxMaxY { get; set; }
            [Data] public float BoundingBoxMaxZ { get; set; }
            [Data] public float BoundingBoxMaxW { get; set; }
            [Data] public float FrameLoop { get; set; }
            [Data] public float FrameEnd { get; set; }
            [Data] public float FramePerSecond { get; set; }
            [Data] public float FrameCount { get; set; }
            [Data] public int UnknownTable1Offset { get; set; }
            [Data] public int UnknownTable1Count { get; set; }
            [Data] public int Unk98 { get; set; }
            [Data] public int Unk9c { get; set; }
        }

        public class InterpolatedMotion_OLD
        {
            public short BoneCount { get; set; }
            public int TotalFrameCount { get; set; }
            public int Unk48 { get; set; }
            public float BoundingBoxMinX { get; set; }
            public float BoundingBoxMinY { get; set; }
            public float BoundingBoxMinZ { get; set; }
            public float BoundingBoxMinW { get; set; }
            public float BoundingBoxMaxX { get; set; }
            public float BoundingBoxMaxY { get; set; }
            public float BoundingBoxMaxZ { get; set; }
            public float BoundingBoxMaxW { get; set; }
            public float FrameLoop { get; set; }
            public float FrameEnd { get; set; }
            public float FramePerSecond { get; set; }
            public float FrameCount { get; set; }
            public int Unk98 { get; set; }
            public int Unk9c { get; set; }

            public List<InitialPose> StaticPose { get; set; }
            public List<BoneAnimationTable> ModelBoneAnimation { get; set; }
            public List<BoneAnimationTable> IKHelperAnimation { get; set; }
            public List<TimelineTable> Timeline { get; set; }
            public List<IKChainTable> IKChains { get; set; }
            public List<UnknownTable8> Table8 { get; set; }
            public List<UnknownTable7> Table7 { get; set; }
            public List<UnknownTable6> Table6 { get; set; }
            public List<IKHelperTable> IKHelpers { get; set; }
            public List<JointTable> Joints { get; set; }
            public FooterTable Footer { get; set; }
        }



        public class BoneAnimationTableInternal
        {
            [Data] public short JointIndex { get; set; }
            [Data] public byte Channel { get; set; }
            [Data] public byte TimelineCount { get; set; }
            [Data] public short TimelineStartIndex { get; set; }

            public override string ToString() =>
                $"{JointIndex} {Transforms[Channel]} ({TimelineStartIndex},{TimelineCount})";
        }

        public class BoneAnimationTable
        {
            public short JointIndex { get; set; }
            public byte Channel { get; set; }
            public byte Pre { get; set; }
            public byte Post { get; set; }
            public byte TimelineCount { get; set; }
            public short TimelineStartIndex { get; set; }

            public override string ToString() =>
                $"{JointIndex} {Transforms[Channel]} ({Pre},{Post}) ({TimelineStartIndex},{TimelineCount})";
        }

        private class TimelineTableInternal
        {
            [Data] public short Time { get; set; }
            [Data] public short ValueIndex { get; set; }
            [Data] public short TangentIndexEaseIn { get; set; }
            [Data] public short TangentIndexEaseOut { get; set; }
        }

        public class TimelineTable
        {
            public Interpolation Interpolation { get; set; }
            public float KeyFrame { get; set; }
            public float Value { get; set; }
            public float TangentEaseIn { get; set; }
            public float TangentEaseOut { get; set; }

            public override string ToString() =>
                $"{KeyFrame} {Value} {Interpolation}:({TangentEaseIn},{TangentEaseOut})";
        }

        public class IKChainTable
        {
            [Data] public byte Unk00 { get; set; }
            [Data] public byte Unk01 { get; set; }
            [Data] public short ModelBoneIndex { get; set; }
            [Data] public short IKHelperIndex { get; set; }
            [Data] public short Table8Index { get; set; }
            [Data] public int Unk08 { get; set; }
        }

        public class UnknownTable8
        {
            [Data] public int Unk00 { get; set; }
            [Data] public float Unk04 { get; set; }
            [Data] public float Unk08 { get; set; }
            [Data] public float Unk0c { get; set; }
            [Data] public float Unk10 { get; set; }
            [Data] public float Unk14 { get; set; }
            [Data] public float Unk18 { get; set; }
            [Data] public float Unk1c { get; set; }
            [Data] public float Unk20 { get; set; }
            [Data] public float Unk24 { get; set; }
            [Data] public float Unk28 { get; set; }
            [Data] public float Unk2c { get; set; }
        }

        public class UnknownTable7
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float Unk04 { get; set; }
        }

        public class UnknownTable6
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Unk02 { get; set; }
            [Data] public float Unk04 { get; set; }
            [Data] public short Unk08 { get; set; }
            [Data] public short Unk0a { get; set; }
        }

        public class JointTable
        {
            [Data] public short JointIndex { get; set; }
            [Data] public short Flag { get; set; }
        }

        public class IKHelperTable
        {
            [Data] public int Index { get; set; }
            [Data] public int ParentIndex { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unk0c { get; set; }
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public float ScaleW { get; set; }
            [Data] public float RotateX { get; set; }
            [Data] public float RotateY { get; set; }
            [Data] public float RotateZ { get; set; }
            [Data] public float RotateW { get; set; }
            [Data] public float TranslateX { get; set; }
            [Data] public float TranslateY { get; set; }
            [Data] public float TranslateZ { get; set; }
            [Data] public float TranslateW { get; set; }
        }

        public class FooterTable
        {
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public float ScaleW { get; set; }
            [Data] public float RotateX { get; set; }
            [Data] public float RotateY { get; set; }
            [Data] public float RotateZ { get; set; }
            [Data] public float RotateW { get; set; }
            [Data] public float TranslateX { get; set; }
            [Data] public float TranslateY { get; set; }
            [Data] public float TranslateZ { get; set; }
            [Data] public float TranslateW { get; set; }
            [Data(Count = 9)] public int[] Unknown { get; set; }
        }


        public bool UnkFlag { get; set; }
        public bool IsRaw { get; }

        public RawMotion_OLD Raw { get; }
        public InterpolatedMotion_OLD Interpolated { get; }

        private Motion(Stream stream)
        {
            stream.Position += ReservedSize;

            var header = BinaryMapping.ReadObject<Header_Old>(stream);
            IsRaw = header.Version == 1;
            UnkFlag = header.Unk04 != 0;

            if (IsRaw)
            {
                var raw = BinaryMapping.ReadObject<RawMotionInternal>(stream);
                Raw = new RawMotion_OLD
                {
                    BoneCount = raw.BoneCount,
                    Unk28 = raw.Unk28,
                    Unk2c = raw.Unk2c,
                    BoundingBoxMinX = raw.BoundingBoxMinX,
                    BoundingBoxMinY = raw.BoundingBoxMinY,
                    BoundingBoxMinZ = raw.BoundingBoxMinZ,
                    BoundingBoxMinW = raw.BoundingBoxMinW,
                    BoundingBoxMaxX = raw.BoundingBoxMaxX,
                    BoundingBoxMaxY = raw.BoundingBoxMaxY,
                    BoundingBoxMaxZ = raw.BoundingBoxMaxZ,
                    BoundingBoxMaxW = raw.BoundingBoxMaxW,
                    FrameLoop = raw.FrameLoop,
                    FrameEnd = raw.FrameEnd,
                    FramePerSecond = raw.FramePerSecond,
                    FrameCount = raw.FrameCount
                };

                Raw.Matrices = new List<Matrix4x4[]>(raw.TotalFrameCount);
                for (var i = 0; i < raw.TotalFrameCount; i++)
                {
                    var matrices = new Matrix4x4[Raw.BoneCount];
                    for (var j = 0; j < Raw.BoneCount; j++)
                        matrices[j] = stream.ReadMatrix4x4();

                    Raw.Matrices.Add(matrices);
                }

                if (raw.Unk2c > 0)
                {
                    stream.Position = ReservedSize + raw.Unk2c;
                    Raw.Matrices2 = new Matrix4x4[raw.TotalFrameCount];
                    for (var j = 0; j < Raw.Matrices2.Length; j++)
                        Raw.Matrices2[j] = stream.ReadMatrix4x4();
                }
                else
                    Raw.Matrices2 = new Matrix4x4[0];
            }
            else
            {
                var reader = new BinaryReader(stream);
                var motion = BinaryMapping.ReadObject<InterpolatedMotionInternal>(stream);
                Interpolated = new InterpolatedMotion_OLD
                {
                    BoneCount = motion.BoneCount,
                    TotalFrameCount = motion.TotalFrameCount,
                    Unk48 = motion.Unk48,
                    BoundingBoxMinX = motion.BoundingBoxMinX,
                    BoundingBoxMinY = motion.BoundingBoxMinY,
                    BoundingBoxMinZ = motion.BoundingBoxMinZ,
                    BoundingBoxMinW = motion.BoundingBoxMinW,
                    BoundingBoxMaxX = motion.BoundingBoxMaxX,
                    BoundingBoxMaxY = motion.BoundingBoxMaxY,
                    BoundingBoxMaxZ = motion.BoundingBoxMaxZ,
                    BoundingBoxMaxW = motion.BoundingBoxMaxW,
                    FrameLoop = motion.FrameLoop,
                    FrameEnd = motion.FrameEnd,
                    FramePerSecond = motion.FramePerSecond,
                    FrameCount = motion.FrameCount,
                    Unk98 = motion.Unk98,
                    Unk9c = motion.Unk9c,
                };

                stream.Position = ReservedSize + motion.StaticPoseOffset;
                Interpolated.StaticPose = Enumerable
                    .Range(0, motion.StaticPoseCount)
                    .Select(x => BinaryMapping.ReadObject<InitialPose>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.ModelBoneAnimationOffset;
                Interpolated.ModelBoneAnimation = Enumerable
                    .Range(0, motion.ModelBoneAnimationCount)
                    .Select(x => BinaryMapping.ReadObject<BoneAnimationTableInternal>(stream))
                    .Select(Map)
                    .ToList();

                stream.Position = ReservedSize + motion.IKHelperAnimationOffset;
                Interpolated.IKHelperAnimation = Enumerable
                    .Range(0, motion.IKHelperAnimationCount)
                    .Select(x => BinaryMapping.ReadObject<BoneAnimationTableInternal>(stream))
                    .Select(Map)
                    .ToList();

                stream.Position = ReservedSize + motion.TimelineOffset;
                var estimatedTimelineCount = (motion.KeyFrameOffset - motion.TimelineOffset) / 8;
                var rawTimeline = Enumerable
                    .Range(0, estimatedTimelineCount)
                    .Select(x => BinaryMapping.ReadObject<TimelineTableInternal>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.KeyFrameOffset;
                var keyFrames = Enumerable
                    .Range(0, motion.KeyFrameCount)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.TransformationValueOffset;
                var estimatedKeyFrameCount = (motion.TangentOffset - motion.TransformationValueOffset) / 4;
                var transformationValues = Enumerable
                    .Range(0, estimatedKeyFrameCount)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.TangentOffset;
                var estimatedTangentCount = (motion.IKChainOffset - motion.TangentOffset) / 4;
                var tangentValues = Enumerable
                    .Range(0, estimatedTangentCount)
                    .Select(x => reader.ReadSingle())
                    .ToList();

                stream.Position = ReservedSize + motion.IKChainOffset;
                Interpolated.IKChains = Enumerable
                    .Range(0, motion.IKChainCount)
                    .Select(x => BinaryMapping.ReadObject<IKChainTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.Table8Offset;
                var estimatedTable8Count = (motion.Table7Offset - motion.Table8Offset) / 0x30;
                Interpolated.Table8 = Enumerable
                    .Range(0, estimatedTable8Count)
                    .Select(x => BinaryMapping.ReadObject<UnknownTable8>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.Table7Offset;
                Interpolated.Table7 = Enumerable
                    .Range(0, motion.Table7Count)
                    .Select(x => BinaryMapping.ReadObject<UnknownTable7>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.Table6Offset;
                Interpolated.Table6 = Enumerable
                    .Range(0, motion.Table6Count)
                    .Select(x => BinaryMapping.ReadObject<UnknownTable6>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.IKHelperOffset;
                Interpolated.IKHelpers = Enumerable
                    .Range(0, motion.TotalBoneCount - motion.BoneCount)
                    .Select(x => BinaryMapping.ReadObject<IKHelperTable>(stream))
                    .ToList();

                stream.Position = ReservedSize + motion.JointIndexOffset;
                Interpolated.Joints = Enumerable
                    .Range(0, motion.TotalBoneCount + 1)
                    .Select(x => BinaryMapping.ReadObject<JointTable>(stream))
                    .ToList();

                Interpolated.Timeline = rawTimeline
                    .Select(x => new TimelineTable
                    {
                        Interpolation = (Interpolation)(x.Time & 3),
                        KeyFrame = keyFrames[Math.Min(keyFrames.Count - 1, x.Time >> 2)],
                        Value = transformationValues[Math.Min(transformationValues.Count - 1, x.ValueIndex)],
                        TangentEaseIn = tangentValues[x.TangentIndexEaseIn],
                        TangentEaseOut = tangentValues[x.TangentIndexEaseOut],
                    })
                    .ToList();

                stream.Position = ReservedSize + motion.FooterOffset;
                Interpolated.Footer = BinaryMapping.ReadObject<FooterTable>(stream);

                stream.Position = ReservedSize + motion.UnknownTable1Offset;
            }
        }

        public static Motion Read(Stream stream) =>
            new Motion(stream);

        public static void Write(Stream stream, Motion motion)
        {
            if (motion.IsRaw)
                Write(stream, motion.Raw, motion.UnkFlag);
            else
                Write(stream, motion.Interpolated, motion.UnkFlag);
        }

        private static void Write(Stream stream, RawMotion_OLD rawMotion, bool unkFlag)
        {
            const int HeaderSize = 0x60;

            stream.Write(new byte[ReservedSize], 0, ReservedSize);
            BinaryMapping.WriteObject(stream, new Header_Old
            {
                Version = 1,
                Unk04 = unkFlag ? 1 : 0,
                ByteCount = HeaderSize +
                    rawMotion.BoneCount * rawMotion.Matrices.Count * Matrix4x4Size +
                    rawMotion.Matrices2.Length * Matrix4x4Size,
                Unk0c = 0,
            });

            BinaryMapping.WriteObject(stream, new RawMotionInternal
            {
                BoneCount = rawMotion.BoneCount,
                Unk14 = 0,
                Unk18 = 0,
                Unk1c = 0,
                FrameCountPerLoop = (int)(rawMotion.FrameEnd - rawMotion.FrameLoop) * 2,
                TotalFrameCount = rawMotion.Matrices.Count,
                Unk28 = rawMotion.Unk28,
                Unk2c = rawMotion.Matrices2.Length > 0 ? HeaderSize +
                    rawMotion.BoneCount * rawMotion.Matrices.Count * Matrix4x4Size : 0,
                BoundingBoxMinX = rawMotion.BoundingBoxMinX,
                BoundingBoxMinY = rawMotion.BoundingBoxMinY,
                BoundingBoxMinZ = rawMotion.BoundingBoxMinZ,
                BoundingBoxMinW = rawMotion.BoundingBoxMinW,
                BoundingBoxMaxX = rawMotion.BoundingBoxMaxX,
                BoundingBoxMaxY = rawMotion.BoundingBoxMaxY,
                BoundingBoxMaxZ = rawMotion.BoundingBoxMaxZ,
                BoundingBoxMaxW = rawMotion.BoundingBoxMaxW,
                FrameLoop = rawMotion.FrameLoop,
                FrameEnd = rawMotion.FrameEnd,
                FramePerSecond = rawMotion.FramePerSecond,
                FrameCount = rawMotion.FrameCount
            });

            foreach (var block in rawMotion.Matrices)
                for (int i = 0; i < block.Length; i++)
                    stream.Write(block[i]);

            for (int i = 0; i < rawMotion.Matrices2.Length; i++)
                stream.Write(rawMotion.Matrices2[i]);
        }

        private static void Write(Stream stream, InterpolatedMotion_OLD motion, bool unkFlag)
        {
            var valuePool = new PoolValues<float>();
            var keyFramePool = new PoolValues<float>(motion.Timeline.Select(x => x.KeyFrame).Distinct().OrderBy(x => x));
            var tangentPool = new PoolValues<float>();
            var rawTimeline = new List<TimelineTableInternal>(motion.Timeline.Count);
            foreach (var item in motion.Timeline)
            {
                rawTimeline.Add(new TimelineTableInternal
                {
                    Time = (short)(((int)item.Interpolation & 3) | (keyFramePool.GetIndex(item.KeyFrame) << 2)),
                    ValueIndex = (short)valuePool.GetIndex(item.Value),
                    TangentIndexEaseIn = (short)tangentPool.GetIndex(item.TangentEaseIn),
                    TangentIndexEaseOut = (short)tangentPool.GetIndex(item.TangentEaseOut),
                });
            }

            var writer = new BinaryWriter(stream);
            var header = new InterpolatedMotionInternal
            {
                BoneCount = motion.BoneCount,
                TotalFrameCount = motion.TotalFrameCount,
                Unk48 = motion.Unk48,
                BoundingBoxMinX = motion.BoundingBoxMinX,
                BoundingBoxMinY = motion.BoundingBoxMinY,
                BoundingBoxMinZ = motion.BoundingBoxMinZ,
                BoundingBoxMinW = motion.BoundingBoxMinW,
                BoundingBoxMaxX = motion.BoundingBoxMaxX,
                BoundingBoxMaxY = motion.BoundingBoxMaxY,
                BoundingBoxMaxZ = motion.BoundingBoxMaxZ,
                BoundingBoxMaxW = motion.BoundingBoxMaxW,
                FrameLoop = motion.FrameLoop,
                FrameEnd = motion.FrameEnd,
                FramePerSecond = motion.FramePerSecond,
                FrameCount = motion.FrameCount,
                Unk98 = motion.Unk98,
                Unk9c = motion.Unk9c,
            };

            stream.Write(new byte[ReservedSize], 0, ReservedSize);
            BinaryMapping.WriteObject(stream, new Header_Old { });
            BinaryMapping.WriteObject(stream, new InterpolatedMotionInternal { });

            header.StaticPoseCount = motion.StaticPose.Count;
            header.StaticPoseOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.StaticPose)
                BinaryMapping.WriteObject(stream, item);

            header.ModelBoneAnimationCount = motion.ModelBoneAnimation.Count;
            header.ModelBoneAnimationOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.ModelBoneAnimation.Select(Map))
                BinaryMapping.WriteObject(stream, item);

            header.IKHelperAnimationCount = motion.IKHelperAnimation.Count;
            header.IKHelperAnimationOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.IKHelperAnimation.Select(Map))
                BinaryMapping.WriteObject(stream, item);

            header.TimelineOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in rawTimeline)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(4);
            header.KeyFrameCount = keyFramePool.Values.Count;
            header.KeyFrameOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in keyFramePool.Values)
                writer.Write(item);

            header.TransformationValueOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in valuePool.Values)
                writer.Write(item);

            header.TangentOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in tangentPool.Values)
                writer.Write(item);

            header.IKChainCount = motion.IKChains.Count;
            header.IKChainOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.IKChains)
                BinaryMapping.WriteObject(stream, item);

            header.Unk48 = (int)(stream.Position - ReservedSize);

            stream.AlignPosition(0x10);
            header.Table8Offset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.Table8)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(0x10);
            header.Table7Count = motion.Table7.Count;
            header.Table7Offset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.Table7)
                BinaryMapping.WriteObject(stream, item);

            header.Table6Count = motion.Table6.Count;
            header.Table6Offset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.Table6)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(0x10);
            header.TotalBoneCount = (short)(motion.BoneCount + motion.IKHelpers.Count);
            header.IKHelperOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.IKHelpers)
                BinaryMapping.WriteObject(stream, item);

            header.JointIndexOffset = (int)(stream.Position - ReservedSize);
            foreach (var item in motion.Joints)
                BinaryMapping.WriteObject(stream, item);

            stream.AlignPosition(0x10);
            header.FooterOffset = (int)(stream.Position - ReservedSize);
            BinaryMapping.WriteObject(stream, motion.Footer);

            header.UnknownTable1Offset = (int)(stream.Position - ReservedSize);
            header.UnknownTable1Count = 0;

            stream.AlignPosition(0x10);
            stream.SetLength(stream.Position);

            stream.SetPosition(ReservedSize);
            BinaryMapping.WriteObject(stream, new Header_Old
            {
                Version = 0,
                Unk04 = unkFlag ? 1 : 0,
                ByteCount = (int)(stream.Length - ReservedSize),
                Unk0c = 0,
            });
            BinaryMapping.WriteObject(stream, header);
        }

        private static BoneAnimationTable Map(BoneAnimationTableInternal obj) => new BoneAnimationTable
        {
            JointIndex = obj.JointIndex,
            Channel = (byte)(obj.Channel & 0xF),
            Pre = (byte)((obj.Channel >> 4) & 3),
            Post = (byte)((obj.Channel >> 6) & 3),
            TimelineStartIndex = obj.TimelineStartIndex,
            TimelineCount = obj.TimelineCount,
        };

        private static BoneAnimationTableInternal Map(BoneAnimationTable obj) => new BoneAnimationTableInternal
        {
            JointIndex = obj.JointIndex,
            Channel = (byte)((obj.Channel & 0xF) | ((obj.Pre & 3) << 4) | ((obj.Post & 3) << 6)),
            TimelineStartIndex = obj.TimelineStartIndex,
            TimelineCount = obj.TimelineCount,
        };

        private Motion(bool isRaw)
        {
            IsRaw = isRaw;
            if (isRaw)
            {
                Raw = new RawMotion_OLD
                {
                    Matrices = new List<Matrix4x4[]>(),
                };
            }
            else
            {
                Interpolated = new InterpolatedMotion_OLD
                {
                    Footer = new FooterTable(),
                    IKChains = new List<IKChainTable>(),
                    IKHelperAnimation = new List<BoneAnimationTable>(),
                    IKHelpers = new List<IKHelperTable>(),
                    Joints = new List<JointTable>(),
                    ModelBoneAnimation = new List<BoneAnimationTable>(),
                    StaticPose = new List<InitialPose>(),
                    Table6 = new List<UnknownTable6>(),
                    Table7 = new List<UnknownTable7>(),
                    Table8 = new List<UnknownTable8>(),
                    Timeline = new List<TimelineTable>(),
                };
            }
        }

        public static Motion CreateInterpolatedFromScratch() => new Motion(false);
    }
}
