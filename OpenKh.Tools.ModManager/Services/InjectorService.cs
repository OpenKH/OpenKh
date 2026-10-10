using OpenKh.Common;
using OpenKh.Engine.Input;
using OpenKh.Kh2;
using OpenKh.Tools.Common;
using OpenKh.Tools.ModManager.Classes;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xe.IO;
using Loggers = Serilog.Log;

namespace OpenKh.Tools.ModManager.Services
{
    public class InjectorService
    {
        // Y'all need to deal with THIS one until I can switch over to DiscUtil.
        private static readonly string[][] GAME_EXECUTABLES =
        [
            [],
            ["SLPM_666.75;1", "SLES_541.14;1", "SLES_542.32;1", "SLES_542.33;1", "SLES_542.34;1", "SLES_542.35;1", "SLPM_662.33;1", "SLUS_210.05;1", "SLPM_550.19;1", "SCAJ_201.64;1"]
        ];

        private static readonly HashSet<string> DENY_FILES = new HashSet<string>
        {
            "dkmovie.x",
            "dktitle.x",
            "gb.x",
            "wm.x",
            "xl_limit.x",
            "xs_bambi.x",
            "xs_dh_break.x",
            "xs_dumbo.x",
            "xs_genie.x",
            "xs_mushu.x",
            "xs_simba.x",
            "xs_tink.x",

            "ovl_title.x",
            "ovl_shop.x",
            "ovl_movie.x",
            "ovl_gumibattle.x",
            "ovl_gumimenu.x",
            "ovl_gumimenu.x",
        };

        Process _targetProcess;

        CancellationTokenSource _cancelSource;
        CancellationToken _cancelToken;

        uint _loadFileFunc;   
        uint _getFileSizeFunc;
        uint _subFileSizeFunc;

        // Hard-Coded for KH2 rn.
        uint _bufferPtr = 0x350E88;
        uint _regionPtr = 0x33CAF0; // F0 FF BD 27 34 00 ?? 3C 00 00 B0 FF ?? ?? ?? 24 08 00 BF FF
        uint _languagePtr = 0x33CAF4;

        uint _hookPtrLoad = 0xFFF00;
        uint _hookPtrSize = 0xFFF00;

        static uint FetchPattern(Stream stream, byte[] pattern, string mask)
        {
            stream.Position = 0x00;

            var patternLen = mask.Length;
            var maxSearch = stream.Length - patternLen;

            for (uint i = 0; i <= maxSearch; i++)
            {
                int j = 0;

                for (; j < patternLen; j++)
                {
                    stream.Position = i + j;

                    if (mask[j] != '?' && pattern[j] != stream.ReadByte())
                        break;
                }

                if (j == patternLen)
                    return i;
            }

            return uint.MaxValue;
        }

        public Config TargetConfig { get; set; }

        byte ResolvePath(string input, out string? finalPath)
        {
            var _fetchBuildPath = PathService.ResolveBuild(TargetConfig);
            var _fetchDataPath = PathService.ResolveData(TargetConfig);

            var _fetchRegionList = Kh2.Constants.Regions;
            var _fetchFileRegion = _fetchRegionList.FirstOrDefault(x => input.Contains($"/{x}/") || input.EndsWith($".a.{x}"));

            var _fetchRegionInput = input.Replace($"/{_fetchFileRegion}/", $"/fm/")
                                         .Replace($".a.{_fetchFileRegion}", $".a.fm")
                                         .Replace($".apdx", $".a.fm");


            if (DENY_FILES.Contains(input))
            {
                finalPath = null;
                return 0x00;
            }

            var _fetchBuildFile = Path.Combine(_fetchBuildPath, input);
            var _fetchBuildFileRegion = Path.Combine(_fetchBuildPath, _fetchRegionInput);

            if (File.Exists(_fetchBuildFile))
            {
                finalPath = _fetchBuildFile;
                return 0x01;
            }

            else if (File.Exists(_fetchBuildFileRegion))
            {
                finalPath = _fetchBuildFileRegion;
                return 0x01;
            }

            finalPath = null;
            return 0x03;
        }

        public InjectorService(Config _targetConfig)
        {
            LogService.Show();

            Loggers.ForContext<InjectorService>().Verbose("Welcome to the Dynamic Mod Loader - v0.01!");

            TargetConfig = _targetConfig;

            _cancelSource = new CancellationTokenSource();
            _cancelToken = _cancelSource.Token;

            Stream? _fetchStream = null;
            var _fetchPathEmu = PathService.ResolveEmulator(TargetConfig);

            var _fetchPathGame = _targetConfig.Frontend.TargetGame == Game.KINGDOM_HEARTS ? TargetConfig.Emulator.RomPath[0] : TargetConfig.Emulator.RomPath[1];

            using (var _fetchStreamISO = new FileStream(_fetchPathGame, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                switch (_targetConfig.Frontend.TargetGame)
                {
                    case Game.KINGDOM_HEARTS_II:
                    {
                        foreach (var _gameExec in GAME_EXECUTABLES[0x01])
                        {
                            var _fetchExecOffset = IsoUtility.GetFileOffset(_fetchStreamISO, _gameExec);

                            if (_fetchExecOffset == -1)
                                continue;

                            var _fetchSubStrExec = new SubStream(_fetchStreamISO, _fetchExecOffset * 0x800, _fetchStreamISO.Length - (_fetchExecOffset * 0x800));

                            _loadFileFunc = FetchPattern(_fetchSubStrExec, [0xF0, 0xFF, 0xBD, 0x27, 0x08, 0x00, 0xBF, 0xFF, 0x00, 0x00, 0xB0, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2D, 0x80, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2D, 0x20, 0x00, 0x00, 0x2D, 0x10, 0x00, 0x02], "xxxxxxxxxxxx????xxxxxxxx????xxxxxxxx") + 0xFF000;
                            _getFileSizeFunc = FetchPattern(_fetchSubStrExec, [0xF0, 0xFF, 0xBD, 0x27, 0x00, 0x00, 0xBF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x40, 0x10, 0x2D, 0x78, 0x00, 0x00, 0x0C, 0x00, 0x4F, 0x8C], "xxxxxxxx????xxxxxxxxxxxxxxxx") + 0xFF000;
                            _subFileSizeFunc = FetchPattern(_fetchSubStrExec, [0xF0, 0xFF, 0xBD, 0x27, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xB0, 0xFF, 0x08, 0x00, 0xBF, 0xFF, 0x2D, 0x80, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2D, 0x28, 0x00, 0x02, 0x09, 0x00, 0x40, 0x14], "xxxx????xxxxxxxxxxxx????????xxxxxxxx") + 0xFF000;

                            break;
                        }
                    } break;
                }
            }
          
            _targetProcess = new Process
            {
                StartInfo = new ProcessStartInfo(_fetchPathEmu)
                {
                    Arguments = $"\"{_fetchPathGame}\"",
                    WorkingDirectory = Path.GetDirectoryName(_fetchPathEmu),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                }
            };

            if (_targetProcess.Start())
            {
                if (OperatingSystem.IsLinux())
                {
                    var _fetchProcessID = _targetProcess.Id;

                    Thread.Sleep(500);

                    _fetchStream = new PINEStream();
                }

                else
                {
                    try
                    {
                        var _fetchProcessID = _targetProcess.Id;
                        var _fetchMemoryName = $"pcsx2_{_fetchProcessID}";

                        Thread.Sleep(1000);

                        var _fetchMemoryMap = MemoryMappedFile.OpenExisting(_fetchMemoryName);

                        if (_fetchMemoryMap == null)
                        {
                            _targetProcess.Kill();
                            return;
                        }

                        _fetchStream = _fetchMemoryMap.CreateViewStream();
                    }

                    catch (FileNotFoundException) { _fetchStream = new ProcessStream(_targetProcess, 0x20000000, 0x20000000); }
                }

                if (_fetchStream == null)
                {
                    _targetProcess.Kill();
                    return;
                }

                Task.Run(() =>
                {
                LOOP_START:

                    if (_cancelToken.IsCancellationRequested || _targetProcess.HasExited)
                    {
                        LogService.Dismiss();
                        return;
                    }

                    _fetchStream.SetPosition(_hookPtrLoad);

                    uint[] _fetchHookLoad =
                    [
                        MIPS.LUI(MIPS.T6, 0x0F),
                        MIPS.SW(MIPS.A0, MIPS.T6, -0x08),
                        MIPS.SW(MIPS.A1, MIPS.T6, -0x0C),
                        MIPS.SW(MIPS.T5, MIPS.T6, -0x04),
                        MIPS.LW(MIPS.T5, MIPS.T6, -0x04),
                        MIPS.BNE(MIPS.T5, 0x00, -2),
                        MIPS.LW(MIPS.V0, MIPS.T6, -0x08),
                        MIPS.BEQ(MIPS.V0, MIPS.Zero, 0x02),
                        MIPS.NOP(),
                        MIPS.ADDIU(MIPS.RA, MIPS.RA, 4),
                        MIPS.ADDIU(MIPS.SP, MIPS.SP, -0x10),
                        MIPS.SD(MIPS.T4, MIPS.SP, 0x08),
                        MIPS.SD(MIPS.S0, MIPS.SP, 0x00),
                        MIPS.JR(MIPS.RA),
                        MIPS.NOP()
                    ];

                    uint[] _fetchHookSize =
                    [
                        MIPS.LUI(MIPS.T6, 0x0F),
                        MIPS.SW(MIPS.A0, MIPS.T6, -0x08),
                        MIPS.SW(MIPS.A1, MIPS.T6, -0x0C),
                        MIPS.SW(MIPS.A2, MIPS.T6, -0x10),
                        MIPS.SW(MIPS.A3, MIPS.T6, -0x14),
                        MIPS.SW(MIPS.T5, MIPS.T6, -0x04),
                        MIPS.LW(MIPS.T5, MIPS.T6, -0x04),
                        MIPS.BNE(MIPS.T5, 0x00, -2),
                        MIPS.LW(MIPS.V0, MIPS.T6, -0x08),
                        MIPS.BEQ(MIPS.V0, MIPS.Zero, 0x03),
                        MIPS.NOP(),
                        MIPS.JR(MIPS.T4),
                        MIPS.NOP(),
                        MIPS.ADDIU(MIPS.SP, MIPS.SP, -0x10),
                        MIPS.SD(MIPS.T4, MIPS.SP, 0x08),
                        MIPS.SD(MIPS.S0, MIPS.SP, 0x00),
                        MIPS.JR(MIPS.RA),
                        MIPS.NOP()
                    ];

                    if (_fetchStream.ReadUInt32() == 0x00)
                    {
                        Loggers.ForContext<InjectorService>().Verbose("Injecting hooks for LoadFile and GetFileSize...");

                        byte[] _fetchHookArray = new byte[_fetchHookLoad.Length * sizeof(uint) + _fetchHookSize.Length * sizeof(uint)];

                        Buffer.BlockCopy(_fetchHookLoad, 0, _fetchHookArray, 0, _fetchHookLoad.Length * sizeof(uint));
                        Buffer.BlockCopy(_fetchHookSize, 0, _fetchHookArray, _fetchHookLoad.Length * sizeof(uint), _fetchHookSize.Length * sizeof(uint));

                        _fetchStream.SetPosition(_hookPtrLoad);
                        _fetchStream.Write(_fetchHookArray);

                        _hookPtrSize = _hookPtrLoad + (uint)_fetchHookLoad.Length * sizeof(uint);

                        Loggers.ForContext<InjectorService>().Verbose("Hooks successfully connected!");
                    }

                    if (_loadFileFunc > 0)
                    {
                        _fetchStream.SetPosition(_loadFileFunc + 0x04);

                        if (_fetchStream.ReadUInt32() != MIPS.JAL(_hookPtrLoad))
                        {
                            uint[] _fetchFunction =
                            [
                                MIPS.ADDIU(MIPS.T4, MIPS.RA, 0),
                                MIPS.JAL(_hookPtrLoad),
                                MIPS.ADDIU(MIPS.T5, MIPS.Zero, 0x01)
                            ];

                            byte[] _fetchFunctionArray = new byte[_fetchFunction.Length * sizeof(uint)];
                            Buffer.BlockCopy(_fetchFunction, 0, _fetchFunctionArray, 0, _fetchFunctionArray.Length);

                            _fetchStream.SetPosition(_loadFileFunc);
                            _fetchStream.Write(_fetchFunctionArray);
                        }
                    }

                    if (_getFileSizeFunc > 0)
                    {
                        _fetchStream.SetPosition(_getFileSizeFunc + 0x04);

                        if (_fetchStream.ReadUInt32() != MIPS.JAL(_hookPtrLoad))
                        {
                            uint[] _fetchFunction =
                            [
                                MIPS.ADDIU(MIPS.T4, MIPS.RA, 0),
                                MIPS.JAL(_hookPtrSize),
                                MIPS.ADDIU(MIPS.T5, MIPS.Zero, 0x02),
                                MIPS.JAL(_subFileSizeFunc),
                                MIPS.NOP(),
                                MIPS.BEQ(MIPS.V0, MIPS.Zero, 2),
                                MIPS.NOP(),
                                MIPS.LW(MIPS.V0, MIPS.V0, 0x0C),
                                MIPS.LD(MIPS.RA, MIPS.SP, 0x08),
                                MIPS.JR(MIPS.RA),
                                MIPS.ADDIU(MIPS.SP, MIPS.SP, 0x10),
                            ];

                            byte[] _fetchFunctionArray = new byte[_fetchFunction.Length * sizeof(uint)];
                            Buffer.BlockCopy(_fetchFunction, 0, _fetchFunctionArray, 0, _fetchFunctionArray.Length);

                            _fetchStream.SetPosition(_getFileSizeFunc);
                            _fetchStream.Write(_fetchFunctionArray);
                        }
                    }

                    _fetchStream.Flush();

                    /////////////////////////////////////////////////////////////////////

                    var _fetchOpcodeAddr = (0x0F << 0x10) - 0x04;

                    _fetchStream.SetPosition(_fetchOpcodeAddr);
                    var _fetchOpcode = _fetchStream.ReadInt32();

                    if (_fetchStream.Position == _fetchOpcode)
                    {
                        LogService.Dismiss();
                        return;
                    }

                    switch (_fetchOpcode)
                    {
                        case 0x01:
                        {
                            _fetchStream.SetPosition(_fetchOpcodeAddr - 0x08);

                            var _destinationPtr = _fetchStream.ReadInt32();
                            var _fileNamePtr = _fetchStream.ReadInt32();

                            _fetchStream.SetPosition(_fileNamePtr);

                            var _fetchSize = 0x00;
                            var _fetchName = _fetchStream.ReadString(0x30, Encoding.ASCII);

                            if (String.IsNullOrEmpty(_fetchName))
                                goto LOOP_START;

                            var _couldResolve = ResolvePath(_fetchName, out var _filePath);

                            if (_couldResolve == 0x01)
                            {
                                Loggers.ForContext<InjectorService>().Verbose($"Redirecting file \"{_fetchName}\" to the build path...");

                                var _fetchData = File.ReadAllBytes(_filePath);
                                _fetchSize = _fetchData.Length;

                                _fetchStream.SetPosition(_destinationPtr);
                                _fetchStream.Write(_fetchData);
                            }

                            else
                                Loggers.ForContext<InjectorService>().Verbose($"Loading file \"{_fetchName}\" from the ISO...");

                            _fetchStream.SetPosition(_fetchOpcodeAddr - 0x04);
                            _fetchStream.Write(_fetchSize);

                            _fetchStream.Flush();
                        }
                        break;

                        case 0x02:
                        {
                            _fetchStream.SetPosition(_fetchOpcodeAddr - 0x04);

                            var _fileNamePtr = _fetchStream.ReadInt32();

                            _fetchStream.SetPosition(_fileNamePtr);
                            var _fetchName = _fetchStream.ReadString(0x30, Encoding.ASCII);

                            if (String.IsNullOrEmpty(_fetchName))
                                goto LOOP_START;

                            var _fetchSize = 0x00;
                            var _couldResolve = ResolvePath(_fetchName, out var _filePath);

                            if (_couldResolve == 0x01)
                            {
                                var _fetchInfo = new FileInfo(_filePath);
                                _fetchSize = (int)_fetchInfo.Length;
                            }

                            _fetchStream.SetPosition(_fetchOpcodeAddr - 0x04);
                            _fetchStream.Write(_fetchSize);

                            _fetchStream.Flush();
                        }
                        break;

                        case 0x00:
                            Thread.Sleep(5);
                            goto LOOP_START;
                    }

                    _fetchStream.SetPosition(_fetchOpcodeAddr);
                    _fetchStream.Write(0x00);

                    Thread.Sleep(5);

                    goto LOOP_START;
                }, _cancelToken);
            }
        }
    }
}
