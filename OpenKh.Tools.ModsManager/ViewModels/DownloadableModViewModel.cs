using OpenKh.Patcher;
using OpenKh.Tools.Common;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class DownloadableModViewModel : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        private readonly IChangeModEnableState _changeModEnableState;
        private string _author;
        private string _name;
        private string _game;
        private bool _isInstalling;
        private string _installStatus;
        private int _installProgress;
        private BitmapImage _iconImage;
        private BitmapImage _previewImage;
        private string _repo;
        private List<ModViewModel> _installedMods;
        private string _description;

        public RelayCommand InstallCommand { get; }

        public DownloadableModViewModel(DownloadableModModel model, IChangeModEnableState changeModEnableState)
        {
            Model = model;
            _changeModEnableState = changeModEnableState;
            Name = model?.Name ?? "Unknown";
            Author = model?.Author ?? "Unknown";
            Description = model?.Description ?? "No description available";
            Game = model?.Game ?? "Unknown";
            Repo = model?.Repository ?? "";
            
            // Obtener imágenes de iconos y previews de forma segura
            LoadImages();
            
            InstalledMods = new List<ModViewModel>();
            InstallCommand = new RelayCommand(_ => Install(_), _ => !IsInstalling);
        }

        private async void LoadImages()
        {
            try
            {
                string iconUrl = DownloadableModsService.GetIconUrl(Repo);
                var iconImage = await DownloadableModsService.GetImageFromUrl(iconUrl);
                
                // Usamos Application.Current.Dispatcher para actualizar la UI de forma segura
                Application.Current.Dispatcher.Invoke(() => 
                {
                    IconImage = iconImage;
                });
                
                string previewUrl = DownloadableModsService.GetPreviewUrl(Repo);
                var previewImage = await DownloadableModsService.GetImageFromUrl(previewUrl);
                
                Application.Current.Dispatcher.Invoke(() => 
                {
                    PreviewImage = previewImage;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading images: {ex.Message}");
            }
        }

        public DownloadableModViewModel(
            DownloadableModModel metadata,
            BitmapImage iconImage,
            BitmapImage previewImage,
            string repo,
            string game,
            IEnumerable<ModViewModel> installedMods)
        {
            Model = metadata;
            Name = metadata?.Name ?? "Unknown";
            Author = metadata?.Author ?? "Unknown";
            Game = game;
            Repo = repo;
            IconImage = iconImage;
            PreviewImage = previewImage;
            InstalledMods = installedMods?.ToList() ?? new List<ModViewModel>();
            InstallCommand = new RelayCommand(_ => Install(_), _ => !IsInstalling);
        }

        public BitmapImage IconImage
        {
            get => _iconImage;
            set
            {
                _iconImage = value;
                OnPropertyChanged();
            }
        }

        public BitmapImage PreviewImage
        {
            get => _previewImage;
            set
            {
                _previewImage = value;
                OnPropertyChanged();
            }
        }

        public string Name 
        { 
            get => _name; 
            set 
            { 
                _name = value; 
                OnPropertyChanged(); 
            } 
        }

        public string Author 
        { 
            get => _author; 
            set 
            { 
                _author = value; 
                OnPropertyChanged(); 
            } 
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public string Repository => Model.Repository;
        public string Game 
        { 
            get => _game; 
            set 
            { 
                _game = value; 
                OnPropertyChanged(); 
            } 
        }
        public DownloadableModModel Model { get; }
        public Visibility UpdateVisibility => Model.UpdateVisibility;

        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                _isInstalling = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(InstallStatusVisibility));
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public string InstallStatus
        {
            get => _installStatus;
            set
            {
                _installStatus = value;
                OnPropertyChanged();
            }
        }

        public int InstallProgress
        {
            get => _installProgress;
            set
            {
                _installProgress = value;
                OnPropertyChanged();
            }
        }

        public Visibility InstallStatusVisibility => !string.IsNullOrEmpty(InstallStatus) ? Visibility.Visible : Visibility.Collapsed;

        public string Repo
        {
            get => _repo;
            set
            {
                _repo = value;
                OnPropertyChanged();
            }
        }

        public List<ModViewModel> InstalledMods
        {
            get => _installedMods;
            set
            {
                _installedMods = value;
                OnPropertyChanged();
            }
        }

        private async void Install(object param)
        {
            try
            {
                IsInstalling = true;
                InstallStatus = "Installing...";
                InstallProgress = 0;
                
                // Reset image caches
                _iconImage = null;
                _previewImage = null;
                
                var success = await DownloadableModsService.InstallMod(Repository, status => 
                {
                    InstallStatus = status;
                    
                    // Intenta interpretar el estado para determinar el progreso
                    if (status.Contains("%"))
                    {
                        try {
                            string percentStr = status.Split('%')[0].Trim();
                            if (percentStr.Contains(" "))
                                percentStr = percentStr.Split(' ').Last();
                            if (int.TryParse(percentStr, out int percent))
                                InstallProgress = percent;
                        }
                        catch (Exception) { /* Ignorar errores de parsing */ }
                    }
                });
                
                if (success)
                {
                    InstallStatus = "Installation successful!";
                    // Refresh mod list
                    _changeModEnableState?.ModEnableStateChanged();
                    
                    // Explicitly call methods to refresh both lists
                    if (_changeModEnableState is MainViewModel mainViewModel)
                    {
                        mainViewModel.ReloadModsList();
                        mainViewModel.RefreshDownloadableMods();
                    }
                    
                    // Notify UI about image property changes
                    OnPropertyChanged(nameof(IconImage));
                    OnPropertyChanged(nameof(PreviewImage));
                }
                else
                {
                    InstallStatus = "Installation failed. Please try again.";
                }
                
                // Clear the status after 3 seconds
                await Task.Delay(3000);
                InstallStatus = null;
            }
            catch (Exception ex)
            {
                InstallStatus = $"Error: {ex.Message}";
                await Task.Delay(3000);
                InstallStatus = null;
            }
            finally
            {
                IsInstalling = false;
            }
        }
    }
}
