using System;
using System.IO;

namespace OpenKh.Tools.ModsManager.Services
{
    /// <summary>
    /// On Linux the games run under Proton/Wine, so any path written into
    /// configuration files read by the game (panacea_settings.txt,
    /// LuaBackend.toml) must be expressed as a Windows path. Wine exposes the
    /// host filesystem through the Z: drive, so /home/user/x becomes
    /// Z:\home\user\x.
    /// </summary>
    public static class WinePathUtil
    {
        /// <summary>
        /// Converts a host path into the path the game itself can open.
        /// Returns the input unchanged on Windows.
        /// </summary>
        public static string ToGamePath(string path)
        {
            if (OperatingSystem.IsWindows() || string.IsNullOrEmpty(path))
                return path;

            var fullPath = Path.GetFullPath(path);
            return "Z:" + fullPath.Replace('/', '\\');
        }

        /// <summary>
        /// Same as ToGamePath but with forward slashes, for TOML files where
        /// backslashes would need escaping. Wine accepts Z:/home/user/x.
        /// </summary>
        public static string ToGamePathForwardSlashes(string path)
        {
            if (OperatingSystem.IsWindows() || string.IsNullOrEmpty(path))
                return path;

            return "Z:" + Path.GetFullPath(path);
        }
    }
}
