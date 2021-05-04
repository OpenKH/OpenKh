using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xe.BinaryMapper;

namespace OpenKh.Audio
{
    public class IopVoice
    {
        public class Header
        {
            [Data] public ulong Magic { get; set; }
            [Data] public uint Unknown { get; set; }
            [Data] public uint EntryCount { get; set; }
        }

        public class Entry
        {
            [Data] public uint Offset { get; set; }
            [Data] public uint Length { get; set; }

            public byte[] Data { get; set; }
        }
    }
}
