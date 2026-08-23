using System.ComponentModel;
using System.Diagnostics;
using OpenKh.Tools.ModsManager.Core.Services;

namespace OpenKh.Tools.ModsManager.Core;

public sealed class GameLaunchService(ModManagerConfigurationService configuration)
{
    private Process? _runningProcess;
    private Pcsx2Injector? _pcsx2Injector;
    private static readonly IReadOnlyDictionary<string, string> Executables =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["kh2"] = "KINGDOM HEARTS II FINAL MIX.exe",
            ["kh1"] = "KINGDOM HEARTS FINAL MIX.exe",
            ["bbs"] = "KINGDOM HEARTS Birth by Sleep FINAL MIX.exe",
            ["Recom"] = "KINGDOM HEARTS Re_Chain of Memories.exe",
            ["kh3d"] = "KINGDOM HEARTS Dream Drop Distance.exe"
        };

    public Task LaunchAsync(GameInfo game, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return configuration.Current.GameEdition switch
        {
            1 => LaunchPcsx2Async(game),
            2 => LaunchPcAsync(game),
            _ => throw new InvalidOperationException("Select a supported game edition in Setup.")
        };
    }

    public event Action? RunningStateChanged;

    public bool IsRunning
    {
        get
        {
            try
            {
                return _runningProcess is { HasExited: false };
            }
            catch
            {
                return false;
            }
        }
    }

    public void Stop()
    {
        var process = _runningProcess;
        _runningProcess = null;
        _pcsx2Injector?.Stop();
        _pcsx2Injector = null;
        if (process is not null)
        {
            try
            {
                if (!process.HasExited)
                {
                    process.CloseMainWindow();
                    if (!process.WaitForExit(1500))
                        process.Kill(true);
                }
            }
            finally
            {
                process.Dispose();
            }
        }
        RunningStateChanged?.Invoke();
    }

    private Task LaunchPcsx2Async(GameInfo game)
    {
        var executable = RequireFile(configuration.Current.Pcsx2Location, "PCSX2");
        var iso = game.Id.ToLowerInvariant() switch
        {
            "kh1" => configuration.Current.IsoLocationKh1,
            "recom" => configuration.Current.IsoLocationRecom,
            _ => configuration.Current.IsoLocationKh2
        };
        iso = RequireFile(iso, $"{game.DisplayName} ISO");
        var process = StartTracked(new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = Path.GetDirectoryName(executable),
            ArgumentList = { iso },
            UseShellExecute = false
        });
        if (OperatingSystem.IsWindows())
        {
            var regionId = Math.Clamp(configuration.Current.RegionId, 0, OpenKh.Kh2.Constants.Regions.Length - 1);
            _pcsx2Injector = new Pcsx2Injector(new Pcsx2OperationDispatcher(configuration, game))
            {
                RegionId = regionId,
                Region = OpenKh.Kh2.Constants.Regions[regionId],
                Language = OpenKh.Kh2.Constants.Languages[regionId]
            };
            _pcsx2Injector.Run(process, new NullDebugging());
        }
        return Task.CompletedTask;
    }

    private Task LaunchPcAsync(GameInfo game)
    {
        var isKh3D = game.Id.Equals("kh3d", StringComparison.OrdinalIgnoreCase);
        var releaseDirectory = isKh3D
            ? configuration.Current.PcReleaseLocationKh3D
            : configuration.Current.PcReleaseLocation;
        if (string.IsNullOrWhiteSpace(releaseDirectory) || !Directory.Exists(releaseDirectory))
            throw new DirectoryNotFoundException($"The {game.DisplayName} installation folder is not configured.");

        WritePanaceaSettings(releaseDirectory, game);
        if (configuration.Current.PcVersion.Equals("Steam", StringComparison.OrdinalIgnoreCase))
        {
            var useDirectLaunch = isKh3D
                ? configuration.Current.SteamApiTrick28
                : configuration.Current.SteamApiTrick1525;
            if (GameLaunchPolicy.ShouldUseSteamClient(
                    configuration.Current.PcVersion,
                    useDirectLaunch,
                    OperatingSystem.IsLinux()))
            {
                OpenSteamGame(isKh3D ? "2552440" : "2552430");
                return Task.CompletedTask;
            }
        }
        else if (configuration.Current.PcVersion.Equals("EGS", StringComparison.OrdinalIgnoreCase))
        {
            OpenEpicGame(isKh3D);
            return Task.CompletedTask;
        }

        if (OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException(
                "Launching Windows game executables directly is not supported on Linux. " +
                "Select Steam or Epic Games Store so a Linux-compatible launcher can start the game.");
        }

        var executable = Path.Combine(releaseDirectory, Executables[game.Id]);
        RequireFile(executable, game.DisplayName);
        StartTracked(new ProcessStartInfo
        {
            FileName = executable,
            WorkingDirectory = releaseDirectory,
            UseShellExecute = false
        });
        return Task.CompletedTask;
    }

    private void WritePanaceaSettings(string releaseDirectory, GameInfo game)
    {
        if (!configuration.Current.PanaceaInstalled)
            return;

        var settingsPath = Path.Combine(releaseDirectory, "panacea_settings.txt");
        var values = File.Exists(settingsPath)
            ? File.ReadAllLines(settingsPath)
                .Where(line => line.Contains('='))
                .Select(line => line.Split('=', 2))
                .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim(), StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        values["mod_path"] = PanaceaPath.ToLoaderPath(
            Path.GetFullPath(Path.Combine(configuration.GetGameModOutputDirectory(game), "..")));
        values["show_console"] = configuration.Current.ShowConsole.ToString().ToLowerInvariant();
        values["quick_launch"] = game.Id;
        File.WriteAllLines(settingsPath, values.Select(value => $"{value.Key}={value.Value}"));
    }

    private static string RequireFile(string? fileName, string description)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            throw new FileNotFoundException($"The {description} file is not configured or cannot be found.", fileName);
        return fileName;
    }

    private static void OpenUri(string uri) => Start(new ProcessStartInfo(uri) { UseShellExecute = true });

    private static void OpenSteamGame(string appId)
    {
        var uri = $"steam://rungameid/{appId}";
        if (!OperatingSystem.IsLinux())
        {
            OpenUri(uri);
            return;
        }

        var launchers = new[]
        {
            (Command: "steam", Arguments: new[] { uri }),
            (Command: "xdg-open", Arguments: new[] { uri }),
            (Command: "gio", Arguments: new[] { "open", uri }),
            (Command: "flatpak", Arguments: new[] { "run", "com.valvesoftware.Steam", uri })
        };
        if (TryStartFirstAvailable(launchers))
            return;

        throw new InvalidOperationException(
            "Steam could not be found. Install Steam or make the steam, xdg-open, gio, or flatpak command available.");
    }

    private static void OpenEpicGame(bool isKh3D)
    {
        var appName = isKh3D
            ? "d1a8f7c478d4439b8c60a5808715dc05"
            : "68c214c58f694ae88c2dab6f209b43e4";
        var epicUri = isKh3D
            ? "com.epicgames.launcher://apps/c8ff067c1c984cd7ab1998e8a9afc8b6%3Aaa743b9f52e84930b0ba1b701951e927%3Ad1a8f7c478d4439b8c60a5808715dc05?action=launch&silent=true"
            : "com.epicgames.launcher://apps/4158b699dd70447a981fee752d970a3e%3A5aac304f0e8948268ddfd404334dbdc7%3A68c214c58f694ae88c2dab6f209b43e4?action=launch&silent=true";
        if (!OperatingSystem.IsLinux())
        {
            OpenUri(epicUri);
            return;
        }

        var heroicUri = $"heroic://launch?appName={appName}&runner=legendary";
        var launchers = new List<(string Command, string[] Arguments)>
        {
            ("heroic", ["--no-gui", "--no-sandbox", heroicUri]),
            ("legendary", ["launch", appName])
        };
        if (IsFlatpakAppInstalled("com.heroicgameslauncher.hgl"))
        {
            launchers.Add((
                "flatpak",
                ["run", "com.heroicgameslauncher.hgl", "--no-gui", "--no-sandbox", heroicUri]));
        }
        launchers.Add(("xdg-open", [heroicUri]));
        launchers.Add(("gio", ["open", heroicUri]));
        launchers.Add(("xdg-open", [epicUri]));
        launchers.Add(("gio", ["open", epicUri]));

        if (TryStartFirstAvailable(launchers))
            return;

        throw new InvalidOperationException(
            "Epic Games could not be launched on Linux. Install Heroic or Legendary, or register an Epic Games URI handler.");
    }

    private static bool TryStartFirstAvailable(
        IEnumerable<(string Command, string[] Arguments)> launchers)
    {
        foreach (var launcher in launchers.Where(candidate => CommandExists(candidate.Command)))
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = launcher.Command,
                    UseShellExecute = false
                };
                foreach (var argument in launcher.Arguments)
                    startInfo.ArgumentList.Add(argument);
                using var process = Process.Start(startInfo);
                if (process is null)
                    continue;
                if (process.WaitForExit(1000) && process.ExitCode != 0)
                    continue;
                return true;
            }
            catch (Win32Exception)
            {
            }
        }

        return false;
    }

    private static bool CommandExists(string command)
    {
        var path = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
            return false;

        return path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(directory => Path.Combine(directory.Trim(), command))
            .Any(File.Exists);
    }

    private static bool IsFlatpakAppInstalled(string applicationId)
    {
        if (!CommandExists("flatpak"))
            return false;

        var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var candidates = new[]
        {
            Path.Combine(homeDirectory, ".var", "app", applicationId),
            Path.Combine(homeDirectory, ".local", "share", "flatpak", "app", applicationId),
            Path.Combine(Path.DirectorySeparatorChar.ToString(), "var", "lib", "flatpak", "app", applicationId)
        };
        return candidates.Any(Directory.Exists);
    }

    private Process StartTracked(ProcessStartInfo startInfo)
    {
        Stop();
        var process = Process.Start(startInfo) ??
            throw new InvalidOperationException($"Could not start {startInfo.FileName}.");
        _runningProcess = process;
        process.EnableRaisingEvents = true;
        process.Exited += (_, _) => HandleProcessExited(process);
        RunningStateChanged?.Invoke();
        return process;
    }

    private void HandleProcessExited(Process process)
    {
        if (!ReferenceEquals(_runningProcess, process))
            return;
        _pcsx2Injector?.Stop();
        _pcsx2Injector = null;
        _runningProcess = null;
        process.Dispose();
        RunningStateChanged?.Invoke();
    }

    private static void Start(ProcessStartInfo startInfo)
    {
        if (Process.Start(startInfo) is null)
            throw new InvalidOperationException($"Could not start {startInfo.FileName}.");
    }
}
