using OpenKh.Common;
using OpenKh.Tools.Common;
using OpenKh.Tools.ModsManager.Interfaces;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class Pcsx2Injector
    {
        private class Offsets
        {
            public string GameName { get; init; }
            public int LoadFile { get; init; }
            public int GetFileSize { get; init; }
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

        private const string KH2FM = "SLPM_666.75;1";

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
        private const uint SP = 0x1D;
        private const uint RA = 0x1F;

        private static uint NOP() => 0x00000000;
        private static uint JR(uint reg) => 0x00000008U | (reg << 21);
        private static uint SYSCALL() => 0x0000000C;
        private static uint JAL(uint offset) =>
            0x0C000000U | (offset / 4);
        private static uint BLTZ(uint reg, short jump) =>
            0x04000000U | (0 << 16) | (reg << 21) | (ushort)jump;
        private static uint BGEZ(uint reg, short jump) =>
            0x04000000U | (1 << 16) | (reg << 21) | (ushort)jump;
        private static uint BLTZL(uint reg, short jump) =>
            0x04000000U | (2 << 16) | (reg << 21) | (ushort)jump;
        private static uint BGEZL(uint reg, short jump) =>
            0x04000000U | (1 << 16) | (reg << 21) | (ushort)jump;
        private static uint BEQ(uint left, uint right, short jump) =>
            0x10000000U | (right << 16) | (left << 21) | (ushort)jump;
        private static uint BNE(uint left, uint right, short jump) =>
            0x14000000U | (right << 16) | (left << 21) | (ushort)jump;
        private static uint ADDIU(uint dst, uint src, short value) =>
            0x24000000U | (dst << 16) | (src << 21) | (ushort)value;
        private static uint LUI(uint dst, ushort value) =>
            0x3C000000U | (dst << 16) | value;
        private static uint LW(uint dst, uint src, short offset) =>
            0x8C000000U | (dst << 16) | (src << 21) | (ushort)offset;
        private static uint SW(uint src, uint dst, short offset) =>
            0xAC000000U | (src << 16) | (dst << 21) | (ushort)offset;
        private static uint LD(uint src, uint dst, short offset) =>
            0xDC000000U | (src << 16) | (dst << 21) | (ushort)offset;
        private static uint SD(uint src, uint dst, short offset) =>
            0xFC000000U | (src << 16) | (dst << 21) | (ushort)offset;


        public enum Operation
        {
            HookExit,
            LoadFile,
            GetFileSize
        }

        private const uint BaseHookPtr = 0xFFF00;
        private const int HookStack = 0x0F;
        private const int ParamOperator = -0x04;
        private const int Param1 = -0x08;
        private const int Param2 = -0x0C;
        private const int Param3 = -0x10;
        private const int Param4 = -0x14;
        private const int ParamReturn = Param1;

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
                    default:
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

        private void WritePatch(Stream stream, Offsets offsets)
        {
            ResetHooks();
            if (offsets != null)
            {
                if (offsets.LoadFile > 0)
                {
                    Log.Info("Injecting {0} function", offsets.LoadFile);
                    WritePatch(stream, offsets.LoadFile,
                        ADDIU(T4, RA, 0),
                        JAL(WriteHook(stream, LoadFileHook)),
                        ADDIU(T5, Zero, (byte)Operation.LoadFile));
                }

                if (offsets.GetFileSize > 0)
                {
                    Log.Info("Injecting {0} function", offsets.GetFileSize);
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
