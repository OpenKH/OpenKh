using OpenKh.Common;
using OpenKh.Common.Utils;
using OpenKh.Kh2.Models.VIF;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using Xe.BinaryMapper;
using static OpenKh.Kh2.Models.ModelCommon;

namespace OpenKh.Kh2.Models
{
    //----------------
    // FILE STRUCTURES
    //----------------

    public class ModelSkeletal
    {
        [Data] public ModelHeader ModelHeader { get; set; }
        [Data] public ushort BoneCount { get; set; }
        [Data] public ushort TextureCount { get; set; }
        [Data] public uint BoneOffset { get; set; }
        [Data] public uint BoneDataOffset { get; set; }
        [Data] public int GroupCount { get; set; }
        public List<SkeletalGroup> Groups { get; set; }
        public SkeletonData BoneData { get; set; }
        public List<Bone> Bones { get; set; }
        public ModelShadow Shadow { get; set; }

        public class SkeletalGroup
        {
            public SkeletalGroupHeader Header { get; set; }
            public byte[] VifData { get; set; }
            public List<DmaPacket> DmaData { get; set; }
            public List<int> BoneMatrix { get; set; }
            public SkeletalMesh Mesh { get; set; }

            public SkeletalGroup()
            {
                Header = new SkeletalGroupHeader();
                DmaData = new List<DmaPacket>();
                BoneMatrix = new List<int>();
            }
        }
        public class SkeletalGroupHeader
        {
            [Data] public int AttributesBitfield { get; set; }
            [Data] public uint TextureIndex { get; set; }
            [Data] public uint PolygonCount { get; set; }
            [Data] public ushort HasVertexBuffer { get; set; }
            [Data] public ushort Alternative { get; set; }
            [Data] public uint DmaPacketOffset { get; set; }
            [Data] public uint BoneMatrixOffset { get; set; }
            [Data] public uint PacketLength { get; set; }
            [Data] public uint Reserved { get; set; } // Padding

            public bool DrawAlphaPhase
            {
                get => BitsUtil.Int.GetBit(AttributesBitfield, 0);
                set => AttributesBitfield = (int)BitsUtil.Int.SetBit(AttributesBitfield, 0, value);
            }
            public bool Alpha
            {
                get => BitsUtil.Int.GetBit(AttributesBitfield, 1);
                set => AttributesBitfield = (int)BitsUtil.Int.SetBit(AttributesBitfield, 1, value);
            }
            public bool Multi
            {
                get => BitsUtil.Int.GetBit(AttributesBitfield, 2);
                set => AttributesBitfield = (int)BitsUtil.Int.SetBit(AttributesBitfield, 2, value);
            }
            public int Part
            {
                get => BitsUtil.Int.GetBits(AttributesBitfield, 6, 10);
                set => AttributesBitfield = BitsUtil.Int.SetBits(AttributesBitfield, 6, 10, value);
            }
            public int Mesh
            {
                get => BitsUtil.Int.GetBits(AttributesBitfield, 11, 15);
                set => AttributesBitfield = BitsUtil.Int.SetBits(AttributesBitfield, 11, 15, value);
            }
            public int Priority
            {
                get => BitsUtil.Int.GetBits(AttributesBitfield, 16, 20);
                set => AttributesBitfield = BitsUtil.Int.SetBits(AttributesBitfield, 16, 20, value);
            }
            public bool AlphaEx
            {
                get => BitsUtil.Int.GetBit(AttributesBitfield, 21);
                set => AttributesBitfield = (int)BitsUtil.Int.SetBit(AttributesBitfield, 21, value);
            }
            public int UVScroll
            {
                get => BitsUtil.Int.GetBits(AttributesBitfield, 22, 26);
                set => AttributesBitfield = BitsUtil.Int.SetBits(AttributesBitfield, 22, 26, value);
            }
            public bool AlphaAdd
            {
                get => BitsUtil.Int.GetBit(AttributesBitfield, 27);
                set => AttributesBitfield = (int)BitsUtil.Int.SetBit(AttributesBitfield, 27, value);
            }
            public bool AlphaSub
            {
                get => BitsUtil.Int.GetBit(AttributesBitfield, 28);
                set => AttributesBitfield = (int)BitsUtil.Int.SetBit(AttributesBitfield, 28, value);
            }
            public bool Specular
            {
                get => BitsUtil.Int.GetBit(AttributesBitfield, 29);
                set => AttributesBitfield = (int)BitsUtil.Int.SetBit(AttributesBitfield, 29, value);
            }
            public bool NoLight
            {
                get => BitsUtil.Int.GetBit(AttributesBitfield, 30);
                set => AttributesBitfield = (int)BitsUtil.Int.SetBit(AttributesBitfield, 30, value);
            }
        }

        /* SkeletalGroup attributes bitfield
         * reserved2
         * noLight
         * specular
         * alphaSub
         * alphaAdd
         * uvScroll [5] // UV scroll index
         * alphaEx  // "Shadow off" from old code
         * priority [5]
         * mesh [5]
         * part [5]
         * reserved1 [3]
         * multi
         * alpha
         * drawAlphaPhase
         */

        //-------------
        // CONSTRUCTORS
        //-------------

        public ModelSkeletal() { }

        public static ModelSkeletal Read(Stream stream)
        {
            int ReservedSize = 0x90;

            // Header
            stream.Position = ReservedSize;
            ModelSkeletal model = BinaryMapping.ReadObject<ModelSkeletal>(stream);

            // Group headers
            model.Groups = new List<SkeletalGroup>();
            for (int i = 0; i < model.GroupCount; i++)
            {
                SkeletalGroup group = new SkeletalGroup();
                group.Header = BinaryMapping.ReadObject<SkeletalGroupHeader>(stream);
                model.Groups.Add(group);
            }

            // Bone Data
            model.BoneData = BinaryMapping.ReadObject<SkeletonData>(stream);

            // Bone List
            model.Bones = new List<Bone>();
            for (int i = 0; i < model.BoneCount; i++)
            {
                model.Bones.Add(BinaryMapping.ReadObject<Bone>(stream));
            }

            // Group data
            foreach (SkeletalGroup group in model.Groups)
            {
                // Padding to 16 bytes
                /*if (stream.Position % 16 != 0)
                {
                    stream.Position += (16 - (stream.Position % 16));
                }*/
                ModelCommon.alignStreamToByte(stream, 16);

                // Option A: Read it all
                // Option B: Would be to get the vifLength by adding the start DMA tags' qwc in the DMA packets
                int vifLength = (int)(ReservedSize + group.Header.DmaPacketOffset) - (int)stream.Position;
                group.VifData = stream.ReadBytes(vifLength);

                for (int i = 0; i < group.Header.PacketLength; i++)
                {
                    group.DmaData.Add(BinaryMapping.ReadObject<DmaPacket>(stream));
                }

                int BoneMatrixCount = stream.ReadInt32();
                for (int i = 0; i < BoneMatrixCount; i++)
                {
                    group.BoneMatrix.Add(stream.ReadInt32());
                }

                group.Mesh = getMeshFromGroup(group, GetBoneMatrices(model.Bones));
            }

            //model.calcVertexAbsolutePositions();
            //model = calcVertexAbsolutePositions(model);

            if (model.ModelHeader.Size > 0)
            {
                stream.Position = ReservedSize + model.ModelHeader.Size;
                model.Shadow = ModelShadow.Read(stream, model.Bones);
            }

            return model;
        }
        public void Write(Stream stream)
        {
            recalcOffsets();

            int ReservedSize = 0x90;
            stream.Position = ReservedSize;

            BinaryMapping.WriteObject(stream, this);
            foreach (SkeletalGroup group in Groups)
            {
                BinaryMapping.WriteObject(stream, group.Header);
            }
            BinaryMapping.WriteObject(stream, this.BoneData);
            foreach (Bone bone in Bones)
            {
                BinaryMapping.WriteObject(stream, bone);
            }
            foreach (SkeletalGroup group in Groups)
            {
                ModelCommon.alignStreamToByte(stream, 16);
                using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
                {
                    writer.Write(group.VifData);
                }
                foreach (DmaPacket dmaPacket in group.DmaData)
                {
                    BinaryMapping.WriteObject(stream, dmaPacket);
                }
                using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
                {
                    writer.Write(group.BoneMatrix.Count);
                foreach (int boneIndex in group.BoneMatrix)
                {
                        writer.Write(boneIndex);
                }
            }
            }

            ModelCommon.alignStreamToByte(stream, 16);
            if (Shadow != null)
            {
                Shadow.Write(stream);
            }
        }

        //----------
        // FUNCTIONS
        //----------

        // Gets the mesh from a given skeletal group
        public static SkeletalMesh getMeshFromGroup(SkeletalGroup group, Matrix4x4[] boneMatrices)
        {
            // Get the VIF-DMA-boneMatrix packets
            List<DmaVifPacket> dmaVifPackets = GetDmaVifPackets(group);

            // Unpacks the VIF packets to VPU data
            List<OpenKh.Ps2.VpuPacket> vpuPackets = new List<OpenKh.Ps2.VpuPacket>();
            foreach (DmaVifPacket dmaVifPacket in dmaVifPackets)
            {
                vpuPackets.Add(unpackVifToVpu(dmaVifPacket.VifPacket));
            }

            // Processes the unpacked data
            VpuGroup vpuGroup = getVpuGroup(vpuPackets, dmaVifPackets);

            // Generates the mesh
            return GetSkeletalMeshFromVpuGroup(vpuGroup, boneMatrices);
        }

        public void recalculateMeshes()
        {
            foreach (SkeletalGroup group in Groups)
            {
                group.Mesh = getMeshFromGroup(group, GetBoneMatrices(Bones));
            }
        }

        //----------------------------
        // INTERNAL FUNCTIONS
        // Kept public for ease of use
        //----------------------------

        // Gets the mesh from a given vpu group
        public static SkeletalMesh GetSkeletalMeshFromVpuGroup(VpuGroup vpuGroup, Matrix4x4[] boneMatrices)
        {
            SkeletalMesh mesh = new SkeletalMesh();

            for (int i = 0; i < vpuGroup.Indices.Count; i++)
            {
                // Vertices
                OpenKh.Ps2.VpuPacket.VertexIndex index = vpuGroup.Indices[i];
                UVBVertex vertex = vpuGroup.Vertices[index.Index];

                UVBVertex completeVertex = new UVBVertex(vertex.BPositions, index.U, index.V, ModelCommon.getAbsolutePosition(vertex.BPositions, boneMatrices));

                if (index.Color != null)
                    completeVertex.Color = new VifCommon.VertexColor((byte)index.Color.R, (byte)index.Color.G, (byte)index.Color.B, (byte)index.Color.A);
                if (index.Normal != null)
                    completeVertex.Normal = new VifCommon.VertexNormal(index.Normal.X, index.Normal.Y, index.Normal.Z);

                mesh.Vertices.Add(completeVertex);

                // Triangles
                if (index.Function == OpenKh.Ps2.VpuPacket.VertexFunction.DrawTriangle ||
                   index.Function == OpenKh.Ps2.VpuPacket.VertexFunction.DrawTriangleDoubleSided)
                {
                    mesh.Triangles.Add(new List<int> { i, i - 2, i - 1 });
                }
                if (index.Function == OpenKh.Ps2.VpuPacket.VertexFunction.DrawTriangleInverse ||
                   index.Function == OpenKh.Ps2.VpuPacket.VertexFunction.DrawTriangleDoubleSided)
                {
                    mesh.Triangles.Add(new List<int> { i, i - 1, i - 2 });
                }
            }

            return mesh;
        }

        // Returns the DmaVifPackets that form the group
        public static List<DmaVifPacket> GetDmaVifPackets(SkeletalGroup group)
        {
            List<DmaVifPacket> packets = new List<DmaVifPacket>();

            // DMA Set
            DmaVifPacket currentPacket = new DmaVifPacket();
            foreach (DmaPacket dmaPacket in group.DmaData)
            {
                if (currentPacket.DmaSet.StartTag == null)
                {
                    currentPacket.DmaSet.StartTag = dmaPacket;
                }
                else if (dmaPacket.DmaTag.Qwc == 0)
                {
                    currentPacket.DmaSet.EndTag = dmaPacket;
                    packets.Add(currentPacket);
                    currentPacket = new DmaVifPacket();
                }
                else
                {
                    currentPacket.DmaSet.DmaPackets.Add(dmaPacket);
                }
            }

            // Bone list
            int currentPacketIndex = 0;
            foreach (int boneIndex in group.BoneMatrix)
            {
                if (boneIndex != -1)
                {
                    packets[currentPacketIndex].BoneList.Add(boneIndex);
                }
                else
                {
                    currentPacketIndex++;
                }
            }

            // VIF Packet
            using (MemoryStream vpuStream = new MemoryStream(group.VifData))
            {
                for (int i = 0; i < packets.Count; i++)
                {
                    DmaVifPacket packet = packets[i];
                    packet.VifPacket = vpuStream.ReadBytes(packet.DmaSet.StartTag.DmaTag.Qwc * 0x10);
                }
            }

            return packets;
        }

        // Unpacks the VIF packet data
        public static OpenKh.Ps2.VpuPacket unpackVifToVpu(byte[] vifPacket)
        {
            OpenKh.Ps2.VifUnpacker unpacker = new OpenKh.Ps2.VifUnpacker(vifPacket);
            unpacker.Run();
            using (var stream = new MemoryStream(unpacker.Memory))
            {
                return OpenKh.Ps2.VpuPacket.Read(stream);
            }
        }

        // VPU data is sequential and it's split in multiple packets.
        // Joins the VPU packets' data in a single VPUGroup.
        public static VpuGroup getVpuGroup(List<OpenKh.Ps2.VpuPacket> vpuPackets, List<DmaVifPacket> dmaVifPackets)
        {
            VpuGroup vpuGroup = new VpuGroup();

            int indexCount = 0;
            for (int i = 0; i < vpuPackets.Count; i++)
            {
                OpenKh.Ps2.VpuPacket vpuPacket = vpuPackets[i];

                // POSITIONS RELATIVE TO BONES
                List<BPosition> positions = new List<BPosition>();

                int currentBoneIndex = 0;
                int vertexCount = 0;
                for (int j = 0; j < vpuPacket.Vertices.Length; j++)
                {
                    Vector4 relativePosition = new Vector4(vpuPacket.Vertices[j].X, vpuPacket.Vertices[j].Y, vpuPacket.Vertices[j].Z, vpuPacket.Vertices[j].W);
                    int boneIndex = dmaVifPackets[i].BoneList[currentBoneIndex];
                    BPosition bPosition = new BPosition(relativePosition, boneIndex);
                    positions.Add(bPosition);

                    vertexCount++;
                    if (vertexCount >= vpuPacket.VertexRange[currentBoneIndex])
                    {
                        currentBoneIndex++;
                        vertexCount = 0;
                    }
                }

                // VERTICES WITH THEIR POSITIONS RELATIVE TO BONES (Spatial position)
                // One bone per vertex (LP)
                if (vpuPacket.VertexWeightedCount == 0)
                {
                    int coun = 0;
                    foreach (BPosition bPosition in positions)
                    {
                        vpuGroup.Vertices.Add(new UVBVertex(new List<BPosition> { bPosition }));
                        coun++;
                    }
                }
                // Multiple bones per vertex (HP)
                else
                {
                    // WeightCount list
                    for (int j = 0; j < vpuPacket.VertexWeightedIndices.Length; j++)
                    {
                        // Vertex list
                        for (int k = 0; k < vpuPacket.VertexWeightedIndices[j].Length; k++)
                        {
                            List<BPosition> currentPositions = new List<BPosition>();
                            foreach (int inde in vpuPacket.VertexWeightedIndices[j][k])
                            {
                                currentPositions.Add(positions[inde]);
                            }
                            vpuGroup.Vertices.Add(new UVBVertex(currentPositions));
                        }
                    }
                }

                // VPU INDICES (Vertices with spacial position + UV)
                int previousIndexCount = vpuGroup.Indices.Count;
                foreach (OpenKh.Ps2.VpuPacket.VertexIndex vertexIndex in vpuPacket.Indices)
                {
                    OpenKh.Ps2.VpuPacket.VertexIndex newVertexIndex = new OpenKh.Ps2.VpuPacket.VertexIndex();
                    newVertexIndex.Index = vertexIndex.Index + indexCount;
                    newVertexIndex.Function = vertexIndex.Function;
                    newVertexIndex.U = vertexIndex.U;
                    newVertexIndex.V = vertexIndex.V;
                    vpuGroup.Indices.Add(newVertexIndex);
                }

                // Colors
                if(vpuPacket.Indices.Length == vpuPacket.Colors.Length)
                {
                    for (int j = 0; j < vpuPacket.Indices.Length; j++)
                    {
                        vpuGroup.Indices[previousIndexCount + j].Color = vpuPacket.Colors[j];
                    }
                }

                // Normals
                if (vpuPacket.Indices.Length == vpuPacket.Normals.Length)
                {
                    for (int j = 0; j < vpuPacket.Indices.Length; j++)
                    {
                        vpuGroup.Indices[previousIndexCount + j].Normal = vpuPacket.Normals[j];
                    }
                }

                indexCount = vpuGroup.Vertices.Count;
            }

            return vpuGroup;
        }

        public void recalcOffsets()
        {
            BoneCount = (ushort)Bones.Count;
            GroupCount = (ushort)Groups.Count;

            int currentCount = 0;
            // Headers
            currentCount += 32;
            // Group headers
            currentCount += Groups.Count * 32;
            BoneDataOffset = (uint)currentCount;
            // Bone data
            currentCount += 0x120;
            BoneOffset = (uint)currentCount;
            // Bones
            currentCount += Bones.Count * 64;

            foreach (SkeletalGroup group in Groups)
            {
                currentCount += group.VifData.Length;
                group.Header.DmaPacketOffset = (uint)(currentCount);
                currentCount += group.DmaData.Count * 16;
                group.Header.BoneMatrixOffset = (uint)(currentCount);
                currentCount += (1 + group.BoneMatrix.Count) * 4;
                int alignTo16 = 16 - (currentCount % 16);
                if (alignTo16 != 16)
                    currentCount += alignTo16;
            }

            GroupCount = Groups.Count;

            if (Shadow == null)
            {
                ModelHeader.Size = 0;
            }
            else
            {
                uint lastPosition = Groups[Groups.Count - 1].Header.BoneMatrixOffset;
                lastPosition += (uint)(1 + Groups[Groups.Count - 1].BoneMatrix.Count) * 4;

                uint remainingUpTo16 = (uint)(lastPosition % 16);
                if (remainingUpTo16 != 0)
                {
                    remainingUpTo16 = 16 - remainingUpTo16;
                }

                ModelHeader.Size = lastPosition + remainingUpTo16;
            }
        }

        // Calculates the absolute position of the vertices based on their positions relative to bones
        // DELETE.............................................................................................................................................................
        public void calcVertexAbsolutePositions()
        {
            Matrix4x4[] boneMatrices = ModelCommon.GetBoneMatrices(this.Bones);

            foreach (SkeletalGroup group in this.Groups)
            {
                foreach (UVBVertex vertex in group.Mesh.Vertices)
                {
                    vertex.Position = ModelCommon.getAbsolutePosition(vertex.BPositions, boneMatrices);
                }
            }
        }
        public static ModelSkeletal calcVertexAbsolutePositions(ModelSkeletal model)
        {
            Matrix4x4[] boneMatrices = ModelCommon.GetBoneMatrices(model.Bones);

            foreach (SkeletalGroup group in model.Groups)
            {
                foreach (UVBVertex vertex in group.Mesh.Vertices)
                {
                    vertex.Position = ModelCommon.getAbsolutePosition(vertex.BPositions, boneMatrices);
                }
            }
            return model;
        }

        //----------------
        // DATA STRUCTURES
        //----------------

        // Packets in which a group is divided
        public class DmaVifPacket
        {
            public byte[] VifPacket { get; set; } // Packed microprogram to be fed to the VPU1 (AKA VU1) through the VIF unpacker
            public DmaSet DmaSet { get; set; } // DMA tags to instruct how to move the VIF packets from memory to the VPU1
            public List<int> BoneList { get; set; }

            public DmaVifPacket()
            {
                DmaSet = new DmaSet();
                BoneList = new List<int>();
            }
        }
        // DMA program to send data from memory to VPU (Through the VIF)
        public class DmaSet
        {
            public DmaPacket StartTag { get; set; }
            public List<DmaPacket> DmaPackets { get; set; }
            public DmaPacket EndTag { get; set; }

            public DmaSet()
            {
                DmaPackets = new List<DmaPacket>();
            }
            public override string ToString()
            {
                return "QWC: " + StartTag.DmaTag.Qwc + "; Address: " + StartTag.DmaTag.Address + "; Packet count: " + DmaPackets.Count;
            }
        }
        // Grouped data from the packets to be processed
        public class VpuGroup
        {
            public List<UVBVertex> Vertices { get; set; } // Spacial location of the vertices
            public List<OpenKh.Ps2.VpuPacket.VertexIndex> Indices { get; set; } // List of vertices with UV and triangles

            public VpuGroup()
            {
                Vertices = new List<UVBVertex>();
                Indices = new List<OpenKh.Ps2.VpuPacket.VertexIndex>();
            }
        }
        // Contains the triangles that form the mesh pointing to a vertex
        // Note: UV coordinates must be calculated using (X / 16 / 256.0f) and V is (1 - V)
        public class SkeletalMesh
        {
            public List<UVBVertex> Vertices { get; set; }
            public List<List<int>> Triangles { get; set; }
            public SkeletalMesh()
            {
                Vertices = new List<UVBVertex>();
                Triangles = new List<List<int>>();
            }
        }
    }
}
