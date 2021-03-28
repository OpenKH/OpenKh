using OpenKh.Common;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace OpenKh.Tools.Common
{
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : Window, INotifyPropertyChanged
    {
        private string message;
        private string website1;
        private string website2;
        private string website3;

        public MessageDialog(string msginp, string lnk1 = null, string lnk2 = null, string lnk3 = null)
        {
            InitializeComponent();

            message = msginp;

            website1 = lnk1;
            website2 = lnk2;
            website3 = lnk3;

            if (lnk1 == null)
                webBox1.Visibility = Visibility.Collapsed;
            if (lnk2 == null)
                webBox2.Visibility = Visibility.Collapsed;
            if (lnk3 == null)
                webBox3.Visibility = Visibility.Collapsed;

            if (lnk1 == null && lnk2 == null && lnk3 == null)
                webSep.Visibility = Visibility.Collapsed;

            DataContext = this;
            MouseDown += (o, e) =>
            {
                if (e.ChangedButton == MouseButton.Left)
                    DragMove();
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string Message
        {
            get => message;
            set => message = OnPropertyChanged(value);
        }

        public string Website1
        {
            get => website1;
            set => website1 = OnPropertyChanged(value);
        }

        public string Website2
        {
            get => website2;
            set => website2 = OnPropertyChanged(value);
        }

        public string Website3
        {
            get => website3;
            set => website3 = OnPropertyChanged(value);
        }

        private T OnPropertyChanged<T>(T obj, [CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return obj;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BeginAnimation(OpacityProperty, new DoubleAnimation()
            {
                From = 0.0,
                To = 1.0,
                EasingFunction = new CubicEase(),
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            });
        }

        private void NavigateURL(object sender, RequestNavigateEventArgs e) =>
            new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = true,
                    FileName = e.Uri.AbsoluteUri
                }
            }.Using(x => x.Start());
    }
}
