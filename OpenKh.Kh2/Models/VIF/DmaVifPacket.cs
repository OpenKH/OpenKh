using OpenKh.Ps2;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2.Models.VIF
{
    /*
     * A DMA-VIF Packet contains a part of a mesh. Due to memory limitations the mesh has to be divided in VIF packets.
     * To load the packets from memory, DMA (Direct Memory Access) tags are used.
     * Each packet has a piece of VIF code, the DMA tags for the mesh and the bones used, and the list of said bones.
     */
    public class DmaVifPacket
    {
        public VpuPacket.VpuHeader Header;
        public byte[] VifCode;
        public ModelCommon.DmaPacket MeshTag;
        public List<ModelCommon.DmaPacket> BoneTags;
        public List<int> BoneList;

        public DmaVifPacket()
        {
            Header = new VpuPacket.VpuHeader();
            VifCode = new byte[0];
            MeshTag = new ModelCommon.DmaPacket();
            BoneTags = new List<ModelCommon.DmaPacket>();
            BoneList = new List<int>();
        }

        public void calcHeader(VifPacket vifPacket, bool singleWeightMode = false, bool hasColors = false, bool hasNormals = false)
        {
            Header.Type = 1;
            Header.VertexColorPtrInc = 0;
            Header.MagicNumber = 0;
            Header.VertexBufferPointer = 0;

            int currentAddress = 0;
            Header.TriStripNodeOffset = hasNormals ? 5 : 4;
            Header.TriStripNodeCount = vifPacket.UvCoords.Count;

            currentAddress += Header.TriStripNodeOffset;
            currentAddress += Header.TriStripNodeCount;

            if(hasColors)
            {
                Header.ColorOffset = currentAddress;
                Header.ColorCount = vifPacket.Colors.Count;
                currentAddress += Header.ColorCount;
            }

            if (hasNormals)
            {
                Header.NormalOffset = currentAddress;
                Header.NormalCount = vifPacket.Normals.Count;
                currentAddress += Header.NormalCount;
            }

            Header.VertexCoordOffset = currentAddress;
            //Header.VertexCoordCount = vifPacket.PositionCoordsVector.Count;
            Header.VertexCoordCount = vifPacket.PositionCoords.Count;

            currentAddress += Header.VertexCoordCount;
            Header.MatrixCountOffset = currentAddress;

            currentAddress += (BoneList.Count % 4 == 0) ? (BoneList.Count / 4) : (BoneList.Count / 4) + 1;

            if (!singleWeightMode)
            {
                Header.WeightGroupCount = vifPacket.WeightGroups.Count;
                Header.WeightGroupCountOffset = currentAddress;
                currentAddress += (Header.WeightGroupCount % 4 == 0) ? (Header.WeightGroupCount / 4) : (Header.WeightGroupCount / 4) + 1;
                Header.VertexIndexOffset = currentAddress;

                int weightEntryCount = 0;
                for (int i = 0; i < vifPacket.WeightGroups.Count; i++) // Weight count list
                {
                    for (int j = 0; j < vifPacket.WeightGroups[i].Count; j++) // Weight list
                    {
                        weightEntryCount++;
                    }
                    currentAddress += (weightEntryCount % 4 == 0) ? (weightEntryCount / 4) : (weightEntryCount / 4) + 1;
                }
            }
            else
            {
                Header.WeightGroupCount = 0;
                Header.WeightGroupCountOffset = 0;
                Header.VertexIndexOffset = 0;
            }

            Header.MatrixOffset = currentAddress;
            Header.MatrixCount = BoneList.Count;
        }
    }
}
