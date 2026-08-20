using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.Views;

namespace OpenKh.Tools.ModsManager
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Re-evaluate command CanExecute after input events, like
                // WPF's CommandManager does, because the shared ViewModels depend
                // on it.
                OpenKh.Tools.Common.Avalonia.CommandManagerBehavior.Attach();

                // The tool's Dark Mode setting only paints backgrounds via
                // ColorTheme bindings; the Fluent theme variant must follow it
                // too, or themed foregrounds (ComboBox selection text, menu
                // items, ...) stay dark-on-dark.
                ApplyTheme();
                ColorThemeService.Instance.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName is null or nameof(ColorThemeService.DarkMode))
                        ApplyTheme();
                };

                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void ApplyTheme() =>
            Current.RequestedThemeVariant = ColorThemeService.Instance.DarkMode
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
    }
}
