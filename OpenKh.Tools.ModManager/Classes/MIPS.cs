using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.Classes
{
    public static class MIPS
    {
        // Converted from https://github.com/Xeeynamo/mipsdump/blob/master/mipsdump/Instructions.fs

        public const uint Zero = 0x00;
        public const uint AT = 0x01;
        public const uint V0 = 0x02;
        public const uint V1 = 0x03;
        public const uint A0 = 0x04;
        public const uint A1 = 0x05;
        public const uint A2 = 0x06;
        public const uint A3 = 0x07;
        public const uint T0 = 0x08;
        public const uint T1 = 0x09;
        public const uint T2 = 0x0A;
        public const uint T3 = 0x0B;
        public const uint T4 = 0x0C;
        public const uint T5 = 0x0D;
        public const uint T6 = 0x0E;
        public const uint T7 = 0x0F;
        public const uint S0 = 0x10;
        public const uint S1 = 0x11;
        public const uint S2 = 0x12;
        public const uint S3 = 0x13;
        public const uint S4 = 0x14;
        public const uint S5 = 0x15;
        public const uint S6 = 0x16;
        public const uint S7 = 0x17;
        public const uint T8 = 0x18;
        public const uint T9 = 0x19;
        public const uint K0 = 0x1A;
        public const uint K1 = 0x1B;
        public const uint GP = 0x1C;
        public const uint SP = 0x1D;
        public const uint FP = 0x1E;
        public const uint RA = 0x1F;

        public enum Regimm
        {
            BLTZ = 0x00,
            BGEZ = 0x01,
            BLTZAL = 0x10,
            BGEZAL = 0x11,
        }

        public enum Op
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

        public enum Special
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

        public enum Cop
        {

            MFC = 0x00,
            CFC = 0x02,
            MTC = 0x04,
            CTC = 0x06,
            BC = 0x10,
        }

        // R-type instructions
        public static uint SLL(uint dst, uint src, uint value) => (uint)Special.SLL | (dst << 11) | (src << 16) | ((value & 0x1Fu) << 6);
        public static uint SRL(uint dst, uint src, uint value) => (uint)Special.SRL | (dst << 11) | (src << 16) | ((value & 0x1Fu) << 6);
        public static uint SRA(uint dst, uint src, uint value) => (uint)Special.SRA | (dst << 11) | (src << 16) | ((value & 0x1Fu) << 6);
        public static uint SLLV(uint dst, uint left, uint right) => (uint)Special.SLLV | (dst << 11) | left << 16 | right << 21;
        public static uint SRAV(uint dst, uint left, uint right) => (uint)Special.SRAV | (dst << 11) | left << 16 | right << 21;
        public static uint SRLV(uint dst, uint left, uint right) => (uint)Special.SRLV | (dst << 11) | left << 16 | right << 21;
        public static uint JR(uint reg) => (uint)Special.JR | ((reg) << 21);
        public static uint JALR(uint reg) => (uint)Special.JALR | ((reg) << 21);
        public static uint SYSCALL(uint code) => (uint)Special.SYSCALL | ((code & 0xffffffu) << 6);
        public static uint BREAK(uint code1, uint code2) => (uint)Special.BREAK | ((code1 & 0x3ffu) << 16) | ((code2 & 0x3ffu) << 6);
        public static uint MFHI(uint reg) => (uint)Special.MFHI | ((reg) << 11);
        public static uint MTHI(uint reg) => (uint)Special.MTHI | ((reg) << 11);
        public static uint MFLO(uint reg) => (uint)Special.MFLO | ((reg) << 11);
        public static uint MTLO(uint reg) => (uint)Special.MTLO | ((reg) << 11);
        public static uint MULT(uint dst, uint src) => (uint)Special.MULT | (src << 16) | (dst << 21);
        public static uint MULTU(uint dst, uint src) => (uint)Special.MULTU | (src << 16) | (dst << 21);
        public static uint DIV(uint dst, uint src) => (uint)Special.DIV | (src << 16) | (dst << 21);
        public static uint DIVU(uint dst, uint src) => (uint)Special.DIVU | (src << 16) | (dst << 21);
        public static uint ADD(uint dst, uint left, uint right) => (uint)Special.ADD | (dst << 11) | (left << 21) | (right << 16);
        public static uint ADDU(uint dst, uint left, uint right) => (uint)Special.ADDU | (dst << 11) | (left << 21) | (right << 16);
        public static uint SUB(uint dst, uint left, uint right) => (uint)Special.SUB | (dst << 11) | (left << 21) | (right << 16);
        public static uint SUBU(uint dst, uint left, uint right) => (uint)Special.SUBU | (dst << 11) | (left << 21) | (right << 16);
        public static uint AND(uint dst, uint left, uint right) => (uint)Special.AND | (dst << 11) | (left << 21) | (right << 16);
        public static uint OR(uint dst, uint left, uint right) => (uint)Special.OR | (dst << 11) | (left << 21) | (right << 16);
        public static uint XOR(uint dst, uint left, uint right) => (uint)Special.XOR | (dst << 11) | (left << 21) | (right << 16);
        public static uint NOR(uint dst, uint left, uint right) => (uint)Special.NOR | (dst << 11) | (left << 21) | (right << 16);
        public static uint SLT(uint dst, uint left, uint right) => (uint)Special.SLT | (dst << 11) | (left << 21) | (right << 16);
        public static uint SLTU(uint dst, uint left, uint right) => (uint)Special.SLTU | (dst << 11) | (left << 21) | (right << 16);

        // Alias R-type instructions
        public static uint NOP() => SLL(Zero, Zero, 0u);
        public static uint MOVE(uint dst, uint src) => OR(dst, src, Zero);
        public static uint NEG(uint dst, uint src) => SUB(dst, Zero, src);
        public static uint NEGU(uint dst, uint src) => SUBU(dst, Zero, src);

        // I-type instructions
        public static uint BLTZ(uint reg, short imm) => (ushort)imm | ((uint)Regimm.BLTZ << 16) | (reg << 21) | ((uint)Op.REGIMM << 26);
        public static uint BGEZ(uint reg, short imm) => (ushort)imm | ((uint)Regimm.BGEZ << 16) | (reg << 21) | ((uint)Op.REGIMM << 26);
        public static uint BLTZAL(uint reg, short imm) => (ushort)imm | ((uint)Regimm.BLTZAL << 16) | (reg << 21) | ((uint)Op.REGIMM << 26);
        public static uint BGEZAL(uint reg, short imm) => (ushort)imm | ((uint)Regimm.BGEZAL << 16) | (reg << 21) | ((uint)Op.REGIMM << 26);
        public static uint BEQ(uint left, uint right, short imm) => (ushort)imm | (right << 16) | (left << 21) | ((uint)Op.BEQ << 26);
        public static uint BNE(uint left, uint right, short imm) => (ushort)imm | (right << 16) | (left << 21) | ((uint)Op.BNE << 26);
        public static uint BLEZ(uint reg, short imm) => (ushort)imm | (reg << 21) | ((uint)Op.BLEZ << 26);
        public static uint BGTZ(uint reg, short imm) => (ushort)imm | (reg << 21) | ((uint)Op.BGTZ << 26);
        public static uint ADDI(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.ADDI << 26);
        public static uint ADDIU(uint dst, uint src, ushort imm) => imm | (dst << 16) | (src << 21) | ((uint)Op.ADDIU << 26);
        public static uint ADDIU(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.ADDIU << 26);
        public static uint SLTI(uint left, uint right, short imm) => (ushort)imm | (left << 16) | (right << 21) | ((uint)Op.SLTI << 26);
        public static uint SLTIU(uint left, uint right, short imm) => (ushort)imm | (left << 16) | (right << 21) | ((uint)Op.SLTIU << 26);
        public static uint ANDI(uint dst, uint src, ushort imm) => imm | (dst << 16) | (src << 21) | ((uint)Op.ANDI << 26);
        public static uint ORI(uint dst, uint src, ushort imm) => imm | (dst << 16) | (src << 21) | ((uint)Op.ORI << 26);
        public static uint XORI(uint dst, uint src, ushort imm) => imm | (dst << 16) | (src << 21) | ((uint)Op.XORI << 26);
        public static uint LUI(uint dst, ushort imm) => imm | (dst << 16) | ((uint)Op.LUI << 26);
        public static uint LB(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LB << 26);
        public static uint LH(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LH << 26);
        public static uint LWL(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWL << 26);
        public static uint LW(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LW << 26);
        public static uint LBU(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LBU << 26);
        public static uint LHU(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LHU << 26);
        public static uint LWR(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWR << 26);
        public static uint SB(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SB << 26);
        public static uint SH(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SH << 26);
        public static uint SWL(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWL << 26);
        public static uint SW(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SW << 26);
        public static uint SWR(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWR << 26);

        // Alias I-type instructions
        public static uint BAL(uint reg, short imm) => BGEZAL(reg, imm);
        public static uint LI(uint dst, short imm) => ADDIU(dst, Zero, (ushort)imm);
        public static uint LIU(uint dst, ushort imm) => ORI(dst, Zero, imm);

        // Co-processor instructions
        public static uint MFC0(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MFC << 21) | ((uint)Op.C0 << 26);
        public static uint MFC1(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MFC << 21) | ((uint)Op.C1 << 26);
        public static uint MFC2(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MFC << 21) | ((uint)Op.C2 << 26);
        public static uint MFC3(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MFC << 21) | ((uint)Op.C3 << 26);
        public static uint MTC0(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MTC << 21) | ((uint)Op.C0 << 26);
        public static uint MTC1(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MTC << 21) | ((uint)Op.C1 << 26);
        public static uint MTC2(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MTC << 21) | ((uint)Op.C2 << 26);
        public static uint MTC3(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.MTC << 21) | ((uint)Op.C3 << 26);
        public static uint CFC0(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CFC << 21) | ((uint)Op.C0 << 26);
        public static uint CFC1(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CFC << 21) | ((uint)Op.C1 << 26);
        public static uint CFC2(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CFC << 21) | ((uint)Op.C2 << 26);
        public static uint CFC3(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CFC << 21) | ((uint)Op.C3 << 26);
        public static uint CTC0(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CTC << 21) | ((uint)Op.C0 << 26);
        public static uint CTC1(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CTC << 21) | ((uint)Op.C1 << 26);
        public static uint CTC2(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CTC << 21) | ((uint)Op.C2 << 26);
        public static uint CTC3(uint dst, uint src) => ((src & 0x1Fu) << 11) | (dst << 16) | ((uint)Cop.CTC << 21) | ((uint)Op.C3 << 26);
        public static uint COP0(uint offset) => (offset & 0x1FFFFFFu) | (1u << 25) | ((uint)Op.C0 << 26);
        public static uint COP1(uint offset) => (offset & 0x1FFFFFFu) | (1u << 25) | ((uint)Op.C1 << 26);
        public static uint COP2(uint offset) => (offset & 0x1FFFFFFu) | (1u << 25) | ((uint)Op.C2 << 26);
        public static uint COP3(uint offset) => (offset & 0x1FFFFFFu) | (1u << 25) | ((uint)Op.C3 << 26);
        public static uint LWC0(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWC0 << 26);
        public static uint LWC1(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWC1 << 26);
        public static uint LWC2(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWC2 << 26);
        public static uint LWC3(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.LWC3 << 26);
        public static uint SWC0(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWC0 << 26);
        public static uint SWC1(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWC1 << 26);
        public static uint SWC2(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWC2 << 26);
        public static uint SWC3(uint dst, uint src, short imm) => (ushort)imm | (dst << 16) | (src << 21) | ((uint)Op.SWC3 << 26);
        public static uint BC0F(short offset) => (ushort)offset | (0x10u << 26) | (0x10u << 20) | (0u << 16);
        public static uint BC0T(short offset) => (ushort)offset | (0x10u << 26) | (0x10u << 20) | (1u << 16);
        public static uint BC1F(short offset) => (ushort)offset | (0x11u << 26) | (0x10u << 20) | (0u << 16);
        public static uint BC1T(short offset) => (ushort)offset | (0x11u << 26) | (0x10u << 20) | (1u << 16);
        public static uint BC2F(short offset) => (ushort)offset | (0x12u << 26) | (0x10u << 20) | (0u << 16);
        public static uint BC2T(short offset) => (ushort)offset | (0x12u << 26) | (0x10u << 20) | (1u << 16);
        public static uint BC3F(short offset) => (ushort)offset | (0x13u << 26) | (0x10u << 20) | (0u << 16);
        public static uint BC3T(short offset) => (ushort)offset | (0x13u << 26) | (0x10u << 20) | (1u << 16);

        // J-type instructions
        public static uint J(uint addr) => (addr & 0x3FFFFFFu) | ((uint)Op.J << 26);
        public static uint JAL(uint addr) => ((addr / 4) & 0x3FFFFFFu) | ((uint)Op.JAL << 26);

        // PlayStation 2 specific instructions
        public static uint LD(uint src, uint dst, short imm) => (ushort)imm | (src << 16) | (dst << 21) | ((uint)Op.LD << 26);
        public static uint SD(uint src, uint dst, short imm) => (ushort)imm | (src << 16) | (dst << 21) | ((uint)Op.SD << 26);
    }
}
