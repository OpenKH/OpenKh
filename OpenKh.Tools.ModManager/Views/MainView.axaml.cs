using Avalonia.Controls;
using Avalonia.Interactivity;
using OpenKh.Tools.ModManager.Models;
using OpenKh.Tools.ModManager.ViewModels;
using System;
using Avalonia.Media;
using System.Collections.Generic;

namespace OpenKh.Tools.ModManager.Views;

public partial class MainView : Window
{
    private void OnTargetGameChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Fetch the context.
        var _fetchContext = DataContext as MainViewModel;

        if (_fetchContext == null)
            return;

        // Re-Initialize the viewmodel.
        _fetchContext.InitViewModel();
    }

    public MainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var _fetchContext = DataContext as MainViewModel;

        if (_fetchContext == null)
            return;

        if (_fetchContext.InstalledMods != null && _fetchContext.InstalledMods.Count > 0)
            _fetchContext.CurrentMod = _fetchContext.InstalledMods[0];

        if (!_fetchContext.DoesConfigExist)
        {
            IsVisible = false;

            var _temporaryOwner = new Window
            {
                ShowInTaskbar = false,
                Background = Brushes.Transparent,
                Width = 0,
                Height = 0,
                WindowDecorations = WindowDecorations.None,
                TransparencyLevelHint = new List<WindowTransparencyLevel>() { WindowTransparencyLevel.Transparent }
            };

            _temporaryOwner.Show();
            _fetchContext.DoesConfigExist = true;

            var _fetchWizard = new SetupWizardView();
            await _fetchWizard.ShowDialog(_temporaryOwner);

            _temporaryOwner.Close();

            IsVisible = true;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Fetch the main ViewModel and serialize the config it has.
        var _fetchViewModel = DataContext as MainViewModel;
        var _fetchConfig = _fetchViewModel != null ? _fetchViewModel.CurrentConfig : null;
        
        if (_fetchConfig != null)
            _fetchConfig.Commit();

        // We gotta still CLOSE the thing, no?
        base.OnClosing(e);
    }
}
