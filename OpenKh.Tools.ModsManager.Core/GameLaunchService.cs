using System.Diagnostics;
using OpenKh.Tools.ModsManager.Services;

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
            if (!useDirectLaunch)
            {
                OpenUri(isKh3D ? "steam://rungameid/2552440" : "steam://rungameid/2552430");
                return Task.CompletedTask;
            }
        }
        else if (configuration.Current.PcVersion.Equals("EGS", StringComparison.OrdinalIgnoreCase))
        {
            OpenUri(isKh3D
                ? "com.epicgames.launcher://apps/c8ff067c1c984cd7ab1998e8a9afc8b6%3Aaa743b9f52e84930b0ba1b701951e927%3Ad1a8f7c478d4439b8c60a5808715dc05?action=launch&silent=true"
                : "com.epicgames.launcher://apps/4158b699dd70447a981fee752d970a3e%3A5aac304f0e8948268ddfd404334dbdc7%3A68c214c58f694ae88c2dab6f209b43e4?action=launch&silent=true");
            return Task.CompletedTask;
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
        values["mod_path"] = Path.GetFullPath(Path.Combine(configuration.GetGameModOutputDirectory(game), ".."));
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
