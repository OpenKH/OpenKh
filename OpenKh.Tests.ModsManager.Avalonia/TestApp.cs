using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using OpenKh.Tools.ModsManager;

[assembly: AvaloniaTestApplication(typeof(OpenKh.Tests.ModsManager.Avalonia.TestApp))]

namespace OpenKh.Tests.ModsManager.Avalonia;

public static class TestApp
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder
        .Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
