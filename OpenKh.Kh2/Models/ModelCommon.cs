using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Models
{
    public class ModelCommon
    {
        //-----------
        // STRUCTURES
        //-----------

        private const int ReservedArea = 0x90;

        public class ModelHeader
        {
            [Data] public int Type { get; set; }
            [Data] public int Subtype { get; set; }
            [Data] public int Attributes { get; set; }
            [Data] public uint Size { get; set; } // Offset of shadow file. 0 if there are no more files
        }

        public enum ModelType : int
        {
            Multi = 1,
            Skeletal = 2,
            Background = 3,
            Shadow = 4
        }

        public enum ModelSubtype : int
        {
            Object_Character = 0,
            Object_Background = 1,

            Background_Map = 0,
            Background_Sky = 1
        }

        public class BoneData
        {
            [Data] public float BoundingBoxMinX { get; set; }
            [Data] public float BoundingBoxMinY { get; set; }
            [Data] public float BoundingBoxMinZ { get; set; }
            [Data] public float BoundingBoxMinW { get; set; }
            [Data] public float BoundingBoxMaxX { get; set; }
            [Data] public float BoundingBoxMaxY { get; set; }
            [Data] public float BoundingBoxMaxZ { get; set; }
            [Data] public float BoundingBoxMaxW { get; set; }
            [Data] public float InverseKinematicsBoneBiasX { get; set; }
            [Data] public float InverseKinematicsBoneBiasY { get; set; }
            [Data] public float InverseKinematicsBoneBiasZ { get; set; }
            [Data] public float InverseKinematicsBoneBiasW { get; set; }
            [Data(Count = 56)] public int[] No { get; set; }
            [Data] public float DistanceFromSkeletonX { get; set; }
            [Data] public float DistanceFromSkeletonY { get; set; }
            [Data] public float DistanceFromSkeletonZ { get; set; }
            [Data] public float DistanceFromSkeletonW { get; set; }
        }

        public class Bone
        {
            [Data] public short Index { get; set; }
            [Data] public short SiblingIndex { get; set; }
            [Data] public short ParentIndex { get; set; }
            [Data] public short ChildIndex { get; set; }
            [Data] public int Reserved { get; set; }
            [Data] public int Flags { get; set; }
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public float ScaleW { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public float RotationW { get; set; }
            [Data] public float TranslationX { get; set; }
            [Data] public float TranslationY { get; set; }
            [Data] public float TranslationZ { get; set; }
            [Data] public float TranslationW { get; set; }
        }
        /* Bone FLags:
         * terminate
         * below
         * enableBias
         * reserved [19]
         * undefined [10]
         */

        // DMA packets contain the DMA tag with instructions for the DMAC as well as 2 extra parameters
        public class DmaPacket
        {
            [Data] public OpenKh.Common.Ps2.DmaTag DmaTag { get; set; }
            [Data] public OpenKh.Common.Ps2.VifCode VifCode { get; set; } // Param 0 (VIF code for models)
            [Data] public int Parameter { get; set; } // Param 1
        }

        // Represents a vertex with a position in space (Absolute and relative to bones) and a UV coordinate
        public class UVBVertex
        {
            public Vector3 Position { get; set; } // Absolute position
            public List<BPosition> BPositions { get; set; } // Positions relative to bones
            public float U { get; set; }
            public float V { get; set; }

            public UVBVertex() { }
            public UVBVertex(List<BPosition> BonePositions, float U = 0, float V = 0)
            {
                this.BPositions = BonePositions;
                this.U = U;
                this.V = V;
            }
        }
        // Position relative to bone. The W coordinate represents the weight of the bone.
        public class BPosition
        {
            public Vector4 Position { get; set; }
            public int BoneIndex { get; set; }
            public BPosition(Vector4 Position = new Vector4(), int BoneIndex = -1)
            {
                this.Position = Position;
                this.BoneIndex = BoneIndex;
            }
            public override string ToString()
            {
                return "[" + BoneIndex + " | " + Position.W + "] <" + Position.X + ", " + Position.Y + ", " + Position.Z + ">";
            }
        }

        //----------
        // FUNCTIONS
        //----------

        // Returns the ordered bone hierarchy from given bone to root
        public static List<Bone> getBoneHierarchy(List<Bone> boneList, int boneIndex)
        {
            List<Bone> boneHierarchy = new List<Bone>();

            Bone currentBone = boneList[boneIndex];
            boneHierarchy.Add(currentBone);

            while (currentBone.ParentIndex != null && currentBone.ParentIndex != -1 && !boneHierarchy.Contains(currentBone))
            {
                currentBone = boneList[currentBone.ParentIndex];
                boneHierarchy.Add(currentBone);
            }

            return boneHierarchy;
        }

        // Returns the absolute SRT matrix for each bone
        public static Matrix4x4[] GetBoneMatrices(List<Bone> boneList)
        {
            Vector3[] absTranslationList = new Vector3[boneList.Count];
            Quaternion[] absRotationList = new Quaternion[boneList.Count];

            for (int i = 0; i < boneList.Count; i++)
            {
                Bone bone = boneList[i];
                int parentIndex = bone.ParentIndex;
                Quaternion absRotation;
                Vector3 absTranslation;

                if (parentIndex == -1)
                {
                    absRotation = Quaternion.Identity;
                    absTranslation = Vector3.Zero;
                }
                else
                {
                    absRotation = absRotationList[parentIndex];
                    absTranslation = absTranslationList[parentIndex];
                }

                Vector3 localTranslation = Vector3.Transform(new Vector3(bone.TranslationX, bone.TranslationY, bone.TranslationZ), Matrix4x4.CreateFromQuaternion(absRotation));
                absTranslationList[i] = absTranslation + localTranslation;

                var localRotation = Quaternion.Identity;
                if (bone.RotationZ != 0)
                    localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitZ, bone.RotationZ));
                if (bone.RotationY != 0)
                    localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitY, bone.RotationY));
                if (bone.RotationX != 0)
                    localRotation *= (Quaternion.CreateFromAxisAngle(Vector3.UnitX, bone.RotationX));
                absRotationList[i] = absRotation * localRotation;
            }

            Matrix4x4[] matrices = new Matrix4x4[boneList.Count];
            for (int i = 0; i < boneList.Count; i++)
            {
                var absMatrix = Matrix4x4.Identity;
                absMatrix *= Matrix4x4.CreateFromQuaternion(absRotationList[i]);
                absMatrix *= Matrix4x4.CreateTranslation(absTranslationList[i]);
                matrices[i] = absMatrix;
            }

            return matrices;
        }

        // Returns the absolute position of the vertex given its position relative to bones
        public static Vector3 getAbsolutePosition(List<BPosition> BPositions, Matrix4x4[] boneMatrices)
        {
            Vector3 finalPos = Vector3.Zero;

            if (BPositions.Count == 1)
            {
                // single joint
                finalPos = Vector3.Transform(
                ToVector3(BPositions[0].Position),
                boneMatrices[BPositions[0].BoneIndex]);
            }
            else
            {
                // multiple joints, using rawPos.W as blend weights
                foreach (BPosition bonePosition in BPositions)
                {
                    finalPos += ToVector3(
                        Vector4.Transform(
                            bonePosition.Position,
                            boneMatrices[bonePosition.BoneIndex]
                        ));
                }
            }
            return finalPos;
        }

        // Aligns the stream to the given byte
        public static void alignStreamToByte(Stream stream, int alignByte)
        {
            if (stream.Position % alignByte != 0)
            {
                stream.Position += (alignByte - (stream.Position % alignByte));
            }
        }

        private static Vector3 ToVector3(Vector4 pos) => new Vector3(pos.X, pos.Y, pos.Z);
    }
}
