using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class Leve
    {
        [Data] public ushort Id { get; set; }
        [Data] public ushort Unk2 { get; set; }
        [Data] public ushort Unk4 { get; set; }
        [Data] public ushort Unk6 { get; set; }
        [Data] public int Exp { get; set; }
    }
}
