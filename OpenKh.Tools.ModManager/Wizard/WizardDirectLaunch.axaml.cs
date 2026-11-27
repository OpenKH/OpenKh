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

namespace OpenKh.Tools.ModManager.Wizard
{
    public partial class WizardDirectLaunch : ContentPage
    {
        public WizardDirectLaunch()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<PageRequestMessage>(this, (registrar, message) =>
            {
                if (message.Self != this)
                    return;

                var _fetchFrontend = message.CurrentConfig.Frontend;

                if (_fetchFrontend.TargetPlatform != Platform.STEAM)
                    message.Reply(false);

                else
                {
                    InstallPanel.IsVisible = false;
                    NotInstallPanel.IsVisible = true;

                    if (PackageService.EnsureSteamAPI(message.CurrentConfig))
                    {
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
                PackageService.InstallSteamAPI(_fetchConfig);

                InstallPanel.IsVisible = true;
                NotInstallPanel.IsVisible = false;
            }
        }

        private void OnUninstallClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (_fetchConfig != null)
            {
                PackageService.RemoveSteamAPI(_fetchConfig);

                InstallPanel.IsVisible = false;
                NotInstallPanel.IsVisible = true;
            }
        }
    }
}
