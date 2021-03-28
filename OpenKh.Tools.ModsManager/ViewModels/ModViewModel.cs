using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using static OpenKh.Tools.ModsManager.Helpers;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class ModViewModel : BaseNotifyPropertyChanged
    {
        private static readonly string FallbackImage = null;
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

            ReadMetadata();
            if (Title != null)
                Name = Title;

            UpdateCommand = new RelayCommand(async _ =>
            {
                InstallModProgressWindow progressWindow = null;
                try
                {
                    progressWindow = Application.Current.Dispatcher.Invoke(() =>
                    {
                        var progressWindow = new InstallModProgressWindow
                        {
                            OperationName = "Updating",
                            ModName = Source,
                            ProgressText = "Initializing",
                            ShowActivated = true
                        };
                        progressWindow.Show();
                        return progressWindow;
                    });

                    await ModsService.Update(Source, progress =>
                    {
                        Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressText = progress);
                    }, nProgress =>
                    {
                        Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressValue = nProgress);
                    });

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressWindow.ProgressText = "Reading latest changes";
                        progressWindow.ProgressValue = 1f;
                    });

                    var mod = ModsService.GetMods(new string[] { Source }).First();
                    ReadMetadata();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        progressWindow.Close();
                    });
                }
                catch (Exception ex)
                {
                    Handle(ex);
                }
                finally
                {
                    Application.Current.Dispatcher.Invoke(() => progressWindow?.Close());
                }
            });
        }

        public RelayCommand UpdateCommand { get; }

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

        public string Title => _model?.Metadata?.Title ?? Name;
        public string Name { get; }
        public string Author { get; }
        public string Source => _model.Name;
        public string AuthorUrl => $"https://github.com/{Author}";
        public string SourceUrl => $"https://github.com/{Source}";
        public string ReportBugUrl => $"https://github.com/{Source}/issues";
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

        private void ReadMetadata() => Task.Run(() =>
        {
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(Homepage));
                OnPropertyChanged(nameof(FilesToPatch));
                UpdateCount = 0;
            });
        });

        private static void LoadImage(string source, string fallback, Action<ImageSource> setter)
        {
            if (string.IsNullOrEmpty(source) && !string.IsNullOrEmpty(fallback))
            {
                LoadImage(fallback, null, setter);
                return;
            }

            try
            {
                if (!File.Exists(source))
                {
                    if (!string.IsNullOrEmpty(fallback))
                        LoadImage(fallback, null, setter);
                    return;
                }

                using (var fs = new FileStream(source, FileMode.Open))
                {
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = fs;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();

                    Application.Current.Dispatcher.Invoke(() => setter(bitmapImage));
                }
            }
            catch
            {
                // Silently fail if the image can not be loaded
            }
        }
    }
}
