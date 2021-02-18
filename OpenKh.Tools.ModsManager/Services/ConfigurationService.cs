using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class ConfigurationService
    {
        private const string EnabledModsPath = "mods.txt";

        static ConfigurationService()
        {
            if (!Directory.Exists(ModsPath))
                Directory.CreateDirectory(ModsPath);
        }

        public static ICollection<string> EnabledMods
        {
            get => File.ReadAllLines(EnabledModsPath);
            set => File.WriteAllLines(EnabledModsPath, value);
        }

        public static string ModsPath => "./mods";
        public static string OutputModPath => @"D:\Repository\openkh\OpenKh.Game\bin\Debug\netcoreapp3.1\mod";
        public static string GameAssetPath => @"D:\Hacking\KH2\export_fm";
    }
}
