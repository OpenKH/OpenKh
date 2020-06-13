using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class Cond
    {
        [Data] public ushort Id { get; set; }
        [Data] public short Reward { get; set; } //either item from 03system or shop upgrades
        [Data] public byte Unk4 { get; set; }
        [Data] public byte Unk5 { get; set; }
        [Data] public byte Unk6 { get; set; }
        [Data] public byte Unk7 { get; set; }
        [Data] public short Count { get; set; }
        [Data] public short UnkA { get; set; }
    }
}
