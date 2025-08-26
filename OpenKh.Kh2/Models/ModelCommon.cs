using OpenKh.Common.Utils;
using OpenKh.Kh2.Models.VIF;
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

        public class SkeletonData
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
            [Data(Count = 56)] public int[] BoneReferences { get; set; } // Ids of the bones (Check comment below)
            [Data] public float DistanceFromSkeletonX { get; set; }
            [Data] public float DistanceFromSkeletonY { get; set; }
            [Data] public float DistanceFromSkeletonZ { get; set; }
            [Data] public float DistanceFromSkeletonW { get; set; }
        }

        /*
         PART_UNKNOWN=-1,
         PART_HEAD=0,
         PART_RF_HEAD=1,
         PART_LB_HEAD=2,
         PART_RB_HEAD=3,
         PART_NECK=4,
         PART_RF_NECK=5,
         PART_LB_NECK=6,
         PART_RB_NECK=7,
         PART_CHEST=8,
         PART_RF_CHEST=9,
         PART_LB_CHEST=10,
         PART_RB_CHEST=11,
         PART_HIP=12,
         PART_RF_HIP=13
         PART_LB_HIP=14,
         PART_RB_HIP=15,
         PART_COLLAR=16,
         PART_RF_COLLAR=17,
         PART_LB_COLLAR=18,
         PART_RB_COLLAR=19,
         PART_UPARM=20,
         PART_RF_UPARM=21,
         PART_LB_UPARM=22,
         PART_RB_UPARM=23,
         PART_FOARM=24,
         PART_RF_FOARM=25,
         PART_LB_FOARM=26,
         PART_RB_FOARM=27,
         PART_HAND=28,
         PART_RF_HAND=29,
         PART_LB_HAND=30,
         PART_RB_HAND=31,
         PART_FEMUR=32,
         PART_RF_FEMUR=33,
         PART_LB_FEMUR=34,
         PART_RB_FEMUR=35,
         PART_LF_TIBIA=36,
         PART_RF_TIBIA=37,
         PART_LB_TIBIA=38,
         PART_RB_TIBIA=39,
         PART_LF_FOOT=40,
         PART_RF_FOOT=41,
         PART_LB_FOOT=42,
         PART_RB_FOOT=43,
         PART_LF_TOES=44,
         PART_RF_TOES=45,
         PART_LB_TOES=46,
         PART_RB_TOES=47,
         PART_WEAPON_L_LINK=48,
         PART_WEAPON_L=49,
         PART_WEAPON_R_LINK=50,
         PART_WEAPON_R=51,
         PART_SPECIAL0=52,
         PART_SPECIAL1=53,
         PART_SPECIAL2=54,
         PART_SPECIAL3=55,
         PART_MAX=56
         */

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

            /* Bone BitfLags:
             * no_envelop
             * not_joint > On when the bone has no rigged vertices
             * The rest is unused
             */


            public bool NoEnvelop
            {
                get => BitsUtil.Int.GetBit(Flags, 0);
                set => Flags = (int)BitsUtil.Int.SetBit(Flags, 0, value);
            }

            public bool NoJoint
            {
                get => BitsUtil.Int.GetBit(Flags, 1);
                set => Flags = (int)BitsUtil.Int.SetBit(Flags, 1, value);
            }

            public override string ToString()
            {
                return "[" + Index + "|" + ParentIndex + "] <" + RotationX + "," + RotationY + "," + RotationZ + "> <" + TranslationX + "," + TranslationY + "," + TranslationZ + ">";
            }
        }

        // DMA packets contain the DMA tag with instructions for the DMAC as well as 2 extra parameters
        public class DmaPacket
        {
            [Data] public OpenKh.Common.Ps2.DmaTag DmaTag { get; set; }
            [Data] public OpenKh.Common.Ps2.VifCode VifCode { get; set; } // Param 0 (VIF code for models)
            [Data] public int Parameter { get; set; } // Param 1

            public DmaPacket()
            {
                DmaTag = new Common.Ps2.DmaTag();
                VifCode = new Common.Ps2.VifCode();
            }

            public override string ToString()
            {
                return (DmaTag.Qwc.ToString("X") + " " + DmaTag.Param.ToString("X") + " " + DmaTag.Address.ToString("X") + " " + VifCode.Cmd.ToString("X") + " " + VifCode.Num.ToString("X") + " " + VifCode.Immediate.ToString("X") + " " + Parameter.ToString("X"));
            }
        }

        // Represents a vertex with a position in space (Absolute and relative to bones) and a UV coordinate
        public class UVBVertex
        {
            public Vector3 Position { get; set; } // Absolute position
            public List<BPosition> BPositions { get; set; } // Positions relative to bones
            public float U { get; set; }
            public float V { get; set; }
            public VifCommon.VertexColor Color { get; set; }
            public VifCommon.VertexNormal Normal { get; set; }

            public UVBVertex() { }
            public UVBVertex(List<BPosition> BonePositions, float U = 0, float V = 0, Vector3 position = new Vector3(), VifCommon.VertexColor color = null, VifCommon.VertexNormal normal = null)
            {
                this.BPositions = BonePositions;
                this.U = U;
                this.V = V;
                Position = position;
                Color = color;
                Normal = normal;
            }

            public override string ToString()
            {
                return "[" + BPositions.Count + "] <" + U + "," + V + "> " + Position;
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
            Matrix4x4[] matrices = new Matrix4x4[boneList.Count];
            /* We compute relative matrices */
            for (int i = 0; i < boneList.Count; i++)
            {
                matrices[i] =
                    Matrix4x4.CreateScale(boneList[i].ScaleX,boneList[i].ScaleY,boneList[i].ScaleZ) * 
                    Matrix4x4.CreateRotationX(boneList[i].RotationX) *
                    Matrix4x4.CreateRotationY(boneList[i].RotationY) * 
                    Matrix4x4.CreateRotationZ(boneList[i].RotationZ) *
                    Matrix4x4.CreateTranslation(boneList[i].TranslationX,boneList[i].TranslationY,boneList[i].TranslationZ);
            }
            /* We compute absolute matrices */
            for (int i = 0; i < boneList.Count; i++)
            {
                if (boneList[i].ParentIndex > -1)
                matrices[i] *= matrices[boneList[i].ParentIndex];
            }
            /* Done. */
            return matrices;
        }

        // Returns the absolute position of the vertex given its position relative to bones
        public static Vector3 getAbsolutePosition(List<BPosition> BPositions, Matrix4x4[] boneMatrices)
        {
            Vector3 finalPos = Vector3.Zero;

            foreach (BPosition bonePosition in BPositions)
            {
                // If only 1 bone is assigned per vertex the code is compressed and W has no value
                if (bonePosition.Position.W == 0)
                {
                    bonePosition.Position = new Vector4(bonePosition.Position.X, bonePosition.Position.Y, bonePosition.Position.Z, 1);
                }

                finalPos += ToVector3(Vector4.Transform(bonePosition.Position, boneMatrices[bonePosition.BoneIndex]));
            }

            return finalPos;
        }

        // Returns the position relative to the bone of the vertex given its absolute position and the weight to the bone
        public static Vector3 getRelativePosition(Vector3 absolutePosition, Matrix4x4 boneMatrix, float weight)
        {
            Matrix4x4 invertedMatrix = new Matrix4x4();
            Matrix4x4.Invert(boneMatrix, out invertedMatrix);

            Vector3 relativePosition = Vector3.Transform(absolutePosition, invertedMatrix);

            relativePosition *= weight;

            return relativePosition;
        }

        // Aligns the stream to the given byte
        public static void alignStreamToByte(Stream stream, int alignByte)
        {
            if (stream.Position % alignByte != 0)
            {
                byte[] extraBytes = new byte[(alignByte - (stream.Position % alignByte))];
                MemoryStream extraStream = new MemoryStream(extraBytes);
                extraStream.CopyTo(stream);
            }
        }

        private static Vector3 ToVector3(Vector4 pos) => new Vector3(pos.X, pos.Y, pos.Z);

        // Returns the generic DMA packet that ends a DMA chain
        public static DmaPacket getEndDmaPacket()
        {
            ModelCommon.DmaPacket endDma = new ModelCommon.DmaPacket();
            endDma.DmaTag = new Common.Ps2.DmaTag();
            endDma.VifCode = new Common.Ps2.VifCode();
            endDma.DmaTag.Param = 0x1000;
            endDma.VifCode.Immediate = 0x1700;

            return endDma;
        }
    }
}
