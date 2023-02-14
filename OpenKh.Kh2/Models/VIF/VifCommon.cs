using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Kh2.Models.VIF
{
    public class VifCommon
    {
        public const byte FLAG_VERTEX = 0x10;
        public const byte FLAG_TRIANGLE = 0x20;
        public const byte FLAG_TRIANGLE_INVERTED = 0x30;

        /*
         * One of the vertices that form a face of a mesh in a model
         * Each vertex has:
         * UV coordinates
         * Triangle flag - Used to generate the triangle strips
         * Vertex color
         * Absolute Position - Position relative to 0,0,0. The sum of all of its relative positions
         * Relative Positions - Positions relative to the weighted bones
         */
        public class VifVertex
        {
            public UVCoord UvCoord;
            public byte TriFlag;
            public VertexColor Color;
            public Vector3 AbsolutePosition;
            public List<BoneRelativePosition> RelativePositions; // WeightGroup

            public VifVertex()
            {
                //UvCoord = new UVCoord(0, 0);
                //Color = new VertexColor(0, 0, 0, 0);
                RelativePositions = new List<BoneRelativePosition>();
            }

            // Size this vertex takes in VPU memory if there are no repeated coordinates, no previous weight group of its size, etc...
            public int worstCaseScenarioVpuSize()
            {
                int vpuSize = 0;

                vpuSize += 1; // Strip Node (UV,PositionId, TriFlag)
                vpuSize += RelativePositions.Count; // Coordinates
                vpuSize += VifUtils.getAmountIfGroupedBy(RelativePositions.Count, 4); // Bone counts
                vpuSize += 1; // Weight counts
                vpuSize += VifUtils.getAmountIfGroupedBy(RelativePositions.Count, 4); // Weight group
                vpuSize += (4 * RelativePositions.Count); // Bone matrices

                if(Color != null) vpuSize += 1; // Color

                return vpuSize;
            }

            // This can be used to make tri-strips
            public bool isSameVertexAs(VifVertex vertex2)
            {
                if (UvCoord?.U != vertex2.UvCoord?.U ||
                   UvCoord?.V != vertex2.UvCoord?.V)
                {
                    return false;
                }

                if (Color?.R != vertex2.Color?.R ||
                    Color?.G != vertex2.Color?.G ||
                    Color?.B != vertex2.Color?.B ||
                    Color?.A != vertex2.Color?.A)
                    return false;

                if (RelativePositions.Count != vertex2.RelativePositions.Count)
                {
                    return false;
                }

                for (int i = 0; i < RelativePositions.Count; i++)
                {
                    if (!RelativePositions[i].isSamePositionAs(vertex2.RelativePositions[i]))
                        return false;
                }

                return true;
            }

            public bool isValidVertex()
            {
                if (UvCoord == null)
                    return false;

                if (AbsolutePosition == null)
                    return false;

                if (RelativePositions == null || RelativePositions.Count == 0)
                    return false;

                // Note: These are required for Skeletal Models. If this is ever used for models with no skeleton they won't have Relative Positions
                foreach (BoneRelativePosition position in RelativePositions)
                {
                    if (position.BoneIndex == null || position.BoneIndex < 0)
                        return false;
                    if (position.Weight == null || position.Weight < 0 || position.Weight > 1)
                        return false;
                    if (position.Coord == null)
                        return false;
                }

                return true;
            }
        }

        public class VifFace
        {
            public List<VifVertex> Vertices;
            public bool Inverted = false;

            public VifFace()
            {
                Vertices = new List<VifVertex>();
                Inverted = false;
            }
            public VifFace(VifVertex vertex1, VifVertex vertex2, VifVertex vertex3, bool inverted = false)
            {
                vertex1.TriFlag = FLAG_VERTEX;
                vertex2.TriFlag = FLAG_VERTEX;
                vertex3.TriFlag = inverted ? FLAG_TRIANGLE_INVERTED : FLAG_TRIANGLE;
                Vertices = new List<VifVertex> { vertex1, vertex2, vertex3 };
                Inverted = inverted;
            }

            // Size this face takes in VPU memory if there are no repeated coordinates, no previous weight group of its size, etc...
            public int worstCaseScenarioVpuSize()
            {
                int size = 0;
                foreach (VifVertex vertex in Vertices)
                {
                    size += vertex.worstCaseScenarioVpuSize();
                }
                return size;
            }
        }

        public class UVCoord
        {
            public short U;
            public short V;

            public UVCoord(short U, short V)
            {
                this.U = U;
                this.V = V;
            }

            public override string ToString()
            {
                return "<" + U + "," + V + ">";
            }
        }

        public class VertexColor
        {
            public byte R;
            public byte G;
            public byte B;
            public byte A;

            public VertexColor(byte R, byte G, byte B, byte A)
            {
                this.R = R;
                this.G = G;
                this.B = B;
                this.A = A;
            }
        }

        // A position in space relative to a bone instead of 0,0,0
        public class BoneRelativePosition
        {
            public int BoneIndex;
            public Vector3 Coord;
            public float Weight;

            public bool isSamePositionAs(BoneRelativePosition position2)
            {
                if (BoneIndex != position2.BoneIndex)
                    return false;

                if (Weight != position2.Weight)
                    return false;

                if (Coord != position2.Coord)
                    return false;

                return true;
            }

            public override string ToString()
            {
                return "[" + BoneIndex + "|" + Weight + "] " + Coord;
            }
        }
    }
}
