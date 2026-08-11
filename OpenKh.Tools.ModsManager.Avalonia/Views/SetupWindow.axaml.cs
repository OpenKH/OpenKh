using Avalonia.Controls;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Media;
using OpenKh.Tools.ModsManager.Avalonia.ViewModels;
using OpenKh.Tools.ModsManager.Core;
using OpenKh.Tools.ModsManager.Avalonia.Services;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class SetupWindow : EmbeddedDialogControl
{
    private PanaceaService? _panacea;
    private LuaBackendService? _luaBackend;
    private GameExtractionService? _extraction;
    private ModManagerConfigurationService? _configuration;
    private readonly GameInstallationDetectionService _installationDetector = new();
    private readonly SteamAppIdService _steamAppId = new();
    private ComboBox? _activeComboBox;

    public SetupWindow()
    {
        InitializeComponent();
        var configuration = new ModManagerConfigurationService(
            InstallationLayout.Detect(AppContext.BaseDirectory));
        Initialize(
            configuration,
            new PanaceaService(configuration),
            new LuaBackendService(configuration),
            new GameExtractionService(configuration));
    }

    public SetupWindow(
        ModManagerConfigurationService configuration,
        PanaceaService panacea,
        LuaBackendService luaBackend,
        GameExtractionService extraction)
    {
        InitializeComponent();
        Initialize(configuration, panacea, luaBackend, extraction);
    }

    private void Initialize(
        ModManagerConfigurationService configuration,
        PanaceaService panacea,
        LuaBackendService luaBackend,
        GameExtractionService extraction)
    {
        _configuration = configuration;
        _panacea = panacea;
        _luaBackend = luaBackend;
        _extraction = extraction;
        DataContext = new SetupWindowViewModel(configuration);
        Opened += (_, _) =>
        {
            SetupScrollViewer.Offset = Vector.Zero;
            GameEditionComboBox.Focus();
        };
        RefreshPanaceaStatus();
        RefreshLuaBackendStatus();
        RefreshSteamAppIdStatus();
        RefreshExtractionAvailability();
    }

    private async void BrowseFolder_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (sender is not Button { Tag: string textBoxName } || GetPathTextBox(textBoxName) is not { } textBox)
            return;

        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Choose a folder",
            AllowMultiple = false
        });
        if (folders.Count > 0 && folders[0].TryGetLocalPath() is { } path)
        {
            textBox.Text = path;
            RefreshExtractionAvailability();
        }
    }

    private async void BrowseFile_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (sender is not Button { Tag: string textBoxName } || GetPathTextBox(textBoxName) is not { } textBox)
            return;

        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Choose a file",
            AllowMultiple = false
        });
        if (files.Count > 0 && files[0].TryGetLocalPath() is { } path)
        {
            textBox.Text = path;
            RefreshExtractionAvailability();
        }
    }

    private void Save_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (DataContext is SetupWindowViewModel viewModel)
            viewModel.Save();
        Close(true);
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(false);

    private void PcReleaseLocation_OnTextChanged(object? sender, TextChangedEventArgs eventArgs) =>
        RefreshInstallationStatuses();

    private void ExtractionSource_OnTextChanged(object? sender, TextChangedEventArgs eventArgs) =>
        RefreshExtractionAvailability();

    private void GameEdition_OnSelectionChanged(object? sender, SelectionChangedEventArgs eventArgs) =>
        RefreshExtractionAvailability();

    private void PcVersion_OnSelectionChanged(object? sender, SelectionChangedEventArgs eventArgs) =>
        RefreshInstallationStatuses();

    private void RefreshInstallationStatuses()
    {
        RefreshPanaceaStatus();
        RefreshLuaBackendStatus();
        RefreshSteamAppIdStatus();
        RefreshExtractionAvailability();
    }

    private void DetectInstallations_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (DataContext is not SetupWindowViewModel viewModel)
            return;

        var platform = viewModel.PcVersionIndex switch
        {
            0 => "EGS",
            1 => "Steam",
            _ => "Other"
        };
        var result = _installationDetector.Detect(platform);
        if (!string.IsNullOrWhiteSpace(result.RemixDirectory))
            viewModel.PcReleaseLocation = result.RemixDirectory;
        if (!string.IsNullOrWhiteSpace(result.Kh3DDirectory))
            viewModel.PcReleaseLocationKh3D = result.Kh3DDirectory;
        DetectionStatusText.Text = result.Message;
        DetectionStatusText.Foreground = global::Avalonia.Media.Brush.Parse(
            result.RemixDirectory is not null || result.Kh3DDirectory is not null ? "#62D6A7" : "#F1B86B");
        RefreshInstallationStatuses();
    }

    private void SteamAppId_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (sender is not Button { Tag: string target } || DataContext is not SetupWindowViewModel viewModel)
            return;
        var isKh3D = target == "28";
        var releaseDirectory = GetPcReleaseDirectory(isKh3D);
        try
        {
            var installed = _steamAppId.IsInstalled(releaseDirectory, isKh3D);
            if (installed)
                _steamAppId.Remove(releaseDirectory, isKh3D);
            else
                _steamAppId.Install(releaseDirectory, isKh3D);
            if (isKh3D)
                viewModel.SteamApiTrick28 = !installed;
            else
                viewModel.SteamApiTrick1525 = !installed;
        }
        catch (Exception exception)
        {
            var statusText = isKh3D ? SteamAppId28StatusText : SteamAppId1525StatusText;
            statusText.Text = exception.Message;
            statusText.Foreground = global::Avalonia.Media.Brush.Parse("#FF8B8B");
            return;
        }
        RefreshSteamAppIdStatus();
    }

    private void RefreshSteamAppIdStatus()
    {
        if (SteamAppId1525Button is null || SteamAppId28Button is null)
            return;
        ApplySteamAppIdStatus(false);
        ApplySteamAppIdStatus(true);
    }

    private void ApplySteamAppIdStatus(bool isKh3D)
    {
        var releaseDirectory = GetPcReleaseDirectory(isKh3D);
        var validDirectory = !string.IsNullOrWhiteSpace(releaseDirectory) && Directory.Exists(releaseDirectory);
        var installed = validDirectory && _steamAppId.IsInstalled(releaseDirectory, isKh3D);
        if (DataContext is SetupWindowViewModel viewModel)
        {
            if (isKh3D)
                viewModel.SteamApiTrick28 = installed;
            else
                viewModel.SteamApiTrick1525 = installed;
        }
        var statusText = isKh3D ? SteamAppId28StatusText : SteamAppId1525StatusText;
        var button = isKh3D ? SteamAppId28Button : SteamAppId1525Button;
        statusText.Text = validDirectory
            ? installed ? "Direct launch is configured" : "Direct launch is not configured"
            : "Configure the game folder first";
        statusText.Foreground = global::Avalonia.Media.Brush.Parse(installed ? "#62D6A7" : "#91A2BA");
        button.Content = installed ? "Remove" : "Create";
        button.IsEnabled = validDirectory;
    }

    private async void ManagePanacea_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_panacea is null || sender is not Button { Tag: string target } button)
            return;
        SetPanaceaButtonsEnabled(false);
        var succeeded = false;
        string? errorMessage = null;
        var isKh3D = target == "28";
        var releaseDirectory = GetPcReleaseDirectory(isKh3D);
        var wasInstalled = _panacea.GetStatus(isKh3D, releaseDirectory).IsInstalled;
        try
        {
            if (wasInstalled)
                await _panacea.RemoveAsync(isKh3D, releaseDirectory);
            else
                await _panacea.InstallAsync(isKh3D, releaseDirectory);
            succeeded = true;
        }
        catch (Exception exception)
        {
            errorMessage = exception.Message;
        }
        finally
        {
            SetPanaceaButtonsEnabled(true);
            if (succeeded)
                RefreshPanaceaStatus();
            else if (errorMessage is not null)
                SetPanaceaStatus(isKh3D, errorMessage, wasInstalled, false);
            button.Focus();
        }
    }

    private void RefreshPanaceaStatus()
    {
        if (_panacea is null)
            return;
        ApplyPanaceaStatus(false, _panacea.GetStatus(false, GetPcReleaseDirectory(false)));
        ApplyPanaceaStatus(true, _panacea.GetStatus(true, GetPcReleaseDirectory(true)));
    }

    private string? GetPcReleaseDirectory(bool isKh3D) =>
        isKh3D ? PcReleaseKh3DTextBox?.Text : PcReleaseTextBox?.Text;

    private void ApplyPanaceaStatus(bool isKh3D, PanaceaStatus status)
    {
        SetPanaceaStatus(isKh3D, status.Message, status.IsInstalled, status.CanInstall);
        var actionButton = isKh3D ? Panacea28ActionButton : Panacea1525ActionButton;
        actionButton.Content = status.IsInstalled ? "Remove" : "Install";
    }

    private void SetPanaceaStatus(bool isKh3D, string message, bool installed, bool canInstall)
    {
        var statusText = isKh3D ? Panacea28StatusText : Panacea1525StatusText;
        var actionButton = isKh3D ? Panacea28ActionButton : Panacea1525ActionButton;
        statusText.Text = message;
        statusText.Foreground = global::Avalonia.Media.Brush.Parse(installed ? "#62D6A7" : "#91A2BA");
        actionButton.IsEnabled = installed || canInstall;
    }

    private void SetPanaceaButtonsEnabled(bool enabled)
    {
        Panacea1525ActionButton.IsEnabled = enabled;
        Panacea28ActionButton.IsEnabled = enabled;
    }

    private async void ManageLuaBackend_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_luaBackend is null || sender is not Button { Tag: string target } button ||
            DataContext is not SetupWindowViewModel viewModel)
        {
            return;
        }

        var isKh3D = target == "28";
        var gameDirectory = GetPcReleaseDirectory(isKh3D);
        var games = GetLuaBackendGames(isKh3D);
        SetLuaBackendBusy(true);
        var statusText = isKh3D ? LuaBackend28StatusText : LuaBackend1525StatusText;
        try
        {
            var installed = _luaBackend.IsInstalled(gameDirectory);
            if (installed)
            {
                _luaBackend.Configure(gameDirectory!, games, viewModel.PcVersionIndex == 1);
                statusText.Text = "Lua Backend was configured successfully.";
            }
            else
            {
                var progress = new Progress<ModOperationProgress>(value =>
                {
                    statusText.Text = value.Percentage is { } percentage
                        ? $"{value.Message} ({percentage:P0})"
                        : value.Message;
                });
                await _luaBackend.InstallAsync(
                    gameDirectory!,
                    games,
                    viewModel.PcVersionIndex == 1,
                    progress);
            }
            statusText.Foreground = Brush.Parse("#62D6A7");
        }
        catch (Exception exception)
        {
            statusText.Text = exception.Message;
            statusText.Foreground = Brush.Parse("#FF8B8B");
        }
        finally
        {
            SetLuaBackendBusy(false);
            RefreshLuaBackendStatus(preserveSuccessfulMessage: true);
            button.Focus();
        }
    }

    private void RemoveLuaBackend_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_luaBackend is null || sender is not Button { Tag: string target })
            return;

        var isKh3D = target == "28";
        var statusText = isKh3D ? LuaBackend28StatusText : LuaBackend1525StatusText;
        try
        {
            _luaBackend.Remove(GetPcReleaseDirectory(isKh3D)!);
            statusText.Text = "Lua Backend was removed.";
            statusText.Foreground = Brush.Parse("#62D6A7");
        }
        catch (Exception exception)
        {
            statusText.Text = exception.Message;
            statusText.Foreground = Brush.Parse("#FF8B8B");
        }
        RefreshLuaBackendStatus(preserveSuccessfulMessage: true);
    }

    private IReadOnlyList<GameInfo> GetLuaBackendGames(bool isKh3D)
    {
        var games = new List<GameInfo>();
        if (isKh3D)
        {
            if (LuaKh3DCheckBox.IsChecked == true)
                games.Add(GameInfo.FromId("kh3d"));
            return games;
        }

        if (LuaKh1CheckBox.IsChecked == true)
            games.Add(GameInfo.FromId("kh1"));
        if (LuaKh2CheckBox.IsChecked == true)
            games.Add(GameInfo.FromId("kh2"));
        if (LuaBbsCheckBox.IsChecked == true)
            games.Add(GameInfo.FromId("bbs"));
        if (LuaRecomCheckBox.IsChecked == true)
            games.Add(GameInfo.FromId("Recom"));
        return games;
    }

    private void RefreshLuaBackendStatus(bool preserveSuccessfulMessage = false)
    {
        if (_luaBackend is null || LuaBackend1525ActionButton is null)
            return;
        ApplyLuaBackendStatus(false, preserveSuccessfulMessage);
        ApplyLuaBackendStatus(true, preserveSuccessfulMessage);
    }

    private void ApplyLuaBackendStatus(bool isKh3D, bool preserveSuccessfulMessage)
    {
        var directory = GetPcReleaseDirectory(isKh3D);
        var validDirectory = !string.IsNullOrWhiteSpace(directory) && Directory.Exists(directory);
        var installed = validDirectory && _luaBackend!.IsInstalled(directory);
        var statusText = isKh3D ? LuaBackend28StatusText : LuaBackend1525StatusText;
        var actionButton = isKh3D ? LuaBackend28ActionButton : LuaBackend1525ActionButton;
        var removeButton = isKh3D ? LuaBackend28RemoveButton : LuaBackend1525RemoveButton;
        if (!preserveSuccessfulMessage)
        {
            statusText.Text = validDirectory
                ? installed ? "Lua Backend is installed." : "Lua Backend is not installed."
                : "Configure the game folder first.";
            statusText.Foreground = Brush.Parse(installed ? "#62D6A7" : "#91A2BA");
        }
        actionButton.Content = installed ? "Configure" : "Install and configure";
        actionButton.IsEnabled = validDirectory;
        removeButton.IsVisible = installed;
        removeButton.IsEnabled = installed;
    }

    private void SetLuaBackendBusy(bool busy)
    {
        LuaBackend1525ActionButton.IsEnabled = !busy;
        LuaBackend28ActionButton.IsEnabled = !busy;
        LuaBackend1525RemoveButton.IsEnabled = !busy;
        LuaBackend28RemoveButton.IsEnabled = !busy;
        SaveButton.IsEnabled = !busy;
        CancelButton.IsEnabled = !busy;
    }

    private void RefreshExtractionAvailability()
    {
        if (_configuration is null || GameEditionComboBox is null || ExtractKh1CheckBox is null)
            return;

        var pcMode = GameEditionComboBox.SelectedIndex != 1;
        var hasRemix = Directory.Exists(PcReleaseTextBox?.Text);
        var hasKh3D = Directory.Exists(PcReleaseKh3DTextBox?.Text);
        var hasKh1 = pcMode ? hasRemix : File.Exists(Kh1IsoTextBox?.Text);
        var hasKh2 = pcMode ? hasRemix : File.Exists(Kh2IsoTextBox?.Text);
        var hasRecom = pcMode ? hasRemix : File.Exists(RecomIsoTextBox?.Text);

        ExtractKh1CheckBox.Content = pcMode ? "KH1 - 23 GB" : "KH1 - 4 GB";
        ExtractKh2CheckBox.Content = pcMode ? "KH2 - 43 GB" : "KH2 - 4 GB";
        ExtractRecomCheckBox.Content = pcMode ? "Re:CoM - 14 GB" : "Re:CoM - 5 GB";
        ExtractBbsCheckBox.Content = "BBS - 19 GB";
        ExtractKh3DCheckBox.Content = "KH3D - 51 GB";

        ExtractKh1CheckBox.IsVisible = hasKh1;
        ExtractKh2CheckBox.IsVisible = hasKh2;
        ExtractRecomCheckBox.IsVisible = hasRecom;
        ExtractBbsCheckBox.IsVisible = pcMode && hasRemix;
        ExtractKh3DCheckBox.IsVisible = pcMode && hasKh3D;

        var availableCount = new[]
        {
            ExtractKh1CheckBox,
            ExtractKh2CheckBox,
            ExtractBbsCheckBox,
            ExtractRecomCheckBox,
            ExtractKh3DCheckBox
        }.Count(checkBox => checkBox.IsVisible);
        ExtractionAvailabilityText.Text = availableCount > 0
            ? $"{availableCount} game{(availableCount == 1 ? " is" : "s are")} available to extract."
            : pcMode
                ? "Configure a valid PC release folder above to select games."
                : "Configure valid ISO files above to select games.";
        ExtractGameDataButton.IsEnabled = availableCount > 0;
        RefreshExistingDataStatus();
    }

    private void RefreshExistingDataStatus()
    {
        if (_configuration is null || ExistingDataStatusText is null)
            return;

        var configuredPath = GameDataTextBox?.Text;
        var path = string.IsNullOrWhiteSpace(configuredPath)
            ? _configuration.GameDataDirectory
            : Path.GetFullPath(configuredPath);
        var hasData = Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any();
        ExistingDataStatusText.Text = hasData
            ? "You already have extracted game data."
            : "You do not have extracted data from a supported game.";
        ExistingDataStatusText.Foreground = Brush.Parse(hasData ? "#62D6A7" : "#91A2BA");
    }

    private async void ExtractGameData_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_extraction is null || DataContext is not SetupWindowViewModel viewModel)
            return;

        var selectedGames = new List<GameInfo>();
        AddSelectedGame(ExtractKh1CheckBox, "kh1", selectedGames);
        AddSelectedGame(ExtractKh2CheckBox, "kh2", selectedGames);
        AddSelectedGame(ExtractBbsCheckBox, "bbs", selectedGames);
        AddSelectedGame(ExtractRecomCheckBox, "Recom", selectedGames);
        AddSelectedGame(ExtractKh3DCheckBox, "kh3d", selectedGames);
        if (selectedGames.Count == 0)
        {
            ExtractionStatusText.Text = "Select at least one game.";
            ExtractionStatusText.Foreground = Brush.Parse("#F1B86B");
            return;
        }

        SetExtractionBusy(true);
        try
        {
            viewModel.Save();
            ExtractionProgressBar.Value = 0;
            ExtractionStatusText.Foreground = Brush.Parse("#91A2BA");
            var progress = new Progress<ModOperationProgress>(value =>
            {
                ExtractionStatusText.Text = value.Message;
                if (value.Percentage is { } percentage)
                    ExtractionProgressBar.Value = percentage;
            });
            await _extraction.ExtractAsync(selectedGames, progress);
            ExtractionProgressBar.Value = 1;
            ExtractionStatusText.Text = "The game data was extracted successfully.";
            ExtractionStatusText.Foreground = Brush.Parse("#62D6A7");
            RefreshExistingDataStatus();
        }
        catch (Exception exception)
        {
            ExtractionStatusText.Text = $"Extraction failed: {exception.Message}";
            ExtractionStatusText.Foreground = Brush.Parse("#FF8B8B");
        }
        finally
        {
            SetExtractionBusy(false);
        }
    }

    private void SetExtractionBusy(bool busy)
    {
        ExtractionProgressBar.IsVisible = busy || ExtractionProgressBar.Value > 0;
        ExtractGameDataButton.IsEnabled = !busy;
        SaveButton.IsEnabled = !busy;
        CancelButton.IsEnabled = !busy;
        SetupScrollViewer.IsEnabled = !busy;
    }

    private static void AddSelectedGame(CheckBox checkBox, string gameId, ICollection<GameInfo> games)
    {
        if (checkBox.IsVisible && checkBox.IsChecked == true)
            games.Add(GameInfo.FromId(gameId));
    }

    private TextBox? GetPathTextBox(string name) => name switch
    {
        "ModStorageTextBox" => ModStorageTextBox,
        "GameDataTextBox" => GameDataTextBox,
        "PcReleaseTextBox" => PcReleaseTextBox,
        "PcReleaseKh3DTextBox" => PcReleaseKh3DTextBox,
        "Pcsx2TextBox" => Pcsx2TextBox,
        "Kh2IsoTextBox" => Kh2IsoTextBox,
        "Kh1IsoTextBox" => Kh1IsoTextBox,
        "RecomIsoTextBox" => RecomIsoTextBox,
        _ => null
    };

    public void HandleControllerAction(ControllerAction action)
    {
        if (HandleActiveComboBox(action))
            return;

        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action == ControllerAction.Cancel)
            Close(false);
        else if (action is ControllerAction.PreviousGame or ControllerAction.NextGame)
            ChangeFocusedSelection(action == ControllerAction.PreviousGame ? -1 : 1);
        else if (action == ControllerAction.Confirm)
            ActivateFocusedControl();
    }

    private bool HandleActiveComboBox(ControllerAction action)
    {
        if (_activeComboBox is not { IsDropDownOpen: true } comboBox)
        {
            _activeComboBox = null;
            return false;
        }

        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem or ControllerAction.PreviousGame)
            ChangeSelection(comboBox, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem or ControllerAction.NextGame)
            ChangeSelection(comboBox, 1);
        else if (action is ControllerAction.Confirm or ControllerAction.Cancel)
        {
            comboBox.IsDropDownOpen = false;
            comboBox.Focus();
            _activeComboBox = null;
        }
        else
            return false;

        return true;
    }

    private void ChangeFocusedSelection(int offset)
    {
        if (FocusManager?.GetFocusedElement() is not ComboBox comboBox || comboBox.ItemCount == 0)
            return;
        ChangeSelection(comboBox, offset);
    }

    private static void ChangeSelection(ComboBox comboBox, int offset)
    {
        if (comboBox.ItemCount == 0)
            return;

        comboBox.SelectedIndex = Math.Clamp(comboBox.SelectedIndex + offset, 0, comboBox.ItemCount - 1);
    }

    private void ActivateFocusedControl()
    {
        var focused = FocusManager?.GetFocusedElement();
        if (focused == SaveButton)
            Save_OnClick(SaveButton, new RoutedEventArgs());
        else if (focused == CancelButton)
            Close(false);
        else if (focused is CheckBox checkBox)
            checkBox.IsChecked = checkBox.IsChecked != true;
        else if (focused is ComboBox comboBox)
        {
            _activeComboBox = comboBox;
            comboBox.IsDropDownOpen = true;
        }
        else if (focused is Button button)
        {
            if (button == ExtractGameDataButton)
                ExtractGameData_OnClick(button, new RoutedEventArgs());
            else if (button == Panacea1525ActionButton || button == Panacea28ActionButton)
                ManagePanacea_OnClick(button, new RoutedEventArgs());
            else if (button == LuaBackend1525ActionButton || button == LuaBackend28ActionButton)
                ManageLuaBackend_OnClick(button, new RoutedEventArgs());
            else if (button == LuaBackend1525RemoveButton || button == LuaBackend28RemoveButton)
                RemoveLuaBackend_OnClick(button, new RoutedEventArgs());
            else if (button == DetectInstallationsButton)
                DetectInstallations_OnClick(button, new RoutedEventArgs());
            else if (button == SteamAppId1525Button || button == SteamAppId28Button)
                SteamAppId_OnClick(button, new RoutedEventArgs());
            else if (button.Tag is string tag && tag is "Pcsx2TextBox" or "Kh2IsoTextBox" or "Kh1IsoTextBox" or "RecomIsoTextBox")
                BrowseFile_OnClick(button, new RoutedEventArgs());
            else if (button.Tag is string)
                BrowseFolder_OnClick(button, new RoutedEventArgs());
        }
        else
            ControllerWindowNavigator.MoveFocus(this, 1);
    }
}
