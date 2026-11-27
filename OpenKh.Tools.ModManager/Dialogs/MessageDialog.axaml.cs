using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Media;

namespace OpenKh.Tools.ModManager.Dialogs
{
    public enum MessageType
    {
        INFO,
        WARNING,
        ERROR
    };

    public partial class MessageDialog : Window
    {
        public MessageType Type { get; set; }

        public MessageDialog()
        {
            InitializeComponent();
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object? sender, RoutedEventArgs e)
        {

            switch (Type)
            {
                case MessageType.INFO:
                    if (OperatingSystem.IsWindows())
                        SystemSounds.Asterisk.Play();

                    MainIcon.Data = Resources["icon_type_info"] as Geometry;
                    break;

                case MessageType.WARNING:
                    if (OperatingSystem.IsWindows())
                        SystemSounds.Exclamation.Play();

                    MainIcon.Data = Resources["icon_type_warning"] as Geometry;
                    break;

                case MessageType.ERROR:
                    if (OperatingSystem.IsWindows())
                        SystemSounds.Hand.Play();

                    MainIcon.Data = Resources["icon_type_error"] as Geometry;
                    break;
            }
        }

        private void OnAcceptClicked(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close(true);
    }
}
