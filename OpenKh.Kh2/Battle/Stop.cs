using System;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Battle
{
    public class Stop
    {
        [Flags]
        public enum Flag : uint
        {
            Exist = 0x01,
            DisableDamageReaction = 0x02,
            Star = 0x04,
            DisableDraw = 0x08,
        }
        [Data] public ushort Id { get; set; }
        [Data] public Flag Flags { get; set; }

        public static List<Stop> Read(Stream stream) => BaseTable<Stop>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Stop> items) =>
            BaseTable<Stop>.Write(stream, 2, items);
    }
}
