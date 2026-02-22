using OpenKh.Common;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{

    public class ChoiceEntry
    {
        [Data] public short Id { get; set; }
        [Data] public short MessageId { get; set; }
    }
    public class Slct
    {

        [Data] public ushort Id { get; set; }
        [Data] public byte ChoiceNum { get; set; }
        [Data] public byte ChoiceDefault { get; set; }

        [Data(Count = 4)] public ChoiceEntry[] Choice { get; set; }
        [Data] public short BaseSequence { get; set; }
        [Data] public short TitleSequence { get; set; }
        [Data] public int Information { get; set; }
        [Data] public int EntryId { get; set; }
        [Data] public int Task { get; set; }
        [Data] public byte PauseMode { get; set; }
        [Data] public byte Flag { get; set; }
        [Data] public byte SoundPause { get; set; }
        [Data(Count = 25)] public byte[] Padding { get; set; }

        public static List<Slct> Read(Stream stream) => BaseTable<Slct>.Read(stream);
        public static void Write(Stream stream, IEnumerable<Slct> entries) =>
            BaseTable<Slct>.Write(stream, 2, entries);
    }
}
