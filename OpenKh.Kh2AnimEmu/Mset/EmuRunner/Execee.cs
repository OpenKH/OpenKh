using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace OpenKh.Kh2Anim.Mset.EmuRunner
{
    public class ExeceeException : Exception
    {
        public ExeceeException() : base() { }
        public ExeceeException(string text) : base(text) { }
    }
    public class TraceDiffException : ExeceeException
    {
        public TraceDiffException(string what, GPR v1, GPR v2) : base("The trace diff hit: " + what + " --- " + HexFmtUt.Format(v1) + " ≠ " + HexFmtUt.Format(v2)) { }
        public TraceDiffException(string what, uint v1, uint v2) : base("The trace diff hit: " + what + " --- " + v1.ToString("x8") + " ≠ " + v2.ToString("x8")) { }
        public TraceDiffException(string what, Vec v1, Vec v2) : base("The trace diff hit: " + what + " --- " + HexFmtUt.Format(v1) + " ≠ " + HexFmtUt.Format(v2)) { }
    }
    public class RecfnnotFound : ExeceeException
    {
        public RecfnnotFound(uint addr) : base("Recfn not found: " + addr.ToString("X8")) { }
        public RecfnnotFound(uint addr, string rc) : base("Recfn not found: " + rc + " " + addr.ToString("X8")) { }
    }
    public class AccessUnpreparedMemoryException : ExeceeException
    {
        public AccessUnpreparedMemoryException(uint addr) : base("Access to undefined memory space: " + addr.ToString("X8")) { }
    }
    public class RecProcIsTooLong : ExeceeException
    {
        public RecProcIsTooLong(int cnt) : base("Recompiler hits limit: " + cnt + " > max") { }
    }
    public class IsR0Exception : Exception
    {
    }

    class HexFmtUt
    {
        public static string Format(Vec v)
        {
            MemoryStream os = new MemoryStream(16);
            BinaryWriter wr = new BinaryWriter(os);
            wr.Write(v.x);
            wr.Write(v.y);
            wr.Write(v.z);
            wr.Write(v.w);
            os.Position = 0;
            BinaryReader br = new BinaryReader(os);
            uint x = br.ReadUInt32();
            uint y = br.ReadUInt32();
            uint z = br.ReadUInt32();
            uint w = br.ReadUInt32();
            return string.Format("{0:x8}_{1:x8}_{2:x8}_{3:x8}", w, z, y, x);
        }

        public static string Format(GPR o)
        {
            return string.Format("{0:x8}_{1:x8}_{2:x8}_{3:x8}", o.UL[3], o.UL[2], o.UL[1], o.UL[0]);
        }
    }
}
