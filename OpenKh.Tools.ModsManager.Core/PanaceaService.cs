using System.Security.Cryptography;
using System.Text;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class PanaceaService(ModManagerConfigurationService configuration)
{
    private const string PanaceaFileName = "OpenKH.Panacea.dll";
    private static readonly byte[] PanaceaSignature = Encoding.ASCII.GetBytes("Welcome to OpenKH Panacea!");
    private static readonly string[] DependencyFileNames =
    [
        "avcodec-vgmstream-59.dll",
        "avformat-vgmstream-59.dll",
        "avutil-vgmstream-57.dll",
        "bass.dll",
        "bass_vgmstream.dll",
        "libatrac9.dll",
        "libcelt-0061.dll",
        "libcelt-0110.dll",
        "libg719_decode.dll",
        "libmpg123-0.dll",
        "libspeex-1.dll",
        "libvorbis.dll",
        "swresample-vgmstream-4.dll"
    ];

    public PanaceaStatus GetStatus(bool isKh3D, string? releaseDirectoryOverride = null)
    {
        var releaseDirectory = GetReleaseDirectory(isKh3D, releaseDirectoryOverride, false);
        if (releaseDirectory is null)
            return new PanaceaStatus(false, false, "Configure the game folder first");

        var sourceDirectory = FindSourceDirectory();
        var installedFiles = GetInstalledLoaderFiles(releaseDirectory).ToArray();
        if (installedFiles.Length == 0)
        {
            var availableVersion = sourceDirectory is null
                ? string.Empty
                : $", available Panacea version {GetSourceVersion(sourceDirectory)}";
            return new PanaceaStatus(
                false,
                sourceDirectory is not null,
                $"Not installed{availableVersion}");
        }

        var installedFile = sourceDirectory is null
            ? installedFiles[0]
            : installedFiles.FirstOrDefault(file =>
                FilesMatch(Path.Combine(sourceDirectory, PanaceaFileName), file)) ?? installedFiles[0];
        var installedVersion = GetInstalledVersion(installedFile, sourceDirectory);
        if (sourceDirectory is null)
        {
            return new PanaceaStatus(
                true,
                false,
                $"Installed, Panacea version {installedVersion}; source files unavailable");
        }

        var sourceFile = Path.Combine(sourceDirectory, PanaceaFileName);
        var current = installedFiles.Any(file => FilesMatch(sourceFile, file)) &&
                      DependencyFileNames.All(file => File.Exists(Path.Combine(releaseDirectory, "dependencies", file)));
        return current
            ? new PanaceaStatus(
                true,
                true,
                $"Installed, Panacea version {installedVersion}, up to date")
            : new PanaceaStatus(
                true,
                true,
                $"Installed, Panacea version {installedVersion}; available {GetSourceVersion(sourceDirectory)}. Reinstall recommended");
    }

    public Task InstallAsync(
        bool isKh3D,
        string? releaseDirectoryOverride = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Install(isKh3D, releaseDirectoryOverride, cancellationToken), cancellationToken);

    public Task RemoveAsync(
        bool isKh3D,
        string? releaseDirectoryOverride = null,
        CancellationToken cancellationToken = default) =>
        Task.Run(() => Remove(isKh3D, releaseDirectoryOverride, cancellationToken), cancellationToken);

    public void SaveSettings()
    {
        configuration.Save();
        WriteSettingsIfInstalled(false);
        WriteSettingsIfInstalled(true);
    }

    private void WriteSettingsIfInstalled(bool isKh3D)
    {
        var releaseDirectory = GetReleaseDirectory(isKh3D, null, false);
        if (string.IsNullOrWhiteSpace(releaseDirectory) || !GetStatus(isKh3D, releaseDirectory).IsInstalled)
            return;

        var settingsFile = Path.Combine(releaseDirectory, "panacea_settings.txt");
        var values = File.Exists(settingsFile)
            ? File.ReadAllLines(settingsFile)
                .Where(line => line.Contains('='))
                .Select(line => line.Split('=', 2))
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        values["mod_path"] = PanaceaPath.ToLoaderPath(configuration.CompiledModsRoot);
        values["show_console"] = configuration.Current.ShowConsole.ToString().ToLowerInvariant();
        values["debug_log"] = configuration.Current.DebugLog.ToString().ToLowerInvariant();
        values["sound_debug"] = configuration.Current.SoundDebug.ToString().ToLowerInvariant();
        values["enable_cache"] = configuration.Current.EnableCache.ToString().ToLowerInvariant();
        values["quick_menu"] = configuration.Current.QuickMenu.ToString().ToLowerInvariant();
        File.WriteAllLines(settingsFile, values.Select(value => $"{value.Key}={value.Value}"));
    }

    private void Install(bool isKh3D, string? releaseDirectoryOverride, CancellationToken cancellationToken)
    {
        var releaseDirectory = GetReleaseDirectory(isKh3D, releaseDirectoryOverride, true)!;
        SaveReleaseDirectory(isKh3D, releaseDirectory);
        var sourceDirectory = FindSourceDirectory() ?? throw new FileNotFoundException(
            "OpenKH.Panacea.dll and its dependencies are not available in this OpenKH build.");
        var missingDependency = DependencyFileNames.FirstOrDefault(file =>
            !File.Exists(Path.Combine(sourceDirectory, file)));
        if (missingDependency is not null)
            throw new FileNotFoundException($"The Panacea dependency '{missingDependency}' is missing.");

        cancellationToken.ThrowIfCancellationRequested();
        var destinationLoader = Path.Combine(
            releaseDirectory,
            OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll");
        var alternateLoader = Path.Combine(
            releaseDirectory,
            OperatingSystem.IsWindows() ? "version.dll" : "DBGHELP.dll");
        File.Copy(Path.Combine(sourceDirectory, PanaceaFileName), destinationLoader, true);
        if (IsPanaceaLoader(alternateLoader))
            File.Delete(alternateLoader);

        var dependencyDirectory = Path.Combine(releaseDirectory, "dependencies");
        Directory.CreateDirectory(dependencyDirectory);
        foreach (var dependency in DependencyFileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Copy(
                Path.Combine(sourceDirectory, dependency),
                Path.Combine(dependencyDirectory, dependency),
                true);
        }

        var settingsFile = Path.Combine(releaseDirectory, "panacea_settings.txt");
        File.WriteAllLines(settingsFile,
        [
            $"mod_path={PanaceaPath.ToLoaderPath(configuration.CompiledModsRoot)}",
            $"show_console={configuration.Current.ShowConsole.ToString().ToLowerInvariant()}"
        ]);
        configuration.Current.PanaceaInstalled = true;
        configuration.Save();
    }

    private void Remove(bool isKh3D, string? releaseDirectoryOverride, CancellationToken cancellationToken)
    {
        var releaseDirectory = GetReleaseDirectory(isKh3D, releaseDirectoryOverride, true)!;
        SaveReleaseDirectory(isKh3D, releaseDirectory);
        foreach (var loader in GetInstalledLoaderFiles(releaseDirectory))
        {
            cancellationToken.ThrowIfCancellationRequested();
            File.Delete(loader);
        }

        var dependencyDirectory = Path.Combine(releaseDirectory, "dependencies");
        foreach (var dependency in DependencyFileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var installedDependency = Path.Combine(dependencyDirectory, dependency);
            if (File.Exists(installedDependency))
                File.Delete(installedDependency);
        }
        if (Directory.Exists(dependencyDirectory) && !Directory.EnumerateFileSystemEntries(dependencyDirectory).Any())
            Directory.Delete(dependencyDirectory);
        var settingsFile = Path.Combine(releaseDirectory, "panacea_settings.txt");
        if (File.Exists(settingsFile))
            File.Delete(settingsFile);

        var otherStatus = GetStatus(!isKh3D);
        configuration.Current.PanaceaInstalled = otherStatus.IsInstalled;
        configuration.Save();
    }

    private string? FindSourceDirectory()
    {
        var candidates = new[]
        {
            AppContext.BaseDirectory,
            Path.Combine(configuration.InstallationDirectory, "Apps", "ModManager"),
            Path.Combine(configuration.InstallationDirectory, "Apps"),
            configuration.InstallationDirectory
        };
        return candidates.FirstOrDefault(directory =>
            File.Exists(Path.Combine(directory, PanaceaFileName)) &&
            DependencyFileNames.All(file => File.Exists(Path.Combine(directory, file))));
    }

    private string GetInstalledVersion(string installedFile, string? sourceDirectory)
    {
        if (sourceDirectory is not null &&
            FilesMatch(installedFile, Path.Combine(sourceDirectory, PanaceaFileName)))
        {
            return GetSourceVersion(sourceDirectory);
        }

        return GetBuildIdentifier(installedFile);
    }

    private string GetSourceVersion(string sourceDirectory)
    {
        var releaseFile = Path.Combine(configuration.InstallationDirectory, "openkh-release");
        if (File.Exists(releaseFile))
        {
            var release = File.ReadLines(releaseFile)
                .Select(line => line.Trim())
                .FirstOrDefault(line => line.Length > 0);
            if (!string.IsNullOrWhiteSpace(release))
                return $"OpenKH {release}";
        }

        return GetBuildIdentifier(Path.Combine(sourceDirectory, PanaceaFileName));
    }

    private static string GetBuildIdentifier(string fileName)
    {
        using var stream = File.OpenRead(fileName);
        var hash = Convert.ToHexString(SHA256.HashData(stream));
        return $"build {hash[..8]}";
    }

    private string? GetReleaseDirectory(bool isKh3D, string? releaseDirectoryOverride, bool required)
    {
        var directory = string.IsNullOrWhiteSpace(releaseDirectoryOverride)
            ? isKh3D
                ? configuration.Current.PcReleaseLocationKh3D
                : configuration.Current.PcReleaseLocation
            : releaseDirectoryOverride.Trim();
        if (!string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory))
            return directory;
        if (required)
            throw new DirectoryNotFoundException("Configure a valid PC release folder before managing Panacea.");
        return null;
    }

    private void SaveReleaseDirectory(bool isKh3D, string releaseDirectory)
    {
        if (isKh3D)
            configuration.Current.PcReleaseLocationKh3D = releaseDirectory;
        else
            configuration.Current.PcReleaseLocation = releaseDirectory;
    }

    private static IEnumerable<string> GetInstalledLoaderFiles(string releaseDirectory)
    {
        var dbgHelp = Path.Combine(releaseDirectory, "DBGHELP.dll");
        var version = Path.Combine(releaseDirectory, "version.dll");
        if (IsPanaceaLoader(dbgHelp))
            yield return dbgHelp;
        if (IsPanaceaLoader(version))
            yield return version;
    }

    private static bool IsPanaceaLoader(string fileName)
    {
        if (!File.Exists(fileName))
            return false;

        try
        {
            return File.ReadAllBytes(fileName).AsSpan().IndexOf(PanaceaSignature) >= 0;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static bool FilesMatch(string left, string right)
    {
        using var leftStream = File.OpenRead(left);
        using var rightStream = File.OpenRead(right);
        return MD5.HashData(leftStream).SequenceEqual(MD5.HashData(rightStream));
    }
}

public sealed record PanaceaStatus(bool IsInstalled, bool CanInstall, string Message);
