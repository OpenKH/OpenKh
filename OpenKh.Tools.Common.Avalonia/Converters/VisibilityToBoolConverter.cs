using Avalonia.Data.Converters;
using System;
using System.Globalization;
using System.Windows;

namespace OpenKh.Tools.Common.Avalonia.Converters
{
    /// <summary>
    /// Bridges the shared ViewModels' WPF-style Visibility properties to
    /// Avalonia's bool IsVisible, e.g.
    /// IsVisible="{Binding FooVisibility, Converter={StaticResource VisibilityToBool}}".
    /// </summary>
    public class VisibilityToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is Visibility visibility && visibility == Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
            value is true ? Visibility.Visible : Visibility.Collapsed;
    }
}
