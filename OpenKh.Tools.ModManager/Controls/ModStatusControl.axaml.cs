using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using OpenKh.Bbs.SystemData;
using OpenKh.Tools.ModManager.Models;
using System;
using System.Globalization;
using System.Windows.Input;

namespace OpenKh.Tools.ModManager.Views
{
    public partial class ModStatusView : ContentPage
    {
        public static readonly StyledProperty<ICommand?> RunRequestedProperty = AvaloniaProperty.Register<ModStatusView, ICommand?>(nameof(RunRequested));
        public ICommand? RunRequested { get => GetValue(RunRequestedProperty); set => SetValue(RunRequestedProperty, value); }

        public static readonly StyledProperty<ICommand?> BuildRunRequestedProperty = AvaloniaProperty.Register<ModStatusView, ICommand?>(nameof(BuildRunRequested));
        public ICommand? BuildRunRequested { get => GetValue(BuildRunRequestedProperty); set => SetValue(BuildRunRequestedProperty, value); }

        public static readonly StyledProperty<ICommand?> SetupRequestedProperty = AvaloniaProperty.Register<ModStatusView, ICommand?>(nameof(SetupRequested));
        public ICommand? SetupRequested { get => GetValue(SetupRequestedProperty); set => SetValue(SetupRequestedProperty, value); }

        public ModStatusView()
        {
            InitializeComponent();
        }

        private void OnRunRequested(object? sender, RoutedEventArgs e)
        {
            if (RunRequested?.CanExecute(null) == true)
                RunRequested.Execute(null);
        }

        private void OnSetupRequested(object? sender, RoutedEventArgs e)
        {
            if (SetupRequested?.CanExecute(null) == true)
                SetupRequested.Execute(null);
        }

        private void OnBuildRunRequested(object? sender, RoutedEventArgs e)
        {
            if (BuildRunRequested?.CanExecute(null) == true)
                BuildRunRequested.Execute(null);
        }
    }
}
