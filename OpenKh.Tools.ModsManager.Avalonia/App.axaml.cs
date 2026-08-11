using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OpenKh.Tools.ModsManager.Avalonia.ViewModels;
using OpenKh.Tools.ModsManager.Avalonia.Views;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia;

public sealed partial class App : Application
{
    private IControllerInputService? _controllerInput;
    private SteamworksPlatformService? _steamworks;

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var layout = InstallationLayout.Detect(AppContext.BaseDirectory, desktop.Args);
            var mainWindow = new MainWindow();
            var configuration = new ModManagerConfigurationService(layout);
            var localInstaller = new LocalModInstaller(configuration);
            var repositoryInstaller = new RepositoryModInstaller(configuration, localInstaller);
            var extractionService = new GameExtractionService(configuration);
            var collectionSettings = new CollectionSettingsService(configuration);
            var panaceaService = new PanaceaService(configuration);
            var luaBackendService = new LuaBackendService(configuration);
            var gameLaunchService = new GameLaunchService(configuration);
            _steamworks = new SteamworksPlatformService();
            _steamworks.TryStart();
            _controllerInput = new SdlControllerInputService();
            var mainViewModel = new MainWindowViewModel(
                new ModCatalogService(configuration),
                repositoryInstaller,
                new ModMaintenanceService(configuration),
                new AvaloniaModInstallPrompt(mainWindow, _controllerInput),
                new AvaloniaOnlineModsPrompt(
                    mainWindow,
                    new OnlineModCatalogService(configuration),
                    repositoryInstaller,
                    _controllerInput),
                new AvaloniaPresetsPrompt(mainWindow, new PresetService(configuration), _controllerInput),
                new AvaloniaSettingsPrompt(
                    mainWindow,
                    configuration,
                    panaceaService,
                    new OpenKhUpdateCheckerService(configuration),
                    _controllerInput),
                new AvaloniaInfoPrompt(mainWindow, _controllerInput),
                new AvaloniaCreatorPrompt(mainWindow, new ModCreatorService(configuration), _controllerInput),
                new AvaloniaUserDialogService(mainWindow, _controllerInput),
                new AvaloniaSetupPrompt(
                    mainWindow,
                    configuration,
                    panaceaService,
                    luaBackendService,
                    extractionService,
                    _controllerInput),
                configuration,
                new ModBuildService(configuration),
                gameLaunchService,
                new AvaloniaCollectionSettingsPrompt(mainWindow, collectionSettings, _controllerInput),
                new PcPackagePatchService(configuration),
                _controllerInput);
            mainWindow.DataContext = mainViewModel;
            mainWindow.Opened += (_, _) => Dispatcher.UIThread.Post(
                async () => await mainViewModel.RunInitialSetupAsync(),
                DispatcherPriority.Background);
            _controllerInput.ActionTriggered += mainWindow.HandleControllerAction;
            desktop.MainWindow = mainWindow;
            desktop.Exit += (_, _) =>
            {
                gameLaunchService.Stop();
                _controllerInput.Dispose();
                _steamworks.Dispose();
            };
            _controllerInput.Start();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
