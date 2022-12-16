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

			new BdxInstructionDesc { Code = 0x0000, CodeMask = 0x003F, Name = "pushImm", CodeSize = 3, Args = new[] { new Arg { Name = "imm32", Type = ArgType.Imm32 } } },
            new BdxInstructionDesc { Code = 0x0010, CodeMask = 0x003F, Name = "pushImmf", CodeSize = 3, Args = new[] { new Arg { Name = "float32", Type = ArgType.Float32 } } },
            new BdxInstructionDesc { Code = 0x0020, CodeMask = 0xFFFF, Name = "pushFromPSp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x0060, CodeMask = 0xFFFF, Name = "pushFromPWp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, WorkPos = true } } },
            new BdxInstructionDesc { Code = 0x00A0, CodeMask = 0xFFFF, Name = "pushFromPSpVal", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x00E0, CodeMask = 0xFFFF, Name = "pushFromPAi", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, AiPos = true } } },
            new BdxInstructionDesc { Code = 0x0030, CodeMask = 0xFFFF, Name = "pushFromFSp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x0070, CodeMask = 0xFFFF, Name = "pushFromFWp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, WorkPos = true } } },
            new BdxInstructionDesc { Code = 0x00B0, CodeMask = 0xFFFF, Name = "pushFromFSpVal", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x00F0, CodeMask = 0xFFFF, Name = "pushFromFAi", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, AiPos = true } } },
            new BdxInstructionDesc { Code = 0x0001, CodeMask = 0xFFCF, Name = "popToSp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x0041, CodeMask = 0xFFCF, Name = "popToWp", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, WorkPos = true } } },
            new BdxInstructionDesc { Code = 0x0081, CodeMask = 0xFFCF, Name = "popToSpVal", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x00C1, CodeMask = 0xFFCF, Name = "popToAi", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, AiPos = true } } },
            new BdxInstructionDesc { Code = 0x0002, CodeMask = 0xFFCF, Name = "memcpyToSp", CodeSize = 3, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 }, new Arg { Name = "imm16_2", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x0042, CodeMask = 0xFFCF, Name = "memcpyToWp", CodeSize = 3, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 }, new Arg { Name = "imm16_2", Type = ArgType.Imm16, WorkPos = true } } },
            new BdxInstructionDesc { Code = 0x0082, CodeMask = 0xFFCF, Name = "memcpyToSpVal", CodeSize = 3, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 }, new Arg { Name = "imm16_2", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x00C2, CodeMask = 0xFFCF, Name = "memcpyToSpAi", CodeSize = 3, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 }, new Arg { Name = "imm16_2", Type = ArgType.Imm16, AiPos = true } } },
            new BdxInstructionDesc { Code = 0x0003, CodeMask = 0x000F, Name = "fetchValue", CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16 } } },
            new BdxInstructionDesc { Code = 0x0004, CodeMask = 0x000F, Name = "memcpy", CodeSize = 1, Args = new[] { new Arg { Name = "ssub", Type = ArgType.Ssub } } },
            new BdxInstructionDesc { Code = 0x0005, CodeMask = 0xFFFF, Name = "citf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0085, CodeMask = 0xFFFF, Name = "neg", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x00C5, CodeMask = 0xFFFF, Name = "inv", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0105, CodeMask = 0xFFFF, Name = "eqz", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0145, CodeMask = 0xFFFF, Name = "abs", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0185, CodeMask = 0xFFFF, Name = "msb", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x01C5, CodeMask = 0xFFFF, Name = "info", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0205, CodeMask = 0xFFFF, Name = "eqz", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0245, CodeMask = 0xFFFF, Name = "neqz", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0285, CodeMask = 0xFFFF, Name = "msbi", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x02C5, CodeMask = 0xFFFF, Name = "ipos", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0055, CodeMask = 0xFFFF, Name = "cfti", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0095, CodeMask = 0xFFFF, Name = "negf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0155, CodeMask = 0xFFFF, Name = "absf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0195, CodeMask = 0xFFFF, Name = "infzf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x01D5, CodeMask = 0xFFFF, Name = "infoezf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0215, CodeMask = 0xFFFF, Name = "eqzf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0255, CodeMask = 0xFFFF, Name = "neqzf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0295, CodeMask = 0xFFFF, Name = "supoezf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x02D5, CodeMask = 0xFFFF, Name = "supzf", CodeSize = 1 },
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
            new BdxInstructionDesc { Code = 0x0286, CodeMask = 0xFFFF, Name = "eqzv", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x02C6, CodeMask = 0xFFFF, Name = "neqzv", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0016, CodeMask = 0xFFFF, Name = "addf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0056, CodeMask = 0xFFFF, Name = "subf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0096, CodeMask = 0xFFFF, Name = "mulf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x00D6, CodeMask = 0xFFFF, Name = "divf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0116, CodeMask = 0xFFFF, Name = "modf", CodeSize = 1 },
            new BdxInstructionDesc { Code = 0x0007, CodeMask = 0xFFCF, Name = "jmp", IsJump = true, CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, IsRelative = true } } },
            new BdxInstructionDesc { Code = 0x0047, CodeMask = 0xFFCF, Name = "jz", IsJump = true, IsJumpConditional = true, CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, IsRelative = true } } },
            new BdxInstructionDesc { Code = 0x0087, CodeMask = 0xFFCF, Name = "jnz", IsJump = true, IsJumpConditional = true, CodeSize = 2, Args = new[] { new Arg { Name = "imm16", Type = ArgType.Imm16, IsRelative = true } } },
            new BdxInstructionDesc { Code = 0x0008, CodeMask = 0x000F, Name = "gosub", IsGosub = true, CodeSize = 2, Args = new[] { new Arg { Name = "ssub", Type = ArgType.Ssub }, new Arg { Name = "imm16", Type = ArgType.Imm16, IsRelative = true } } },
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
            new BdxInstructionDesc { Code = 0x000B, CodeMask = 0x000F, Name = "gosub32", IsGosub = true, CodeSize = 3, Args = new[] { new Arg { Name = "ssub", Type = ArgType.Ssub }, new Arg { Name = "imm32", Type = ArgType.Imm32, IsRelative = true } } },

            #endregion
        };
    }
}
