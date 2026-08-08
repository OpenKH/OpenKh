using OpenKh.Tools.ModsManager.Services;
using Xunit;

namespace OpenKh.Tests.ModsManager
{
    public class OpenkhInstallationTest
    {
        [Theory]
        [InlineData(@"C:\OpenKh", @"C:\OpenKh")]
        [InlineData(@"C:\OpenKh\Apps", @"C:\OpenKh")]
        [InlineData(@"C:\OpenKh\apps", @"C:\OpenKh")]
        [InlineData(@"C:\OpenKh\Apps\ModManager", @"C:\OpenKh")]
        [InlineData(@"C:\OpenKh\apps\modmanager", @"C:\OpenKh")]
        public void GetDirectoryReturnsInstallationRoot(string applicationDirectory, string expectedDirectory)
        {
            Assert.Equal(
                Path.GetFullPath(expectedDirectory),
                OpenkhInstallation.GetDirectory(applicationDirectory),
                ignoreCase: true
            );
        }

        [Fact]
        public void GetModManagerExecutableSupportsCurrentAndPreviousLayouts()
        {
            var installationDirectory = Path.Combine(
                Path.GetTempPath(),
                $"openkh-installation-{Guid.NewGuid():N}"
            );
            var rootExecutable = Path.Combine(installationDirectory, "OpenKh.Tools.ModsManager.exe");
            var previousExecutable = Path.Combine(
                installationDirectory,
                "Apps",
                "ModManager",
                "OpenKh.Tools.ModsManager.exe"
            );
            var currentExecutable = Path.Combine(
                installationDirectory,
                "Apps",
                "OpenKh.Tools.ModsManager.exe"
            );

            try
            {
                Directory.CreateDirectory(installationDirectory);
                File.WriteAllText(rootExecutable, string.Empty);
                Assert.Equal(rootExecutable, OpenkhInstallation.GetModManagerExecutable(installationDirectory));

                Directory.CreateDirectory(Path.GetDirectoryName(previousExecutable)!);
                File.WriteAllText(previousExecutable, string.Empty);
                Assert.Equal(previousExecutable, OpenkhInstallation.GetModManagerExecutable(installationDirectory));

                File.WriteAllText(currentExecutable, string.Empty);
                Assert.Equal(currentExecutable, OpenkhInstallation.GetModManagerExecutable(installationDirectory));
            }
            finally
            {
                if (Directory.Exists(installationDirectory))
                    Directory.Delete(installationDirectory, recursive: true);
            }
        }
    }
}
