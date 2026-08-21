using OpenKh.Tools.ModsManager.Core;
using OpenKh.Patcher;
using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;

namespace OpenKh.Tests.ModsManager.Core;

public sealed class SetupServicesTests : IDisposable
{
    private static readonly string[] PanaceaDependencies =
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

    private readonly string _rootDirectory = Path.Combine(
        Path.GetTempPath(),
        "OpenKhSetupServicesTests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void NewConfigurationDefaultsToPcSteamGlobal()
    {
        var configuration = new ModManagerConfiguration();

        Assert.Equal(2, configuration.GameEdition);
        Assert.Equal("Steam", configuration.PcVersion);
        Assert.Equal("en", configuration.PcReleaseLanguage);
    }

    [Fact]
    public void LegacyGameEngineConfigurationIsMigratedToPcRelease()
    {
        Directory.CreateDirectory(_rootDirectory);
        File.WriteAllText(Path.Combine(_rootDirectory, "mods-manager.yml"), "gameEdition: 0\npcVersion: ''\n");

        var service = CreateConfigurationService();

        Assert.Equal(2, service.Current.GameEdition);
        Assert.Equal("Steam", service.Current.PcVersion);
    }

    [Fact]
    public void OfficialConfigurationNamesAreLoadedAndPreserved()
    {
        Directory.CreateDirectory(_rootDirectory);
        var configurationPath = Path.Combine(_rootDirectory, "mods-manager.yml");
        var modDirectory = Path.Combine(_rootDirectory, "creator-mod");
        File.WriteAllText(configurationPath, $$"""
            extractedGameDataPath: '{{Path.Combine(_rootDirectory, "extracted")}}'
            installedModsPath: '{{Path.Combine(_rootDirectory, "installed")}}'
            installedCollectionsPath: '{{Path.Combine(_rootDirectory, "collections")}}'
            compiledModPath: '{{Path.Combine(_rootDirectory, "compiled")}}'
            yamlGenPrefs:
            - label: Existing creator setup
              gameDataPath: '{{Path.Combine(_rootDirectory, "creator-data")}}'
              modYmlFilePath: '{{Path.Combine(modDirectory, "mod.yml")}}'
            """);

        var configuration = ModManagerConfiguration.Load(configurationPath);

        Assert.Equal(Path.Combine(_rootDirectory, "extracted"), configuration.GameDataPath);
        Assert.Equal(Path.Combine(_rootDirectory, "installed"), configuration.ModCollectionPath);
        Assert.Equal(Path.Combine(_rootDirectory, "collections"), configuration.ModCollectionsPath);
        Assert.Equal(Path.Combine(_rootDirectory, "compiled"), configuration.GameModPath);
        Assert.Equal(modDirectory, Assert.Single(configuration.CreatorPreferences).ModDirectory);

        configuration.Save(configurationPath);
        var saved = File.ReadAllText(configurationPath);
        Assert.Contains("extractedGameDataPath:", saved);
        Assert.Contains("installedModsPath:", saved);
        Assert.Contains("installedCollectionsPath:", saved);
        Assert.Contains("compiledModPath:", saved);
        Assert.Contains("yamlGenPrefs:", saved);
        Assert.Contains("modYmlFilePath:", saved);
        Assert.DoesNotContain("gameDataPath:", saved.Split("yamlGenPrefs:")[0]);
        Assert.DoesNotContain("modCollectionPath:", saved);
        Assert.DoesNotContain("creatorPreferences:", saved);
    }

    [Fact]
    public void InterimAvaloniaConfigurationIsMigratedToOfficialNames()
    {
        Directory.CreateDirectory(_rootDirectory);
        var configurationPath = Path.Combine(_rootDirectory, "mods-manager.yml");
        var modDirectory = Path.Combine(_rootDirectory, "creator-mod");
        File.WriteAllText(configurationPath, $$"""
            gameDataPath: '{{Path.Combine(_rootDirectory, "extracted")}}'
            modCollectionPath: '{{Path.Combine(_rootDirectory, "installed")}}'
            modCollectionsPath: '{{Path.Combine(_rootDirectory, "collections")}}'
            gameModPath: '{{Path.Combine(_rootDirectory, "compiled")}}'
            creatorPreferences:
            - label: Avalonia creator setup
              modDirectory: '{{modDirectory}}'
              gameDataPath: '{{Path.Combine(_rootDirectory, "creator-data")}}'
              diffToolPath: diff-tool
            """);

        var configuration = ModManagerConfiguration.Load(configurationPath);

        Assert.Equal(Path.Combine(_rootDirectory, "extracted"), configuration.GameDataPath);
        Assert.Equal(Path.Combine(_rootDirectory, "installed"), configuration.ModCollectionPath);
        Assert.Equal(Path.Combine(_rootDirectory, "collections"), configuration.ModCollectionsPath);
        Assert.Equal(Path.Combine(_rootDirectory, "compiled"), configuration.GameModPath);
        var preference = Assert.Single(configuration.CreatorPreferences);
        Assert.Equal(modDirectory, preference.ModDirectory);
        Assert.Equal("diff-tool", preference.DiffToolPath);

        var migrated = File.ReadAllText(configurationPath);
        Assert.Contains("extractedGameDataPath:", migrated);
        Assert.Contains("installedModsPath:", migrated);
        Assert.Contains("yamlGenPrefs:", migrated);
        Assert.DoesNotContain("modCollectionPath:", migrated);
        Assert.DoesNotContain("creatorPreferences:", migrated);
    }

    [Fact]
    public void PreviewDevViewSettingEnablesPatchingTools()
    {
        Directory.CreateDirectory(_rootDirectory);
        var configurationPath = Path.Combine(_rootDirectory, "mods-manager.yml");
        File.WriteAllText(configurationPath, "devView: true\n");

        var configuration = ModManagerConfiguration.Load(configurationPath);

        Assert.True(configuration.EnablePatching);
        var migrated = File.ReadAllText(configurationPath);
        Assert.Contains("enablePatching: true", migrated);
        Assert.DoesNotContain("devView:", migrated);
    }

    [Fact]
    public void LegacyCleanupOnlyTargetsApplicationFiles()
    {
        var appsDirectory = Path.Combine(_rootDirectory, "Apps");
        Directory.CreateDirectory(appsDirectory);
        Directory.CreateDirectory(Path.Combine(_rootDirectory, "mods"));
        Directory.CreateDirectory(Path.Combine(_rootDirectory, "data"));
        Directory.CreateDirectory(Path.Combine(_rootDirectory, "presets"));
        Directory.CreateDirectory(Path.Combine(_rootDirectory, "AdvancedTools"));
        File.WriteAllText(Path.Combine(_rootDirectory, "OpenKh.Launcher.exe"), "launcher");
        File.WriteAllText(Path.Combine(_rootDirectory, "OpenKh.Tools.ModsManager.exe"), "compatibility launcher");
        File.WriteAllText(Path.Combine(appsDirectory, "OpenKh.Tools.ModsManager.exe"), "mod manager");
        File.WriteAllText(Path.Combine(_rootDirectory, "old-library.dll"), "old application file");
        File.WriteAllText(Path.Combine(_rootDirectory, "mods-manager.yml"), "launchGame: kh2");
        File.WriteAllText(Path.Combine(_rootDirectory, "mods-KH2.txt"), "OpenKH/example");
        File.WriteAllText(Path.Combine(appsDirectory, "legacy-release-files.txt"), "old-library.dll\n");
        File.WriteAllText(Path.Combine(appsDirectory, "legacy-release-directories.txt"), "AdvancedTools\n");

        var paths = LegacyInstallationCleanup.GetLegacyPaths(_rootDirectory);

        Assert.Contains(Path.Combine(_rootDirectory, "old-library.dll"), paths);
        Assert.Contains(Path.Combine(_rootDirectory, "AdvancedTools"), paths);
        Assert.DoesNotContain(Path.Combine(_rootDirectory, "OpenKh.Launcher.exe"), paths);
        Assert.DoesNotContain(Path.Combine(_rootDirectory, "OpenKh.Tools.ModsManager.exe"), paths);
        Assert.DoesNotContain(Path.Combine(_rootDirectory, "mods-manager.yml"), paths);
        Assert.DoesNotContain(Path.Combine(_rootDirectory, "mods-KH2.txt"), paths);
        Assert.DoesNotContain(Path.Combine(_rootDirectory, "mods"), paths);
        Assert.DoesNotContain(Path.Combine(_rootDirectory, "data"), paths);
        Assert.DoesNotContain(Path.Combine(_rootDirectory, "presets"), paths);
    }

    [Fact]
    public void UpdateEnvironmentFindsLauncherAndPackagedVersion()
    {
        var applicationRoot = Path.Combine(_rootDirectory, "release");
        var appsDirectory = Path.Combine(applicationRoot, "Apps");
        var dataDirectory = Path.Combine(_rootDirectory, "data-root");
        Directory.CreateDirectory(appsDirectory);
        Directory.CreateDirectory(dataDirectory);
        var launcherName = OperatingSystem.IsWindows() ? "OpenKh.Launcher.exe" : "OpenKh.Launcher";
        var launcherPath = Path.Combine(applicationRoot, launcherName);
        File.WriteAllText(launcherPath, "launcher");
        File.WriteAllText(Path.Combine(applicationRoot, "openkh-release"), "release2-test");

        Assert.Equal(applicationRoot, OpenKhUpdateEnvironment.FindApplicationRoot(appsDirectory));
        Assert.Equal(
            applicationRoot,
            OpenKhUpdateEnvironment.FindVersionDirectory(dataDirectory, appsDirectory));
        Assert.Equal(launcherPath, OpenKhUpdateEnvironment.FindLauncher(dataDirectory, appsDirectory));
    }

    [Fact]
    public void ExtractedGameDataIsDetectedUsingGameSpecificFiles()
    {
        var dataDirectory = Path.Combine(_rootDirectory, "data");
        Directory.CreateDirectory(Path.Combine(dataDirectory, "kh1"));
        Directory.CreateDirectory(Path.Combine(dataDirectory, "bbs", "message"));
        Directory.CreateDirectory(Path.Combine(dataDirectory, "Recom", "SYS"));
        File.WriteAllText(Path.Combine(dataDirectory, "kh1", "btltbl.bin"), "data");
        File.WriteAllText(Path.Combine(dataDirectory, "unrelated.txt"), "not game data");

        var games = GameDataDetectionService.FindExtractedGames(dataDirectory);

        Assert.Equal(["kh1", "bbs", "Recom"], games.Select(game => game.Id));
    }

    [Fact]
    public async Task PanaceaUsesUnsavedSetupPathAndCanBeRemoved()
    {
        Directory.CreateDirectory(_rootDirectory);
        var gameDirectory = Path.Combine(_rootDirectory, "game");
        Directory.CreateDirectory(gameDirectory);
        File.WriteAllText(
            Path.Combine(_rootDirectory, "OpenKH.Panacea.dll"),
            "Welcome to OpenKH Panacea! current loader");
        File.WriteAllText(Path.Combine(_rootDirectory, "openkh-release"), "release2-test");
        foreach (var dependency in PanaceaDependencies)
            File.WriteAllText(Path.Combine(_rootDirectory, dependency), dependency);
        var service = CreateConfigurationService();
        var panacea = new PanaceaService(service);

        await panacea.InstallAsync(false, gameDirectory);

        var installedStatus = panacea.GetStatus(false, gameDirectory);
        Assert.True(installedStatus.IsInstalled);
        Assert.Contains("Panacea version OpenKH release2-test", installedStatus.Message);
        Assert.True(File.Exists(Path.Combine(gameDirectory, OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll")));
        Assert.Equal(gameDirectory, service.Current.PcReleaseLocation);

        await panacea.RemoveAsync(false, gameDirectory);

        Assert.False(panacea.GetStatus(false, gameDirectory).IsInstalled);
    }

    [Fact]
    public async Task PanaceaCanRemoveAnOlderInstallationWithoutSourceFiles()
    {
        var gameDirectory = Path.Combine(_rootDirectory, "game");
        var dependencyDirectory = Path.Combine(gameDirectory, "dependencies");
        Directory.CreateDirectory(dependencyDirectory);
        var loaderPath = Path.Combine(
            gameDirectory,
            OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll");
        File.WriteAllText(loaderPath, "Welcome to OpenKH Panacea! older loader");
        foreach (var dependency in PanaceaDependencies)
            File.WriteAllText(Path.Combine(dependencyDirectory, dependency), "older dependency");
        File.WriteAllText(Path.Combine(gameDirectory, "panacea_settings.txt"), "show_console=false");
        var service = CreateConfigurationService();
        var panacea = new PanaceaService(service);

        var installedStatus = panacea.GetStatus(false, gameDirectory);
        Assert.True(installedStatus.IsInstalled);
        Assert.Contains("source files unavailable", installedStatus.Message);

        await panacea.RemoveAsync(false, gameDirectory);

        Assert.False(File.Exists(loaderPath));
        Assert.False(Directory.Exists(dependencyDirectory));
        Assert.False(File.Exists(Path.Combine(gameDirectory, "panacea_settings.txt")));
        Assert.False(panacea.GetStatus(false, gameDirectory).IsInstalled);
    }

    [Theory]
    [InlineData("Steam", true, true, true)]
    [InlineData("Steam", false, true, true)]
    [InlineData("Steam", true, false, false)]
    [InlineData("Steam", false, false, true)]
    [InlineData("EGS", false, true, false)]
    public void GameLaunchPolicyUsesSteamForEveryLinuxSteamLaunch(
        string pcVersion,
        bool directLaunchConfigured,
        bool isLinux,
        bool expected)
    {
        Assert.Equal(
            expected,
            GameLaunchPolicy.ShouldUseSteamClient(pcVersion, directLaunchConfigured, isLinux));
    }

    [Fact]
    public void SteamAppIdCanBeCreatedAndRemoved()
    {
        var gameDirectory = Path.Combine(_rootDirectory, "steam-game");
        Directory.CreateDirectory(gameDirectory);
        var service = new SteamAppIdService();

        service.Install(gameDirectory, false);

        Assert.True(service.IsInstalled(gameDirectory, false));
        Assert.Equal("2552430", File.ReadAllText(Path.Combine(gameDirectory, "steam_appid.txt")));

        service.Remove(gameDirectory, false);

        Assert.False(File.Exists(Path.Combine(gameDirectory, "steam_appid.txt")));
    }

    [Fact]
    public void PresetsSaveLoadAndRemoveEnabledMods()
    {
        var presets = new PresetService(CreateConfigurationService());

        presets.Save("Boss practice", ["OpenKH/example", "local-tools"]);

        Assert.Contains("Boss practice", presets.GetNames());
        Assert.Equal(["OpenKH/example", "local-tools"], presets.Load("Boss practice"));

        presets.Remove("Boss practice");
        Assert.DoesNotContain("Boss practice", presets.GetNames());
    }

    [Fact]
    public void CreatorWritesValidModMetadata()
    {
        var directory = Path.Combine(_rootDirectory, "created-mod");
        var path = new ModCreatorService().Create(
            directory,
            "Example Mod",
            "OpenKH Community",
            "Example description",
            GameInfo.FromId("kh2"));

        using var stream = File.OpenRead(path);
        var metadata = Metadata.Read(stream);
        Assert.Equal("Example Mod", metadata.Title);
        Assert.Equal("OpenKH Community", metadata.OriginalAuthor);
        Assert.Equal("kh2", metadata.Game);
        Assert.Empty(metadata.Assets);
    }

    [Fact]
    public void CreatorCanAppendFilesAndPersistPreferences()
    {
        var gameData = Path.Combine(_rootDirectory, "data");
        var sourceFile = Path.Combine(gameData, "msg", "en", "example.bar");
        Directory.CreateDirectory(Path.GetDirectoryName(sourceFile)!);
        File.WriteAllText(sourceFile, "game data");
        var modDirectory = Path.Combine(_rootDirectory, "creator-mod");
        var configuration = CreateConfigurationService();
        var creator = new ModCreatorService(configuration);
        creator.Create(modDirectory, "Creator Mod", "OpenKH", "Description", GameInfo.FromId("kh2"));

        creator.AppendCopyFiles(modDirectory, gameData, ["msg/en/example.bar"]);
        creator.SavePreference(new CreatorPreference
        {
            Label = "KH2 workflow",
            ModDirectory = modDirectory,
            GameDataPath = gameData,
            DiffToolPath = "diff-tool"
        });

        using var stream = File.OpenRead(Path.Combine(modDirectory, "mod.yml"));
        var metadata = Metadata.Read(stream);
        var asset = Assert.Single(metadata.Assets);
        Assert.Equal("copy", asset.Method);
        Assert.Equal("msg/en/example.bar", asset.Name);
        Assert.True(File.Exists(Path.Combine(modDirectory, "msg", "en", "example.bar")));
        Assert.Equal("KH2 workflow", Assert.Single(creator.GetPreferences()).Label);
    }

    [Fact]
    public void LuaBackendConfigurationUsesOpenKhScriptsAndCanBeRemoved()
    {
        var gameDirectory = Path.Combine(_rootDirectory, "lua-game");
        Directory.CreateDirectory(gameDirectory);
        File.WriteAllText(Path.Combine(gameDirectory, "LuaBackend.dll"), "backend");
        File.WriteAllText(
            Path.Combine(gameDirectory, "LuaBackend.toml"),
            "[kh2]\nscripts = [{ path = \"scripts/kh2/\", relative = true }]\n" +
            "game_docs = \"Documents/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"\n" +
            "# game_docs = \"My Games/KINGDOM HEARTS HD 1.5+2.5 ReMIX\"\n");
        var configuration = CreateConfigurationService();
        var service = new LuaBackendService(configuration);

        service.Configure(gameDirectory, [GameInfo.FromId("kh2")], true);

        var text = File.ReadAllText(Path.Combine(gameDirectory, "LuaBackend.toml"));
        var expectedScripts = Path.Combine(configuration.GetGameModOutputDirectory(GameInfo.FromId("kh2")), "scripts")
            .Replace("\\", "/");
        Assert.Contains(expectedScripts, text);
        Assert.Contains("[kh2]\nscripts =", text.Replace("\r\n", "\n"));
        Assert.DoesNotContain("[kh2]scripts =", text);
        Assert.Contains("game_docs = \"My Games/", text);
        Assert.True(service.IsInstalled(gameDirectory));

        service.Remove(gameDirectory);

        Assert.False(service.IsInstalled(gameDirectory));
    }

    [Fact]
    public void LuaBackendConfigurationPreservesScriptsIndentation()
    {
        var gameDirectory = Path.Combine(_rootDirectory, "lua-indentation");
        Directory.CreateDirectory(gameDirectory);
        File.WriteAllText(
            Path.Combine(gameDirectory, "LuaBackend.toml"),
            "[kh1]\r\n  scripts = [{ path = \"scripts/kh1/\", relative = true }]\r\n" +
            "exe = \"KINGDOM HEARTS FINAL MIX.exe\"\r\n");
        var service = new LuaBackendService(CreateConfigurationService());

        service.Configure(gameDirectory, [GameInfo.FromId("kh1")], false);

        var text = File.ReadAllText(Path.Combine(gameDirectory, "LuaBackend.toml"));
        Assert.Contains("[kh1]\r\n  scripts =", text);
        Assert.DoesNotContain("[kh1]scripts =", text);
    }

    [Fact]
    public void LuaBackendConfigurationRepairsJoinedSectionHeader()
    {
        var gameDirectory = Path.Combine(_rootDirectory, "lua-joined-header");
        Directory.CreateDirectory(gameDirectory);
        File.WriteAllText(
            Path.Combine(gameDirectory, "LuaBackend.toml"),
            "[kh2]scripts = [{ path = \"scripts/kh2/\", relative = true }]\n" +
            "exe = \"KINGDOM HEARTS II FINAL MIX.exe\"\n");
        var service = new LuaBackendService(CreateConfigurationService());

        service.Configure(gameDirectory, [GameInfo.FromId("kh2")], false);

        var text = File.ReadAllText(Path.Combine(gameDirectory, "LuaBackend.toml"));
        Assert.Contains("[kh2]\nscripts =", text.Replace("\r\n", "\n"));
        Assert.DoesNotContain("[kh2]scripts =", text);
    }

    [Fact]
    public void LuaBackendConfigurationReplacesMultilineScriptsArrayAndRemainsValid()
    {
        var gameDirectory = Path.Combine(_rootDirectory, "lua-multiline");
        Directory.CreateDirectory(gameDirectory);
        File.WriteAllText(
            Path.Combine(gameDirectory, "LuaBackend.toml"),
            "[kh2]\r\n" +
            "scripts = [\r\n" +
            "  { path = \"scripts/kh2/\", relative = true },\r\n" +
            "  { path = \"C:/old/scripts\", relative = false },\r\n" +
            "]\r\n" +
            "exe = \"KINGDOM HEARTS II FINAL MIX.exe\"\r\n" +
            "\r\n[bbs]\r\n" +
            "scripts = [{ path = \"scripts/bbs/\", relative = true }]\r\n");
        var service = new LuaBackendService(CreateConfigurationService());

        service.Configure(gameDirectory, [GameInfo.FromId("kh2")], false);
        service.Configure(gameDirectory, [GameInfo.FromId("kh2")], false);

        var text = File.ReadAllText(Path.Combine(gameDirectory, "LuaBackend.toml"));
        var normalized = text.Replace("\r\n", "\n");
        Assert.Contains("[kh2]\nscripts =", normalized);
        Assert.DoesNotContain("[kh2]scripts =", normalized);
        Assert.DoesNotContain("C:/old/scripts", normalized);
        Assert.Contains("\nexe = \"KINGDOM HEARTS II FINAL MIX.exe\"", normalized);
        Assert.Contains("[bbs]\nscripts =", normalized);
    }

    [Fact]
    public async Task FastRestoreOnlyRemovesGeneratedModOutput()
    {
        var configuration = CreateConfigurationService();
        var game = GameInfo.FromId("kh2");
        var output = configuration.GetGameModOutputDirectory(game);
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "generated.bin"), "generated");

        await new PcPackagePatchService(configuration).RestoreAsync(game, false);

        Assert.False(Directory.Exists(output));
    }

    [Fact]
    public async Task OnlineCatalogUsesOfficialFeedImagesAndDenyList()
    {
        var handler = new CatalogHttpHandler();
        using var client = new HttpClient(handler);
        var cacheDirectory = Path.Combine(_rootDirectory, "online-cache");
        var service = new OnlineModCatalogService(CreateConfigurationService(), client, cacheDirectory);

        var mods = await service.LoadAsync(GameInfo.FromId("kh2"), ["Owner/Installed"]);

        var mod = Assert.Single(mods);
        Assert.Equal("Owner/Visible", mod.Repository);
        Assert.Equal("Visible Mod", mod.Title);
        Assert.True(File.Exists(mod.IconPath));
        Assert.True(File.Exists(mod.PreviewPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
            Directory.Delete(_rootDirectory, true);
    }

    private ModManagerConfigurationService CreateConfigurationService() => new(
        InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]));

    private sealed class CatalogHttpHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsoluteUri ?? string.Empty;
            if (path.EndsWith("downloadable-mods.json", StringComparison.Ordinal))
            {
                return Response("""
                    {"mods":{"kh2":[
                      {"repo":"Owner/Visible"},
                      {"repo":"Owner/Denied"},
                      {"repo":"Owner/Installed"}
                    ]}}
                    """);
            }
            if (path.EndsWith("deny.txt", StringComparison.Ordinal))
                return Response("Owner/Denied\n");
            if (path.EndsWith("mod.yml", StringComparison.Ordinal) && path.Contains("Owner/Visible", StringComparison.Ordinal))
            {
                return Response("title: Visible Mod\noriginalAuthor: Test Author\ndescription: Test description\ngame: kh2\nassets: []\n");
            }
            if (path.EndsWith("icon.png", StringComparison.Ordinal) ||
                path.EndsWith("preview.png", StringComparison.Ordinal))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent([1, 2, 3, 4])
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        private static Task<HttpResponseMessage> Response(string content) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8)
            });
    }
}
