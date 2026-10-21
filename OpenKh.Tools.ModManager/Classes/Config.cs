using CommunityToolkit.Mvvm.ComponentModel;
using OpenKh.Tools.ModManager.Services;
using SharpYaml;
using SharpYaml.Model;
using SharpYaml.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace OpenKh.Tools.ModManager.Classes
{
    public enum Platform : int
    {
        PCSX2,
        STEAM,
        EPIC_GAMES_STORE,
        MICROSOFT_STORE
    };

    public enum BuildType : int
    {
        PATCH,
        PANACEA
    };

    public enum Game : int
    {
        KINGDOM_HEARTS,
        KINGDOM_HEARTS_II,
        CHAIN_OF_MEMORIES,
        BIRTH_BY_SLEEP,
        DREAM_DROP_DISTANCE
    };

    // Frontend Configuration
    public partial class Frontend : ObservableObject
    {
        [ObservableProperty]
        private bool _isLog;

        [ObservableProperty]
        private Platform _targetPlatform;

        [ObservableProperty]
        private Game _targetGame;

        [ObservableProperty]
        private BuildType _modBuildType;

        [ObservableProperty]
        private string? _launchArguments;

        [ObservableProperty]
        private string? _modPath;

        [ObservableProperty]
        private string? _buildPath;

        [ObservableProperty]
        private string[]? _targetPreset;

        [ObservableProperty]
        private string? _presetPath;

        [ObservableProperty]
        private bool _updateMods;

        [ObservableProperty]
        private string? _dataPath;

        [ObservableProperty]
        private string[]? _gamePath;
    }

    // Panacea Configuration
    public partial class Panacea : ObservableObject
    {
        [ObservableProperty]
        public bool _isConsole;

        [ObservableProperty]
        public bool _isDebug;

        [ObservableProperty]
        public bool _isSoundDebug;

        [ObservableProperty]
        public bool _isCacheActive;
    }

    // PCSX2 Configuration
    public partial class Emulator : ObservableObject
    {
        [ObservableProperty]
        public string[]? _emuPath;

        [ObservableProperty]
        public string[]? _romPath;
    }

    public partial class Config : ObservableObject
    {
        public static Dictionary<Game, string> GameExecutable = new Dictionary<Game, string>()
        {
            { Game.KINGDOM_HEARTS, "KINGDOM HEARTS FINAL MIX.exe" },
            { Game.KINGDOM_HEARTS_II, "KINGDOM HEARTS II FINAL MIX.exe" },
            { Game.CHAIN_OF_MEMORIES, "KINGDOM HEARTS Re_Chain of Memories.exe" },
            { Game.BIRTH_BY_SLEEP, "KINGDOM HEARTS Birth by Sleep FINAL MIX.exe" },
            { Game.DREAM_DROP_DISTANCE, "KINGDOM HEARTS Dream Drop Distance.exe" },
        };

        public static Dictionary<Game, string> GameShorthand = new Dictionary<Game, string>()
        {
            { Game.KINGDOM_HEARTS, "kh1" },
            { Game.KINGDOM_HEARTS_II, "kh2" },
            { Game.CHAIN_OF_MEMORIES, "Recom" },
            { Game.BIRTH_BY_SLEEP, "bbs" },
            { Game.DREAM_DROP_DISTANCE, "kh3d" },
        };

        [ObservableProperty]
        private Frontend _frontend;

        [ObservableProperty]
        private Panacea _panacea;

        [ObservableProperty]
        private Emulator _emulator;

        public static Config Load()
        {
            var _fetchConfigFile = System.IO.Path.Combine(AppContext.BaseDirectory, "config.yml");
            var _fetchConfigRAW = File.ReadAllText(_fetchConfigFile);

            return YamlSerializer.Deserialize<Config>(_fetchConfigRAW);
        }

        public bool IsValid()
        {
            if (Frontend.TargetPlatform != Platform.PCSX2)
            {
                var _fetchTargetGame = Frontend.TargetGame;

                // Check if the game is Dream Drop Distance and the second Game Path has been declared.
                // Otherwise, check if the game is NOT Dream Drop Distance.
                // If neither of these are true, the config isn't valid.

                var _isDDDConfigValid = _fetchTargetGame != Game.DREAM_DROP_DISTANCE || (_fetchTargetGame == Game.DREAM_DROP_DISTANCE && Frontend.GamePath.Length == 2 && !String.IsNullOrEmpty(Frontend.GamePath[1]));

                if (!_isDDDConfigValid)
                    return false;

                else
                {
                    // Fetch the second Game Path if the game is Dream Drop Distance, fetch the first one otherwise.
                    var _fetchGamePath = _fetchTargetGame == Game.DREAM_DROP_DISTANCE ? PathService.ResolvePath28(this) : PathService.ResolvePath1525(this);

                    if (String.IsNullOrEmpty(_fetchGamePath))
                        return false;

                    else
                    {
                        // Construct the paths for the game executable and Panacea.
                        var _fetchExePath = System.IO.Path.Combine(_fetchGamePath, Config.GameExecutable[Frontend.TargetGame]);
                        var _fetchSettingsPath = System.IO.Path.Combine(_fetchGamePath, "panacea_settings.txt");

                        // Verify the game executable and directory exists as configured. If the build type is PANACEA, also verify Panacea's existence.
                        var _isGameConfigValid = Directory.Exists(_fetchGamePath) && File.Exists(_fetchExePath);
                        var _isPanaceaConfigValid = (Frontend.ModBuildType == BuildType.PANACEA && File.Exists(_fetchSettingsPath)) || Frontend.ModBuildType == BuildType.PATCH;

                        if (_isPanaceaConfigValid && Frontend.ModBuildType == BuildType.PANACEA)
                            _isPanaceaConfigValid = PackageService.EnsurePanacea(this);

                        // If either are not valid, mark the config as faulty.
                        if (!_isGameConfigValid || !_isPanaceaConfigValid)
                            return false;

                        else
                            return true;
                    }
                }
            }

            else
                return true;
        }

        public void Commit()
        {
            var _fetchConfigFile = System.IO.Path.Combine(AppContext.BaseDirectory, "config.yml");
            var _fetchSerialize = YamlSerializer.Serialize(this);

            File.WriteAllText(_fetchConfigFile, _fetchSerialize);
        }
    }
}
