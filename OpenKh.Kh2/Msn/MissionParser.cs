using OpenKh.Kh2.Ard;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Kh2.Msn
{
    public static class MissionParser
    {
        public static string Decompile(IEnumerable<Mission.Function> functions)
        {
            var sb = new StringBuilder();
            foreach(var function in functions)
            {
                
            }

            return sb.ToString();
        }

        public static string AsText(Mission.Function function)
        {
            var p = function.Parameters;
            switch (function.Opcode)
            {
                case Mission.Operation.StartEvent:
                    return $"StartEvent AnimLoader 0x{(byte)((p[0] >> 0) & 0xff):x02} Seqd 0x{(byte)((p[0] >> 8) & 0xff):x02} Object {(byte)((p[0] >> 16) & 0xff)}";
            }
        }
    }
}
