using OpenKh.Tools.ModsManager.Services;
using Xunit;

namespace OpenKh.Tests.ModsManager
{
    public class OpenkhInstallationTest
    {
        [Theory]
        [InlineData(@"C:\OpenKh", @"C:\OpenKh")]
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
    }
}
