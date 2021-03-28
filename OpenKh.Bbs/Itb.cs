using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xe.BinaryMapper;
using OpenKh.Common;

namespace OpenKh.Bbs
{
    public class Itb
    {
        private const uint MagicCode = 0x425449;
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

        public class ITBData
        {
            [Data] public ushort TreasureBoxID { get; set; }
            [Data] public ushort ItemID { get; set; }
            [Data] public byte ItemKind { get; set; }
            [Data] public byte WorldID { get; set; }
            [Data] public byte ReportID { get; set; }
            [Data] public byte Padding3 { get; set; }
        }

        public Header header = new Header();
        public List<ITBData> AllITB = new List<ITBData>();

        public static Itb Read(Stream stream)
        {
            Itb itb = new Itb();

            itb.header = BinaryMapping.ReadObject<Header>(stream);

            itb.AllITB = new List<ITBData>();

            for (int i = 0; i < itb.header.ItemsTotal; i++)
            {
                itb.AllITB.Add(BinaryMapping.ReadObject<ITBData>(stream));
            }

            return itb;
        }

        public static void Write(Stream stream, Itb itb)
        {
            BinaryMapping.WriteObject<Header>(stream, itb.header);

            for (int i = 0; i < itb.header.ItemsTotal; i++)
            {
                BinaryMapping.WriteObject<ITBData>(stream, itb.AllITB[i]);
            }
        }

        public static bool IsValid(Stream stream) =>
            stream.Length >= 0x1C &&
            stream.SetPosition(0).ReadUInt32() == MagicCode &&
            stream.SetPosition(0).ReadUInt16() == version;
    }
}
