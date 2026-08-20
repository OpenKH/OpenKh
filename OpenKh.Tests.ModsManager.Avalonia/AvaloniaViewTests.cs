using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using OpenKh.Tools.ModsManager.UserControls;
using OpenKh.Tools.ModsManager.Views;
using Xunit;

namespace OpenKh.Tests.ModsManager.Avalonia;

public class AvaloniaViewTests
{
    [AvaloniaFact]
    public void AppResourcesCanBeLoaded()
    {
        Assert.NotNull(global::Avalonia.Application.Current);
        Assert.NotEmpty(global::Avalonia.Application.Current!.Styles);
    }

    [AvaloniaFact]
    public void ReusableControlsCanLoadTheirXaml()
    {
        Control[] controls =
        {
            new FolderSelectorControl(),
            new SaveFileSelectorControl(),
            new TaskStatusByIconControl(),
            new TaskStatusObserverControl(),
        };

        Assert.All(controls, control => Assert.NotNull(control));
    }

    [AvaloniaFact]
    public void ModSearchWindowHasRuntimeLoadableParameterlessConstructor()
    {
        var window = new ModSearchWindow();

        Assert.NotNull(window.Content);
        window.Close();
    }

    private static readonly Func<Window>[] AdditionalWindows =
    {
        () => new PresetsWindow(),
        () => new NotepadWindow(),
        () => new SelectModTargetFilesWindow(),
        () => new CopySourceFilesWindow(),
        () => new DebuggingWindow(),
        () => new InstallModProgressWindow(),
        () => new WorkInProgressWindow(),
        () => new YamlGeneratorWindow(),
        () => new SetupWizardWindow(),
    };

    [AvaloniaFact]
    public void AdditionalWindowsCanLoadTheirXaml()
    {
        foreach (var createWindow in AdditionalWindows)
        {
            var window = createWindow();

            Assert.True(window.Content is not null, $"{window.GetType().Name} did not load its XAML content.");
            window.Close();
        }
    }
}
