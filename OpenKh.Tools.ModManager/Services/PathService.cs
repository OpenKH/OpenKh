using HarfBuzzSharp;
using Microsoft.Win32;
using OpenKh.Tools.ModManager.Classes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.Services
{
    public static class PathService
    {
        public static string ResolveMod(Config input, bool barePath = false)
        {
            var _fetchGameTarget = barePath ? "" : Config.GameShorthand[input.Frontend.TargetGame];
            var _fetchGamePreset = (barePath || input.Frontend.TargetPreset == null) ? "" : input.Frontend.TargetPreset[(int)input.Frontend.TargetGame];

            if (String.IsNullOrEmpty(_fetchGamePreset))
            {
                var _fetchTargetPath = Path.Combine(AppContext.BaseDirectory, "mods", _fetchGameTarget);

                if (!String.IsNullOrEmpty(input.Frontend.ModPath))
                {
                    if (!Path.IsPathFullyQualified(input.Frontend.ModPath))
                        _fetchTargetPath = Path.Combine(AppContext.BaseDirectory, input.Frontend.ModPath, _fetchGameTarget);

                    else
                        _fetchTargetPath = Path.Combine(input.Frontend.ModPath, _fetchGameTarget);
                }

                if (!Directory.Exists(_fetchTargetPath))
                    Directory.CreateDirectory(_fetchTargetPath);

                return _fetchTargetPath;
            }

            else
            {
                var _fetchNormalStr = _fetchGamePreset.ToLower().Replace(" ", "_");

                foreach (var _fetchChar in Path.GetInvalidFileNameChars())
                    _fetchNormalStr = _fetchNormalStr.Replace(_fetchChar, '-');

                var _fetchFinalPath = Path.Combine(ResolvePreset(input, false), _fetchNormalStr);

                if (!Directory.Exists(_fetchFinalPath))
                    Directory.CreateDirectory(_fetchFinalPath);

                return _fetchFinalPath;
            }
        }

        public static string ResolveBuild(Config input, bool barePath = false)
        {
            var _fetchGameTarget = barePath ? "" : Config.GameShorthand[input.Frontend.TargetGame];

            var _fetchTargetPath = Path.Combine(AppContext.BaseDirectory, "build", _fetchGameTarget);

            if (!String.IsNullOrEmpty(input.Frontend.BuildPath))
            {
                if (!Path.IsPathFullyQualified(input.Frontend.BuildPath))
                    _fetchTargetPath = Path.Combine(AppContext.BaseDirectory, input.Frontend.BuildPath, _fetchGameTarget);

                else
                    _fetchTargetPath = Path.Combine(input.Frontend.BuildPath, _fetchGameTarget);
            }

            if (!Directory.Exists(_fetchTargetPath))
                Directory.CreateDirectory(_fetchTargetPath);

            return _fetchTargetPath;
        }

        public static string ResolvePreset(Config input, bool barePath = false)
        {
            var _fetchGameTarget = barePath ? "" : Config.GameShorthand[input.Frontend.TargetGame];

            var _fetchTargetPath = Path.Combine(AppContext.BaseDirectory, "preset", _fetchGameTarget);

            if (!String.IsNullOrEmpty(input.Frontend.PresetPath))
            {
                if (!Path.IsPathFullyQualified(input.Frontend.PresetPath))
                    _fetchTargetPath = Path.Combine(AppContext.BaseDirectory, input.Frontend.PresetPath, _fetchGameTarget);

                else
                    _fetchTargetPath = Path.Combine(input.Frontend.PresetPath, _fetchGameTarget);
            }

            if (!Directory.Exists(_fetchTargetPath))
                Directory.CreateDirectory(_fetchTargetPath);

            return _fetchTargetPath;
        }

        public static string? ResolveData(Config input, bool barePath = false)
        {
            var _fetchGameTarget = barePath ? "" : Config.GameShorthand[input.Frontend.TargetGame];

            string? _fetchTargetPath = null;

            if (!String.IsNullOrEmpty(input.Frontend.DataPath))
            {
                if (!Path.IsPathFullyQualified(input.Frontend.DataPath))
                    _fetchTargetPath = Path.Combine(AppContext.BaseDirectory, input.Frontend.DataPath, _fetchGameTarget);

                else
                    _fetchTargetPath = Path.Combine(input.Frontend.DataPath, _fetchGameTarget);

                if (!Directory.Exists(_fetchTargetPath))
                    Directory.CreateDirectory(_fetchTargetPath);

                return _fetchTargetPath;
            }

            else
                return null;
        }

        public static string? ResolveEmulator(Config input, bool barePath = false)
        {
            if (String.IsNullOrEmpty(input.Emulator.EmuPath[0]))
                return null;

            else
                return input.Emulator.EmuPath[0];
        }


        public static string ResolvePath1525(Config input)
        {
            var _fetchGamePath = input.Frontend.GamePath;

            if (_fetchGamePath == null)
                return "";

            var _fetchTargetGame = _fetchGamePath[0];

            if (String.IsNullOrEmpty(_fetchTargetGame))
                return "";
            
            return Path.GetFullPath(_fetchTargetGame);
        }

        public static string ResolvePath28(Config input)
        {
            var _fetchGamePath = input.Frontend.GamePath;

            if (_fetchGamePath == null)
                return "";

            var _fetchTargetGame = _fetchGamePath[1];

            if (String.IsNullOrEmpty(_fetchTargetGame))
                return "";
            
            return Path.GetFullPath(_fetchTargetGame);
        }

        public static string? ResolveGame(Config input) => input.Frontend.GamePath != null ? (input.Frontend.TargetGame == Game.DREAM_DROP_DISTANCE ? input.Frontend.GamePath[1] : input.Frontend.GamePath[0]) : null;

        public static bool? ResolveRegionJP(Config input)
        {
            var _fetchGamePath = ResolveGame(input);

            if (input.Frontend.TargetPlatform == Platform.STEAM)
            {
                var _targetStr = input.Frontend.TargetGame == Game.DREAM_DROP_DISTANCE ? "KH2.8_system_WW.png" : "KH1.5+2.5_system_WW.png";
                var _targetExec = Path.Combine(_fetchGamePath, input.Frontend.TargetGame == Game.DREAM_DROP_DISTANCE ? "KINGDOM HEARTS HD 2.8 Launcher.exe" : "KINGDOM HEARTS HD 1.5+2.5 Launcher.exe");

                var _fetchFileRAW = File.ReadAllBytes(_targetExec);

                var _fetchFileSpan = _fetchFileRAW.AsSpan(0, _fetchFileRAW.Length);
                var _fetchStrBytes = Encoding.UTF8.GetBytes(_targetStr);

                if (_fetchFileSpan.IndexOf(_fetchStrBytes) >= 0x00)
                    return false;

                else
                    return true;
            }

            else if (input.Frontend.TargetPlatform == Platform.EPIC_GAMES_STORE)
            {
                var _checkPathJP = Path.Combine(_fetchGamePath, "Image", "jp");
                var _checkPathEN = Path.Combine(_fetchGamePath, "Image", "en");

                if (Directory.Exists(_checkPathJP) && !Directory.Exists(_checkPathEN))
                    return true;

                else
                    return false;
            }

            else
                return null;
        }

        public static string[]? FetchSteamLibraries()
        {
            var _fetchFolders = new List<string>();
            var _fetchGameFolders = new List<string>();

            if (OperatingSystem.IsWindows())
            {
                var _fetchSteamKey = Registry.LocalMachine.OpenSubKey("Software\\Valve\\Steam") ?? Registry.LocalMachine.OpenSubKey("Software\\Wow6432Node\\Valve\\Steam");
                var _fetchInstallDir = _fetchSteamKey.GetValue("InstallPath").ToString();
                _fetchFolders = new List<string>() { _fetchInstallDir };
            }

            else if (OperatingSystem.IsLinux())
            {
                var _fetchHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

                // It is stupid that I have to do this.
                // If someone knows a better way PLEASE tell me.
                var _steamPossibleDirs = new List<string>()
                {
                    Path.Combine(_fetchHome, ".steam/steam"),
                    Path.Combine(_fetchHome, ".local/share/Steam"),
                    Path.Combine(_fetchHome, ".var/app/com.valvesoftware.Steam/.steam"),
                    Path.Combine(_fetchHome, ".var/app/com.valvesoftware.Steam/data/Steam")
                };

                _fetchFolders = _steamPossibleDirs.Where(x => Directory.Exists(x)).ToList();
            }

            _fetchFolders.AsParallel().ForAll(_fetchFolder =>
            {
                var _fetchConfigPath = Path.Combine(_fetchFolder, "steamapps", "libraryfolders.vdf");

                if (File.Exists(_fetchConfigPath))
                {
                    var _fetchLibraryConfig = File.ReadAllLines(_fetchConfigPath);
                    var _pathRegex = new Regex("path[^\"]*\"\\s*\"([^\"]*)\"", RegexOptions.None, TimeSpan.FromMilliseconds(500));

                    _fetchLibraryConfig.AsParallel().ForAll(_fetchLine =>
                    {
                        var _fetchMatch = _pathRegex.Match(_fetchLine);

                        if (_fetchMatch.Success)
                        {
                            var _fetchValue = Regex.Unescape(_fetchMatch.Groups[1].Value);
                            _fetchGameFolders.Add(_fetchValue);
                        }
                    });
                }
            });

            return _fetchGameFolders.Count != 0x00 ? _fetchGameFolders.ToArray() : null;
        }
    }
}
