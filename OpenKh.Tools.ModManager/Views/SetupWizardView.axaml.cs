using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using OpenKh.Tools.ModManager.Classes;
using OpenKh.Tools.ModManager.Services;
using OpenKh.Tools.ModManager.ViewModels;
using OpenKh.Tools.ModManager.Wizard;
using SharpYaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static OpenKh.Tools.ModManager.Wizard.WizardPanaceaSetup;

namespace OpenKh.Tools.ModManager.Views
{
    public record BlockNextRequest();
    public record UnblockNextRequest();

    public record BlockAllRequest();
    public record UnblockAllRequest();

    public class PageRequestMessage : RequestMessage<bool>
    {
        public object Self { get; }
        public Config CurrentConfig { get; }

        public PageRequestMessage(object self, Config currentConfig)
        {
            Self = self;
            CurrentConfig = currentConfig;
        }
    }

    public partial class SetupWizardView : Window
    {
        private WizardViewModel _rootModel { get; set; }

        public SetupWizardView()
        {
            _rootModel = new WizardViewModel();

            _rootModel.CurrentWizardPage = new WizardGameSetup();
            _rootModel.PastWizardPages = new ObservableCollection<object?>();

            _rootModel.FutureWizardPages = new ObservableCollection<object?>()
            {
                new WizardPanaceaSetup(),
                new WizardScriptSetup(),
                new WizardDirectLaunch(),
                new WizardExtractSetup(),
            };

            var _fetchApplication = Application.Current;

            // Uhh how the fuck?
            if (_fetchApplication == null)
                throw new NullReferenceException("Application is null, this should not be possible.");

            var _fetchLifetime = _fetchApplication.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var _fetchMainView = _fetchLifetime.MainWindow;

            // Uhh how the fuck, electric bogaloo?
            if (_fetchMainView == null)
                throw new NullReferenceException("MainView is null, this ALSO should not be possible.");

            var _fetchContext = _fetchMainView.DataContext as MainViewModel;
            DataContext = _fetchContext;

            WeakReferenceMessenger.Default.Register<BlockNextRequest>(this, (registrar, message) =>
            {
                NextButton.IsEnabled = false;
                FinishButton.IsEnabled = false;
            });

            WeakReferenceMessenger.Default.Register<UnblockNextRequest>(this, (registrar, message) =>
            {
                NextButton.IsEnabled = _rootModel.FutureWizardPages.Count > 0;
                FinishButton.IsEnabled = true;
            });

            WeakReferenceMessenger.Default.Register<BlockAllRequest>(this, (registrar, message) =>
            {
                NextButton.IsEnabled = false;
                BackButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
                FinishButton.IsEnabled = false;
            });

            WeakReferenceMessenger.Default.Register<UnblockAllRequest>(this, (registrar, message) =>
            {
                NextButton.IsEnabled = _rootModel.FutureWizardPages.Count > 0;
                BackButton.IsEnabled = _rootModel.PastWizardPages.Count > 0;
                CancelButton.IsEnabled = true;
                FinishButton.IsEnabled = true;
            });

            InitializeComponent();
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            if (!CancelButton.IsEnabled)
                e.Cancel = true;

            base.OnClosing(e);
        }

        private void OnNextPage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (_fetchConfig != null)
            {
                var _fetchBackList = _rootModel.PastWizardPages;
                var _fetchNextList = _rootModel.FutureWizardPages;

                var _fetchNextPage = _fetchNextList.FirstOrDefault();

                while (_fetchNextPage != null)
                {
                    _fetchNextList.Remove(_fetchNextPage);
                    _fetchBackList.Add(_rootModel.CurrentWizardPage);

                    _rootModel.CurrentWizardPage = _fetchNextPage;

                    var _fetchMessage = new PageRequestMessage(_rootModel.CurrentWizardPage, _fetchConfig);
                    WeakReferenceMessenger.Default.Send(_fetchMessage);

                    if (_fetchMessage.HasReceivedResponse)
                    {
                        if (!_fetchMessage.Response)
                            _fetchNextPage = _fetchNextList.FirstOrDefault();

                        else
                            break;
                    }
                }

                if (_fetchNextPage == null)
                    OnSubmitClick(sender, e);

                if (_fetchNextList.Count == 0)
                {
                    NextButton.IsEnabled = false;

                    FinishButton.IsVisible = true;
                    CancelButton.IsVisible = false;
                }

                BackButton.IsEnabled = true;
            }
        }

        private void OnBackPage(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (_fetchConfig != null)
            {
                var _fetchBackList = _rootModel.PastWizardPages;
                var _fetchNextList = _rootModel.FutureWizardPages;

                var _fetchBackPage = _fetchBackList.LastOrDefault();

                while (_fetchBackPage != null)
                {
                    _fetchBackList.Remove(_fetchBackPage);
                    _fetchNextList.Insert(0, _rootModel.CurrentWizardPage);

                    _rootModel.CurrentWizardPage = _fetchBackPage;

                    var _fetchMessage = new PageRequestMessage(_rootModel.CurrentWizardPage, _fetchConfig);
                    WeakReferenceMessenger.Default.Send(_fetchMessage);

                    if (_fetchMessage.HasReceivedResponse)
                    {
                        if (!_fetchMessage.Response)
                            _fetchBackPage = _fetchBackList.LastOrDefault();

                        else
                            break;
                    }
                }

                if (_fetchBackPage == null)
                    OnSubmitClick(sender, e);

                if (_fetchBackList.Count == 0)
                    BackButton.IsEnabled = false;

                FinishButton.IsVisible = false;
                CancelButton.IsVisible = true;

                NextButton.IsEnabled = true;
            }
        }

        private void OnSubmitClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var _fetchContext = DataContext as MainViewModel;
            var _fetchConfig = _fetchContext != null ? _fetchContext.CurrentConfig : null;

            if (_fetchConfig != null)
            {
                _fetchContext.ConfigurationValid = _fetchConfig.IsValid();
                _fetchConfig.Commit();

                Close();
            }
        }
    }
}
