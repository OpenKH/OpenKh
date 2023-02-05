using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Models.VIF
{
    /*
     * Multiple functions to process VIF structures and related data
     */
    public class VifProcessor
    {
        private static bool USE_UNCOMPRESSED = false; // For testing purposes
        private static bool FORCE_MULTIWEIGHT = false; // For testing purposes

        // VIF packet limits
        public static int VERTEX_LIMIT = 0xFF; // Shouldn't need to cap, but just in case
        public static int MEMORY_LIMIT = 0xE0;

        public static List<DmaVifPacket> vifMeshToDmaVifPackets(VifMesh mesh)
        {
            List<DmaVifPacket> dmaVifChain = new List<DmaVifPacket>();

            List<VifPacket> vifPackets = (USE_UNCOMPRESSED) ? divideMeshInPacketsUncompressed(mesh) : divideMeshInPackets(mesh);

            int currentVifAddress = 0;
            foreach (VifPacket vifPacket in vifPackets)
            {
                DmaVifPacket dmaVifPacket = generateDmaVifPacket(vifPacket, currentVifAddress);
                dmaVifChain.Add(dmaVifPacket);
                currentVifAddress += dmaVifPacket.VifCode.Length;
            }

            return dmaVifChain;
        }

        // Divides the mesh in smaller VIF packets. All of the vertices, coordinates and other data is stored.
        // If this function doesn't output a correct VIF packet, there's something funcamentally wrong in the process.
        public static List<VifPacket> divideMeshInPacketsUncompressed(VifMesh mesh)
        {
            // If you want to convert only specific meshes for debugging purposes set them here. Otherwise leave the list empty
            List<int> DEBUG_ONLY_THESE_FACES = new List<int> { };

            List<VifPacket> vifPackets = new List<VifPacket>();

            VifPacket currentPacket = new VifPacket();
            List<int> weightCountByVertex = new List<int>();

            for (int iFace = 0; iFace < mesh.Faces.Count; iFace++)
            {
                if (DEBUG_ONLY_THESE_FACES.Count != 0 && !DEBUG_ONLY_THESE_FACES.Contains(iFace))
                    continue;

                VifCommon.VifFace face = mesh.Faces[iFace];

                // CHECK IF NEW PACKAGE IS NEEDED
                if (currentPacket.getPacketLengthInVPUWithBones() + face.worstCaseScenarioVpuSize() >= 0xFF)
                {
                    throw new System.Exception("DmaVifPacket size exceeded - reduce MEMORY_LIMIT");
                }
                else if (currentPacket.UvCoords.Count >= VERTEX_LIMIT ||
                         currentPacket.getPacketLengthInVPUWithBones() + face.worstCaseScenarioVpuSize() >= MEMORY_LIMIT)
                {
                    for (int i = 0; i < currentPacket.PositionIds.Count; i++)
                    {
                        for (int j = 0; j < weightCountByVertex[i] - 1; j++)
                        {
                            currentPacket.PositionIds[i] += (byte)currentPacket.WeightGroups[j].Count;
                        }
                    }
                    currentPacket.generateBoneData();
                    vifPackets.Add(currentPacket);
                    currentPacket = new VifPacket();
                    weightCountByVertex = new List<int>();
                }

                // Add vertex data
                foreach (VifCommon.VifVertex vertex in face.Vertices)
                {
                    currentPacket.UvCoords.Add(vertex.UvCoord);
                    currentPacket.Flags.Add(vertex.TriFlag);

                    currentPacket.createWeightGroupsUpTo(vertex.RelativePositions.Count); // Make sure that the weight group exist

                    weightCountByVertex.Add(vertex.RelativePositions.Count);
                    currentPacket.PositionIds.Add((byte)currentPacket.WeightGroups[vertex.RelativePositions.Count - 1].Count);

                    List<int> weights = new List<int>();
                    for (int i = 0; i < vertex.RelativePositions.Count; i++)
                    {
                        weights.Add(currentPacket.PositionCoords.Count);
                        currentPacket.PositionCoords.Add(vertex.RelativePositions[i]);
                    }
                    currentPacket.WeightGroups[vertex.RelativePositions.Count - 1].Add(weights);
                }

                currentPacket.generateBoneData(); // To properly calculate packet size
            }

            for (int i = 0; i < currentPacket.PositionIds.Count; i++)
            {
                for (int j = 0; j < weightCountByVertex[i] - 1; j++)
                {
                    currentPacket.PositionIds[i] += (byte)currentPacket.WeightGroups[j].Count;
                }
            }
            currentPacket.generateBoneData();
            vifPackets.Add(currentPacket);

            return vifPackets;
        }

        /*
         * Divides the mesh in smaller VIF packets. It compresses all of the data that can be compressed.
         * - TriStrips (Reuse vertices in previous triangles, eliminating the vertex and changing triFlags): NOT DONE
         * - Position coordinates reuse: DONE
         * - Don't use weights in single-weight packets: DONE
         */
        public static List<VifPacket> divideMeshInPackets(VifMesh mesh)
        {
            List<VifPacket> vifPackets = new List<VifPacket>();

            VifPacket currentPacket = new VifPacket();
            List<VifCommon.VifVertex> verticesInCurrentPacket = new List<VifCommon.VifVertex>();
            int vifAddressPosition = 0;


            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                VifCommon.VifFace face = mesh.Faces[i];

                // CHECK IF NEW PACKAGE IS NEEDED
                // HARD LIMIT
                if (currentPacket.getPacketLengthInVPUWithBones() + face.worstCaseScenarioVpuSize() >= 0xFF)
                {
                    throw new System.Exception("DmaVifPacket size exceeded. Reduce memory limit.");
                }
                // ADJUSTED BY PARAMS LIMIT
                else if (currentPacket.UvCoords.Count + 3 > VERTEX_LIMIT ||
                         currentPacket.getPacketLengthInVPUWithBones() + face.worstCaseScenarioVpuSize() >= MEMORY_LIMIT)
                {
                    currentPacket.optimizeCoordSpace();
                    foreach (VifCommon.VifVertex iVertex in verticesInCurrentPacket)
                    {
                        int foundId = currentPacket.findPositionId(iVertex.RelativePositions);
                        if (foundId == -1)
                            throw new Exception("Position not found when building the VIF packet. Check how they are stored and how they are found.");
                        currentPacket.PositionIds.Add((byte)foundId);
                    }
                    currentPacket.generateBoneData();
                    vifPackets.Add(currentPacket);
                    currentPacket = new VifPacket();
                    verticesInCurrentPacket = new List<VifCommon.VifVertex>();
                }

                // ADD VERTICES
                foreach (VifCommon.VifVertex vertex in face.Vertices)
                {
                    // Add basic vertex data
                    currentPacket.UvCoords.Add(vertex.UvCoord);
                    currentPacket.Flags.Add(vertex.TriFlag);

                    // Add positions
                    currentPacket.createWeightGroupsUpTo(vertex.RelativePositions.Count); // Make sure that the weight group exist
                    int posId = currentPacket.findPositionId(vertex.RelativePositions);
                    if (posId == -1)
                    {
                        List<int> weights = new List<int>();
                        for (int j = 0; j < vertex.RelativePositions.Count; j++)
                        {
                            weights.Add(currentPacket.PositionCoords.Count);
                            currentPacket.PositionCoords.Add(vertex.RelativePositions[j]);
                        }

                        currentPacket.WeightGroups[vertex.RelativePositions.Count - 1].Add(weights);
                    }
                }

                currentPacket.generateBoneData();
                verticesInCurrentPacket.AddRange(face.Vertices);
            }

            currentPacket.optimizeCoordSpace();
            foreach (VifCommon.VifVertex iVertex in verticesInCurrentPacket)
            {
                int foundId = currentPacket.findPositionId(iVertex.RelativePositions);
                if (foundId == -1)
                    throw new Exception("Position not found when building the VIF packet. Check how they are stored and how they are found.");
                currentPacket.PositionIds.Add((byte)foundId);
            }
            currentPacket.generateBoneData();
            vifPackets.Add(currentPacket);

            return vifPackets;
        }

        // Turns the VIF packet into a packet with the VIF code, the DMA tags to read the code and the bone list.
        // These are the packets that MDLX use.
        public static DmaVifPacket generateDmaVifPacket(VifPacket vifPacket, int meshAddress = 0)
        {
            DmaVifPacket packet = new DmaVifPacket();
            packet.BoneList = vifPacket.BoneList;

            bool singleWeightMode = true;

            if (FORCE_MULTIWEIGHT)
            {
                singleWeightMode = false;
            }
            else
            {
                singleWeightMode = (vifPacket.WeightGroups.Count <= 1);
            }
            if (!USE_UNCOMPRESSED && singleWeightMode)
                vifPacket.fixIndicesForSingleWeight();

            packet.calcHeader(vifPacket, singleWeightMode);

            MemoryStream binStream = new MemoryStream(0);

            // HEADER
            using (var writer = new BinaryWriter(binStream, Encoding.UTF8, true))
            {
                writer.Write(VifUtils.UNPACK);
                writer.Write((byte)0x00);
                writer.Write(VifUtils.UNIT_SIZE_INT);
                writer.Write((byte)0x04);
                writer.Write(VifUtils.READ_SIZE_16);
            }
            BinaryMapping.WriteObject(binStream, packet.Header);

            using (var writer = new BinaryWriter(binStream, Encoding.UTF8, true))
            {
                // UV COORDINATES
                writer.Write(VifUtils.UNPACK);
                writer.Write((byte)0x04);
                writer.Write(VifUtils.UNIT_SIZE_INT);
                writer.Write((byte)packet.Header.TriStripNodeCount);
                writer.Write(VifUtils.READ_SIZE_4);
                for (int i = 0; i < vifPacket.UvCoords.Count; i++)
                {
                    writer.Write(vifPacket.UvCoords[i].U);
                    writer.Write(vifPacket.UvCoords[i].V);
                }
                writer.Write(VifUtils.END_UV);

                // POSITION INDICES
                writer.Write(VifUtils.UNPACK);
                writer.Write((byte)0x04);
                writer.Write(VifUtils.UNIT_SIZE_BYTE);
                writer.Write((byte)packet.Header.TriStripNodeCount);
                writer.Write(VifUtils.READ_SIZE_1);
                foreach (byte positionIndex in vifPacket.PositionIds)
                {
                    writer.Write((byte)positionIndex);
                }
            }
            ModelCommon.alignStreamToByte(binStream, 4);

            using (var writer = new BinaryWriter(binStream, Encoding.UTF8, true))
            {
                writer.Write(VifUtils.END_INDICES);

                // TRI FLAGS
                writer.Write(VifUtils.UNPACK);
                writer.Write((byte)0x04);
                writer.Write(VifUtils.UNIT_SIZE_BYTE);
                writer.Write((byte)packet.Header.TriStripNodeCount);
                writer.Write(VifUtils.READ_SIZE_1);
                foreach (byte flag in vifPacket.Flags)
                {
                    writer.Write((byte)flag);
                }
            }
            ModelCommon.alignStreamToByte(binStream, 4);

            using (var writer = new BinaryWriter(binStream, System.Text.Encoding.UTF8, true))
            {
                if (singleWeightMode)
                {
                    writer.Write(VifUtils.END_VERTICES_1);
                    writer.Write(VifUtils.END_VERTICES_2);
                    writer.Write(VifUtils.END_VERTICES_3);
                    writer.Write(VifUtils.END_VERTICES_4);
                }

                // POSITIONS
                writer.Write(VifUtils.UNPACK);
                writer.Write((byte)packet.Header.VertexCoordOffset);
                writer.Write(VifUtils.UNIT_SIZE_INT);
                writer.Write((byte)packet.Header.VertexCoordCount);
                if (singleWeightMode)
                    writer.Write(VifUtils.READ_SIZE_12);
                else
                    writer.Write(VifUtils.READ_SIZE_16);
                foreach (VifCommon.BoneRelativePosition coordinate in vifPacket.PositionCoords)
                {
                    writer.Write(coordinate.Coord.X);
                    writer.Write(coordinate.Coord.Y);
                    writer.Write(coordinate.Coord.Z);
                    if (!singleWeightMode)
                    {
                        writer.Write(coordinate.Weight);
                    }
                }

                // BONE COUNTS
                writer.Write(VifUtils.UNPACK);
                writer.Write((byte)packet.Header.MatrixCountOffset);
                writer.Write(VifUtils.UNIT_SIZE_INT);
                int boneCountsSize = VifUtils.getAmountIfGroupedBy(packet.Header.MatrixCount, 4);

                if (!singleWeightMode)
                {
                    //boneCountsSize += (packet.Header.WeightGroupCount % 4 == 0) ? (packet.Header.WeightGroupCount / 4) : (packet.Header.WeightGroupCount / 4) + 1;
                    boneCountsSize += VifUtils.getAmountIfGroupedBy(packet.Header.WeightGroupCount, 4);

                    for (int i = 0; i < vifPacket.WeightGroups.Count; i++)
                    {
                        if (vifPacket.WeightGroups.Count == 0)
                            continue;

                        int groupSize = i + 1;
                        int groupCount = vifPacket.WeightGroups[i].Count;
                        int totalSize = groupSize * groupCount;

                        boneCountsSize += VifUtils.getAmountIfGroupedBy(totalSize, 4);
                    }
                }

                writer.Write((byte)boneCountsSize);
                writer.Write(VifUtils.READ_SIZE_16);

                // BONE ASSIGNS
                for (int i = 0; i < vifPacket.BoneCounts.Count; i++)
                {
                    writer.Write(vifPacket.BoneCounts[i]);
                }
                int padding = VifUtils.getAmountRemainingIfGroupedBy(vifPacket.BoneCounts.Count, 4);
                while (padding != 0)
                {
                    writer.Write(0x00000000);
                    padding--;
                }

                if (!singleWeightMode)
                {
                    // BONE WEIGHT COUNTS
                    for (int i = 0; i < vifPacket.WeightGroups.Count; i++)
                    {
                        writer.Write(vifPacket.WeightGroups[i].Count);
                    }
                    padding = VifUtils.getAmountRemainingIfGroupedBy(vifPacket.WeightGroups.Count, 4);
                    while (padding != 0)
                    {
                        writer.Write(0x00000000);
                        padding--;
                    }
                    // BONE WEIGHTS
                    for (int i = 0; i < vifPacket.WeightGroups.Count; i++) // By weight counts: [1][2][3]...
                    {
                        // Print
                        for (int j = 0; j < vifPacket.WeightGroups[i].Count; j++)
                        {
                            for (int k = 0; k < vifPacket.WeightGroups[i][j].Count; k++)
                            {
                                writer.Write(vifPacket.WeightGroups[i][j][k]);
                            }
                        }
                        // Pad to quadword
                        int groupSize = i + 1;
                        int groupCount = vifPacket.WeightGroups[i].Count;
                        int totalSize = groupSize * groupCount;
                        padding = VifUtils.getAmountRemainingIfGroupedBy(totalSize, 4);
                        while (padding != 0)
                        {
                            writer.Write(0x00000000);
                            padding--;
                        }
                    }
                }
            }

            ModelCommon.alignStreamToByte(binStream, 16);

            // Stream closure
            binStream.Position = 0;
            packet.VifCode = binStream.ToArray();
            binStream.Close();

            // DMA tags
            packet.MeshTag = new ModelCommon.DmaPacket();
            packet.MeshTag.DmaTag.Qwc = (ushort)(packet.VifCode.Length / 16);
            packet.MeshTag.DmaTag.Param = 0x3000;
            packet.MeshTag.DmaTag.Address = meshAddress;
            for (int i = 0; i < packet.BoneList.Count; i++)
            {
                ModelCommon.DmaPacket boneTag = new ModelCommon.DmaPacket();
                boneTag.DmaTag.Qwc = 4;
                boneTag.DmaTag.Param = 0x3000;
                boneTag.DmaTag.Address = packet.BoneList[i];
                boneTag.VifCode.Cmd = 1;
                boneTag.VifCode.Num = 1;
                boneTag.VifCode.Immediate = 0x0100;
                boneTag.Parameter = 0x6C048000;
                if (i == 0)
                    boneTag.Parameter += packet.Header.MatrixOffset;
                else
                    boneTag.Parameter = packet.BoneTags[i - 1].Parameter + 4;

                if ((boneTag.Parameter - 0x6C048000) > 0xff)
                {
                    throw new Exception("DMA Vif packet too big. Try making smaller packages");
                }

                packet.BoneTags.Add(boneTag);
            }

            return packet;
        }

        // Creates the final Skeletal Group form the DMA-VIF packets.
        public static ModelSkeletal.SkeletalGroup getSkeletalGroup(List<DmaVifPacket> DmaVifChain, uint textureId, int baseAddress = 0)
        {
            ModelSkeletal.SkeletalGroup group = new ModelSkeletal.SkeletalGroup();
            group.VifData = new byte[0];
            //group.Header.TextureIndex = textureId; // TODO: Figure out the texture Id
            group.Header.TextureIndex = 0;

            foreach (DmaVifPacket packet in DmaVifChain)
            {
                // Bone matrix
                if (group.BoneMatrix.Count != 0)
                    group.BoneMatrix.Add(-1);
                foreach (int boneIndex in packet.BoneList)
                {
                    group.BoneMatrix.Add(boneIndex);
                }

                // VIF code
                group.VifData = group.VifData.Concat(packet.VifCode).ToArray();

                // DMA data
                List<ModelCommon.DmaPacket> dmaData = new List<ModelCommon.DmaPacket>();
                packet.MeshTag.DmaTag.Address += baseAddress;
                dmaData.Add(packet.MeshTag);
                for (int i = 0; i < packet.BoneTags.Count; i++)
                {
                    dmaData.Add(packet.BoneTags[i]);
                }
                dmaData.Add(ModelCommon.getEndDmaPacket());

                group.DmaData.AddRange(dmaData);

                group.Header.PolygonCount += (uint)packet.Header.TriStripNodeCount;
            }

            group.Header.PolygonCount = (uint)(group.Header.PolygonCount / 3);
            group.Header.DmaPacketOffset = (uint)(baseAddress + group.VifData.Length);
            group.Header.BoneMatrixOffset = (uint)(group.Header.DmaPacketOffset + group.DmaData.Count * 16);
            group.Header.PacketLength = (uint)group.DmaData.Count;

            return group;
        }

        public static int getGroupSize(ModelSkeletal.SkeletalGroup group)
        {
            int size = 0;

            size += group.VifData.Length;
            size += group.DmaData.Count * 16;
            size += (1 + group.BoneMatrix.Count) * 4;
            int alignTo16 = 16 - (size % 16);
            if (alignTo16 != 16)
                size += alignTo16;

            return size;
        }
    }
}
