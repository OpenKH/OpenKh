using OpenKh.Tools.ModsManager.Models;
using System.Windows;
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

        public bool IsHosted => _model.Name.Contains('/');
        public string Path => _model.Path;
        public Visibility SourceVisibility => IsHosted ? Visibility.Visible : Visibility.Collapsed;
        public Visibility LocalVisibility => !IsHosted ? Visibility.Visible : Visibility.Collapsed;

        public string Title => _model.Metadata.Title;
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
    }
}
