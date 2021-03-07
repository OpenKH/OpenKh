using OpenKh.Tools.ModsManager.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class ModViewModel : BaseNotifyPropertyChanged
    {
        private readonly ModModel _model;
        private readonly IChangeModEnableState _changeModEnableState;

        public ModViewModel(ModModel model, IChangeModEnableState changeModEnableState)
        {
            _model = model;
            _changeModEnableState = changeModEnableState;

            var nameIndex = Source.IndexOf('/');
            if (nameIndex > 0)
            {
                Author = Source[0..nameIndex];
                Name = Source[(nameIndex + 1)..];
            }
            else
            {
                Author = _model.Metadata.OriginalAuthor;
                Name = Source;
            }

            LoadImage();
        }

        public bool Enabled
        {
            get => _model.IsEnabled;
            set
            {
                _model.IsEnabled = value;
                _changeModEnableState.ModEnableStateChanged();
                OnPropertyChanged();
            }
        }

        public ImageSource Image { get; private set; }

        public bool IsHosted => _model.Name.Contains('/');
        public string Path => _model.Path;
        public Visibility SourceVisibility => IsHosted ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LocalVisibility => !IsHosted ? Visibility.Visible : Visibility.Collapsed;

        public string Title => _model.Metadata.Title;
        public string Name { get; }
        public string Author { get; }
        public string Source => _model.Name;

        public string GithubUrl
        {
            get
            {
                if (Source == null)
                    return null;
                return $"https://github.com/{Source}";
            }
        }

        public string Homepage
        {
            get
            {
                if (Source == null)
                    return null;

                var author = System.IO.Path.GetDirectoryName(Source);
                var project = System.IO.Path.GetFileName(Source);
                return $"https://{author}.github.io/{project}";
            }
        }

        private Task LoadImage() => Task.Run(() =>
        {
            if (string.IsNullOrEmpty(_model.ImageSource))
                return; // Should set a default image

            try
            {
                var uri = new Uri(_model.ImageSource);
                if (uri.Scheme == "file" && !File.Exists(uri.AbsolutePath))
                    return; // Should set a default image

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = uri;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Image = bitmapImage;
                    OnPropertyChanged(nameof(Image));
                });
            }
            catch
            {
                // Silently fail if the image can not be loaded
            }
        });
    }
}
