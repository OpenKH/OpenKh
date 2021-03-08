using OpenKh.Tools.ModsManager.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class ModViewModel : BaseNotifyPropertyChanged
    {
        private static readonly string FallbackImage;
        private readonly ModModel _model;
        private readonly IChangeModEnableState _changeModEnableState;
        private int _updateCount;

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

            LoadImage(_model.IconImageSource, FallbackImage, image =>
            {
                IconImage = image;
                OnPropertyChanged(nameof(IconImage));
            });
            LoadImage(_model.PreviewImageSource, null, image =>
            {
                PreviewImage = image;
                OnPropertyChanged(nameof(PreviewImage));
                OnPropertyChanged(nameof(PreviewImageVisibility));
            });
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

        public ImageSource IconImage { get; private set; }
        public ImageSource PreviewImage { get; private set; }
        public Visibility PreviewImageVisibility => PreviewImage != null ? Visibility.Visible : Visibility.Collapsed;

        public bool IsHosted => _model.Name.Contains('/');
        public string Path => _model.Path;
        public Visibility SourceVisibility => IsHosted ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LocalVisibility => !IsHosted ? Visibility.Visible : Visibility.Collapsed;

        public string Title => _model.Metadata.Title;
        public string Name { get; }
        public string Author { get; }
        public string Source => _model.Name;
        public string AuthorUrl => $"https://github.com/{Author}";
        public string SourceUrl => $"https://github.com/{Source}";
        public string FilesToPatch => string.Join('\n', GetFilesToPatch());

        public string Description => _model.Metadata.Description;

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

        public int UpdateCount
        {
            get => _updateCount;
            set
            {
                _updateCount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUpdateAvailable));
                OnPropertyChanged(nameof(UpdateVisibility));
            }
        }

        public bool IsUpdateAvailable => UpdateCount > 0;
        public Visibility UpdateVisibility => IsUpdateAvailable ? Visibility.Visible : Visibility.Collapsed;

        private IEnumerable<string> GetFilesToPatch()
        {
            foreach (var asset in _model.Metadata.Assets)
            {
                yield return asset.Name;
                if (asset.Multi != null)
                {
                    foreach (var multiAsset in asset.Multi)
                        yield return multiAsset.Name;
                }
            }
        }

        private static void LoadImage(string source, string fallback, Action<ImageSource> setter)
        {
            if (string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(fallback))
            {
                LoadImage(fallback, null, setter);
                return;
            }

            try
            {
                var uri = new Uri(source);
                if (uri.Scheme == "file" && !File.Exists(uri.AbsolutePath))
                {
                    if (!string.IsNullOrEmpty(fallback))
                        LoadImage(fallback, null, setter);
                    return;
                }

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = uri;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                Application.Current.Dispatcher.Invoke(() => setter(bitmapImage));
            }
            catch
            {
                // Silently fail if the image can not be loaded
            }
        }
    }
}
