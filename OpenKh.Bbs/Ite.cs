using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xe.BinaryMapper;
using OpenKh.Common;

namespace OpenKh.Bbs
{
    public class Ite
    {
        private const uint MagicCode = 0x455449;
        private const ushort version = 1;

        public class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public ushort Version { get; set; }
            [Data] public ushort Padding1 { get; set; }
            [Data] public ushort WeaponDataCount { get; set; }
            [Data] public ushort FlavorDataCount { get; set; }
            [Data] public ushort KeyItemDataCount { get; set; }
            [Data] public ushort KeyItemHideDataCount { get; set; }
            [Data] public ushort SynthesisDataCount { get; set; }
            [Data] public ushort Padding { get; set; }
        }

        public class ITEData
        {
            [Data] public ushort ItemID { get; set; }
            [Data] public byte Padding1 { get; set; }
            [Data] public byte Padding2 { get; set; }
        }

        public Header header;
        public List<ITEData> WeaponList = new List<ITEData>();
        public List<ITEData> FlavorList = new List<ITEData>();
        public List<ITEData> KeyItemList = new List<ITEData>();
        public List<ITEData> KeyItemHideList = new List<ITEData>();
        public List<ITEData> SynthesisList = new List<ITEData>();

        public static Ite Read(Stream stream)
        {
            Ite ite = new Ite();

            ite.header = BinaryMapping.ReadObject<Header>(stream);

            // Weapon.
            for (int i = 0; i < ite.header.WeaponDataCount; i++)
            {
                ite.WeaponList.Add(BinaryMapping.ReadObject<ITEData>(stream));
            }

            // Flavor.
            for (int i = 0; i < ite.header.FlavorDataCount; i++)
            {
                ite.FlavorList.Add(BinaryMapping.ReadObject<ITEData>(stream));
            }

            // Key Item.
            for (int i = 0; i < ite.header.KeyItemDataCount; i++)
            {
                ite.KeyItemList.Add(BinaryMapping.ReadObject<ITEData>(stream));
            }

            // Key Item Hide.
            for (int i = 0; i < ite.header.KeyItemHideDataCount; i++)
            {
                ite.KeyItemHideList.Add(BinaryMapping.ReadObject<ITEData>(stream));
            }

            // Synthesis.
            for (int i = 0; i < ite.header.SynthesisDataCount; i++)
            {
                ite.SynthesisList.Add(BinaryMapping.ReadObject<ITEData>(stream));
            }

            return ite;
        }

        public static void Write(Stream stream, Ite ite)
        {
            BinaryMapping.WriteObject<Header>(stream, ite.header);

            // Weapon.
            for (int i = 0; i < ite.header.WeaponDataCount; i++)
            {
                BinaryMapping.WriteObject<ITEData>(stream, ite.WeaponList[i]);
            }

            // Flavor.
            for (int i = 0; i < ite.header.FlavorDataCount; i++)
            {
                BinaryMapping.WriteObject<ITEData>(stream, ite.FlavorList[i]);
            }

            // Key Item.
            for (int i = 0; i < ite.header.KeyItemDataCount; i++)
            {
                BinaryMapping.WriteObject<ITEData>(stream, ite.KeyItemList[i]);
            }

            // Key Item Hide.
            for (int i = 0; i < ite.header.KeyItemHideDataCount; i++)
            {
                BinaryMapping.WriteObject<ITEData>(stream, ite.KeyItemHideList[i]);
            }

            // Synthesis.
            for (int i = 0; i < ite.header.SynthesisDataCount; i++)
            {
                BinaryMapping.WriteObject<ITEData>(stream, ite.SynthesisList[i]);
            }
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 0x1C &&
            stream.SetPosition(0).ReadUInt32() == MagicCode &&
            stream.SetPosition(0).ReadUInt16() == version;
    }
}
