using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Command.Bdxio.Models.BdxInstructionDesc;

namespace OpenKh.Command.Bdxio.Models
{
    public class BdxInstructionDescs
    {
        public static IEnumerable<BdxInstructionDesc> GetDescs() => _descs;

        public static BdxInstructionDesc? FindByCode(ushort code)
        {
            return _descs
                .FirstOrDefault(it => (code & it.CodeMask) == it.Code);
        }

        private static readonly BdxInstructionDesc[] _descs = new[]
        {
            #region Generated

			new BdxInstructionDesc { Code = 0x0000, CodeMask = 0x003F, Name = "push", CodeRevealerLabeling = true, CodeSize = 3, Args = new[] { new Arg { Name = "imm32", Type = ArgType.Imm32 } }, OldNames = new[] { "pushImm" } },
            new BdxInstructionDesc { Code = 0x0010, CodeMask = 0x003F, Name = "push.s", CodeSize = 3, Args = new[] { new Arg { Name = "float32", Type = ArgType.Float32 } }, OldNames = new[] { "pushImmf" } },
            new BdxInstructionDesc { Code = 0x0020, CodeMask = 0xFFFF, Name = "push.sp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } }, OldNames = new[] { "pushFromPSp" } },
            new BdxInstructionDesc { Code = 0x0060, CodeMask = 0xFFFF, Name = "push.wp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, WorkPos = true } }, OldNames = new[] { "pushFromPWp" } },
            new BdxInstructionDesc { Code = 0x00A0, CodeMask = 0xFFFF, Name = "push.sp.d", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } }, OldNames = new[] { "pushFromPSpVal" } },
            new BdxInstructionDesc { Code = 0x00E0, CodeMask = 0xFFFF, Name = "push.bd", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, AiPos = true } }, OldNames = new[] { "pushFromPAi" } },
            new BdxInstructionDesc { Code = 0x0030, CodeMask = 0xFFFF, Name = "push.d.sp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } }, OldNames = new[] { "pushFromFSp" } },
            new BdxInstructionDesc { Code = 0x0070, CodeMask = 0xFFFF, Name = "push.d.wp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, WorkPos = true } }, OldNames = new[] { "pushFromFWp" } },
            new BdxInstructionDesc { Code = 0x00B0, CodeMask = 0xFFFF, Name = "push.d.sp.d", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } }, OldNames = new[] { "pushFromFSpVal" } },
            new BdxInstructionDesc { Code = 0x00F0, CodeMask = 0xFFFF, Name = "push.d.bd", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, AiPos = true } }, OldNames = new[] { "pushFromFAi" } },
            new BdxInstructionDesc { Code = 0x0001, CodeMask = 0xFFCF, Name = "pop.sp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } }, OldNames = new[] { "popToSp" } },
            new BdxInstructionDesc { Code = 0x0041, CodeMask = 0xFFCF, Name = "pop.wp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, WorkPos = true } }, OldNames = new[] { "popToWp" } },
            new BdxInstructionDesc { Code = 0x0081, CodeMask = 0xFFCF, Name = "pop.sp.d", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } }, OldNames = new[] { "popToSpVal" } },
            new BdxInstructionDesc { Code = 0x00C1, CodeMask = 0xFFCF, Name = "pop.bd", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, AiPos = true } }, OldNames = new[] { "popToAi" } },
            new BdxInstructionDesc { Code = 0x0002, CodeMask = 0xFFCF, Name = "memcpy.sp", CodeSize = 3, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 }, new Arg { Name = "imm16_2", Type = ArgType.Imm16 } }, OldNames = new[] { "memcpyToSp" } },
            new BdxInstructionDesc { Code = 0x0042, CodeMask = 0xFFCF, Name = "memcpy.wp", CodeSize = 3, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 }, new Arg { Name = "imm16_2", Type = ArgType.Imm16, WorkPos = true } }, OldNames = new[] { "memcpyToWp" } },
            new BdxInstructionDesc { Code = 0x0082, CodeMask = 0xFFCF, Name = "memcpy.sp.d", CodeSize = 3, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 }, new Arg { Name = "imm16_2", Type = ArgType.Imm16 } }, OldNames = new[] { "memcpyToSpVal" } },
            new BdxInstructionDesc { Code = 0x00C2, CodeMask = 0xFFCF, Name = "memcpy.bd", CodeSize = 3, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 }, new Arg { Name = "imm16_2", Type = ArgType.Imm16, AiPos = true } }, OldNames = new[] { "memcpyToSpAi" } },
            new BdxInstructionDesc { Code = 0x0003, CodeMask = 0x000F, Name = "push.d.pop", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } }, OldNames = new[] { "fetchValue" } },
            new BdxInstructionDesc { Code = 0x0004, CodeMask = 0x000F, Name = "memcpy", CodeSize = 1, Args = new[] { new Arg { Name = "ssub", Type = ArgType.Ssub } } },
            new BdxInstructionDesc { Code = 0x0005, CodeMask = 0xFFFF, Name = "cvt.w.s", CodeSize = 1, OldNames = new[] { "citf" } },
            new BdxInstructionDesc { Code = 0x0085, CodeMask = 0xFFFF, Name = "neg", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x00C5, CodeMask = 0xFFFF, Name = "not", CodeSize = 1, OldNames = new[] { "inv" } },
            new BdxInstructionDesc { Code = 0x0105, CodeMask = 0xFFFF, Name = "seqz", CodeSize = 1, OldNames = new[] { "eqz" } },
            new BdxInstructionDesc { Code = 0x0145, CodeMask = 0xFFFF, Name = "abs", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0185, CodeMask = 0xFFFF, Name = "sltz", CodeSize = 1, OldNames = new[] { "msb" } },
            new BdxInstructionDesc { Code = 0x01C5, CodeMask = 0xFFFF, Name = "slez", CodeSize = 1, OldNames = new[] { "info" } },
            new BdxInstructionDesc { Code = 0x0205, CodeMask = 0xFFFF, Name = "seqz", CodeSize = 1, OldNames = new[] { "eqz" } },
            new BdxInstructionDesc { Code = 0x0245, CodeMask = 0xFFFF, Name = "snez", CodeSize = 1, OldNames = new[] { "neqz" } },
            new BdxInstructionDesc { Code = 0x0285, CodeMask = 0xFFFF, Name = "sgez", CodeSize = 1, OldNames = new[] { "msbi" } },
            new BdxInstructionDesc { Code = 0x02C5, CodeMask = 0xFFFF, Name = "sgtz", CodeSize = 1, OldNames = new[] { "ipos" } },
            new BdxInstructionDesc { Code = 0x0055, CodeMask = 0xFFFF, Name = "cvt.s.w", CodeSize = 1, OldNames = new[] { "cfti" } },
            new BdxInstructionDesc { Code = 0x0095, CodeMask = 0xFFFF, Name = "neg.s", CodeSize = 1, OldNames = new[] { "negf" } },
            new BdxInstructionDesc { Code = 0x0155, CodeMask = 0xFFFF, Name = "abs.s", CodeSize = 1, OldNames = new[] { "absf" } },
            new BdxInstructionDesc { Code = 0x0195, CodeMask = 0xFFFF, Name = "sltz.s", CodeSize = 1, OldNames = new[] { "infzf" } },
            new BdxInstructionDesc { Code = 0x01D5, CodeMask = 0xFFFF, Name = "slez.s", CodeSize = 1, OldNames = new[] { "infoezf" } },
            new BdxInstructionDesc { Code = 0x0215, CodeMask = 0xFFFF, Name = "seqz.s", CodeSize = 1, OldNames = new[] { "eqzf" } },
            new BdxInstructionDesc { Code = 0x0255, CodeMask = 0xFFFF, Name = "snez.s", CodeSize = 1, OldNames = new[] { "neqzf" } },
            new BdxInstructionDesc { Code = 0x0295, CodeMask = 0xFFFF, Name = "sgez.s", CodeSize = 1, OldNames = new[] { "supoezf" } },
            new BdxInstructionDesc { Code = 0x02D5, CodeMask = 0xFFFF, Name = "sgtz.s", CodeSize = 1, OldNames = new[] { "supzf" } },
            new BdxInstructionDesc { Code = 0x0006, CodeMask = 0xFFFF, Name = "add", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0046, CodeMask = 0xFFFF, Name = "sub", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0086, CodeMask = 0xFFFF, Name = "mul", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x00C6, CodeMask = 0xFFFF, Name = "div", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0106, CodeMask = 0xFFFF, Name = "mod", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0146, CodeMask = 0xFFFF, Name = "and", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0186, CodeMask = 0xFFFF, Name = "or", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x01C6, CodeMask = 0xFFFF, Name = "xor", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0206, CodeMask = 0xFFFF, Name = "sll", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0246, CodeMask = 0xFFFF, Name = "sra", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0286, CodeMask = 0xFFFF, Name = "land", CodeSize = 1, OldNames = new[] { "eqzv" } },
            new BdxInstructionDesc { Code = 0x02C6, CodeMask = 0xFFFF, Name = "lor", CodeSize = 1, OldNames = new[] { "neqzv" } },
            new BdxInstructionDesc { Code = 0x0016, CodeMask = 0xFFFF, Name = "add.s", CodeSize = 1, OldNames = new[] { "addf" } },
            new BdxInstructionDesc { Code = 0x0056, CodeMask = 0xFFFF, Name = "sub.s", CodeSize = 1, OldNames = new[] { "subf" } },
            new BdxInstructionDesc { Code = 0x0096, CodeMask = 0xFFFF, Name = "mul.s", CodeSize = 1, OldNames = new[] { "mulf" } },
            new BdxInstructionDesc { Code = 0x00D6, CodeMask = 0xFFFF, Name = "div.s", CodeSize = 1, OldNames = new[] { "divf" } },
            new BdxInstructionDesc { Code = 0x0116, CodeMask = 0xFFFF, Name = "mod.s", CodeSize = 1, OldNames = new[] { "modf" } },
            new BdxInstructionDesc { Code = 0x0007, CodeMask = 0xFFCF, Name = "b", IsJump = true, CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, IsRelative = true } }, OldNames = new[] { "jmp" } },
            new BdxInstructionDesc { Code = 0x0047, CodeMask = 0xFFCF, Name = "beqz", IsJump = true, IsJumpConditional = true, CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, IsRelative = true } }, OldNames = new[] { "jz" } },
            new BdxInstructionDesc { Code = 0x0087, CodeMask = 0xFFCF, Name = "bnez", IsJump = true, IsJumpConditional = true, CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, IsRelative = true } }, OldNames = new[] { "jnz" } },
            new BdxInstructionDesc { Code = 0x0008, CodeMask = 0x000F, Name = "jal", IsGosub = true, CodeSize = 2, Args = new[] { new Arg { Name = "ssub", Type = ArgType.Ssub }, new Arg { Name = "imm16", Type = ArgType.Imm16, IsRelative = true } }, OldNames = new[] { "gosub" } },
            new BdxInstructionDesc { Code = 0x0009, CodeMask = 0xFFCF, Name = "halt", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0049, CodeMask = 0xFFCF, Name = "exit", NeverReturn = true, CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0089, CodeMask = 0xFFCF, Name = "ret", IsGosubRet = true, CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x00C9, CodeMask = 0xFFCF, Name = "drop", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0149, CodeMask = 0xFFCF, Name = "dup", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0189, CodeMask = 0xFFCF, Name = "sin", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x01C9, CodeMask = 0xFFCF, Name = "cos", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0209, CodeMask = 0xFFCF, Name = "degr", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0249, CodeMask = 0xFFCF, Name = "radd", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x000A, CodeMask = 0x000F, Name = "syscall", IsSyscall = true, CodeSize = 2, Args = new[] { new Arg { Name = "ssub", Type = ArgType.Ssub }, new Arg { Name = "imm16", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x000B, CodeMask = 0x000F, Name = "jal32", IsGosub = true, CodeSize = 3, Args = new[] { new Arg { Name = "ssub", Type = ArgType.Ssub }, new Arg { Name = "imm32", Type = ArgType.Imm32, IsRelative = true } }, OldNames = new[] { "gosub32" } },

            #endregion
        };
    }
}
