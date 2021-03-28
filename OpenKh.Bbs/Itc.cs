using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xe.BinaryMapper;
using OpenKh.Common;

namespace OpenKh.Bbs
{
    public class Itc
    {
        private const uint MagicCode = 0x435449;
        private const ushort version = 1;

        public class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public ushort Version { get; set; }
            [Data] public ushort Padding1 { get; set; }
            [Data] public ushort ItemsTotal { get; set; }
            [Data] public ushort Padding { get; set; }
            [Data] public byte ItemCountDP { get; set; }
            [Data] public byte ItemCountSW { get; set; }
            [Data] public byte ItemCountCD { get; set; }
            [Data] public byte ItemCountSB { get; set; }
            [Data] public byte ItemCountYT { get; set; }
            [Data] public byte ItemCountRG { get; set; }
            [Data] public byte ItemCountJB { get; set; }
            [Data] public byte ItemCountHE { get; set; }
            [Data] public byte ItemCountLS { get; set; }
            [Data] public byte ItemCountDI { get; set; }
            [Data] public byte ItemCountPP { get; set; }
            [Data] public byte ItemCountDC { get; set; }
            [Data] public byte ItemCountKG { get; set; }
            [Data] public byte ItemCountVS { get; set; }
            [Data] public byte ItemCountBD { get; set; }
            [Data] public byte ItemCountWM { get; set; }
        }

        public class ITCData
        {
            [Data] public ushort CollectionID { get; set; }
            [Data] public ushort ItemID { get; set; }
            [Data] public byte WorldID { get; set; }
            [Data] public byte Padding1 { get; set; }
            [Data] public byte Padding2 { get; set; }
            [Data] public byte Padding3 { get; set; }
        }

        public Header header = new Header();
        public List<ITCData> AllITC = new List<ITCData>();

        public static Itc Read(Stream stream)
        {
            Itc itc = new Itc();

            itc.header = BinaryMapping.ReadObject<Header>(stream);

            itc.AllITC = new List<ITCData>();

            for (int i = 0; i < itc.header.ItemsTotal; i++)
            {
                itc.AllITC.Add(BinaryMapping.ReadObject<ITCData>(stream));
            }

            return itc;
        }

        public static void Write(Stream stream, Itc itc)
        {
            BinaryMapping.WriteObject<Header>(stream, itc.header);

            for (int i = 0; i < itc.header.ItemsTotal; i++)
            {
                BinaryMapping.WriteObject<ITCData>(stream, itc.AllITC[i]);
            }
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 0x1C &&
            stream.SetPosition(0).ReadUInt32() == MagicCode &&
            stream.SetPosition(0).ReadUInt16() == version;
    }
}
