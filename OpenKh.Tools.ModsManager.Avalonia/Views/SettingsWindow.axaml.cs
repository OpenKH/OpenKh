using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenKh.Tools.ModsManager.Avalonia.Services;
using OpenKh.Tools.ModsManager.Core;

namespace OpenKh.Tools.ModsManager.Avalonia.Views;

public sealed partial class SettingsWindow : Window
{
    private readonly ModManagerConfigurationService? _configuration;
    private readonly PanaceaService? _panacea;
    private readonly OpenKhUpdateCheckerService? _updateChecker;
    private string? _updateDownloadUrl;

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(
        ModManagerConfigurationService configuration,
        PanaceaService panacea,
        OpenKhUpdateCheckerService updateChecker) : this()
    {
        _configuration = configuration;
        _panacea = panacea;
        _updateChecker = updateChecker;
        var settings = configuration.Current;
        AutoUpdateModsCheckBox.IsChecked = settings.AutoUpdateMods;
        ShowConsoleCheckBox.IsChecked = settings.ShowConsole;
        DebugLogCheckBox.IsChecked = settings.DebugLog;
        SoundDebugCheckBox.IsChecked = settings.SoundDebug;
        EnableCacheCheckBox.IsChecked = settings.EnableCache;
        QuickMenuCheckBox.IsChecked = settings.QuickMenu;
    }

    private void Save_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_configuration is null || _panacea is null)
            return;
        var settings = _configuration.Current;
        settings.AutoUpdateMods = AutoUpdateModsCheckBox.IsChecked == true;
        settings.ShowConsole = ShowConsoleCheckBox.IsChecked == true;
        settings.DebugLog = DebugLogCheckBox.IsChecked == true;
        settings.SoundDebug = SoundDebugCheckBox.IsChecked == true;
        settings.EnableCache = EnableCacheCheckBox.IsChecked == true;
        settings.QuickMenu = QuickMenuCheckBox.IsChecked == true;
        _panacea.SaveSettings();
        Close(true);
    }

    private async void CheckOpenKhUpdates_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_updateChecker is null)
            return;

        CheckOpenKhUpdatesButton.IsEnabled = false;
        InstallOpenKhUpdateButton.IsVisible = false;
        UpdateStatusText.Text = "Checking the complete OpenKH package for updates...";
        try
        {
            var result = await _updateChecker.CheckAsync();
            _updateDownloadUrl = result.DownloadUrl;
            if (result.HasUpdate)
            {
                UpdateStatusText.Text = $"OpenKH update available. Current: {result.CurrentVersion}. Latest: {result.LatestVersion}.";
                InstallOpenKhUpdateButton.IsVisible = true;
            }
            else if (string.IsNullOrWhiteSpace(result.LatestVersion))
            {
                UpdateStatusText.Text = "No compatible OpenKH release package was found.";
            }
            else
            {
                UpdateStatusText.Text = $"The complete OpenKH package is up to date ({result.CurrentVersion}).";
            }
        }
        catch (Exception exception)
        {
            UpdateStatusText.Text = $"OpenKH update check failed: {exception.Message}";
        }
        finally
        {
            CheckOpenKhUpdatesButton.IsEnabled = true;
        }
    }

    private void InstallOpenKhUpdate_OnClick(object? sender, RoutedEventArgs eventArgs)
    {
        if (_configuration is null)
            return;

        try
        {
            var launcherPath = Path.Combine(_configuration.InstallationDirectory, "OpenKh.Launcher.exe");
            if (File.Exists(launcherPath))
            {
                Process.Start(new ProcessStartInfo(launcherPath)
                {
                    UseShellExecute = true,
                    WorkingDirectory = _configuration.InstallationDirectory
                });
                UpdateStatusText.Text = "The OpenKH Launcher was opened. Use its update button to install the complete package.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(_updateDownloadUrl))
            {
                Process.Start(new ProcessStartInfo(_updateDownloadUrl) { UseShellExecute = true });
                UpdateStatusText.Text = "The complete OpenKH package download was opened in your browser.";
            }
        }
        catch (Exception exception)
        {
            UpdateStatusText.Text = $"Could not open the OpenKH updater: {exception.Message}";
        }
    }

    private void Cancel_OnClick(object? sender, RoutedEventArgs eventArgs) => Close(false);

    public void HandleControllerAction(ControllerAction action)
    {
        if (action is ControllerAction.PreviousControl or ControllerAction.PreviousItem)
            ControllerWindowNavigator.MoveFocus(this, -1);
        else if (action is ControllerAction.NextControl or ControllerAction.NextItem)
            ControllerWindowNavigator.MoveFocus(this, 1);
        else if (action == ControllerAction.Cancel)
            Close(false);
        else if (action == ControllerAction.Confirm)
            ActivateFocusedControl();
    }

    private void ActivateFocusedControl()
    {
        var focused = FocusManager?.GetFocusedElement();
        if (focused == SaveButton)
            Save_OnClick(SaveButton, new RoutedEventArgs());
        else if (focused == CancelButton)
            Close(false);
        else if (focused == CheckOpenKhUpdatesButton)
            CheckOpenKhUpdates_OnClick(CheckOpenKhUpdatesButton, new RoutedEventArgs());
        else if (focused == InstallOpenKhUpdateButton)
            InstallOpenKhUpdate_OnClick(InstallOpenKhUpdateButton, new RoutedEventArgs());
        else if (focused is CheckBox checkBox)
            checkBox.IsChecked = checkBox.IsChecked != true;
        else
            ControllerWindowNavigator.MoveFocus(this, 1);
    }
}
