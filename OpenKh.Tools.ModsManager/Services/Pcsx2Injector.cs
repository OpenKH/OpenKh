using OpenKh.Common;
using OpenKh.Tools.Common;
using OpenKh.Tools.ModsManager.Interfaces;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xe.BinaryMapper;

namespace OpenKh.Tools.ModsManager.Services
{
    public class Pcsx2Injector
    {
        private class Offsets
        {
            public string GameName { get; init; }
            public int LoadFile { get; init; }
            public int GetFileSize { get; init; }
            public int LoadFileTask { get; init; }
            public int LoadFileAsync { get; init; }

            public int GetFileSizeRecom { get; init; }
            public int RegionInit { get; init; }
            public int BufferPointer { get; init; }
            public int RegionForce { get; init; }
            public int RegionId { get; init; }
            public int RegionPtr { get; init; }
            public int LanguagePtr { get; init; }

        }

        private class Patch
        {
            public string Game { get; init; }
            public string Name { get; init; }
            public int Address { get; init; }
            public uint[] Pattern { get; init; }
            public uint[] NewPattern { get; init; }
        }

        private class LoadFileTask
        {
            [Data] public int Flags { get; set; }
            [Data] public int Unk04 { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unk0C { get; set; }
            [Data] public int Unk10 { get; set; }
            [Data] public int Unk14 { get; set; }
            [Data] public int Unk18 { get; set; }
            [Data] public int Unk1C { get; set; }
            [Data] public int Unk20 { get; set; }
            [Data] public int IsoBlock { get; set; }
            [Data] public int Length { get; set; }
            [Data] public int PtrDestination { get; set; }
            [Data] public int FinalLength { get; set; }
            [Data] public int Unk34 { get; set; }
        }

        private const string KH2FM = "SLPM_666.75;1";

        // Converted from https://github.com/Xeeynamo/mipsdump/blob/master/mipsdump/Instructions.fs
        private const uint Zero = 0x00;
        private const uint AT = 0x01;
        private const uint V0 = 0x02;
        private const uint V1 = 0x03;
        private const uint A0 = 0x04;
        private const uint A1 = 0x05;
        private const uint A2 = 0x06;
        private const uint A3 = 0x07;
        private const uint T0 = 0x08;
        private const uint T1 = 0x09;
        private const uint T2 = 0x0A;
        private const uint T3 = 0x0B;
        private const uint T4 = 0x0C;
        private const uint T5 = 0x0D;
        private const uint T6 = 0x0E;
        private const uint T7 = 0x0F;
        private const uint S0 = 0x10;
        private const uint S1 = 0x11;
        private const uint S2 = 0x12;
        private const uint S3 = 0x13;
        private const uint S4 = 0x14;
        private const uint S5 = 0x15;
        private const uint S6 = 0x16;
        private const uint S7 = 0x17;
        private const uint T8 = 0x18;
        private const uint T9 = 0x19;
        private const uint K0 = 0x1A;
        private const uint K1 = 0x1B;
        private const uint GP = 0x1C;
        private const uint SP = 0x1D;
        private const uint FP = 0x1E;
        private const uint RA = 0x1F;

        private enum Regimm
        {
            BLTZ = 0x00,
            BGEZ = 0x01,
            BLTZAL = 0x10,
            BGEZAL = 0x11,
        }

        private enum Op
        {
            SPECIAL = 0x00,
            REGIMM = 0x01,
            J = 0x02,
            JAL = 0x03,
            BEQ = 0x04,
            BNE = 0x05,
            BLEZ = 0x06,
            BGTZ = 0x07,
            ADDI = 0x08,
            ADDIU = 0x09,
            SLTI = 0x0A,
            SLTIU = 0x0B,
            ANDI = 0x0C,
            ORI = 0x0D,
            XORI = 0x0E,
            LUI = 0x0F,
            C0 = 0x10,
            C1 = 0x11,
            C2 = 0x12,
            C3 = 0x13,
            LB = 0x20,
            LH = 0x21,
            LWL = 0x22,
            LW = 0x23,
            LBU = 0x24,
            LHU = 0x25,
            LWR = 0x26,
            SB = 0x28,
            SH = 0x29,
            SWL = 0x2a,
            SW = 0x2b,
            SWR = 0x2e,
            LWC0 = 0x30,
            LWC1 = 0x31,
            LWC2 = 0x32,
            LWC3 = 0x33,
            LD = 0x37,
            SWC0 = 0x38,
            SWC1 = 0x39,
            SWC2 = 0x3a,
            SWC3 = 0x3b,
            SD = 0x3f,
        }

        private enum Special
        {
            SLL = 0x00,
            SRL = 0x02,
            SRA = 0x03,
            SLLV = 0x04,
            SRLV = 0x06,
            SRAV = 0x07,
            JR = 0x08,
            JALR = 0x09,
            SYSCALL = 0x0C,
            BREAK = 0x0D,
            MFHI = 0x10,
            MTHI = 0x11,
            MFLO = 0x12,
            MTLO = 0x13,
            MULT = 0x18,
            MULTU = 0x19,
            DIV = 0x1A,
            DIVU = 0x1B,
            ADD = 0x20,
            ADDU = 0x21,
            SUB = 0x22,
            SUBU = 0x23,
            AND = 0x24,
            OR = 0x25,
            XOR = 0x26,
            NOR = 0x27,
            SLT = 0x2A,
            SLTU = 0x2B,
        }

        private enum Cop
        {

            MFC = 0x00,
            CFC = 0x02,
            MTC = 0x04,
            CTC = 0x06,
            BC = 0x10,
        }

        // R-type instructions
        private static uint SLL(uint dst, uint src, uint value) => (uint)Special.SLL | (dst << 11) | (src << 16) | ((value & 0x1Fu) << 6);
        private static uint SRL(uint dst, uint src, uint value) => (uint)Special.SRL | (dst << 11) | (src << 16) | ((value & 0x1Fu) << 6);
        private static uint SRA(uint dst, uint src, uint value) => (uint)Special.SRA | (dst << 11) | (src << 16) | ((value & 0x1Fu) << 6);
        private static uint SLLV(uint dst, uint left, uint right) => (uint)Special.SLLV | (dst << 11) | left << 16 | right << 21;
        private static uint SRAV(uint dst, uint left, uint right) => (uint)Special.SRAV | (dst << 11) | left << 16 | right << 21;
        private static uint SRLV(uint dst, uint left, uint right) => (uint)Special.SRLV | (dst << 11) | left << 16 | right << 21;
        private static uint JR(uint reg) => (uint)Special.JR | ((reg) << 21);
        private static uint JALR(uint reg) => (uint)Special.JALR | ((reg) << 21);
        private static uint SYSCALL(uint code) => (uint)Special.SYSCALL | ((code & 0xffffffu) << 6);
        private static uint BREAK(uint code1, uint code2) => (uint)Special.BREAK | ((code1 & 0x3ffu) << 16) | ((code2 & 0x3ffu) << 6);
        private static uint MFHI(uint reg) => (uint)Special.MFHI | ((reg) << 11);
        private static uint MTHI(uint reg) => (uint)Special.MTHI | ((reg) << 11);
        private static uint MFLO(uint reg) => (uint)Special.MFLO | ((reg) << 11);
        private static uint MTLO(uint reg) => (uint)Special.MTLO | ((reg) << 11);
        private static uint MULT(uint dst, uint src) => (uint)Special.MULT | (src << 16) | (dst << 21);
        private static uint MULTU(uint dst, uint src) => (uint)Special.MULTU | (src << 16) | (dst << 21);
        private static uint DIV(uint dst, uint src) => (uint)Special.DIV | (src << 16) | (dst << 21);
        private static uint DIVU(uint dst, uint src) => (uint)Special.DIVU | (src << 16) | (dst << 21);
        private static uint ADD(uint dst, uint left, uint right) => (uint)Special.ADD | (dst << 11) | (left << 21) | (right << 16);
        private static uint ADDU(uint dst, uint left, uint right) => (uint)Special.ADDU | (dst << 11) | (left << 21) | (right << 16);
        private static uint SUB(uint dst, uint left, uint right) => (uint)Special.SUB | (dst << 11) | (left << 21) | (right << 16);
        private static uint SUBU(uint dst, uint left, uint right) => (uint)Special.SUBU | (dst << 11) | (left << 21) | (right << 16);
        private static uint AND(uint dst, uint left, uint right) => (uint)Special.AND | (dst << 11) | (left << 21) | (right << 16);
        private static uint OR(uint dst, uint left, uint right) => (uint)Special.OR | (dst << 11) | (left << 21) | (right << 16);
        private static uint XOR(uint dst, uint left, uint right) => (uint)Special.XOR | (dst << 11) | (left << 21) | (right << 16);
        private static uint NOR(uint dst, uint left, uint right) => (uint)Special.NOR | (dst << 11) | (left << 21) | (right << 16);
        private static uint SLT(uint dst, uint left, uint right) => (uint)Special.SLT | (dst << 11) | (left << 21) | (right << 16);
        private static uint SLTU(uint dst, uint left, uint right) => (uint)Special.SLTU | (dst << 11) | (left << 21) | (right << 16);

        // Alias R-type instructions
        private static uint NOP() => SLL(Zero, Zero, 0u);
        private static uint MOVE(uint dst, uint src) => OR(dst, src, Zero);
        private static uint NEG(uint dst, uint src) => SUB(dst, Zero, src);
        private static uint NEGU(uint dst, uint src) => SUBU(dst, Zero, src);

        // I-type instructions
        private static uint BLTZ(uint reg, short imm) => (ushort)imm | ((uint)Regimm.BLTZ << 16) | (reg << 21) | ((uint)Op.REGIMM << 26);
        private static uint BGEZ(uint reg, short imm) => (ushort)imm | ((uint)Regimm.BGEZ << 16) | (reg << 21) | ((uint)Op.REGIMM << 26);
        private static uint BLTZAL(uint reg, short imm) => (ushort)imm | ((uint)Regimm.BLTZAL << 16) | (reg << 21) | ((uint)Op.REGIMM << 26);
        private static uint BGEZAL(uint reg, short imm) => (ushort)imm | ((uint)Regimm.BGEZAL << 16) | (reg << 21) | ((uint)Op.REGIMM << 26);
        private static uint BEQ(uint left, uint right, short imm) => (ushort)imm | (right << 16) | (left << 21) | ((uint)Op.BEQ << 26);
        private static uint BNE(uint left, uint right, short imm) => (ushort)imm | (right << 16) | (left << 21) | ((uint)Op.BNE << 26);
        private static uint BLEZ(uint reg, short imm) => (ushort)imm | (reg << 21) | ((uint)Op.BLEZ << 26);
        private static uint BGTZ(uint reg, short imm) => (ushort)imm | (reg << 21) | ((uint)Op.BGTZ << 26);
        private static uint ADDI(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.ADDI << 26);
        private static uint ADDIU(uint dst, uint src, ushort imm) => imm | (dst << 16) | (src << 21) | ((uint)Op.ADDIU << 26);
        private static uint ADDIU(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.ADDIU << 26);
        private static uint SLTI(uint left, uint right, short imm) => (ushort)imm | (left << 16) | (right << 21) | ((uint)Op.SLTI << 26);
        private static uint SLTIU(uint left, uint right, short imm) => (ushort)imm | (left << 16) | (right << 21) | ((uint)Op.SLTIU << 26);
        private static uint ANDI(uint dst, uint src, ushort imm) => imm | (dst << 16) | (src << 21) | ((uint)Op.ANDI << 26);
        private static uint ORI(uint dst, uint src, ushort imm) => imm | (dst << 16) | (src << 21) | ((uint)Op.ORI << 26);
        private static uint XORI(uint dst, uint src, ushort imm) => imm | (dst << 16) | (src << 21) | ((uint)Op.XORI << 26);
        private static uint LUI(uint dst, ushort imm) => imm | (dst << 16) | ((uint)Op.LUI << 26);
        private static uint LB(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LB << 26);
        private static uint LH(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LH << 26);
        private static uint LWL(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWL << 26);
        private static uint LW(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LW << 26);
        private static uint LBU(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LBU << 26);
        private static uint LHU(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LHU << 26);
        private static uint LWR(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWR << 26);
        private static uint SB(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SB << 26);
        private static uint SH(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SH << 26);
        private static uint SWL(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWL << 26);
        private static uint SW(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SW << 26);
        private static uint SWR(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWR << 26);

        // Alias I-type instructions
        private static uint BAL(uint reg, short imm) => BGEZAL(reg, imm);
        private static uint LI(uint dst, short imm) => ADDIU(dst, Zero, (ushort)imm);
        private static uint LIU(uint dst, ushort imm) => ORI(dst, Zero, imm);

        // Co-processor instructions
        private static uint MFC0(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MFC << 21) | ((uint)Op.C0 << 26);
        private static uint MFC1(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MFC << 21) | ((uint)Op.C1 << 26);
        private static uint MFC2(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MFC << 21) | ((uint)Op.C2 << 26);
        private static uint MFC3(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MFC << 21) | ((uint)Op.C3 << 26);
        private static uint MTC0(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MTC << 21) | ((uint)Op.C0 << 26);
        private static uint MTC1(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MTC << 21) | ((uint)Op.C1 << 26);
        private static uint MTC2(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MTC << 21) | ((uint)Op.C2 << 26);
        private static uint MTC3(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MTC << 21) | ((uint)Op.C3 << 26);
        private static uint CFC0(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CFC << 21) | ((uint)Op.C0 << 26);
        private static uint CFC1(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CFC << 21) | ((uint)Op.C1 << 26);
        private static uint CFC2(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CFC << 21) | ((uint)Op.C2 << 26);
        private static uint CFC3(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CFC << 21) | ((uint)Op.C3 << 26);
        private static uint CTC0(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CTC << 21) | ((uint)Op.C0 << 26);
        private static uint CTC1(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CTC << 21) | ((uint)Op.C1 << 26);
        private static uint CTC2(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CTC << 21) | ((uint)Op.C2 << 26);
        private static uint CTC3(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CTC << 21) | ((uint)Op.C3 << 26);
        private static uint COP0(uint offset) => (offset & 0x1FFFFFFu) | (1u << 25) | ((uint)Op.C0 << 26);
        private static uint COP1(uint offset) => (offset & 0x1FFFFFFu) | (1u << 25) | ((uint)Op.C1 << 26);
        private static uint COP2(uint offset) => (offset & 0x1FFFFFFu) | (1u << 25) | ((uint)Op.C2 << 26);
        private static uint COP3(uint offset) => (offset & 0x1FFFFFFu) | (1u << 25) | ((uint)Op.C3 << 26);
        private static uint LWC0(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWC0 << 26);
        private static uint LWC1(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWC1 << 26);
        private static uint LWC2(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWC2 << 26);
        private static uint LWC3(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWC3 << 26);
        private static uint SWC0(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWC0 << 26);
        private static uint SWC1(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWC1 << 26);
        private static uint SWC2(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWC2 << 26);
        private static uint SWC3(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWC3 << 26);
        private static uint BC0F(short offset) => (ushort)offset | (0x10u << 26) | (0x10u << 20) | (0u << 16);
        private static uint BC0T(short offset) => (ushort)offset | (0x10u << 26) | (0x10u << 20) | (1u << 16);
        private static uint BC1F(short offset) => (ushort)offset | (0x11u << 26) | (0x10u << 20) | (0u << 16);
        private static uint BC1T(short offset) => (ushort)offset | (0x11u << 26) | (0x10u << 20) | (1u << 16);
        private static uint BC2F(short offset) => (ushort)offset | (0x12u << 26) | (0x10u << 20) | (0u << 16);
        private static uint BC2T(short offset) => (ushort)offset | (0x12u << 26) | (0x10u << 20) | (1u << 16);
        private static uint BC3F(short offset) => (ushort)offset | (0x13u << 26) | (0x10u << 20) | (0u << 16);
        private static uint BC3T(short offset) => (ushort)offset | (0x13u << 26) | (0x10u << 20) | (1u << 16);

        // J-type instructions
        private static uint J(uint addr) => (addr & 0x3FFFFFFu) | ((uint)Op.J << 26);
        private static uint JAL(uint addr) => ((addr / 4) & 0x3FFFFFFu) | ((uint)Op.JAL << 26);

        // PlayStation 2 specific instructions
        private static uint LD(uint src, uint dst, short imm) => (ushort)imm | (src << 16) | (dst << 21) | ((uint)Op.LD << 26);
        private static uint SD(uint src, uint dst, short imm) => (ushort)imm | (src << 16) | (dst << 21) | ((uint)Op.SD << 26);

        public enum Operation
        {
            HookExit,
            LoadFile,
            GetFileSize,
            GetFileSizeRecom,
            LoadFileTask,
            LoadFileAsync,
        }

        private const uint BaseHookPtr = 0xFFF00;
        private const int HookStack = 0x0F;
        private const int ParamOperator = -0x04;
        private const int Param1 = -0x08;
        private const int Param2 = -0x0C;
        private const int Param3 = -0x10;
        private const int Param4 = -0x14;
        private const int ParamReturn = Param1;
        private const int ParamReturn2 = Param2;

        private static readonly uint[] LoadFileHook = new uint[]
        {
            // Input:
            // T4 return program counter
            // T5 Operation
            // RA fallback program counter
            //
            // Work:
            // T6 Hook stack
            // V0 Return value
            // V1 syscall parameter
            //
            LUI(T6, HookStack),
            SW(A0, T6, Param1),
            SW(A1, T6, Param2),
            SW(T5, T6, ParamOperator),
            LW(T5, T6, ParamOperator),
            BNE(T5, (byte)Operation.HookExit, -2),
            LW(V0, T6, ParamReturn),
            BEQ(V0, Zero, 2),
            NOP(),
            ADDIU(RA, RA, 4),
            ADDIU(SP, SP, -0x10),
            SD(T4, SP, 0x08),
            SD(S0, SP, 0x00),
            JR(RA),
            NOP(),
        };

        private static readonly uint[] GetFileSizeHook = new uint[]
        {
            // Input:
            // A0 const char* fileName
            // T4 return program counter
            // T5 Operation
            // RA fallback program counter
            //
            // Work:
            // T6 Hook stack
            // V0 Return value
            // V1 syscall parameter
            //
            LUI(T6, HookStack),
            SW(A0, T6, Param1),
            SW(A1, T6, Param2),
            SW(A2, T6, Param3),
            SW(A3, T6, Param4),
            SW(T5, T6, ParamOperator),
            LW(T5, T6, ParamOperator),
            BNE(T5, (byte)Operation.HookExit, -2),
            LW(V0, T6, ParamReturn),
            BEQ(V0, Zero, 2),
            NOP(),
            JR(T4),
            NOP(),
            ADDIU(SP, SP, -0x10),
            SD(T4, SP, 0x08),
            SD(S0, SP, 0x00),
            JR(RA),
            NOP(),
        };

        private static readonly uint[] LoadFileTaskHook = new uint[]
        {
            // Input:
            // S0 DstPtr
            // S1 Filename
            // T4 return program counter
            // T5 Operation
            // V0 IdxFilePtr
            //
            // Work:
            // T6 Hook stack
            // V0 Return value
            // 
            LUI(T6, HookStack),
            SW(S1, T6, Param1), // Filename
            SW(S0, T6, Param2), // DstPtr
            SW(V0, T6, Param3), // LoadFileTask
            SW(T5, T6, ParamOperator), // Operation
            LW(T5, T6, ParamOperator),
            BNE(T5, (byte)Operation.HookExit, -2),
            LW(V1, T6, ParamReturn),
            BEQ(V1, Zero, 3),
            MOVE(S2, V0),
            BEQ(Zero, Zero, 2),
            ADDIU(RA, RA, 0x98), // skip the remainder of the function
            LI(V0, -1),
            JR(RA),
            NOP(),
        };

        private static readonly uint[] LoadFileAsyncHook = new uint[]
        {
            // Input:
            //
            // Work:
            //
            LUI(T6, HookStack),
            LW(T2, V0, -0x2A10),
            LW(T3, V0, -0x2A24),
            ADDI(T4, V0, -0x29F8),
            LW(T0, T4, 0),
            ADDIU(T1, T3, 8),
            LW(T4, T3, 0x38),
            LW(T4, T4, 0),
            SW(T0, T6, Param1), // FileDirID
            BEQ(T2, T4, 9),
            SW(T1, T6, Param2), // FileNamePtr
            ADDIU(T4, Zero, (byte)Operation.GetFileSizeRecom),
            SW(T4, T6, ParamOperator), // Operation
            LW(T4, T6, ParamOperator),
            BNE(T4, (byte)Operation.HookExit, -2),
            LW(T4, T6, ParamReturn),
            BEQ(T4, Zero, 7),
            NOP(),
            BEQ(Zero, Zero, 8),
            SW(T2, T6, Param3), // MemDstPtr
            SW(T5, T6, ParamOperator), // Operation
            LW(T5, T6, ParamOperator),
            BNE(T5, (byte)Operation.HookExit, -2),
            LW(T4, T6, ParamReturn),
            LUI(V1, 0x5B), // For Fallback
            BEQ(T4, Zero, 5),
            LW(A2, V0, -0x29F4),
            ADD(T2, T2, T4),
            SW(T2, V0, -0x2A10),
            ADDIU(V0, Zero, 1),
            ADDIU(RA, RA, 0x64),
            JR(RA),
            NOP(),
        };

        private static readonly uint[] GetFileSizeRecomHook = new uint[]
        {
            LUI(T6, HookStack),
            LUI(V1, 0x5C),
            ADDI(T4, V1, -0x29F8),
            LW(T4, T4, 0),
            SW(T4, T6, Param1), // FileDirID
            SW(S2, T6, Param2), // FileNamePtr
            SW(T5, T6, ParamOperator), // Operation
            LW(T5, T6, ParamOperator),
            BNE(T5, (byte)Operation.HookExit, -2),
            LW(V1, T6, ParamReturn),
            // For Fallback
            BNE(V1, Zero, 2),
            NOP(),
            LW(V1, S2, 0x18),
            JR(RA),
            SW(V1, S1, 0x28),
        };



        private static readonly uint[] RegionInitPatch = new uint[]
        {
            JR(RA),
            NOP(),
        };

        private static readonly string[] MemoryCardPatch = new string[]
        {
            "SLPM", "666", "75",
            "SLPM", "666", "75",
            "SLPM", "666", "75", "666", "75", "75", "75",
        };

        private static readonly Offsets[] _offsets = new Offsets[]
        {
            new Offsets
            {
                GameName = "SLPS_251.97;1",
                LoadFileTask = 0x1204C0,
            },
            new Offsets
            {
                GameName = "SLPS_251.98;1",
                LoadFileTask = 0x1204C0,
            },
            new Offsets {
                GameName = "SLUS_217.99;1",
                LoadFileAsync = 0x1A0C6C,
                GetFileSizeRecom = 0x1A11A0,
            },
            new Offsets
            {
                GameName = "SLPM_662.33;1",
                LoadFile = 0x167F20,
                GetFileSize = 0x1AC698,
                RegionInit = 0x105CA0,
                BufferPointer = 0x35E680,
                RegionForce = 0x37FCC8,
                RegionId = 0x349514,
                RegionPtr = 0x349510,
                LanguagePtr = 0x349510, // same as Region
            },
            new Offsets
            {
                GameName = "SLUS_210.05;1",
                LoadFile = 0x167C50,
                GetFileSize = 0x1AC760,
                RegionInit = 0x105CB0,
                BufferPointer = 0x35EE98,
                RegionForce = 0x3806D8,
                RegionId = 0x349D44,
                RegionPtr = 0x349D40,
                LanguagePtr = 0x349D40, // same as Region
            },
            new Offsets
            {
                GameName = "SLES_541.14;1",
                LoadFile = 0x167CA8,
                GetFileSize = 0x1AC858,
                RegionInit = 0x105CB0,
                BufferPointer = 0x35F3A8,
                RegionForce = 0x378378,
                RegionId = 0x34A24C,
                RegionPtr = 0x34A240,
                LanguagePtr = 0x34A244,
            },
            new Offsets
            {
                GameName = "SLES_542.32;1",
                LoadFile = 0x167CA8,
                GetFileSize = 0x1AC858,
                RegionInit = 0x105CB0,
                BufferPointer = 0x35F3A8,
                RegionForce = 0x378378,
                RegionId = 0x34A24C,
                RegionPtr = 0x34A240,
                LanguagePtr = 0x34A244,
            },
            new Offsets
            {
                GameName = "SLES_542.33;1",
                LoadFile = 0x167CA8,
                GetFileSize = 0x1AC858,
                RegionInit = 0x105CB0,
                BufferPointer = 0x35F3A8,
                RegionForce = 0x378378,
                RegionId = 0x34A24C,
                RegionPtr = 0x34A240,
                LanguagePtr = 0x34A244,
            },
            new Offsets
            {
                GameName = "SLES_542.34;1",
                LoadFile = 0x167CA8,
                GetFileSize = 0x1AC858,
                RegionInit = 0x105CB0,
                BufferPointer = 0x35F3A8,
                RegionForce = 0x378378,
                RegionId = 0x34A24C,
                RegionPtr = 0x34A240,
                LanguagePtr = 0x34A244,
            },
            new Offsets
            {
                GameName = "SLES_542.35;1",
                LoadFile = 0x167CA8,
                GetFileSize = 0x1AC858,
                RegionInit = 0x105CB0,
                BufferPointer = 0x35F3A8,
                RegionForce = 0x378378,
                RegionId = 0x34A24C,
                RegionPtr = 0x34A240,
                LanguagePtr = 0x34A244,
            },
            new Offsets
            {
                GameName = KH2FM,
                LoadFile = 0x1682b8,
                GetFileSize = 0x1AE1B0,
                RegionInit = 0x105AF8,
                BufferPointer = 0x350E88,
                RegionForce = 0x369E98,
                RegionId = 0x33CAFC,
                RegionPtr = 0x33CAF0,
                LanguagePtr = 0x33CAF4,
            },
        };

        private readonly IOperationDispatcher _operationDispatcher;
        private const int OperationAddress = (HookStack << 16) - 4;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _injectorTask;
        private uint _hookPtr;
        private uint _nextHookPtr;
        private Offsets _myOffsets;

        public Pcsx2Injector(IOperationDispatcher operationDispatcher)
        {
            _operationDispatcher = operationDispatcher;
            
        }

        public int RegionId { get; set; }
        public string Region { get; set; }
        public string Language { get; set; }

        public void Run(Process process, IDebugging debugging)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _injectorTask = Task.Run(async () =>
            {
                Log.Info("Waiting for the game to boot");
                var gameName = await Pcsx2MemoryService.GetPcsx2ApplicationName(process, _cancellationToken);
                using var processStream = new ProcessStream(process, 0x20000000, 0x2000000);

                Log.Info("Injecting code");
                _myOffsets = _offsets.FirstOrDefault(x => x.GameName == gameName);
                if (_myOffsets == null)
                {
                    Log.Err($"Game {gameName} not recognized. Exiting from the injector service.");
                    return;
                }

                WritePatch(processStream, _myOffsets);

                Log.Info("Executing the injector main loop");
                MainLoop(processStream, debugging);
                debugging.HideDebugger();
            }, _cancellationToken);
        }

        public void Stop()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _injectorTask?.Wait();
            }
            catch
            {

            }
        }

        private void MainLoop(Stream stream, IDebugging debugging)
        {
            var isProcessDead = false;
            while (!_cancellationToken.IsCancellationRequested && !isProcessDead)
            {
                var operation = stream.SetPosition(OperationAddress).ReadInt32();
                if (stream.Position == OperationAddress)
                    break; // The emulator stopped its execution

                switch ((Operation)operation)
                {
                    case Operation.LoadFile:
                        OperationCopyFile(stream);
                        break;
                    case Operation.GetFileSize:
                        OperationGetFileSize(stream);
                        break;
                    case Operation.GetFileSizeRecom:
                        OperationGetFileSizeRecom(stream);
                        break;
                    case Operation.LoadFileTask:
                        OperationLoadFileTask(stream);
                        break;
                    case Operation.LoadFileAsync:
                        OperationLoadFileAsync(stream);
                        break;
                    case Operation.HookExit:
                        Thread.Sleep(1);
                        continue;
                }

                stream.SetPosition(OperationAddress).Write((int)Operation.HookExit);
            }
        }

        private void OperationCopyFile(Stream stream)
        {
            const int ParameterCount = 2;
            stream.SetPosition(OperationAddress - ParameterCount * sizeof(uint));

            var ptrMemDst = stream.ReadInt32();
            var ptrFileName = stream.ReadInt32();
            var fileName = ReadString(stream, ptrFileName);
            if (string.IsNullOrEmpty(fileName))
                return;

            var returnValue = _operationDispatcher.LoadFile(stream.SetPosition(ptrMemDst), fileName);
            stream.SetPosition(OperationAddress - 4).Write(returnValue);
        }

        private void OperationGetFileSize(Stream stream)
        {
            const int ParameterCount = 1;
            stream.SetPosition(OperationAddress - ParameterCount * sizeof(uint));

            var ptrFileName = stream.ReadInt32();
            var fileName = ReadString(stream, ptrFileName);
            if (string.IsNullOrEmpty(fileName))
                return;

            var returnValue = _operationDispatcher.GetFileSize(fileName);
            stream.SetPosition(OperationAddress - 4).Write(returnValue);
        }

        private void OperationGetFileSizeRecom(Stream stream)
        {
            const int ParameterCount = 2;
            stream.SetPosition(OperationAddress - ParameterCount * sizeof(int));

            var ptrFileName = stream.ReadInt32();
            var fileDirID = stream.ReadInt32();

            var fileName = ReadString(stream, ptrFileName);
            var returnValue = 0;

            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = Path.Combine(fileDirID.ToString(), fileName);
                returnValue = _operationDispatcher.GetFileSize(fileName);
            }
            stream.SetPosition(OperationAddress - 4).Write(returnValue);
        }

        private void OperationLoadFileTask(Stream stream)
        {
            const int ParameterCount = 3;
            stream.SetPosition(OperationAddress - ParameterCount * sizeof(uint));

            var ptrTask = stream.ReadInt32();
            var ptrMemDst = stream.ReadInt32();
            var ptrFileName = stream.ReadInt32();
            var returnValue = 0;

            var fileName = ReadString(stream, ptrFileName);
            var fileLength = -1;

            if (!string.IsNullOrEmpty(fileName))
            {
                fileLength = _operationDispatcher.LoadFile(stream.SetPosition(ptrMemDst), fileName);
                if (fileLength > 0)
                {
                    returnValue = fileLength;
                }
                else
                {
                    fileLength = -1;
                }
            }

            stream.SetPosition(ptrTask + 44).Write(ptrMemDst);
            // Write the return value so the hook can exit
            stream.SetPosition(OperationAddress - 4).Write(returnValue);
            // Wait for the emulator to get to the await loop
            Thread.Sleep(4);
            // Signal that the file has finished loading
            stream.SetPosition(ptrTask + 48).Write(fileLength);
        }

        private void OperationLoadFileAsync(Stream stream)
        {
            const int ParameterCount = 3;
            stream.SetPosition(OperationAddress - ParameterCount * sizeof(uint));

            var ptrMemDst = stream.ReadUInt32();
            var ptrFilename = stream.ReadInt32();
            var fileDirID = stream.ReadInt32();
            var returnValue = 0;

            var fileName = ReadString(stream, ptrFilename);

            if (!string.IsNullOrEmpty(fileName))
            {
                fileName = Path.Combine(fileDirID.ToString(), fileName);
                var fileLength = _operationDispatcher.LoadFile(stream.SetPosition(ptrMemDst), fileName);
                if (fileLength > 0)
                {
                    returnValue = fileLength;
                }
            }
            stream.SetPosition(OperationAddress - 4).Write(returnValue);
        }

        private void WritePatch(Stream stream, Offsets offsets)
        {
            ResetHooks();
            if (offsets != null)
            {
                if (offsets.LoadFileTask > 0)
                {
                    Log.Info("Injecting {0} function", nameof(offsets.LoadFileTask));
                    WritePatch(stream, offsets.LoadFileTask,
                        ADDIU(T4, RA, 0),
                        JAL(WriteHook(stream, LoadFileTaskHook)),
                        ADDIU(T5, Zero, (byte)Operation.LoadFileTask));
                }

                if (offsets.LoadFileAsync > 0)
                {
                    Log.Info("Injecting {0} function", nameof(offsets.LoadFileAsync));
                    WritePatch(stream, offsets.LoadFileAsync,
                        JAL(WriteHook(stream, LoadFileAsyncHook)),
                        ADDIU(T5, Zero, (byte)Operation.LoadFileAsync));
                }

                if (offsets.LoadFile > 0)
                {
                    Log.Info("Injecting {0} function", nameof(offsets.LoadFile));
                    WritePatch(stream, offsets.LoadFile,
                        ADDIU(T4, RA, 0),
                        JAL(WriteHook(stream, LoadFileHook)),
                        ADDIU(T5, Zero, (byte)Operation.LoadFile));
                }

                if (offsets.GetFileSize > 0)
                {
                    Log.Info("Injecting {0} function", nameof(offsets.GetFileSize));
                    var subGetFileSizePtr = stream.SetPosition(offsets.GetFileSize + 8).ReadUInt32();
                    WritePatch(stream, offsets.GetFileSize,
                        ADDIU(T4, RA, 0),
                        JAL(WriteHook(stream, GetFileSizeHook)),
                        ADDIU(T5, Zero, (byte)Operation.GetFileSize),
                        subGetFileSizePtr,
                        NOP(),
                        BEQ(V0, Zero, 2),
                        NOP(),
                        LW(V0, V0, 0x0C),
                        LD(RA, SP, 0x08),
                        JR(RA),
                        ADDIU(SP, SP, 0x10));
                }

                if (offsets.GetFileSizeRecom > 0)
                {
                    Log.Info("Injecting {0} function", nameof(offsets.GetFileSizeRecom));
                    WritePatch(stream, offsets.GetFileSizeRecom,
                        JAL(WriteHook(stream, GetFileSizeRecomHook)),
                        ADDIU(T5, Zero, (byte)Operation.GetFileSizeRecom));
                }

                if (RegionId > 0)
                {
                    Log.Info("Injecting {0} function", "RegionInit");
                    WritePatch(stream, offsets.RegionInit, RegionInitPatch);
                    WritePatch(stream, offsets.RegionForce, Region);
                    WritePatch(stream, offsets.RegionForce + 8, Language);
                    WritePatch(stream, offsets.RegionId, RegionId);
                    WritePatch(stream, offsets.RegionPtr, offsets.RegionForce);
                    WritePatch(stream, offsets.LanguagePtr, offsets.RegionForce + 8);

                    if (offsets.GameName == KH2FM)
                        PatchKh2FmPs2(stream);
                }
            }
        }

        private void PatchKh2FmPs2(Stream stream)
        {
            // Always use "SLPM" for memory card regardless the region
            WritePatch(stream, 0x240138,
                ADDIU(V0, Zero, (int)Kh2.Constants.RegionId.FinalMix));

            // Always use "BI" for memory card regardless the region
            WritePatch(stream, 0x2402E8,
                ADDIU(V0, Zero, (int)Kh2.Constants.RegionId.FinalMix),
                NOP());

            // Always use "KH2J" header for saves
            WritePatch(stream, 0x105870,
                LUI(V0, 0x4A32),
                ADDIU(V0, V0, 0x484B),
                JR(RA),
                NOP());

            // Fix weird game bug where KH2FM would crash on map change
            // when the region is different from JP or FM.
            WritePatch(stream, 0x015ABE8, ADDIU(V0, Zero, 1));

            // Fix issue where KH2FM fails to load movie cutscenes
            // when the region is different from FM.
            WritePatch(stream, 0x022BC68, ADDIU(A2, A1, -0x6130));
        }

        private void ResetHooks() => _nextHookPtr = BaseHookPtr;

        private uint WriteHook(Stream stream, params uint[] patch)
        {
            _hookPtr = _nextHookPtr;
            _nextHookPtr += WritePatch(stream, _hookPtr, patch);
            return _hookPtr;
        }

        private uint WritePatch(Stream stream, long offset, params uint[] patch)
        {
            if (offset == 0)
                return 0;

            stream.SetPosition(offset);
            foreach (var word in patch)
                stream.Write(word);
            stream.Flush();
            return (uint)(patch.Length * sizeof(uint));
        }

        private void WritePatch(Stream stream, long offset, int patch)
        {
            if (offset == 0)
                return;
            stream.SetPosition(offset).Write(patch);
        }

        private void WritePatch(Stream stream, long offset, string patch)
        {
            if (offset == 0)
                return;

            stream.SetPosition(offset);
            foreach (var ch in patch)
                stream.Write((byte)ch);
        }

        private static string ReadString(Stream stream, int ptr)
        {
            const int MaxFileNameLength = 0x30;
            static bool IsValidFileName(string fileName) => fileName.Length >= 2 &&
                char.IsLetterOrDigit(fileName[0]) && char.IsLetterOrDigit(fileName[1]);

            var rawFileName = stream.SetPosition(ptr).ReadBytes(MaxFileNameLength);
            var sbFileName = new StringBuilder();
            for (var i = 0; i < rawFileName.Length; i++)
            {
                var ch = (char)rawFileName[i];
                if (ch == '\0')
                    break;
                sbFileName.Append(ch);
            }

            var fileName = sbFileName.ToString();
            if (!IsValidFileName(fileName))
                return null;

            return fileName;
        }
    }
}
