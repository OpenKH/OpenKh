using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using OpenKh.Tools.ModManager.Models;
using OpenKh.Tools.ModManager.Services;
using OpenKh.Tools.ModManager.ViewModels;
using SharpYaml;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.ModManager.Views
{
    public partial class CatalogView : Window
    {
        private MainViewModel _referenceModel { get; set; }

        public CatalogView()
        {
            InitializeComponent();
            DataContext = new CatalogViewModel();
            Loaded += OnViewLoaded;
        }

        private async void OnViewLoaded(object? sender, RoutedEventArgs e)
        {
            if (!Design.IsDesignMode)
            {
                var _fetchContext = DataContext as CatalogViewModel;

                if (_fetchContext == null)
                    return;

                var _fetchApplication = Application.Current;

                // Uhh how the fuck?
                if (_fetchApplication != null)
                {
                    var _fetchLifetime = _fetchApplication.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
                    var _fetchMainView = _fetchLifetime != null ? _fetchLifetime.MainWindow : null;

                    // Uhh how the fuck, electric bogaloo?
                    if (_fetchMainView != null)
                    {
                        var _fetchReference = _fetchMainView.DataContext;

                        if (_fetchReference != null)
                        {
                            _referenceModel = (MainViewModel)_fetchReference;
                            _fetchContext.CurrentConfig = _referenceModel.CurrentConfig;

                            await _fetchContext.InitializeView();

                            MainSearch.IsEnabled = true;
                            MainProgress.IsVisible = false;
                        }
                    }
                }
            }
        }

        private void SearchTextChanged(object? sender, TextChangedEventArgs e)
        {
        }
    }
}
