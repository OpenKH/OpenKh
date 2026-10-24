using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Messaging;
using OpenKh.Tools.ModManager.Classes;
using OpenKh.Tools.ModManager.Services;
using OpenKh.Tools.ModManager.ViewModels;
using OpenKh.Tools.ModManager.Views;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OpenKh.Tools.ModManager.Wizard
{
    public partial class WizardPanaceaSetup : ContentPage
    {
        public WizardPanaceaSetup()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<PageRequestMessage>(this, (registrar, message) =>
            {
                if (message.Self != this)
                    return;
                
                if (message.CurrentConfig.Frontend.TargetPlatform == Platform.PCSX2)
                    message.Reply(false);

                else
                {
                    InstallPanel.IsVisible = false;
                    NotInstallPanel.IsVisible = true;

                    var _fetchConfig = message.CurrentConfig;
                    _fetchConfig.Frontend.ModBuildType = BuildType.PATCH;

                    if (PackageService.EnsurePanacea(_fetchConfig))
                    {
                        _fetchConfig.Frontend.ModBuildType = BuildType.PANACEA;

                        InstallPanel.IsVisible = true;
                        NotInstallPanel.IsVisible = false;
                    }

                    message.Reply(true);
                }
            });
        }

        private void OnInstallClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (_fetchConfig != null)
            {
                PackageService.InstallPanacea(_fetchConfig);

                InstallPanel.IsVisible = true;
                NotInstallPanel.IsVisible = false;

                _fetchConfig.Frontend.ModBuildType = BuildType.PANACEA;
            }
        }

        private void OnUninstallClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (_fetchConfig != null)
            {
                PackageService.RemovePanacea(_fetchConfig);

                InstallPanel.IsVisible = false;
                NotInstallPanel.IsVisible = true;

                _fetchConfig.Frontend.ModBuildType = BuildType.PATCH;
            }
        }
    }
}
