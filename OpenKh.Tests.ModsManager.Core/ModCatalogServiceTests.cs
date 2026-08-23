using OpenKh.Tools.ModsManager.Core;
using Xunit;

namespace OpenKh.Tests.ModsManager.Core;

public sealed class ModCatalogServiceTests : IDisposable
{
    private readonly string _rootDirectory = Path.Combine(
        Path.GetTempPath(),
        "OpenKhModCatalogTests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task LoadReadsMetadataAndKeepsEnabledOrderFirst()
    {
        CreateMod("Author/EnabledSecond", "Enabled second", "Author", "Second description");
        CreateMod("Author/EnabledFirst", "Enabled first", "Author", "First description");
        CreateMod("LocalMod", "Local title", "Local author", "Local description");
        File.WriteAllLines(Path.Combine(_rootDirectory, "mods-KH2.txt"),
        [
            "Author/EnabledFirst",
            "Author/EnabledSecond"
        ]);

        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var service = new ModCatalogService(layout);
        var mods = await service.LoadAsync(GameInfo.FromId("kh2"));

        Assert.Equal(3, mods.Count);
        Assert.Equal("Author/EnabledFirst", mods[0].Id);
        Assert.Equal("Enabled first", mods[0].Name);
        Assert.True(mods[0].IsEnabled);
        Assert.Equal("Author/EnabledSecond", mods[1].Id);
        Assert.False(mods[2].IsEnabled);
    }

    [Fact]
    public async Task SaveEnabledOrderUsesLegacyCompatibleFile()
    {
        CreateMod("Author/First", "First", "Author", "Description");
        CreateMod("Author/Second", "Second", "Author", "Description");
        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var service = new ModCatalogService(layout);
        var game = GameInfo.FromId("kh2");
        var mods = (await service.LoadAsync(game)).ToList();
        mods[1].IsEnabled = true;

        service.SaveEnabledOrder(game, mods);

        Assert.Equal(
            new[] { mods[1].Id },
            File.ReadAllLines(Path.Combine(_rootDirectory, "mods-KH2.txt")));
        Assert.Equal(
            mods.Select(mod => mod.Id),
            File.ReadAllLines(Path.Combine(_rootDirectory, "mod-order-KH2.txt")));
    }

    [Fact]
    public async Task RefreshKeepsTheCompleteSavedOrderIncludingDisabledMods()
    {
        CreateMod("Author/First", "First", "Author", "Description");
        CreateMod("Author/Second", "Second", "Author", "Description");
        CreateMod("Author/Third", "Third", "Author", "Description");
        var layout = InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]);
        var service = new ModCatalogService(layout);
        var game = GameInfo.FromId("kh2");
        var mods = (await service.LoadAsync(game)).ToList();
        var customOrder = new[] { mods[2], mods[0], mods[1] };
        customOrder[1].IsEnabled = true;

        service.SaveEnabledOrder(game, customOrder);
        var refreshed = await service.LoadAsync(game);

        Assert.Equal(customOrder.Select(mod => mod.Id), refreshed.Select(mod => mod.Id));
        Assert.False(refreshed[0].IsEnabled);
        Assert.True(refreshed[1].IsEnabled);
        Assert.False(refreshed[2].IsEnabled);
    }

    [Fact]
    public async Task LoadKeepsPcPatchesBelowOpenKhModsWithoutChangingRelativeOrder()
    {
        CreateMod("Author/PcPatchFirst", "First legacy mod (KH2PCPATCH)", "Author", "Legacy");
        CreateMod("Author/OpenKhFirst", "First OpenKH mod", "Author", "OpenKH");
        CreateMod("Author/PcPatchSecond", "Second legacy mod (KH2PCPATCH)", "Author", "Legacy");
        CreateMod("Author/OpenKhSecond", "Second OpenKH mod", "Author", "OpenKH");
        File.WriteAllLines(Path.Combine(_rootDirectory, "mod-order-KH2.txt"),
        [
            "Author/PcPatchFirst",
            "Author/OpenKhFirst",
            "Author/PcPatchSecond",
            "Author/OpenKhSecond"
        ]);
        var service = new ModCatalogService(
            InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]));

        var mods = await service.LoadAsync(GameInfo.FromId("kh2"));

        Assert.Equal(
            new[]
            {
                "Author/OpenKhFirst",
                "Author/OpenKhSecond",
                "Author/PcPatchFirst",
                "Author/PcPatchSecond"
            },
            mods.Select(mod => mod.Id));
        Assert.False(mods[0].IsPcPatch);
        Assert.False(mods[1].IsPcPatch);
        Assert.True(mods[2].IsPcPatch);
        Assert.True(mods[3].IsPcPatch);
    }

    [Fact]
    public async Task SaveEnabledOrderKeepsPcPatchesBelowOpenKhMods()
    {
        CreateMod("Author/PcPatch", "Legacy mod (KH2PCPATCH)", "Author", "Legacy");
        CreateMod("Author/OpenKh", "OpenKH mod", "Author", "OpenKH");
        var service = new ModCatalogService(
            InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]));
        var game = GameInfo.FromId("kh2");
        var mods = (await service.LoadAsync(game)).ToArray();

        service.SaveEnabledOrder(game, mods.Reverse());

        Assert.Equal(
            new[] { "Author/OpenKh", "Author/PcPatch" },
            File.ReadAllLines(Path.Combine(_rootDirectory, "mod-order-KH2.txt")));
    }

    [Fact]
    public async Task NewlyInstalledModMovesToHighestOpenKhPriority()
    {
        CreateMod("Author/ExistingFirst", "Existing first", "Author", "OpenKH");
        CreateMod("Author/ExistingSecond", "Existing second", "Author", "OpenKH");
        CreateMod("Author/PcPatch", "Legacy mod (KH2PCPATCH)", "Author", "Legacy");
        var service = new ModCatalogService(
            InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]));
        var game = GameInfo.FromId("kh2");
        service.SaveEnabledOrder(game, await service.LoadAsync(game));
        CreateMod("Author/NewSeed", "New seed", "Author", "OpenKH");

        service.MoveInstalledModToHighestPriority(game, "Author/NewSeed");
        var mods = await service.LoadAsync(game);

        Assert.Equal(
            new[]
            {
                "Author/NewSeed",
                "Author/ExistingFirst",
                "Author/ExistingSecond",
                "Author/PcPatch"
            },
            mods.Select(mod => mod.Id));
    }

    [Fact]
    public async Task LoadIncludesFilesModifiedByTheMod()
    {
        var modDirectory = Path.Combine(_rootDirectory, "mods", "kh2", "Author", "FilesMod");
        Directory.CreateDirectory(modDirectory);
        File.WriteAllText(Path.Combine(modDirectory, "mod.yml"), """
            title: Files mod
            originalAuthor: Author
            description: Modifies multiple files
            assets:
            - name: msg/en/sys.bar
              method: copy
              multi:
              - name: msg/fr/sys.bar
            - name: scripts/kh2/example.lua
              method: copy
            """);
        var service = new ModCatalogService(InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]));

        var mods = await service.LoadAsync(GameInfo.FromId("kh2"));

        var mod = Assert.Single(mods);
        Assert.Equal(
            ["msg/en/sys.bar", "msg/fr/sys.bar", "scripts/kh2/example.lua"],
            mod.FilesToPatch);
    }

    [Fact]
    public async Task LoadIncludesSourceAndIssueLinksForRepositoryMods()
    {
        CreateMod("Author/Hosted", "Hosted", "Author", "Description");
        var modDirectory = Path.Combine(_rootDirectory, "mods", "kh2", "Author", "Hosted");
        var gitDirectory = Path.Combine(modDirectory, ".git");
        Directory.CreateDirectory(gitDirectory);
        File.WriteAllText(Path.Combine(gitDirectory, "config"), """
            [remote "origin"]
                url = git@github.com:Author/Hosted.git
                fetch = +refs/heads/*:refs/remotes/origin/*
            """);
        var service = new ModCatalogService(InstallationLayout.Detect("ignored", ["--data-root", _rootDirectory]));

        var mod = Assert.Single(await service.LoadAsync(GameInfo.FromId("kh2")));

        Assert.Equal("https://github.com/Author/Hosted", mod.SourceUrl);
        Assert.Equal("https://github.com/Author/Hosted/issues", mod.ReportBugUrl);
    }

    public void Dispose()
    {
        if (Directory.Exists(_rootDirectory))
            Directory.Delete(_rootDirectory, true);
    }

    private void CreateMod(string id, string title, string author, string description)
    {
        var modDirectory = Path.Combine(_rootDirectory, "mods", "kh2", id.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(modDirectory);
        File.WriteAllText(Path.Combine(modDirectory, "mod.yml"), $"""
            title: {title}
            originalAuthor: {author}
            description: {description}
            assets: []
            """);
    }
}
