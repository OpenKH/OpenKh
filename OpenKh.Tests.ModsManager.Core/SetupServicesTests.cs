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
    public async Task PanaceaUsesUnsavedSetupPathAndCanBeRemoved()
    {
        Directory.CreateDirectory(_rootDirectory);
        var gameDirectory = Path.Combine(_rootDirectory, "game");
        Directory.CreateDirectory(gameDirectory);
        File.WriteAllText(Path.Combine(_rootDirectory, "OpenKH.Panacea.dll"), "loader");
        foreach (var dependency in PanaceaDependencies)
            File.WriteAllText(Path.Combine(_rootDirectory, dependency), dependency);
        var service = CreateConfigurationService();
        var panacea = new PanaceaService(service);

        await panacea.InstallAsync(false, gameDirectory);

        Assert.True(panacea.GetStatus(false, gameDirectory).IsInstalled);
        Assert.True(File.Exists(Path.Combine(gameDirectory, OperatingSystem.IsWindows() ? "DBGHELP.dll" : "version.dll")));
        Assert.Equal(gameDirectory, service.Current.PcReleaseLocation);

        await panacea.RemoveAsync(false, gameDirectory);

        Assert.False(panacea.GetStatus(false, gameDirectory).IsInstalled);
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
        Assert.Contains("game_docs = \"My Games/", text);
        Assert.True(service.IsInstalled(gameDirectory));

        service.Remove(gameDirectory);

        Assert.False(service.IsInstalled(gameDirectory));
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
