using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Kh2.Models.VIF
{
    public class VifUtils
    {
        // VIF CONSTANTS
        public const byte READ_SIZE_16 = 0x6C;
        public const byte READ_SIZE_12 = 0x78;
        public const byte READ_SIZE_4 = 0x65;
        public const byte READ_SIZE_4_2 = 0x6E;
        public const byte READ_SIZE_1 = 0x72;

        public const byte UNIT_SIZE_INT = 0x80;
        public const byte UNIT_SIZE_BYTE = 0xC0;

        // VIF CODES
        public const uint  UNPACK = 0x01000101;
        public const ulong END_UV = 0xCFCFCFCF20000000;
        public const ulong END_INDICES = 0x3F3F3F3F20000000;
        public const ulong END_VERTICES_1 = 0x3F80000031000000;
        public const ulong END_VERTICES_2 = 0x3F8000003F800000;
        public const ulong END_VERTICES_3 = 0x200000003F800000;
        public const uint  END_VERTICES_4 = 0x80808080;
        public const ulong END_BONE_DATA = 0x0000000000000000;
        public const uint  DMA_BONE_PARAM0 = 0x01000101;

        public static bool isSameVector(Vector4 vec1, Vector4 vec2)
        {
            return (vec1.X == vec2.X &&
                    vec1.Y == vec2.Y &&
                    vec1.Z == vec2.Z &&
                    vec1.W == vec2.W);
        }
        public static bool isSameVector(VifCommon.BoneRelativePosition vec1, Vector4 vec2)
        {
            return (vec1.Coord.X == vec2.X &&
                    vec1.Coord.Y == vec2.Y &&
                    vec1.Coord.Z == vec2.Z &&
                    vec1.BoneIndex == vec2.W);
        }

        // Returns the amount of groups needed to group the entries in groups of the given size
        public static int getAmountIfGroupedBy(int entryCount, int groupSize)
        {
            if (entryCount == 0)
                return 0;

            int amount = entryCount / groupSize;

            if (entryCount % groupSize != 0)
                amount++;

            return amount;
        }

        // Returns the amount of entries left to fill the last group when grouping the entries in groups of the given size
        public static int getAmountRemainingIfGroupedBy(int entryCount, int groupSize)
        {
            int overflow = entryCount % groupSize;

            if (overflow == 0)
                return 0;

            return groupSize - overflow;
        }

        // Returns the base address of an Skeletal group within the file
        public static int calcBaseAddress(int boneCount, int groupCount)
        {
            int currentCount = 0;

            // Header
            currentCount += 32;

            // Group headers
            currentCount += groupCount * 32;

            // Bone data
            currentCount += 0x120;

            // Bones
            currentCount += boneCount * 64;

            return currentCount;
        }
    }
}
