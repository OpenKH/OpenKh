using OpenKh.Common;
using OpenKh.Tools.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xe.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public class Pcsx2Injector
    {
        private class Patch
        {
            public string Game { get; set; }
            public string Name { get; set; }
            public int Address { get; set; }
            public uint[] Pattern { get; set; }
            public uint[] NewPattern { get; set; }
        }

        private static readonly uint[] LoadFilePatch = new uint[]
        {
            0x3c0e0010, // lui	   t6, 0x0010       store in 0x100000
            0xadc4fff4, // sw	   a0, -0xC(t6)     const char* filename
            0xadc5fff8, // sw	   a1, -0x8(t6)     void* memDst
            0x240d0001, // addiu   t5, zero, 1      mode = LoadFile
            0xadcdfffc, // sw      t5, -0x4(t6)     
            0x8dcdfffc, // lw      t5, -0x4(t6)     loop until mode=0
            0x15a0fffe, // bne     t5, zero, 0x1682cc
            0x8dc2fff8, // lw      v0, -0x8(t6)     check return value
            0x14400002, // bne     v0, zero, 0x1682e4
            0x24030064, // addiu   v1,zero,0x64
            0x0000000C, // syscall
            0x00000000, // nop
            0x03e00008, // jr      ra
            0x00000000, // nop
        };

        private static readonly uint[] GetFileSizePatch = new uint[]
        {
            0x3c0e0010, // lui	   t6, 0x0010
            0xadc4fff8, // sw	   a0, -0x8(t6) const char* filename
            0x240d0002, // addiu   t5, zero, 2  mode = GetFileSize
            0xadcdfffc, // sw      t5, -0x4(t6) int mode
            0x8dcdfffc, // lw      t5, -0x4(t6)
            0x15a0fffe, // bne     t5, zero, -8
            0x00000000, // nop
            0x8dc2fff8, // lw      v0, -0x8(t6)
            0x03e00008, // jr      $ra
            0x00000000, // nop
        };
        private const string KH2FM = "SLPM_666.75;1";
        private const string KH2JP = "SLPM_662.33;1";
        private const string KH2US = "SLUS_210.05;1";
        private const string KH2UK = "SLES_541.14;1";
        private const string KH2FR = "SLES_542.32;1";
        private const string KH2DE = "";
        private const string KH2IT = "SLES_542.34;1";
        private const string KH2ES = "SLES_542.35;1";

        private static readonly Patch[] _patches = new Patch[]
        {
            new Patch
            {
                Game = KH2JP,
                Name = "LoadFile",
                Address = 0x167F20,
                NewPattern = LoadFilePatch,
            },
            new Patch
            {
                Game = KH2JP,
                Name = "GetFileSize",
                Address = 0x1AC698,
                NewPattern = GetFileSizePatch,
            },
            new Patch
            {
                Game = KH2US,
                Name = "LoadFile",
                Address = 0x167C50,
                NewPattern = LoadFilePatch,
            },
            new Patch
            {
                Game = KH2US,
                Name = "GetFileSize",
                Address = 0x1AC760,
                NewPattern = GetFileSizePatch,
            },
            new Patch
            {
                Game = KH2UK,
                Name = "LoadFile",
                Address = 0x167CA8,
                NewPattern = LoadFilePatch,
            },
            new Patch
            {
                Game = KH2UK,
                Name = "GetFileSize",
                Address = 0x1AC858,
                NewPattern = GetFileSizePatch,
            },
            new Patch
            {
                Game = KH2FR,
                Name = "LoadFile",
                Address = 0x167CA8,
                NewPattern = LoadFilePatch,
            },
            new Patch
            {
                Game = KH2FR,
                Name = "GetFileSize",
                Address = 0x1AC858,
                NewPattern = GetFileSizePatch,
            },
            new Patch
            {
                Game = KH2DE,
                Name = "LoadFile",
                Address = 0x167CA8,
                NewPattern = LoadFilePatch,
            },
            new Patch
            {
                Game = KH2DE,
                Name = "GetFileSize",
                Address = 0x1AC858,
                NewPattern = GetFileSizePatch,
            },
            new Patch
            {
                Game = KH2IT,
                Name = "LoadFile",
                Address = 0x167CA8,
                NewPattern = LoadFilePatch,
            },
            new Patch
            {
                Game = KH2IT,
                Name = "GetFileSize",
                Address = 0x1AC858,
                NewPattern = GetFileSizePatch,
            },
            new Patch
            {
                Game = KH2ES,
                Name = "LoadFile",
                Address = 0x167CA8,
                NewPattern = LoadFilePatch,
            },
            new Patch
            {
                Game = KH2ES,
                Name = "GetFileSize",
                Address = 0x1AC858,
                NewPattern = GetFileSizePatch,
            },
            new Patch
            {
                Game = KH2FM,
                Name = "LoadFile",
                Address = 0x1682b8,
                NewPattern = LoadFilePatch,
            },
            new Patch
            {
                Game = KH2FM,
                Name = "GetFileSize",
                Address = 0x1AE1B0,
                NewPattern = GetFileSizePatch,
            },
        };

        private readonly IOperationDispatcher _operationDispatcher;
        private const int OperationAddress = 0xFFFFC;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _injectorTask;

        public Pcsx2Injector(IOperationDispatcher operationDispatcher)
        {
            _operationDispatcher = operationDispatcher;
        }

        public void Run(Process process)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _injectorTask = Task.Run(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    var gameName = await Pcsx2MemoryService.GetPcsx2ApplicationName(process, _cancellationToken);
                    var processStream = new ProcessStream(process, 0x20000000, 0x2000000);
                    WritePatch(processStream, gameName);
                    MainLoop(processStream);
                }
            }, _cancellationToken);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _injectorTask.Wait();
        }

        private void MainLoop(Stream stream)
        {
            var isProcessDead = false;
            while (!_cancellationToken.IsCancellationRequested && !isProcessDead)
            {
                var operation = stream.SetPosition(OperationAddress).ReadInt32();
                switch (operation)
                {
                    case 1:
                        OperationCopyFile(stream);
                        break;
                    case 2:
                        OperationGetFileSize(stream);
                        break;
                    default:
                        break;
                    case 0:
                        Thread.Sleep(1);
                        continue;
                }

                stream.SetPosition(OperationAddress).Write(0);
            }
        }

        private void OperationCopyFile(Stream stream)
        {
            const int ParameterCount = 2;
            stream.SetPosition(OperationAddress - ParameterCount * sizeof(uint));

            var ptrFileName = stream.ReadInt32();
            var ptrMemDst = stream.ReadInt32();
            var fileName = ReadString(stream, ptrFileName);
            if (string.IsNullOrEmpty(fileName))
                return;

            var dstStream = new BufferedStream(new SubStream(stream, ptrMemDst, stream.Length - ptrMemDst));
            var returnValue = _operationDispatcher.LoadFile(dstStream, fileName);
            dstStream.Flush();

            stream.SetPosition(OperationAddress - 4).Write(returnValue ? 1 : 0);
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

        private void WritePatch(Stream stream, string game)
        {
            var bufferedStream = new BufferedStream(stream);
            foreach (var patch in _patches.Where(x => x.Game == game))
                WritePatch(bufferedStream, patch);
            bufferedStream.Flush();
        }

        private void WritePatch(Stream stream, Patch patch)
        {
            stream.SetPosition(patch.Address);
            foreach (var word in patch.NewPattern)
                stream.Write(word);
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
