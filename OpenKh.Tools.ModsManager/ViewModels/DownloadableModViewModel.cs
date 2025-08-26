using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.Views;
using System;
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
        private readonly DownloadableModModel _model;
        private bool _isInstalling;

        // Event to notify when mod installation is complete
        public event Action<DownloadableModViewModel> ModInstalled;

        public DownloadableModViewModel(DownloadableModModel model)
        {
            _model = model;
            InstallCommand = new RelayCommand(_ => InstallMod(), _ => !IsInstalling);
        }

        public string Repo => _model.Repo;
        public string Title => _model.Title;
        public string Author => _model.OriginalAuthor;
        public string Description => _model.Description;
        public string Game => _model.Game;
        public BitmapImage IconImage => _model.IconImage;
        public BitmapImage ScreenshotImage => _model.ScreenshotImageSource;
        public string RepoUrl => $"https://github.com/{_model.Repo}";

        public ICommand InstallCommand { get; }

        public bool IsInstalling
        {
            get => _isInstalling;
            set
            {
                _isInstalling = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private async void InstallMod()
        {
            if (IsInstalling)
                return;

            IsInstalling = true;
            InstallModProgressWindow progressWindow = null;
            try
            {
                // Create and show the progress window
                progressWindow = Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = new InstallModProgressWindow
                    {
                        ModName = Repo,
                        ProgressText = "Initializing",
                        ShowActivated = true
                    };
                    window.Show();
                    return window;
                });

                // Install mod with progress updates
                await Task.Run(() => ModsService.InstallModFromGithub(Repo,
                    progress => {
                        Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressText = progress);
                    },
                    nProgress => {
                        Application.Current.Dispatcher.Invoke(() => progressWindow.ProgressValue = nProgress);
                    }
                ));

                // Notify that the mod was successfully installed
                ModInstalled?.Invoke(this);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error installing mod {Title}: {ex.Message}",
                    "Error",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => progressWindow?.Close());
                IsInstalling = false;
            }
        }
    }
}
