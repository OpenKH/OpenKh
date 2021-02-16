using OpenKh.Common;
using System;

namespace OpenKh.Ps2
{
    /// <summary>
    /// EE_Users_Manual
    /// 6.4 VIFcode Reference
    /// </summary>
    public class VifUnpacker
    {
        public enum State
        {
            End,
            Run,
            Interrupt,
            Microprogram,
        }

        public enum MaskType
        {
            Write,
            Row,
            Col,
            Skip
        }

        private struct VIFn_Cycle
        {
            public readonly byte CL;
            public readonly byte WL;

            public bool IsSkippingWrite => CL >= WL;
            public bool IsFillingWrite => CL < WL;

            public VIFn_Cycle(ushort immediate)
            {
                CL = (byte)(immediate & 0xFF);
                WL = (byte)((immediate >> 8) & 0xFF);
            }
        }

        private struct VIFn_Mask
        {
            public VIFn_Mask(uint value)
            {
                Masks = new Vector4Mask[]
                {
                    new Vector4Mask((value >> 0) & 0xff),
                    new Vector4Mask((value >> 8) & 0xff),
                    new Vector4Mask((value >> 16) & 0xff),
                    new Vector4Mask((value >> 24) & 0xff),
                };
            }

            public Vector4Mask[] Masks { get; }
        }

        private struct Vector4Mask
        {
            public Vector4Mask(uint value)
            {
                Mask = (byte)value;
            }

            public byte Mask { get; }

            public MaskType X => (MaskType)((Mask >> 0) & 3);
            public MaskType Y => (MaskType)((Mask >> 2) & 3);
            public MaskType Z => (MaskType)((Mask >> 4) & 3);
            public MaskType W => (MaskType)((Mask >> 6) & 3);
        }

        private struct Opcode
        {
            private readonly uint _opcode;

            public ushort Immediate => (ushort)(_opcode & 0xffff);
            public byte Num => (byte)((_opcode >> 16) & 0xff);
            public byte Cmd => (byte)((_opcode >> 24) & 0x7f);
            public bool Interrupt => _opcode >= 0x80000000;

            public bool IsUnpack => (Cmd & CmdMaskUnpack) == CmdMaskUnpack;
            public uint UnpackAddress => _opcode & 0x1ff;
            public bool UnpackIsUnsigned => (_opcode & 0x400) == 0;
            public bool UnpackAddsTops => (_opcode & 0x800) != 0;
            public uint UnpackVl => (_opcode >> 24) & 3;
            public uint UnpackVn => (_opcode >> 26) & 3;
            public bool UnpackMask => ((_opcode >> 28) & 1) != 0;

            public Opcode(uint opcode)
            {
                _opcode = opcode;
            }

            public static Opcode Read(byte[] code, int pc)
            {
                var opcode = (uint)(
                    code[pc + 0] |
                    (code[pc + 1] << 8) |
                    (code[pc + 2] << 16) |
                    (code[pc + 3] << 24));

                return new Opcode(opcode);
            }
        }

        private const int OpcodeAlignment = 0x4;
        private const int VertexAlignment = 0x10;

        // Used to adjust the data alignment in the VIF packet
        private const byte CmdNop = 0b0000000;

        // Writes the value of the immediate to VIFn_CYCLE register
        private const byte CmdStcycl = 0b0000001;

        // Activates the microprogram
        private const byte CmdMscal = 0b0010100;

        // Activates the microprogram
        private const byte CmdMscnt = 0b0010111;

        // Sets the data mask pattern
        private const byte CmdStmask = 0b0100000;

        // Sets the filling data for row registers
        private const byte CmdStrow = 0b0110000;

        // Sets the filling data for column registers
        private const byte CmdStcol = 0b0110001;

        // Transfer data to the VU Mem
        private const byte CmdMaskUnpack = 0b1100000;

        private Func<uint>[] _readSigned;
        private Func<uint>[] _readUnsigned;
        private Action<Func<uint>>[] _unpacker;

        private Vector4Mask DefaultMask = new Vector4Mask(0);
        private readonly byte[] _code;
        private readonly byte[] _mem = new byte[16 * 1024];
        private readonly uint[] _vifnCol;
        private readonly uint[] _vifnRow;
        private int _programCounter;
        private VIFn_Cycle _vifnCycle;
        private VIFn_Mask _vifnMask;
        private int _destinationAddress;
        private int _unpackMaskIndex;
        private bool _enableMask;

        public VifUnpacker(byte[] code)
        {
            _code = code;
            _programCounter = 0;
            _vifnMask = new VIFn_Mask(0);
            _vifnCol = new uint[4];
            _vifnRow = new uint[4];

            _readSigned = new Func<uint>[]
            {
                    ReadInt32,
                    ReadInt16,
                    ReadInt8,
                    ReadInt16,
            };
            _readUnsigned = new Func<uint>[]
            {
                    ReadUInt32,
                    ReadUInt16,
                    ReadUInt8,
                    ReadUInt16
            };
            _unpacker = new Action<Func<uint>>[]
            {
                UnpackSingle,
                UnpackVector2,
                UnpackVector3,
                UnpackVector4,
            };
        }

        public byte[] Memory => _mem;

        public int Vif1_Tops { get; set; }

        private Vector4Mask NextMask()
        {
            if (_enableMask)
                return _vifnMask.Masks[(_unpackMaskIndex++) & 3];

            return DefaultMask;
        }

        public State Run()
        {
            while (true)
            {
                var state = Step();
                if (state == State.End ||
                    state == State.Microprogram)
                    return state;
            }
        }

        private State Step()
        {
            if (_programCounter >= _code.Length)
                return State.End;

            var opcode = new Opcode(ReadUInt32());

            if (opcode.IsUnpack)
            {
                Unpack(opcode);
            }
            else
            {
                switch (opcode.Cmd)
                {
                    case CmdNop:
                        break;
                    case CmdStcycl:
                        // KH2 is not really using it.. so we are going to to the same.
                        _vifnCycle = new VIFn_Cycle(opcode.Immediate);
                        break;
                    case CmdMscal:
                        // opcode.Immediate needs to be used as execution address for the microprogram.
                        return State.Microprogram;
                    case CmdMscnt:
                        // The difference with Mscal is that the execution address will be the
                        // most recent end of the previous microcode execution.
                        return State.Microprogram;
                    case CmdStmask:
                        _vifnMask = new VIFn_Mask(ReadUInt32());
                        break;
                    case CmdStrow:
                        _vifnRow[0] = ReadUInt32();
                        _vifnRow[1] = ReadUInt32();
                        _vifnRow[2] = ReadUInt32();
                        _vifnRow[3] = ReadUInt32();
                        break;
                    case CmdStcol:
                        _vifnCol[0] = ReadUInt32();
                        _vifnCol[1] = ReadUInt32();
                        _vifnCol[2] = ReadUInt32();
                        _vifnCol[3] = ReadUInt32();
                        break;
                    default:
                        throw new Exception($"VIF1 cmd {opcode.Cmd:X02}@{_programCounter:X} not implemented!");
                }
            }

            return State.Run;
        }

        private void Unpack(Opcode opcode)
        {
            _destinationAddress = (int)opcode.UnpackAddress;
            //if (opcode.UnpackAddsTops)
                _destinationAddress += Vif1_Tops;
            _destinationAddress *= VertexAlignment;
            _unpackMaskIndex = 0;

            var reader = opcode.UnpackIsUnsigned ?
                _readUnsigned[opcode.UnpackVl] :
                _readSigned[opcode.UnpackVl];
            var unpacker = _unpacker[opcode.UnpackVn];
            _enableMask = opcode.UnpackMask;

            for (var i = 0; i < opcode.Num; i++)
            {
                unpacker(reader);
            }

            _programCounter = Helpers.Align(_programCounter, OpcodeAlignment);
        }

        private uint ReadInt8() => (uint)(sbyte)_code[_programCounter++];
        private uint ReadUInt8() => _code[_programCounter++];
        private uint ReadInt16() => (uint)(short)(
            _code[_programCounter++] | (_code[_programCounter++] << 8));
        private uint ReadUInt16() => (ushort)(
            _code[_programCounter++] | (_code[_programCounter++] << 8));
        private uint ReadInt32() => (uint)(
            _code[_programCounter++] | (_code[_programCounter++] << 8) |
            (_code[_programCounter++] << 16) | (_code[_programCounter++] << 24));
        private uint ReadUInt32() => (uint)(
            _code[_programCounter++] | (_code[_programCounter++] << 8) |
            (_code[_programCounter++] << 16) | (_code[_programCounter++] << 24));

        public void UnpackSingle(Func<uint> reader)
        {
            var currentMask = NextMask();
            var value = reader();

            if (currentMask.X == MaskType.Write) Write(value); Next();
            if (currentMask.Y == MaskType.Write) Write(value); Next();
            if (currentMask.Z == MaskType.Write) Write(value); Next();
            if (currentMask.W == MaskType.Write) Write(value); Next();
        }

        public void UnpackVector2(Func<uint> reader)
        {
            var currentMask = NextMask();
            var x = reader();
            var y = reader();

            if (currentMask.X == MaskType.Write) Write(x); Next();
            if (currentMask.Y == MaskType.Write) Write(y); Next();

            // While PS2 docs says that the following two values will
            // be indeterminate, PCSX2 seems to follow this exact
            // logic. Probably to emulate an undefined behaviour.
            // We are going to do the same, just in case.
            if (currentMask.Z == MaskType.Write) Write(x); Next();
            if (currentMask.W == MaskType.Write) Write(y); Next();
        }

        private void UnpackVector3(Func<uint> reader)
        {
            var currentMask = NextMask();

            if (currentMask.X == MaskType.Write) Write(reader()); Next();
            if (currentMask.Y == MaskType.Write) Write(reader()); Next();
            if (currentMask.Z == MaskType.Write) Write(reader()); Next();

            // According to PCSX2, the following logic emulates the
            // behaviour of the real hardware.. Time for some hacks!
            if (currentMask.W == MaskType.Write)
            {
                var oldProgramCounter = _programCounter;
                Write(reader()); // do not call Next() here!
                _programCounter = oldProgramCounter;
            }

            Next();
        }

        private void UnpackVector4(Func<uint> reader)
        {
            var currentMask = NextMask();

            if (currentMask.X == MaskType.Write) Write(reader()); Next();
            if (currentMask.Y == MaskType.Write) Write(reader()); Next();
            if (currentMask.Z == MaskType.Write) Write(reader()); Next();
            if (currentMask.W == MaskType.Write) Write(reader()); Next();
        }

        private void Write(uint value)
        {
            _mem[_destinationAddress + 0] = (byte)(value & 0xff);
            _mem[_destinationAddress + 1] = (byte)((value >> 8) & 0xff);
            _mem[_destinationAddress + 2] = (byte)((value >> 16) & 0xff);
            _mem[_destinationAddress + 3] = (byte)((value >> 24) & 0xff);
        }
        private void Next() => _destinationAddress += 4;
    }
}
