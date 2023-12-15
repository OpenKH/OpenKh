using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.Bdxio.Models
{
    public record BdxInstructionDesc
    {
        public ushort Code { get; set; }
        public ushort CodeMask { get; set; }

        public string Name { get; set; } = "";
        public bool IsSyscall { get; set; }
        public bool IsGosub { get; set; }
        public bool IsJump { get; set; }
        public bool IsJumpConditional { get; set; }
        public bool NeverReturn { get; set; }
        public bool IsGosubRet { get; set; }
        public bool CodeRevealerLabeling { get; set; }
        public int CodeSize { get; set; }
        public Arg[] Args { get; set; } = new Arg[0];
        public string[] OldNames { get; set; } = new string[0];

        public override string ToString() => Name;

        public record Arg
        {
            public string Name { get; set; } = "";
            public ArgType Type { get; set; }

            /// <summary>
            /// Is relative offset to bdx instruction
            /// </summary>
            public bool IsRelative { get; set; }
            public bool AiPos { get; set; }
            public bool WorkPos { get; set; }
        }

        public enum ArgType
        {
            Ssub,
            Imm16,
            Imm32,
            Float32,
        }
    }
}
