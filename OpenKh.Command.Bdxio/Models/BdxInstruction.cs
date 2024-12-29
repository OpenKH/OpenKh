using OpenKh.Common.Utils;

namespace OpenKh.Command.Bdxio.Models
{
    public class BdxInstruction
    {
        public ushort Code { get; set; }

        public BdxInstruction(ushort code)
        {
            Code = code;
        }

        public byte Opcode
        {
            get => (byte)BitsUtil.Int.GetBits(Code, 0, 4);
            set => Code = (ushort)BitsUtil.Int.SetBits(Code, 0, 4, value);
        }

        public byte Sub
        {
            get => (byte)BitsUtil.Int.GetBits(Code, 4, 2);
            set => Code = (ushort)BitsUtil.Int.SetBits(Code, 4, 2, value);
        }

        public ushort Ssub
        {
            get => (ushort)BitsUtil.Int.GetBits(Code, 6, 12);
            set => Code = (ushort)BitsUtil.Int.SetBits(Code, 6, 12, value);
        }
    }
}
