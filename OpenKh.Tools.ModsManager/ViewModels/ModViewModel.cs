using OpenKh.Common;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public interface IChangeCollectionModEnableState
    {
        void CollectionModEnableStateChanged();
    }
    public class ModViewModel : BaseNotifyPropertyChanged, IChangeCollectionModEnableState
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        private static readonly string FallbackImage = null;
        private readonly ModModel _model;
        private CollectionSettingsViewModel _selectedCollectionValue;
        private readonly IChangeModEnableState _changeModEnableState;
        private int _updateCount;
        public ObservableCollection<CollectionSettingsViewModel> CollectionModsList { get; set; }

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
                Author = _model.Metadata?.OriginalAuthor;
                Name = Source;
            }

            ReadMetadata();
            if (IsCollection)
            {
                ReloadCollectionModsList();
                CollectionSelectedValue = CollectionModsList.FirstOrDefault();
            }

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
                    Log.Warn("Unable to update the mod `{0}`: {1}\n"
                        , Source
                        , Log.FormatSecondaryLinesWithIndent(ex.ToString(), "  ")
                    );
                    Handle(ex);
                }
                finally
                {
                    Application.Current.Dispatcher.Invoke(() => progressWindow?.Close());
                }
            });

            CollectionSettingsCommand = new RelayCommand(_ =>
            {
                var view = new CollectionSettingsView();
                view.DataContext = this;
                if (view.ShowDialog() != true)
                {
                    if (!ConfigurationService.EnabledCollectionMods.ContainsKey(_model.Name))
                    {
                        var temp = ConfigurationService.EnabledCollectionMods;
                        temp[_model.Name] = new Dictionary<string, bool> { };
                        ConfigurationService.EnabledCollectionMods = temp;
                    }
                    _model.CollectionOptionalEnabledAssets = ConfigurationService.EnabledCollectionMods[_model.Name];
                    FilesToPatch = string.Join('\n', GetFilesToPatch());
                    return;
                }
            });
        }

        public RelayCommand UpdateCommand { get; }

        public RelayCommand CollectionSettingsCommand { get; set; }

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

        public CollectionSettingsViewModel CollectionSelectedValue
        {
            get => _selectedCollectionValue;
            set
            {
                _selectedCollectionValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsModSelected));
                OnPropertyChanged(nameof(IsModUnselectedMessageVisible));
            }
        }

        public bool IsModSelected => CollectionSelectedValue != null;
        public ImageSource IconImage { get; private set; }
        public ImageSource PreviewImage { get; private set; }
        public Visibility PreviewImageVisibility => PreviewImage != null ? Visibility.Visible : Visibility.Collapsed;

        public bool IsHosted => _model.Name.Contains('/');
        public bool IsCollection => _model.Metadata.IsCollection;
        public string Path => _model.Path;
        public Visibility SourceVisibility => IsHosted ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LocalVisibility => !IsHosted ? Visibility.Visible : Visibility.Collapsed;
        public Visibility CollectionSettingsVisibility => IsCollection ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsModUnselectedMessageVisible => !IsModSelected ? Visibility.Visible : Visibility.Collapsed;

        public string Title => _model?.Metadata?.Title ?? Name;
        public string Name { get; }
        public string Author { get; }
        public string Source => _model.Name;
        public string AuthorUrl => $"https://github.com/{Author}";
        public string SourceUrl => $"https://github.com/{Source}";
        public string ReportBugUrl => $"https://github.com/{Source}/issues";
        public string FilesToPatch
        {
            get
            {
                return string.Join('\n', GetFilesToPatch());
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public string Description => _model.Metadata?.Description;

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
            foreach (var asset in _model.Metadata?.Assets ?? Enumerable.Empty<Patcher.AssetFile>())
            {
                var isOptionalEnabled = false;
                if (asset.CollectionOptional == true)
                {
                    _model.CollectionOptionalEnabledAssets.TryGetValue(asset.Name, out isOptionalEnabled);
                    if (!isOptionalEnabled)
                        continue;
                }
                if (_model.Metadata.IsCollection)
                    if (asset.Game != ConfigurationService.LaunchGame)
                        continue;
                yield return isOptionalEnabled ? $"{asset.Name} (optional, enabled)" : asset.Name;
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

        private void ReloadCollectionModsList()
        {
            CollectionModsList = new ObservableCollection<CollectionSettingsViewModel>(
                ModsService.GetCollectionOptionalMods(_model).Select(Map));
            OnPropertyChanged(nameof(CollectionModsList));
        }
        public void CollectionModEnableStateChanged()
        {
            var mods = CollectionModsList.ToList();
            var current = new Dictionary<string, bool> { };
            var holder = ConfigurationService.EnabledCollectionMods;
            if (holder.ContainsKey(_model.Name))
            {
                current = holder[_model.Name];
                foreach (KeyValuePair<string, bool> entry in current)
                    foreach (var mod in mods)
                        if (mod.Name == entry.Key)
                        {
                            current[mod.Name] = mod.Enabled;
                        }
            }
            else
            {
                holder[_model.Name] = new Dictionary<string, bool> { };
                current = holder[_model.Name];
                foreach (var mod in mods)
                {
                    current[mod.Name] = mod.Enabled;
                }
            }
            holder[_model.Name] = current;
            ConfigurationService.EnabledCollectionMods = holder;
        }
        private CollectionSettingsViewModel Map(CollectionModModel mod) => new (mod, this);

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
