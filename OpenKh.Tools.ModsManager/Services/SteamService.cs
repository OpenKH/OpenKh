using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Tools.ModsManager.Services
{
    /// <summary>
    /// Edits the per-user Steam client configuration (localconfig.vdf) to set
    /// launch options for a game. Used by the Linux build to add
    /// WINEDLLOVERRIDES so Proton loads the Panacea (version.dll) and
    /// LuaBackend (dinput8.dll) native DLLs instead of Wine's builtins.
    /// </summary>
    public static class SteamService
    {
        public const string AppIdKh1525 = "2552430";
        public const string AppIdKh28 = "2552440";

        /// <summary>
        /// The launch options required for Panacea/LuaBackend under Proton.
        /// </summary>
        public const string WineDllOverridesLaunchOptions =
            "WINEDLLOVERRIDES=\"version,dinput8=n,b\" %command%";

        public static bool IsSteamRunning() =>
            Process.GetProcessesByName("steam").Length > 0;

        /// <summary>
        /// All localconfig.vdf files of all logged-in Steam accounts, across
        /// the known Steam install locations.
        /// </summary>
        public static IEnumerable<string> FindLocalConfigFiles()
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var steamRoots = new[]
            {
                Path.Combine(home, ".local", "share", "Steam"),
                Path.Combine(home, ".steam", "steam"),
                Path.Combine(home, ".steam", "root"),
                Path.Combine(home, ".var", "app", "com.valvesoftware.Steam", ".local", "share", "Steam"),
            };

            return steamRoots
                .Where(Directory.Exists)
                // ~/.steam/steam and ~/.steam/root are usually symlinks to
                // ~/.local/share/Steam; resolve them so each physical file is
                // only visited once.
                .Select(root => new DirectoryInfo(root).ResolveLinkTarget(returnFinalTarget: true)?.FullName
                    ?? Path.GetFullPath(root))
                .Distinct()
                .Where(Directory.Exists)
                .SelectMany(root =>
                {
                    var userData = Path.Combine(root, "userdata");
                    if (!Directory.Exists(userData))
                        return Enumerable.Empty<string>();
                    return Directory.EnumerateDirectories(userData)
                        .Select(accountDir => Path.Combine(accountDir, "config", "localconfig.vdf"))
                        .Where(File.Exists);
                })
                .Distinct();
        }

        /// <summary>
        /// Ensures the given app's LaunchOptions contain
        /// <see cref="WineDllOverridesLaunchOptions"/>, preserving any options
        /// the user already set. Steam must not be running, or it will
        /// overwrite the file with its in-memory state on exit.
        /// </summary>
        /// <returns>
        /// The number of localconfig.vdf files updated (0 also when every
        /// file already had the options set).
        /// </returns>
        public static int EnsureLaunchOptions(string appId)
        {
            var updated = 0;
            foreach (var configFile in FindLocalConfigFiles())
            {
                var content = File.ReadAllText(configFile);
                var newContent = SetLaunchOptions(content, appId);
                if (newContent != null && newContent != content)
                {
                    File.Copy(configFile, configFile + ".openkh.bak", true);
                    File.WriteAllText(configFile, newContent);
                    updated++;
                }
            }
            return updated;
        }

        /// <summary>
        /// Returns true when every found Steam account already has the
        /// required launch options for the given app.
        /// </summary>
        public static bool AreLaunchOptionsSet(string appId)
        {
            var files = FindLocalConfigFiles().ToList();
            if (files.Count == 0)
                return false;
            return files.All(file =>
            {
                var current = TryGetLaunchOptions(File.ReadAllText(file), appId);
                return current != null && current.Contains("WINEDLLOVERRIDES");
            });
        }

        internal static string TryGetLaunchOptions(string vdfContent, string appId)
        {
            var appBlock = FindAppBlock(vdfContent, appId);
            if (appBlock == null)
                return null;
            var (blockStart, blockEnd) = appBlock.Value;
            var launchOptions = FindKeyValue(vdfContent, blockStart, blockEnd, "LaunchOptions");
            if (launchOptions == null)
                return null;
            return VdfUnescape(vdfContent.Substring(
                launchOptions.Value.valueStart,
                launchOptions.Value.valueEnd - launchOptions.Value.valueStart));
        }

        /// <summary>
        /// Returns the updated VDF text, the original text when nothing needs
        /// changing, or null when the app has no entry in this file.
        /// </summary>
        internal static string SetLaunchOptions(string vdfContent, string appId)
        {
            var appBlock = FindAppBlock(vdfContent, appId);
            if (appBlock == null)
                return null;
            var (blockStart, blockEnd) = appBlock.Value;

            var existing = FindKeyValue(vdfContent, blockStart, blockEnd, "LaunchOptions");
            if (existing == null)
            {
                // Insert a LaunchOptions line right after the block's opening brace.
                var insertion = "\n\t\t\t\t\t\"LaunchOptions\"\t\t\"" + VdfEscape(WineDllOverridesLaunchOptions) + "\"";
                return vdfContent.Insert(blockStart + 1, insertion);
            }

            var (valueStart, valueEnd) = existing.Value;
            var currentValue = VdfUnescape(vdfContent.Substring(valueStart, valueEnd - valueStart));
            if (currentValue.Contains("WINEDLLOVERRIDES"))
                return vdfContent;

            string newValue;
            if (currentValue.Contains("%command%"))
                // Prepend the env var to whatever command line the user built.
                newValue = "WINEDLLOVERRIDES=\"version,dinput8=n,b\" " + currentValue;
            else if (currentValue.Trim().Length == 0)
                newValue = WineDllOverridesLaunchOptions;
            else
                // Plain arguments: keep them as game arguments after %command%.
                newValue = WineDllOverridesLaunchOptions + " " + currentValue;

            return vdfContent
                .Remove(valueStart, valueEnd - valueStart)
                .Insert(valueStart, VdfEscape(newValue));
        }

        /// <summary>
        /// Finds the character range (opening brace index, closing brace
        /// index) of the app's block under Software > Valve > Steam > apps.
        /// </summary>
        private static (int blockStart, int blockEnd)? FindAppBlock(string content, string appId)
        {
            // Walk the whole document tracking the current key path; VDF is a
            // simple sequence of quoted strings and braces.
            var pathStack = new List<string>();
            string pendingKey = null;
            var i = 0;
            while (i < content.Length)
            {
                var c = content[i];
                if (c == '"')
                {
                    var (token, next) = ReadQuoted(content, i);
                    if (pendingKey == null)
                    {
                        pendingKey = token;
                    }
                    else
                    {
                        // Key/value pair completed; not a block.
                        pendingKey = null;
                    }
                    i = next;
                }
                else if (c == '{')
                {
                    pathStack.Add(pendingKey ?? string.Empty);
                    pendingKey = null;
                    if (IsAppsPath(pathStack, appId))
                    {
                        var end = FindMatchingBrace(content, i);
                        if (end > i)
                            return (i, end);
                        return null;
                    }
                    i++;
                }
                else if (c == '}')
                {
                    if (pathStack.Count > 0)
                        pathStack.RemoveAt(pathStack.Count - 1);
                    pendingKey = null;
                    i++;
                }
                else
                {
                    i++;
                }
            }
            return null;
        }

        private static bool IsAppsPath(List<string> path, string appId)
        {
            // Expected: UserLocalConfigStore / Software / Valve / Steam / apps / <appId>
            // Casing varies between Steam versions, so compare case-insensitively.
            if (path.Count < 3 || !string.Equals(path[^1], appId, StringComparison.OrdinalIgnoreCase))
                return false;
            return string.Equals(path[^2], "apps", StringComparison.OrdinalIgnoreCase)
                && string.Equals(path[^3], "Steam", StringComparison.OrdinalIgnoreCase);
        }

        private static int FindMatchingBrace(string content, int openIndex)
        {
            var depth = 0;
            var i = openIndex;
            while (i < content.Length)
            {
                var c = content[i];
                if (c == '"')
                {
                    var (_, next) = ReadQuoted(content, i);
                    i = next;
                    continue;
                }
                if (c == '{')
                    depth++;
                else if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                        return i;
                }
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Looks for a "key" "value" pair directly inside the given block and
        /// returns the raw (still escaped) value range.
        /// </summary>
        private static (int valueStart, int valueEnd)? FindKeyValue(
            string content, int blockStart, int blockEnd, string key)
        {
            var i = blockStart + 1;
            var depth = 0;
            string pendingKey = null;
            int pendingKeyEnd = -1;
            while (i < blockEnd)
            {
                var c = content[i];
                if (c == '"')
                {
                    var start = i;
                    var (token, next) = ReadQuoted(content, i);
                    if (depth == 0)
                    {
                        if (pendingKey == null)
                        {
                            pendingKey = token;
                            pendingKeyEnd = next;
                        }
                        else
                        {
                            if (string.Equals(pendingKey, key, StringComparison.OrdinalIgnoreCase))
                                return (start + 1, next - 1);
                            pendingKey = null;
                        }
                    }
                    i = next;
                }
                else if (c == '{')
                {
                    depth++;
                    pendingKey = null;
                    i++;
                }
                else if (c == '}')
                {
                    depth--;
                    pendingKey = null;
                    i++;
                }
                else
                {
                    i++;
                }
            }
            return null;
        }

        /// <summary>
        /// Reads a quoted VDF string starting at the opening quote; returns
        /// the unescaped token and the index just past the closing quote.
        /// </summary>
        private static (string token, int next) ReadQuoted(string content, int openQuote)
        {
            var sb = new StringBuilder();
            var i = openQuote + 1;
            while (i < content.Length)
            {
                var c = content[i];
                if (c == '\\' && i + 1 < content.Length)
                {
                    sb.Append(content[i + 1]);
                    i += 2;
                }
                else if (c == '"')
                {
                    return (sb.ToString(), i + 1);
                }
                else
                {
                    sb.Append(c);
                    i++;
                }
            }
            return (sb.ToString(), i);
        }

        private static string VdfEscape(string value) =>
            value.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string VdfUnescape(string value) =>
            value.Replace("\\\"", "\"").Replace("\\\\", "\\");
    }
}
