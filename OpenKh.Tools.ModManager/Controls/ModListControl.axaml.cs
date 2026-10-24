using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using OpenKh.Tools.ModManager.Models;
using OpenKh.Tools.ModManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace OpenKh.Tools.ModManager.Views
{
    public partial class ModListView : ContentPage
    {
        public static readonly StyledProperty<ICommand?> ModToggledProperty = AvaloniaProperty.Register<ModStatusView, ICommand?>(nameof(ModToggled));
        public ICommand? ModToggled { get => GetValue(ModToggledProperty); set => SetValue(ModToggledProperty, value); }

        public ModListView()
        {
            InitializeComponent();
        }

        private void OnModCheckChanged(object? sender, RoutedEventArgs e)
        {
            var _fetchSender = sender as CheckBox;
            var _fetchParent = _fetchSender.Parent.DataContext as ModModel;

            if (_fetchParent == null)
                return;

            var _targetTuple = new Tuple<ModModel, bool>(_fetchParent, _fetchSender.IsChecked ?? false);

            if (ModToggled?.CanExecute(_targetTuple) == true)
                ModToggled.Execute(_targetTuple);
        }
    }
}
