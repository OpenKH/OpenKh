using OpenKh.Game.Debugging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OpenKh.Game
{
    public static class Config
    {
        private class ActualConfig
        {
            public int resolutionWidth { get; set; } = 0;
            public int resolutionHeight { get; set; } = 0;
            public float resolutionBoost { get; set; } = 2.0f;
            public bool isFullScreen { get; set; } = false;
            public string dataPath { get; set; } = "./data";
            public string modPath { get; set; } = "./mod";
            public int regionId { get; set; } = -1;
            public bool enforceInternationalTextEncoding { get; set; } = false;
            public string idxFilePath { get; set; } = "KH2.IDX";
            public string imgFilePath { get; set; } = "KH2.IMG";
            public bool debugMode { get; set; } = true;
            public float gameSpeed { get; set; } = 1.0f;

            internal static ActualConfig Default() => new ActualConfig();

            internal static ActualConfig ReadFromFile(string filePath) =>
                new DeserializerBuilder()
                .Build()
                .Deserialize<ActualConfig>(File.ReadAllText(filePath));

            internal void WriteToFile(string filePath)
            {
                using var writer = new StreamWriter(filePath);

                new SerializerBuilder()
                    .Build()
                    .Serialize(writer, this);
            }
        }

        private const string ConfigFilePath = "./config.yml";
        private static readonly string ActualConfigFilePath = Path.GetFullPath("./config.yml");
        private static ActualConfig _config = ActualConfig.Default();
        private static CancellationTokenSource _tokenSource;

        public delegate void ConfigurationChange();
        public static event ConfigurationChange OnConfigurationChange;

        public static int ResolutionWidth { get => _config.resolutionWidth; set => _config.resolutionWidth = value; }
        public static int ResolutionHeight { get => _config.resolutionHeight; set => _config.resolutionHeight = value; }
        public static float ResolutionBoost { get => _config.resolutionBoost; set => _config.resolutionBoost = value; }
        public static bool IsFullScreen { get => _config.isFullScreen; set => _config.isFullScreen = value; }
        public static string DataPath { get => _config.dataPath; set => _config.dataPath = value; }
        public static string ModPath { get => _config.modPath; set => _config.modPath = value; }
        public static int RegionId { get => _config.regionId; set => _config.regionId = value; }
        public static bool EnforceInternationalTextEncoding { get => _config.enforceInternationalTextEncoding; set => _config.enforceInternationalTextEncoding = value; }
        public static string IdxFilePath { get => _config.idxFilePath; set => _config.idxFilePath = value; }
        public static string ImgFilePath { get => _config.imgFilePath; set => _config.imgFilePath = value; }
        public static bool DebugMode { get => _config.debugMode; set => _config.debugMode = value; }
        public static float GameSpeed { get => _config.gameSpeed; set => _config.gameSpeed = value; }

        private static void InternalOpen()
        {
            if (!File.Exists(ConfigFilePath))
            {
                Log.Info($"Configuration file not found at {ActualConfigFilePath}. Creating default configuraiton.");
                Save();
            }
            else
            {
                Log.Info($"Load configuration file from {ActualConfigFilePath}");
                try
                {
                    _config = ActualConfig.ReadFromFile(ActualConfigFilePath) ?? ActualConfig.Default();
                }
                catch (Exception ex)
                {
                    // Do not close the entire game for silly reasons
                    Log.Err(ex.Message);
                }
            }
        }

        private static void InternalSave()
        {
            _config.WriteToFile(ActualConfigFilePath);
        }

        public static void Open()
        {
            InternalOpen();
            InternalSave(); // expand the config with the new structure
        }

        public static void Listen()
        {
            if (_tokenSource != null)
                return;

            _tokenSource = new CancellationTokenSource();
            Task.Run(() =>
            {
                using var fsWatcher = new FileSystemWatcher()
                {
                    Path = Path.GetDirectoryName(ActualConfigFilePath),
                    Filter = Path.GetFileName(ActualConfigFilePath),
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true,
                };

                fsWatcher.Changed += (object sender, FileSystemEventArgs e) =>
                {
                    Log.Info("Configuration file has changed");
                    Thread.Sleep(50);
                    InternalOpen();
                    OnConfigurationChange?.Invoke();
                };

                while (!_tokenSource.Token.IsCancellationRequested)
                    Thread.Sleep(1000);

            }, _tokenSource.Token);
        }

        public static void Save()
        {
            Log.Info($"Save configuration file to {ActualConfigFilePath}");
            InternalSave();
        }

        public static void Close()
        {
            _tokenSource?.Dispose();
        }
    }
}
