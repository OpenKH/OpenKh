using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;
using static OpenKh.Kh2.Models.ModelCommon;

namespace OpenKh.Kh2.Models
{
    public class ModelShadow
    {
        [Data] public ModelHeader ModelHeader { get; set; }
        [Data] public short BoneCount { get; set; }
        [Data] public short TextureCount { get; set; } // NOT USED IN SHADOW
        [Data] public int BoneOffset { get; set; } // NOT USED IN SHADOW
        [Data] public int BoneDataOffset { get; set; } // NOT USED IN SHADOW
        [Data] public int GroupCount { get; set; }
        public List<ShadowGroup> Groups { get; set; }
        public List<Bone> Bones { get; set; } // Inherited from skeletal model

        public class ShadowGroup
        {
            public ShadowGroupHeader Header { get; set; }
            public byte[] VifData { get; set; }
            public List<DmaPacket> DmaData { get; set; }
            public List<int> BoneMatrix { get; set; }

            public ModelSkeletal.SkeletalMesh Mesh { get; set; } // Processed data

            public ShadowGroup()
            {
                Header = new ShadowGroupHeader();
                DmaData = new List<DmaPacket>();
                BoneMatrix = new List<int>();
            }
        }
        public class ShadowGroupHeader
        {
            [Data] public int AttributesBitfield { get; set; }
            [Data] public uint TextureIndex { get; set; } // NOT USED IN SHADOW
            [Data] public uint PolygonCount { get; set; }
            [Data] public ushort HasVertexBuffer { get; set; } // NOT USED IN SHADOW
            [Data] public ushort Alternative { get; set; } // NOT USED IN SHADOW
            [Data] public uint DmaPacketOffset { get; set; }
            [Data] public uint BoneMatrixOffset { get; set; }
            [Data] public uint PacketLength { get; set; }
            [Data] public uint Reserved { get; set; }
        }

        /* ShadowGroup attributes bitfield
         * reserved [22]
         * mesh [5]
         * part [5]
         */

        //-------------
        // CONSTRUCTORS
        //-------------

        public ModelShadow() { }

        public static ModelShadow Read(Stream stream, List<Bone> Bones)
        {
            long initialStreamPosition = stream.Position;

            // Header
            ModelShadow model = BinaryMapping.ReadObject<ModelShadow>(stream);

            // Group headers
            model.Groups = new List<ShadowGroup>();
            for (int i = 0; i < model.GroupCount; i++)
            {
                ShadowGroup group = new ShadowGroup();
                group.Header = BinaryMapping.ReadObject<ShadowGroupHeader>(stream);
                model.Groups.Add(group);
            }

            // Group data
            foreach (ShadowGroup group in model.Groups)
            {
                // Padding to 16 bytes
                /*if (stream.Position % 16 != 0)
                {
                    stream.Position += (16 - (stream.Position % 16));
                }*/
                ModelCommon.alignStreamToByte(stream, 16);

                int vifLength = (int)(initialStreamPosition + group.Header.DmaPacketOffset) - (int)stream.Position;
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

                // Note: VPU packets don't have vertices in shadow models
                //group.Mesh = getMeshFromGroup(group, GetBoneMatrices(model.Bones));
            }

            return model;
        }
        public void Write(Stream stream)
        {
            BinaryMapping.WriteObject(stream, this);
            foreach (ShadowGroup group in Groups)
            {
                BinaryMapping.WriteObject(stream, group.Header);
            }
            foreach (ShadowGroup group in Groups)
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
        }

        //----------
        // FUNCTIONS
        //----------

        // Gets the mesh from a given skeletal group
        public static ModelSkeletal.SkeletalMesh getMeshFromGroup(ShadowGroup group, Matrix4x4[] boneMatrices)
        {
            // Get the VIF-DMA-boneMatrix packets
            List<ModelSkeletal.DmaVifPacket> dmaVifPackets = GetDmaVifPackets(group);

            // Unpacks the VIF packets to VPU data
            List<OpenKh.Ps2.VpuPacket> vpuPackets = new List<OpenKh.Ps2.VpuPacket>();
            foreach (ModelSkeletal.DmaVifPacket dmaVifPacket in dmaVifPackets)
            {
                vpuPackets.Add(ModelSkeletal.unpackVifToVpu(dmaVifPacket.VifPacket));
            }

            // Processes the unpacked data
            ModelSkeletal.VpuGroup vpuGroup = ModelSkeletal.getVpuGroup(vpuPackets, dmaVifPackets);

            // Generates the mesh
            return ModelSkeletal.GetSkeletalMeshFromVpuGroup(vpuGroup, boneMatrices);
        }

        // Returns the DmaVifPackets that form the group
        public static List<ModelSkeletal.DmaVifPacket> GetDmaVifPackets(ShadowGroup group)
        {
            List<ModelSkeletal.DmaVifPacket> packets = new List<ModelSkeletal.DmaVifPacket>();

            // DMA Set
            ModelSkeletal.DmaVifPacket currentPacket = new ModelSkeletal.DmaVifPacket();
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
                    currentPacket = new ModelSkeletal.DmaVifPacket();
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
                    ModelSkeletal.DmaVifPacket packet = packets[i];
                    packet.VifPacket = vpuStream.ReadBytes(packet.DmaSet.StartTag.DmaTag.Qwc * 0x10);
                }
            }

            return packets;
        }
    }
}
