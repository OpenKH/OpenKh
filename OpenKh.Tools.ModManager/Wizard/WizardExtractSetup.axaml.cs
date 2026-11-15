using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using OpenKh.Tools.ModManager.Classes;
using OpenKh.Tools.ModManager.Services;
using OpenKh.Tools.ModManager.ViewModels;
using OpenKh.Tools.ModManager.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.Wizard
{
    public partial class WizardExtractSetup : ContentPage
    {
        public WizardExtractSetup()
        {
            InitializeComponent();

            WeakReferenceMessenger.Default.Register<PageRequestMessage>(this, (registrar, message) =>
            {
                if (message.Self != this)
                    return;

                message.Reply(true);
            });
        }

        private async Task<bool> ExtractTask(List<bool> extractList, Config currentConfig)
        {
            var _fetchResult = await ModService.Extract
                                (
                                    extractList,
                                    currentConfig,
                                    currentConfig.Frontend.TargetPlatform != Platform.PCSX2,
                                    (int processed, int total) =>
                                    {
                                        Dispatcher.UIThread.Post(() =>
                                        {
                                            ProgressExtract.Maximum = total;
                                            ProgressExtract.Value = processed;
                                        });

                                        if (ModService.CancelToken.IsCancellationRequested)
                                            return false;

                                        return true;
                                    }
                                );

            ExtractStartButton.IsVisible = true;
            ExtractStopButton.IsVisible = false;

            WeakReferenceMessenger.Default.Send(new UnblockAllRequest());

            if (_fetchResult != 0x00)
            {
                currentConfig.Frontend.DataPath = null;

                ProgressExtract.Value = 0;
                ProgressExtract.Maximum = 100;

                return false;
            }

            else
                return true;
        }

        private async void OnExtractPathClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchTopLevel = TopLevel.GetTopLevel(this);
            var _storageProvider = _fetchTopLevel != null ? _fetchTopLevel.StorageProvider : null;

            if (_storageProvider != null)
            {
                var _fetchFolder = await _storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select a Folder for Game Data...",
                    AllowMultiple = false
                });

                if (_fetchFolder.Count >= 1)
                    PathExtractData.Text = _fetchFolder[0].Path.LocalPath;
            }
        }

        private async void OnExtractStart(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (_fetchConfig != null)
            {
                ExtractStartButton.IsVisible = false;
                ExtractStopButton.IsVisible = true;

                var _fetchExtractList = new List<bool>()
                {
                    (ExtractPC1.IsChecked ?? false) || (ExtractPS1.IsChecked ?? false),
                    (ExtractPC2.IsChecked ?? false) || (ExtractPS2.IsChecked ?? false),
                    (ExtractPC3.IsChecked ?? false) || (ExtractPS3.IsChecked ?? false),
                    (ExtractPC4.IsChecked ?? false) || (ExtractPS4.IsChecked ?? false),
                    (ExtractPC5.IsChecked ?? false) || (ExtractPS5.IsChecked ?? false)
                };

                WeakReferenceMessenger.Default.Send(new BlockAllRequest());
                ExtractTask(_fetchExtractList, _fetchConfig);
            }
        }

        private void OnExtractStop(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ModService.CancelTokenSource.Cancel();
    }
}
