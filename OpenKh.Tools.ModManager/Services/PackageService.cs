using OpenKh.Tools.ModManager.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tomlyn;
using Tomlyn.Model;

namespace OpenKh.Tools.ModManager.Services
{
    public static class PackageService
    {
        private static Dictionary<Game, string> _backendGameIDs = new Dictionary<Game, string>()
        {
            { Game.KINGDOM_HEARTS, "kh1" },
            { Game.KINGDOM_HEARTS_II, "kh2" },
            { Game.CHAIN_OF_MEMORIES, "recom" },
            { Game.BIRTH_BY_SLEEP, "bbs" },
            { Game.DREAM_DROP_DISTANCE, "kh3d" },
        };

        public static bool EnsurePanacea(Config currentConfig)
        {
            var _fetchPaths = currentConfig.Frontend.GamePath;

            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            if (String.IsNullOrEmpty(_fetchPath1525) && String.IsNullOrEmpty(_fetchPath28))
                return false;

            var _fetchSettings1525 = Path.Combine(_fetchPath1525, "panacea_settings.txt");
            var _fetchAssembly1525 = Path.Combine(_fetchPath1525, OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll");

            var _fetchSettings28 = Path.Combine(_fetchPath28, "panacea_settings.txt");
            var _fetchAssembly28 = Path.Combine(_fetchPath28, OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll");

            var _isConfigValid1525 = false;
            var _isConfigValid28 = false;

            var _regexModPath = new Regex("mod_path=(.*)", RegexOptions.None, TimeSpan.FromMilliseconds(500));

            if (File.Exists(_fetchAssembly1525) && File.Exists(_fetchSettings1525))
            {
                var _fetchSettingsRAW = File.ReadAllLines(_fetchSettings1525);
                var _fetchPanaceaPath = _fetchSettingsRAW.FirstOrDefault(x => _regexModPath.IsMatch(x));

                if (_fetchPanaceaPath != null)
                {
                    var _fetchMatch = _regexModPath.Match(_fetchPanaceaPath);
                    var _fetchValue = _fetchMatch.Groups[1].Value;

                    var _fetchConfigPath = Path.GetFullPath(_fetchValue);

                    var _fetchManagerPath = PathService.ResolveBuild(currentConfig, true);
                    var _comparisonRules = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                    if (String.Equals(_fetchConfigPath, _fetchManagerPath, _comparisonRules))
                        _isConfigValid1525 = true;
                }
            }

            if (File.Exists(_fetchAssembly28) && File.Exists(_fetchSettings28))
            {
                var _fetchSettingsRAW = File.ReadAllLines(_fetchSettings28);
                var _fetchPanaceaPath = _fetchSettingsRAW.FirstOrDefault(x => _regexModPath.IsMatch(x));

                if (_fetchPanaceaPath != null)
                {
                    var _fetchMatch = _regexModPath.Match(_fetchPanaceaPath);
                    var _fetchValue = _fetchMatch.Groups[1].Value;

                    var _fetchConfigPath = Path.GetFullPath(_fetchValue);

                    var _fetchManagerPath = PathService.ResolveBuild(currentConfig, true);
                    var _comparisonRules = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                    if (String.Equals(_fetchConfigPath, _fetchManagerPath, _comparisonRules))
                        _isConfigValid28 = true;
                }
            }

            _isConfigValid1525 = String.IsNullOrEmpty(_fetchPath1525) || _isConfigValid1525;
            _isConfigValid28 = String.IsNullOrEmpty(_fetchPath28) || _isConfigValid28;

            return _isConfigValid28 && _isConfigValid1525;
        }

        public static void InstallPanacea(Config currentConfig)
        {
            var _fetchBuildPath = PathService.ResolveBuild(currentConfig, true);

            var _createPath = $"mod_path={_fetchBuildPath}";
            var _fetchPanaceaPath = Path.Combine(AppContext.BaseDirectory, "assembly", "OpenKh.Research.Panacea.dll");
            var _fetchDependenciesPath = Path.Combine(AppContext.BaseDirectory, "assembly", "dependencies");

            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            var _fetchTarget1525 = Path.Combine(_fetchPath1525, OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll");
            var _fetchTarget28 = Path.Combine(_fetchPath28, OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll");

            if (!String.IsNullOrEmpty(_fetchPath1525))
            {
                var _fetchDependencyDir = Path.Combine(_fetchPath1525, "dependencies");

                if (!Directory.Exists(_fetchDependencyDir))
                    Directory.CreateDirectory(_fetchDependencyDir);

                var _fetchDependencyFiles = Directory.GetFiles(_fetchDependenciesPath);

                foreach (var _file in _fetchDependencyFiles)
                {
                    var _fetchTargetPath = Path.Combine(_fetchDependencyDir, Path.GetFileName(_file));
                    File.Copy(_file, _fetchTargetPath, true);
                }

                File.Copy(_fetchPanaceaPath, _fetchTarget1525, true);
                File.WriteAllText(Path.Combine(_fetchPath1525, "panacea_settings.txt"), _createPath);
            }

            if (!String.IsNullOrEmpty(_fetchPath28))
            {
                var _fetchDependencyDir = Path.Combine(_fetchPath28, "dependencies");

                if (!Directory.Exists(_fetchDependencyDir))
                    Directory.CreateDirectory(_fetchDependencyDir);

                var _fetchDependencyFiles = Directory.GetFiles(_fetchDependenciesPath);

                foreach (var _file in _fetchDependencyFiles)
                {
                    var _fetchTargetPath = Path.Combine(_fetchDependencyDir, Path.GetFileName(_file));
                    File.Copy(_file, _fetchTargetPath, true);
                }

                File.Copy(_fetchPanaceaPath, _fetchTarget28, true);
                File.WriteAllText(Path.Combine(_fetchPath28, "panacea_settings.txt"), _createPath);
            }

            InstallOverrides(currentConfig);
        }

        public static void RemovePanacea(Config currentConfig)
        {
            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            var _fetchTarget1525 = Path.Combine(_fetchPath1525, OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll");
            var _fetchTarget28 = Path.Combine(_fetchPath28, OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll");

            if (!String.IsNullOrEmpty(_fetchPath1525))
            {
                var _fetchDependencyDir = Path.Combine(_fetchPath1525, "dependencies");

                if (!Directory.Exists(_fetchDependencyDir))
                    Directory.Delete(_fetchDependencyDir, true);

                File.Delete(_fetchTarget1525);
                File.Delete(Path.Combine(_fetchPath1525, "panacea_settings.txt"));
            }

            if (!String.IsNullOrEmpty(_fetchPath28))
            {
                var _fetchDependencyDir = Path.Combine(_fetchPath28, "dependencies");

                if (!Directory.Exists(_fetchDependencyDir))
                    Directory.Delete(_fetchDependencyDir, true);

                File.Delete(_fetchTarget28);
                File.Delete(Path.Combine(_fetchPath28, "panacea_settings.txt"));
            }
        }

        public static bool EnsureBackend(Config currentConfig)
        {
            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            if (String.IsNullOrEmpty(_fetchPath1525) && String.IsNullOrEmpty(_fetchPath28))
                return false;

            var _fetchPanacea1525 = Path.Combine(_fetchPath1525, "panacea_settings.txt");
            var _fetchPanacea28 = Path.Combine(_fetchPath28, "panacea_settings.txt");

            var _fetchSettings1525 = Path.Combine(_fetchPath1525, "LuaBackend.toml");
            var _fetchAssembly1525 = Path.Combine(_fetchPath1525, File.Exists(_fetchPanacea1525) ? "LuaBackend.dll" : (OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll"));

            var _fetchSettings28 = Path.Combine(_fetchPath28, "LuaBackend.toml");
            var _fetchAssembly28 = Path.Combine(_fetchPath28, File.Exists(_fetchPanacea28) ? "LuaBackend.dll" : (OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll"));

            var _isConfigValid1525 = false;
            var _isConfigValid28 = false;

            if (File.Exists(_fetchAssembly1525) && File.Exists(_fetchSettings1525))
            {
                var _fetchSettingsRAW = File.ReadAllText(_fetchSettings1525);
                var _fetchSettingsSerial = TomlSerializer.Deserialize<TomlTable>(_fetchSettingsRAW);

                if (_fetchSettingsSerial != null)
                {
                    var _fetchTruthTable = new bool[4];

                    for (var i = 0; i < _fetchTruthTable.Length; i++)
                    {
                        var _fetchGame = (Game)i;
                        var _fetchLuaGameID = _backendGameIDs[_fetchGame];
                        var _fetchManagerGameID = Config.GameShorthand[_fetchGame];

                        var _fetchGameBuildPath = PathService.ResolveBuild(currentConfig, true);
                        var _fetchGameScriptPath = Path.Combine(_fetchGameBuildPath, _fetchManagerGameID, "scripts");

                        var _fetchTableRoot = _fetchSettingsSerial[_fetchLuaGameID] as TomlTable;
                        var _fetchTableScript = _fetchTableRoot != null ? _fetchTableRoot["scripts"] as TomlArray : null;

                        if (_fetchTableScript == null)
                            continue;

                        foreach (var _fetchScript in _fetchTableScript)
                        {
                            var _fetchTable = _fetchScript as TomlTable;

                            if (_fetchTable == null)
                                continue;

                            var _fetchPath = (string)_fetchTable["path"];
                            var _fetchIsRelative = (bool)_fetchTable["relative"];

                            if (_fetchIsRelative || String.IsNullOrEmpty(_fetchPath))
                                continue;

                            var _fetchFullPath = Path.GetFullPath(_fetchPath);
                            var _comparisonRules = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                            if (String.Equals(_fetchGameScriptPath, _fetchFullPath, _comparisonRules))
                            {
                                _fetchTruthTable[i] = true;
                                break;
                            }
                        }
                    }

                    _isConfigValid1525 = _fetchTruthTable.All(x => x == true);
                }
            }

            if (File.Exists(_fetchAssembly28) && File.Exists(_fetchSettings28))
            {
                var _fetchSettingsRAW = File.ReadAllText(_fetchSettings28);
                var _fetchSettingsSerial = TomlSerializer.Deserialize<TomlTable>(_fetchSettingsRAW);

                var _fetchGameBuildPath = PathService.ResolveBuild(currentConfig, true);
                var _fetchGameScriptPath = Path.Combine(_fetchGameBuildPath, "ddd", "scripts");

                var _fetchTableRoot = _fetchSettingsSerial["kh3d"] as TomlTable;
                var _fetchTableScript = _fetchTableRoot["scripts"] as TomlArray;

                foreach (TomlTable _fetchScript in _fetchTableScript)
                {
                    var _fetchPath = (string)_fetchScript["path"];
                    var _fetchIsRelative = (bool)_fetchScript["relative"];

                    if (_fetchIsRelative || String.IsNullOrEmpty(_fetchPath))
                        continue;

                    var _fetchFullPath = Path.GetFullPath(_fetchPath);
                    var _comparisonRules = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

                    if (String.Equals(_fetchGameScriptPath, _fetchFullPath, _comparisonRules))
                    {
                        _isConfigValid28 = true;
                        break;
                    }
                }
            }

            _isConfigValid1525 = String.IsNullOrEmpty(_fetchPath1525) || _isConfigValid1525;
            _isConfigValid28 = String.IsNullOrEmpty(_fetchPath28) || _isConfigValid28;

            return _isConfigValid28 && _isConfigValid1525;
        }

        public static void InstallBackend(Config currentConfig)
        {
            var _templateConfig = "[{0}]\n" +
                                  "scripts = [{{ path = \"{1}\", relative = false }}]\n" +
                                  "exe = \"{2}\"\n" +
                                  "game_docs = \"{3}\"\n";

            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            var _configLines = new List<string>();

            for (int i = 0x00; i < _backendGameIDs.Count(); i++)
            {
                var _fetchGame = (Game)i;
                var _fetchLuaGameID = _backendGameIDs[_fetchGame];
                var _fetchExecutable = Config.GameExecutable[_fetchGame];
                var _fetchManagerGameID = Config.GameShorthand[_fetchGame];

                var _fetchGamePath = _fetchGame == Game.DREAM_DROP_DISTANCE ? "KINGDOM HEARTS HD 2.8 Final Chapter Prologue" : "KINGDOM HEARTS HD 1.5+2.5 ReMIX";
                var _fetchFolderBuild = PathService.ResolveBuild(currentConfig, true);

                var _fetchFolderScripts = Path.Combine(_fetchFolderBuild, _fetchManagerGameID, "scripts");
                var _fetchFolderDocs = Path.Combine(currentConfig.Frontend.TargetPlatform == Platform.STEAM ? "My Games" : "", _fetchGamePath);

                var _formatTemplate = String.Format(_templateConfig, _fetchLuaGameID, _fetchFolderScripts, _fetchExecutable, _fetchFolderDocs).Replace("\\", "/");

                _configLines.AddRange(_formatTemplate.Split('\n'));
            }

            var _fetchBackendPath = Path.Combine(AppContext.BaseDirectory, "assembly", "LuaBackend.dll");

            if (!String.IsNullOrEmpty(_fetchPath1525))
            {
                var _fetchPanacea1525 = Path.Combine(_fetchPath1525, "panacea_settings.txt");
                var _fetchAssembly1525 = Path.Combine(_fetchPath1525, File.Exists(_fetchPanacea1525) ? "LuaBackend.dll" : (OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll"));

                File.Copy(_fetchBackendPath, _fetchAssembly1525, true);
                File.WriteAllLines(Path.Combine(_fetchPath1525, "LuaBackend.toml"), _configLines);
            }

            if (!String.IsNullOrEmpty(_fetchPath28))
            {
                var _fetchPanacea28 = Path.Combine(_fetchPath28, "panacea_settings.txt");
                var _fetchAssembly28 = Path.Combine(_fetchPath28, File.Exists(_fetchPanacea28) ? "LuaBackend.dll" : (OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll"));

                File.Copy(_fetchBackendPath, _fetchAssembly28, true);
                File.WriteAllLines(Path.Combine(_fetchPath28, "LuaBackend.toml"), _configLines);
            }

            InstallOverrides(currentConfig);
        }

        public static void RemoveBackend(Config currentConfig)
        {
            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            if (!String.IsNullOrEmpty(_fetchPath1525))
            {
                var _fetchPanacea1525 = Path.Combine(_fetchPath1525, "panacea_settings.txt");
                var _fetchAssembly1525 = Path.Combine(_fetchPath1525, File.Exists(_fetchPanacea1525) ? "LuaBackend.dll" : (OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll"));

                File.Delete(_fetchAssembly1525);
                File.Delete(Path.Combine(_fetchPath1525, "LuaBackend.toml"));
            }

            if (!String.IsNullOrEmpty(_fetchPath28))
            {
                var _fetchPanacea28 = Path.Combine(_fetchPath28, "panacea_settings.txt");
                var _fetchAssembly28 = Path.Combine(_fetchPath28, File.Exists(_fetchPanacea28) ? "LuaBackend.dll" : (OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll"));

                File.Delete(_fetchAssembly28);
                File.Delete(Path.Combine(_fetchPath28, "LuaBackend.toml"));
            }
        }

        public static bool EnsureSteamAPI(Config currentConfig)
        {
            var _isValid1525 = false;
            var _isValid28 = false;

            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            if (String.IsNullOrEmpty(_fetchPath1525) && String.IsNullOrEmpty(_fetchPath28))
                return false;

            var _fetchSteamID1525 = Path.Combine(_fetchPath1525, "steam_appid.txt");
            var _fetchSteamID28 = Path.Combine(_fetchPath28, "steam_appid.txt");

            if (!File.Exists(_fetchSteamID1525) && !File.Exists(_fetchSteamID28))
                return false;

            if (!File.Exists(_fetchSteamID1525))
                _isValid1525 = String.IsNullOrEmpty(_fetchPath1525);

            else
            {
                var _fetchContents = File.ReadAllText(_fetchSteamID1525);
                _isValid1525 = String.Equals(_fetchContents, "2552430");
            }

            if (!File.Exists(_fetchSteamID28))
                _isValid28 = String.IsNullOrEmpty(_fetchPath28);

            else
            {
                var _fetchContents = File.ReadAllText(_fetchSteamID28);
                _isValid28 = String.Equals(_fetchContents, "2552440");
            }

            return _isValid1525 && _isValid28;
        }

        public static void InstallSteamAPI(Config currentConfig)
        {
            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            var _fetchSteamID1525 = Path.Combine(_fetchPath1525, "steam_appid.txt");
            var _fetchSteamID28 = Path.Combine(_fetchPath28, "steam_appid.txt");

            if (!String.IsNullOrEmpty(_fetchPath1525))
                File.WriteAllText(_fetchSteamID1525, "2552430");

            if (!String.IsNullOrEmpty(_fetchPath28))
                File.WriteAllText(_fetchSteamID28, "2552440");
        }

        public static void RemoveSteamAPI(Config currentConfig)
        {
            var _fetchPath1525 = PathService.ResolvePath1525(currentConfig);
            var _fetchPath28 = PathService.ResolvePath28(currentConfig);

            var _fetchSteamID1525 = Path.Combine(_fetchPath1525, "steam_appid.txt");
            var _fetchSteamID28 = Path.Combine(_fetchPath28, "steam_appid.txt");

            var _isValid1525 = !String.IsNullOrEmpty(_fetchPath1525) && File.Exists(_fetchSteamID1525);
            var _isValid28 = !String.IsNullOrEmpty(_fetchPath28) && File.Exists(_fetchSteamID28);

            if (_isValid1525)
                File.Delete(_fetchSteamID1525);

            if (_isValid28)
                File.Delete(_fetchSteamID28);
        }

        public static bool InstallOverrides(Config currentConfig)
        {
            if (OperatingSystem.IsLinux())
            {
                var _fetchFolders = PathService.FetchSteamLibraries();

                if (_fetchFolders == null)
                    return false;

                _fetchFolders.AsParallel().ForAll(_fetchFolder =>
                {
                    var _fetchDirectory1525 = Path.Combine(_fetchFolder, "steamapps", "compatdata", "2552430");
                    var _fetchDirectory28 = Path.Combine(_fetchFolder, "steamapps", "compatdata", "2552440");

                    if (Directory.Exists(_fetchDirectory1525))
                    {
                        var _fetchRegistry1525 = Path.Combine(_fetchDirectory1525, "pfx", "user.reg");

                        if (File.Exists(_fetchRegistry1525))
                        {
                            var _fetchRegistryRAW = new List<string>(File.ReadAllLines(_fetchRegistry1525));
                            var _fetchOverride = _fetchRegistryRAW.FirstOrDefault(x => x.Contains("\"version\"=\"native,builtin\""));

                            if (_fetchOverride == null)
                            {
                                var _fetchOverridesLine = _fetchRegistryRAW.First(x => x.Contains(@"[Software\\Wine\\DllOverrides]"));
                                var _fetchOverridesPosition = _fetchRegistryRAW.IndexOf(_fetchOverridesLine);

                                _fetchRegistryRAW.Insert(_fetchOverridesPosition + 0x02, "\"version\"=\"native,builtin\"");

                                File.WriteAllLines(_fetchRegistry1525, _fetchRegistryRAW.ToArray());
                            }
                        }
                    }

                    if (Directory.Exists(_fetchDirectory28))
                    {
                        var _fetchRegistry28 = Path.Combine(_fetchDirectory28, "pfx", "user.reg");

                        if (File.Exists(_fetchRegistry28))
                        {
                            var _fetchRegistryRAW = new List<string>(File.ReadAllLines(_fetchRegistry28));
                            var _fetchOverride = _fetchRegistryRAW.FirstOrDefault(x => x.Contains("\"version\"=\"native,builtin\""));

                            if (_fetchOverride == null)
                            {
                                var _fetchOverridesLine = _fetchRegistryRAW.First(x => x.Contains(@"[Software\\Wine\\DllOverrides]"));
                                var _fetchOverridesPosition = _fetchRegistryRAW.IndexOf(_fetchOverridesLine);

                                _fetchRegistryRAW.Insert(_fetchOverridesPosition + 0x02, "\"version\"=\"native,builtin\"");

                                File.WriteAllLines(_fetchRegistry28, _fetchRegistryRAW.ToArray());
                            }
                        }
                    }
                });

                return true;
            }

            else
                return false;
        }
    }
}
