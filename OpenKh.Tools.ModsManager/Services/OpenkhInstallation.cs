using System;
using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    public static class OpenkhInstallation
    {
        private const string ModManagerExecutableName = "OpenKh.Tools.ModsManager.exe";

        public static string Directory => GetDirectory(AppContext.BaseDirectory);

        public static string GetDirectory(string applicationBaseDirectory)
        {
            var applicationDirectory = new DirectoryInfo(
                Path.GetFullPath(applicationBaseDirectory).TrimEnd(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar
                )
            );
            if (applicationDirectory.Name.Equals("Apps", StringComparison.OrdinalIgnoreCase)
                && applicationDirectory.Parent != null)
            {
                return applicationDirectory.Parent.FullName;
            }

            var appsDirectory = applicationDirectory.Parent;
            var installationDirectory = appsDirectory?.Parent;
            return applicationDirectory.Name.Equals("ModManager", StringComparison.OrdinalIgnoreCase)
                    && appsDirectory?.Name.Equals("Apps", StringComparison.OrdinalIgnoreCase) == true
                    && installationDirectory != null
                ? installationDirectory.FullName
                : applicationDirectory.FullName;
        }

        public static string GetModManagerExecutable(string installationDirectory)
        {
            var packagedPath = Path.Combine(installationDirectory, "Apps", ModManagerExecutableName);
            if (File.Exists(packagedPath))
                return packagedPath;

            var previousPackagedPath = Path.Combine(
                installationDirectory,
                "Apps",
                "ModManager",
                ModManagerExecutableName
            );

            return File.Exists(previousPackagedPath)
                ? previousPackagedPath
                : Path.Combine(installationDirectory, ModManagerExecutableName);
        }
    }
}
