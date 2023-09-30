using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class ConfigurationService
    {
        private class Config
        {
            private static readonly IDeserializer _deserializer =
                new DeserializerBuilder()
                .IgnoreFields()
                .IgnoreUnmatchedProperties()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            private static readonly ISerializer _serializer =
                new SerializerBuilder()
                .IgnoreFields()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            public int WizardVersionNumber { get; set; }
            public string ModCollectionPath { get; internal set; }
            public string GameModPath { get; internal set; }
            public string GameDataPath { get; internal set; }
            public int GameEdition { get; internal set; }
            public string IsoLocation { get; internal set; }
            public string OpenKhGameEngineLocation { get; internal set; }
            public string Pcsx2Location { get; internal set; }
            public string PcReleaseLocation { get; internal set; }
            public string PcReleaseLanguage { get; internal set; } = "en";
            public int RegionId { get; internal set; }
            public bool PanaceaInstalled { get; internal set; }
            public bool DevView { get; internal set; }
            public bool AutoUpdateMods { get; internal set; }
            public bool isEGSVersion { get; internal set; } = true;
            public bool kh1 { get; internal set; }
            public bool kh2 { get; internal set; } = true;
            public bool bbs { get; internal set; }
            public bool recom { get; internal set; }
            public string LaunchGame { get; internal set; } = "kh2";

            public void Save(string fileName)
            {
                using var writer = new StreamWriter(fileName);
                _serializer.Serialize(writer, this);
            }

            public static Config Open(string fileName)
            {
                if (!File.Exists(fileName))
                    return new Config();

                using var reader = new StreamReader(fileName);
                return _deserializer.Deserialize<Config>(reader);
            }
        }

        private static string StoragePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        private static string ConfigPath = Path.Combine(StoragePath, "mods-manager.yml");
        private static string EnabledModsPathKH1 = Path.Combine(StoragePath, "mods-KH1.txt");
        private static string EnabledModsPathKH2 = Path.Combine(StoragePath, "mods-KH2.txt");
        private static string EnabledModsPathBBS = Path.Combine(StoragePath, "mods-BBS.txt");
        private static string EnabledModsPathRECOM = Path.Combine(StoragePath, "mods-ReCoM.txt");
        private static readonly Config _config = Config.Open(ConfigPath);
        public static string PresetPath = Path.Combine(StoragePath, "presets");

        static ConfigurationService()
        {
            string modsPath = Path.GetFullPath(Path.Combine(ModCollectionPath, ".."));
            if (!Directory.Exists(Path.Combine(modsPath, "kh2")))
                Directory.CreateDirectory(Path.Combine(modsPath, "kh2"));
            if (!Directory.Exists(Path.Combine(modsPath, "kh1")))
                Directory.CreateDirectory(Path.Combine(modsPath, "kh1"));
            if (!Directory.Exists(Path.Combine(modsPath, "bbs")))
                Directory.CreateDirectory(Path.Combine(modsPath, "bbs"));
            if (!Directory.Exists(Path.Combine(modsPath, "Recom")))
                Directory.CreateDirectory(Path.Combine(modsPath, "Recom"));
            if (!Directory.Exists(PresetPath))
                Directory.CreateDirectory(PresetPath);


            Task.Run(async () =>
            {
                using var client = new HttpClient();
                var response = await client
                    .GetAsync("https://raw.githubusercontent.com/OpenKH/mods-manager-feed/main/featured.txt");
                FeaturedMods = (await response.Content.ReadAsStringAsync())
                    .Split("\n")
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            });

            Task.Run(async () =>
            {
                using var client = new HttpClient();
                var response = await client
                    .GetAsync("https://raw.githubusercontent.com/OpenKH/mods-manager-feed/main/deny.txt");
                BlacklistedMods = (await response.Content.ReadAsStringAsync())
                    .Split("\n")
                    .Where(x => !string.IsNullOrEmpty(x))
                    .ToList();
            });
        }
        
        public static ICollection<string> EnabledMods
        {
            get
            {
                switch (LaunchGame)
                {
                    case "kh1":
                        return File.Exists(EnabledModsPathKH1) ? File.ReadAllLines(EnabledModsPathKH1) : new string[0];
                    case "bbs":
                        return File.Exists(EnabledModsPathBBS) ? File.ReadAllLines(EnabledModsPathBBS) : new string[0];
                    case "Recom":
                        return File.Exists(EnabledModsPathRECOM) ? File.ReadAllLines(EnabledModsPathRECOM) : new string[0];
                    default:
                        return File.Exists(EnabledModsPathKH2) ? File.ReadAllLines(EnabledModsPathKH2) : new string[0];
                }
            }
            set
            {
                switch (LaunchGame)
                {
                    case "kh1":
                        File.WriteAllLines(EnabledModsPathKH1, value);
                        break;
                    case "bbs":
                        File.WriteAllLines(EnabledModsPathBBS, value);
                        break;
                    case "Recom":
                        File.WriteAllLines(EnabledModsPathRECOM, value);
                        break;
                    default:
                        File.WriteAllLines(EnabledModsPathKH2, value);
                        break;
                }
            }
        }

        public static ICollection<string> FeaturedMods { get; private set; }
        public static ICollection<string> BlacklistedMods { get; private set; }

        public static int WizardVersionNumber
        {
            get => _config.WizardVersionNumber;
            set
            {
                _config.WizardVersionNumber = value;
                _config.Save(ConfigPath);
            }
        }

        public static string ModCollectionPath
        {
            get => _config.ModCollectionPath ?? Path.GetFullPath(Path.Combine(StoragePath, "mods", LaunchGame));
            set
            {
                _config.ModCollectionPath = value;
                _config.Save(ConfigPath);
            }
        }

        public static string GameModPath
        {
            get => _config.GameModPath ?? Path.GetFullPath(Path.Combine(StoragePath, "mod"));
            set
            {
                _config.GameModPath = value;
                _config.Save(ConfigPath);
            }
        }

        public static string GameDataLocation
        {
            get => _config.GameDataPath ?? Path.GetFullPath(Path.Combine(StoragePath, "data"));
            set
            {
                _config.GameDataPath = value;
                _config.Save(ConfigPath);
            }
        }

        public static int GameEdition
        {
            get => _config.GameEdition;
            set
            {
                _config.GameEdition = value;
                _config.Save(ConfigPath);
            }
        }

        public static string IsoLocation
        {
            get => _config.IsoLocation;
            set
            {
                _config.IsoLocation = value;
                _config.Save(ConfigPath);
            }
        }

        public static string OpenKhGameEngineLocation
        {
            get => _config.OpenKhGameEngineLocation ?? Path.GetFullPath("OpenKh.Game.exe");
            set
            {
                _config.OpenKhGameEngineLocation = value;
                _config.Save(ConfigPath);
            }
        }

        public static string Pcsx2Location
        {
            get => _config.Pcsx2Location;
            set
            {
                _config.Pcsx2Location = value;
                _config.Save(ConfigPath);
            }
        }

        public static string PcReleaseLocation
        {
            get => _config.PcReleaseLocation;
            set
            {
                _config.PcReleaseLocation = value;
                _config.Save(ConfigPath);
            }
        }

        public static string PcReleaseLanguage
        {
            get => _config.PcReleaseLanguage;
            set
            {
                _config.PcReleaseLanguage = value;
                _config.Save(ConfigPath);
            }
        }

        public static int RegionId
        {
            get => _config.RegionId;
            set
            {
                _config.RegionId = value;
                _config.Save(ConfigPath);
            }
        }

        public static bool PanaceaInstalled
        {
            get => _config.PanaceaInstalled;
            set
            {
                _config.PanaceaInstalled = value;
                _config.Save(ConfigPath);
            }
        }
        public static bool DevView
        {
            get => _config.DevView;
            set
            {
                _config.DevView = value;
                _config.Save(ConfigPath);
            }
        }
        public static bool AutoUpdateMods
        {
            get => _config.AutoUpdateMods;
            set
            {
                _config.AutoUpdateMods = value;
                _config.Save(ConfigPath);
            }
        }
        public static bool IsEGSVersion
        {
            get => _config.isEGSVersion;
            set
            {
                _config.isEGSVersion = value;
                _config.Save(ConfigPath);
            }
        }
        public static bool kh1
        {
            get => _config.kh1;
            set
            {
                _config.kh1 = value;
                _config.Save(ConfigPath);
            }
        }
        public static bool kh2
        {
            get => _config.kh2;
            set
            {
                _config.kh2 = value;
                _config.Save(ConfigPath);
            }
        }
        public static bool bbs
        {
            get => _config.bbs;
            set
            {
                _config.bbs = value;
                _config.Save(ConfigPath);
            }
        }
        public static bool recom
        {
            get => _config.recom;
            set
            {
                _config.recom = value;
                _config.Save(ConfigPath);
            }
        }
        public static string LaunchGame
        {
            get => _config.LaunchGame;
            set
            {
                _config.LaunchGame = value;
                _config.Save(ConfigPath);
            }
        }
    }
}
