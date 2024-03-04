using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenKh.Tools.Kh2FontImageEditor.UserControls
{
    /// <summary>
    /// Interaction logic for ImagerTablet.xaml
    /// </summary>
    public partial class ImagerTablet : UserControl
    {
        public ImagerTablet()
        {
            InitializeComponent();
        }

        #region Caption
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register(
            "Caption",
            typeof(string),
            typeof(ImagerTablet)
        );

        public string? Caption
        {
            get => (string?)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }
        #endregion

        #region Image
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(
            "Image",
            typeof(ImageSource),
            typeof(ImagerTablet)
        );

        public ImageSource? Image
        {
            get => (ImageSource?)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
        #endregion

        #region ImportCommand
        public static readonly DependencyProperty ImportCommandProperty = DependencyProperty.Register(
            "ImportCommand",
            typeof(ICommand),
            typeof(ImagerTablet)
        );

        public ICommand? ImportCommand
        {
            get => (ICommand?)GetValue(ImportCommandProperty);
            set => SetValue(ImportCommandProperty, value);
        }
        #endregion

        #region ExportCommand
        public static readonly DependencyProperty ExportCommandProperty = DependencyProperty.Register(
            "ExportCommand",
            typeof(ICommand),
            typeof(ImagerTablet)
        );

        public ICommand? ExportCommand
        {
            get => (ICommand?)GetValue(ExportCommandProperty);
            set => SetValue(ExportCommandProperty, value);
        }
        #endregion

        #region EditSpacingCommand
        public static readonly DependencyProperty EditSpacingCommandProperty = DependencyProperty.Register(
            "EditSpacingCommand",
            typeof(ICommand),
            typeof(ImagerTablet)
        );

        public ICommand? EditSpacingCommand
        {
            get => (ICommand?)GetValue(EditSpacingCommandProperty);
            set => SetValue(EditSpacingCommandProperty, value);
        }
        #endregion

        #region ImportCommandParameter
        public static readonly DependencyProperty ImportCommandParameterProperty = DependencyProperty.Register(
            "ImportCommandParameter",
            typeof(object),
            typeof(ImagerTablet)
        );

        public object? ImportCommandParameter
        {
            get => (object?)GetValue(ImportCommandParameterProperty);
            set => SetValue(ImportCommandParameterProperty, value);
        }
        #endregion

        #region ExportCommandParameter
        public static readonly DependencyProperty ExportCommandParameterProperty = DependencyProperty.Register(
            "ExportCommandParameter",
            typeof(object),
            typeof(ImagerTablet)
        );

        public object? ExportCommandParameter
        {
            get => (object?)GetValue(ExportCommandParameterProperty);
            set => SetValue(ExportCommandParameterProperty, value);
        }
        #endregion   

        #region EditSpacingCommandParameter
        public static readonly DependencyProperty EditSpacingCommandParameterProperty = DependencyProperty.Register(
            "EditSpacingCommandParameter",
            typeof(object),
            typeof(ImagerTablet)
        );

        public object? EditSpacingCommandParameter
        {
            get => (object?)GetValue(EditSpacingCommandParameterProperty);
            set => SetValue(EditSpacingCommandParameterProperty, value);
        }
        #endregion   
    }
}
