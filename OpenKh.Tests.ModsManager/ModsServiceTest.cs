using OpenKh.Tools.ModsManager.Services;
using Xunit;
using Xunit.Abstractions;

namespace OpenKh.Tests.ModsManager
{
    [Collection("Sequential")]
    public class ModsServiceTest : IDisposable
    {
        private readonly ITestOutputHelper output;

        public ModsServiceTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void Dispose()
        {
            try
            {
                DeleteDir(Path.Combine(ConfigurationService.ModsGamePath, "test"));
            }
            catch (Exception e)
            {
                output.WriteLine(e.Message);
            }
            try
            {
                DeleteDir(Path.Combine(ConfigurationService.ModCollectionsPath, "test"));
            }
            catch (Exception e)
            {
                output.WriteLine(e.Message);
            }
            try
            {
                File.Delete(Path.Combine(ConfigurationService.PresetPath, "..", "mods-KH2.txt"));
            }
            catch (Exception e)
            {
                output.WriteLine(e.Message);
            }
            GC.SuppressFinalize(this);
        }

        private void AddMod()
        {
            var modDir = ConfigurationService.ModsGamePath;
            Directory.CreateDirectory(Path.Combine(modDir, "test/test"));
            using (File.Create(Path.Combine(modDir, "test/test/mod.yml"))) { }
        }

        private void EnableMod()
        {
            ConfigurationService.EnabledMods = new List<string> { "test/test" };
        }

        private void AddCollectionMod()
        {
            var modDir = ConfigurationService.ModCollectionsPath;
            Directory.CreateDirectory(Path.Combine(modDir, "test/test"));
            using (File.Create(Path.Combine(modDir, "test/test/mod.yml")))
            { }
        }

        private static void DeleteDir(string target)
        {
            string[] files = Directory.GetFiles(target);
            string[] dirs = Directory.GetDirectories(target);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDir(dir);
            }

            Directory.Delete(target, false);
        }

        [Fact]
        public void ModsServiceNoModsTest()
	    {
            var mods = ModsService.Mods;
            Assert.Empty(mods);
        }

        [Fact]
        public void ModsServiceModsOneStandardTest()
        {
            AddMod();
            var mods = ModsService.Mods;
            Assert.Single(mods);
            Dispose();
        }

        [Fact]
        public void ModsServiceModsOneCollectionTest()
        {
            AddCollectionMod();
            var mods = ModsService.Mods;
            Assert.Single(mods);
            Dispose();
        }

        [Fact]
        public void ModsServiceUnorderedModsEmptyTest()
        {
            var mods = ModsService.UnorderedMods;
            Assert.Empty(mods);
        }

        [Fact]
        public void ModsServiceUnorderedModsOneStandardTest()
        {
            AddMod();
            var mods = ModsService.UnorderedMods;
            Assert.Single(mods);
            Dispose();
        }

        [Fact]
        public void ModsServiceUnorderedModsOneCollectionTest()
        {
            AddCollectionMod();
            var mods = ModsService.UnorderedMods;
            Assert.Single(mods);
            Dispose();
        }

        [Fact]
        public void ModsServiceNoEnabledModsTest()
        {
            var mods = ModsService.EnabledMods;
            Assert.Empty(mods);
        }

        [Fact]
        public void ModsServiceEnabledModsTest()
        {
            AddMod();
            EnableMod();
            var mods = ModsService.EnabledMods;
            Assert.Single(mods);
            Dispose();
        }

        [Fact]
        public void ModsServiceIsModBlockedFalse()
        {
            Assert.False(ModsService.IsModBlocked("test/test"));
        }

        [Fact]
        public void ModsServiceIsUserBlockedFalse()
        {
            Assert.False(ModsService.IsUserBlocked("test/test"));
        }
    }
}

