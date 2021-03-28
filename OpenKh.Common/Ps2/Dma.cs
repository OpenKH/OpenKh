using Xe.BinaryMapper;

namespace OpenKh.Common.Ps2
{
    /// <summary>
    /// EE User Manual, 6.3.2
    /// </summary>
    public enum VifOpcode : byte
    {
        NOP = 0b00000000,
        STCYCL = 0b00000001,
        OFFSET = 0b00000010,
        BASE = 0b00000011,
        ITOP = 0b00000100,
        STMOD = 0b00000101,
        MSKPATH3 = 0b00000110,
        MARK = 0b00000111,
        FLUSHE = 0b00010000,
        FLUSH = 0b00010001,
        FLUSHA = 0b00010011,
        MSCAL = 0b00010100,
        MSCALF = 0b00010101,
        MSCNT = 0b00010111,
        STMASK = 0b00100000,
        STROW = 0b00110000,
        STCOL = 0b00110001,
        MPG = 0b01001010,
        DIRECT = 0b01010000,
        DIRECTH = 0b01010001
    }

    /// <summary>
    /// EE User Manual, 6.3.2
    /// </summary>
    public class VifCode
    {
        [Data] public byte Cmd { get; set; }
        [Data] public byte Num { get; set; }
        [Data] public ushort Immediate { get; set; }

        public VifOpcode Opcode
        {
            get => (VifOpcode)(Cmd & 7);
            set => Cmd = (byte)((byte)value | (Interrupt ? 0x80 : 0));
        }

        public bool Interrupt
        {
            get => (Cmd >> 7) != 0;
            set => Cmd = (byte)((byte)Opcode | (value ? 0x80 : 0));
        }
    }

    /// <summary>
    /// EE User Manual, 5.6
    /// </summary>
    public class DmaTag
    {
        /// <summary>
        /// Quadword count; packet size
        /// </summary>
        [Data] public ushort Qwc { get; set; }
        [Data] public ushort Param { get; set; }
        [Data] public int Address { get; set; }

        public int TagId
        {
            get => (Param >> 12) & 7;
            set => Param = (ushort)(((value & 7) << 12) | (Irq ? 0x8000 : 0));
        }

        public bool Irq
        {
            get => (Param >> 15) != 0;
            set => Param = (ushort)((TagId << 12) | (value ? 0x8000 : 0));
        }
    }
}
