using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Kh2.Models.VIF
{
    public class VifUtils
    {
        // VIF CODES
        public const uint UNPACK = 0x01000101;

        // First number is the amount, second number is the size being read. M indicates using a mask
        public const byte UNPACK_TYPE_1_32 = 0x60;
        public const byte UNPACK_TYPE_1_16 = 0x61;
        public const byte UNPACK_TYPE_1_8 = 0x62;
        public const byte UNPACK_TYPE_2_32 = 0x64;
        public const byte UNPACK_TYPE_2_16 = 0x65;
        public const byte UNPACK_TYPE_2_8 = 0x66;
        public const byte UNPACK_TYPE_3_32 = 0x68;
        public const byte UNPACK_TYPE_3_16 = 0x69;
        public const byte UNPACK_TYPE_3_8 = 0x6A;
        public const byte UNPACK_TYPE_4_32 = 0x6C;
        public const byte UNPACK_TYPE_4_16 = 0x6D;
        public const byte UNPACK_TYPE_4_8 = 0x6E;
        public const byte UNPACK_TYPE_4_5 = 0x6F; // 5+5+5+1
        public const byte UNPACK_TYPE_M_1_32 = 0x70;
        public const byte UNPACK_TYPE_M_1_16 = 0x71;
        public const byte UNPACK_TYPE_M_1_8 = 0x72;
        public const byte UNPACK_TYPE_M_2_32 = 0x74;
        public const byte UNPACK_TYPE_M_2_16 = 0x75;
        public const byte UNPACK_TYPE_M_2_8 = 0x76;
        public const byte UNPACK_TYPE_M_3_32 = 0x78;
        public const byte UNPACK_TYPE_M_3_16 = 0x79;
        public const byte UNPACK_TYPE_M_3_8 = 0x7A;
        public const byte UNPACK_TYPE_M_4_32 = 0x7C;
        public const byte UNPACK_TYPE_M_4_16 = 0x7D;
        public const byte UNPACK_TYPE_M_4_8 = 0x7E;
        public const byte UNPACK_TYPE_M_4_5 = 0x7F; // 5+5+5+1

        // Unpack options:
        // First bit: TOPS is added to starting address (File address + unpack address)
        // Second bit: Zero extended (If unset sign extended)
        public const byte UNPACK_OPTIONS_SIGN = 0x80; // 1000
        public const byte UNPACK_OPTIONS_ZERO = 0xC0; // 1100

        public const uint SET_MASK = 0x20000000;
        public const uint INDICES_MASK = 0xCFCFCFCF;
        public const uint FLAGS_MASK = 0x3F3F3F3F;
        public const uint NORMALS_MASK = 0xC0C0C0C0;
        public const uint NORMALS_RESERVE_MASK = 0x3F3F3F3F;
        public const uint COORDS_MASK = 0x80808080;

        public const uint SET_COLUMN = 0x31000000;

        // VIF CONSTANTS
        public const byte READ_SIZE_16 = 0x6C;
        public const byte READ_SIZE_12 = 0x78;
        public const byte READ_SIZE_4 = 0x65;
        public const byte READ_SIZE_4_2 = 0x6E;
        public const byte READ_SIZE_1 = 0x72;


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
