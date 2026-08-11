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
