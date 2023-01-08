using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2.Models.VIF
{
    /*
     * A VIF mesh has to be divided in a group of packets, due to memory limitations.
     * 
     */
    public class VifPacket
    {
        public List<VifCommon.UVCoord> UvCoords;
        public List<byte> PositionIds;
        public List<byte> Flags;
        public List<VifCommon.BoneRelativePosition> PositionCoords;
        public List<int> BoneList;
        public List<int> BoneCounts;
        public List<List<List<int>>> WeightGroups; // List by weight count (Eg: 1 weight, 2 weights, 3 weights...) - list by entries (Eg: Weight group for vertex 1, WG for vertex 2...) - list of entry's weights (Eg: This weight group has coords 1, 2 & 3)
        public Dictionary<int, List<WeightGroup>> WeightGroupsByWeightCount;

        public VifPacket()
        {
            UvCoords = new List<VifCommon.UVCoord>();
            PositionIds = new List<byte>();
            Flags = new List<byte>();
            PositionCoords = new List<VifCommon.BoneRelativePosition>();
            BoneList = new List<int>();
            BoneCounts = new List<int>();
            WeightGroups = new List<List<List<int>>>();
            WeightGroupsByWeightCount = new Dictionary<int, List<WeightGroup>>();
        }

        // Weight group associated to a vertex
        public class WeightGroup
        {
            public List<int> PositionCoordIds; // Identifiers to positions relative to bones

            public WeightGroup()
            {
                PositionCoordIds = new List<int>();
            }
        }

        public bool isSingleWeight()
        {
            return (WeightGroups.Count <= 1);
        }

        public int getPacketLengthInVPUWithBones()
        {
            return getPacketLengthInVPU() + (BoneCounts.Count * 4);
        }

        public int getPacketLengthInVPU()
        {
            int packetLength = 4; // Header

            packetLength += UvCoords.Count; // 1 line per vertex (Uv, posId, flag)

            packetLength += PositionCoords.Count; // 1 per position

            int boneCount = 0;
            if (BoneCounts.Count == 0)
            {
                boneCount = VifUtils.getAmountIfGroupedBy(BoneCounts.Count, 4); // 1 for every 4 bone count
            }
            else
            {
                List<int> bonesUsed = new List<int>();
                foreach (VifCommon.BoneRelativePosition position in PositionCoords)
                {
                    if (!bonesUsed.Contains(position.BoneIndex))
                        bonesUsed.Add(position.BoneIndex);
                }
                boneCount = bonesUsed.Count;
            }
            packetLength += VifUtils.getAmountIfGroupedBy(boneCount, 4); // 1 for every 4 bone count

            packetLength += VifUtils.getAmountIfGroupedBy(WeightGroups.Count, 4); // 1 for every 4 weight groups

            for (int i = 0; i < WeightGroups.Count; i++) // 1 for every 4 position ids
            {
                packetLength += VifUtils.getAmountIfGroupedBy(WeightGroups[i].Count * (i + 1), 4);
            }

            //packetLength += colorCount; // 1 per vertex color; not implemented

            return packetLength;
        }

        public void createWeightGroupsUpTo(int weightGroupCount)
        {
            if (WeightGroups.Count < weightGroupCount)
            {
                int groupsToCreate = weightGroupCount - WeightGroups.Count;
                for (int j = 0; j < groupsToCreate; j++)
                {
                    WeightGroups.Add(new List<List<int>>());
                    WeightGroupsByWeightCount.Add(WeightGroupsByWeightCount.Count + 1, new List<WeightGroup>());
                }
            }
        }

        public int findPositionId(List<VifCommon.BoneRelativePosition> posCoords)
        {
            List<List<int>> myWeightCountList = WeightGroups[posCoords.Count - 1];

            int baseIndex = 0;
            for (int i = 0; i < posCoords.Count - 1; i++)
            {
                baseIndex += WeightGroups[i].Count;
            }

            for (int i = 0; i < myWeightCountList.Count; i++) // For each entry that has the same weight count as me
            {
                List<int> entry = myWeightCountList[i];
                List<VifCommon.BoneRelativePosition> currentWeightGroup = new List<VifCommon.BoneRelativePosition>();
                foreach (int posId in entry)
                {
                    currentWeightGroup.Add(PositionCoords[posId]);
                }

                bool isSamePos = true;
                for (int j = 0; j < posCoords.Count; j++)
                {
                    if (!(posCoords[j].isSamePositionAs(currentWeightGroup[j])))
                    {
                        isSamePos = false;
                    }
                }

                if (isSamePos)
                {
                    return baseIndex + i;
                }
            }

            return -1;
        }
        public int findPositionIdByWeightCount(List<VifCommon.BoneRelativePosition> posCoords)
        {
            List<WeightGroup> weightGroupList = WeightGroupsByWeightCount[posCoords.Count];

            int baseIndex = 0;
            for (int i = 1; i < posCoords.Count; i++)
            {
                baseIndex += WeightGroupsByWeightCount[i].Count;
            }

            for (int i = 0; i < weightGroupList.Count; i++)
            {
                WeightGroup entry = weightGroupList[i];
                List<VifCommon.BoneRelativePosition> currentWeightGroup = new List<VifCommon.BoneRelativePosition>();
                foreach (int posId in entry.PositionCoordIds)
                {
                    currentWeightGroup.Add(PositionCoords[posId]);
                }

                bool isSamePos = true;
                for (int j = 0; j < posCoords.Count; j++)
                {
                    if (!(posCoords[j].isSamePositionAs(currentWeightGroup[j])))
                    {
                        isSamePos = false;
                    }
                }

                if (isSamePos)
                {
                    return baseIndex + i;
                }
            }

            return -1;
        }

        // Generates the bone data based on the relative positions
        public void generateBoneData()
        {
            this.BoneList = new List<int>();
            this.BoneCounts = new List<int>();

            int currentBone = -1;
            foreach (VifCommon.BoneRelativePosition position in PositionCoords)
            {
                if (currentBone != position.BoneIndex)
                {
                    currentBone = (int)position.BoneIndex;
                    this.BoneList.Add(currentBone);
                    this.BoneCounts.Add(1);
                }
                else
                {
                    this.BoneCounts[this.BoneList.Count - 1] = this.BoneCounts[this.BoneList.Count - 1] + 1;
                }
            }
        }

        // In order to optimize space it reorders the coordinates grouped by bone, to reduce the bone DMA tags and matrices
        public void optimizeCoordSpace()
        {
            // Store old order
            Dictionary<VifCommon.BoneRelativePosition, int> positionIndexOld = new Dictionary<VifCommon.BoneRelativePosition, int>();
            for (int i = 0; i < PositionCoords.Count; i++)
            {
                positionIndexOld.Add(PositionCoords[i], i);
            }

            // Group by bone
            Dictionary<int, List<VifCommon.BoneRelativePosition>> positionsByBone = new Dictionary<int, List<VifCommon.BoneRelativePosition>>();
            for (int i = 0; i < PositionCoords.Count; i++)
            {
                VifCommon.BoneRelativePosition currentCoord = PositionCoords[i];
                if (!positionsByBone.ContainsKey(currentCoord.BoneIndex))
                {
                    positionsByBone.Add(currentCoord.BoneIndex, new List<VifCommon.BoneRelativePosition>());
                }
                positionsByBone[currentCoord.BoneIndex].Add(currentCoord);
            }

            // Make new list
            List<VifCommon.BoneRelativePosition> positionCoordsNew = new List<VifCommon.BoneRelativePosition>();
            foreach (int boneIndex in positionsByBone.Keys)
            {
                positionCoordsNew.AddRange(positionsByBone[boneIndex]);
            }

            // Get new order
            Dictionary<VifCommon.BoneRelativePosition, int> positionIndexNew = new Dictionary<VifCommon.BoneRelativePosition, int>();
            for (int i = 0; i < positionCoordsNew.Count; i++)
            {
                positionIndexNew.Add(positionCoordsNew[i], i);
            }

            PositionCoords = new List<VifCommon.BoneRelativePosition>(positionIndexNew.Keys);

            // Index equivalence
            Dictionary<int, int> oldIndexToNew = new Dictionary<int, int>();
            foreach (VifCommon.BoneRelativePosition oldPos in positionIndexOld.Keys)
            {
                oldIndexToNew.Add(positionIndexOld[oldPos], positionIndexNew[oldPos]);
            }

            // Set weight indices to new value
            foreach (List<List<int>> weightGroupByCount in WeightGroups)
            {
                foreach (List<int> weightGroup in weightGroupByCount)
                {
                    for (int i = 0; i < weightGroup.Count; i++)
                    {
                        weightGroup[i] = oldIndexToNew[weightGroup[i]];
                    }
                }
            }
            foreach(List<WeightGroup> weightGroupList in WeightGroupsByWeightCount.Values)
            {
                foreach (WeightGroup weightGroup in weightGroupList)
                {
                    for (int i = 0; i < weightGroup.PositionCoordIds.Count; i++)
                    {
                        weightGroup.PositionCoordIds[i] = oldIndexToNew[weightGroup.PositionCoordIds[i]];
                    }
                }
            }
        }

        // In case that single weight is used, the indices must point directly to the coords
        public void fixIndicesForSingleWeight()
        {
            List<List<int>> weightGroupCount0 = WeightGroups[0];
            List<WeightGroup> weightGroupCount0_ = WeightGroupsByWeightCount[1];
            for (int i = 0; i < PositionIds.Count; i++)
            {
                List<int> myWeightGroup = weightGroupCount0[PositionIds[i]];
                //WeightGroup myWeightGroup_ = weightGroupCount0_[PositionIds[i]];
                PositionIds[i] = (byte)myWeightGroup[0];

                //PositionIds[i] = (byte)myWeightGroup_.PositionCoordIds[0];
            }
        }
    }
}
