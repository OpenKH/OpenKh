using OpenKh.Tools.ModsManager.Models;
using OpenKh.Tools.ModsManager.Services;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.ViewModels
{
    public class CollectionSettingsViewModel : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        private readonly CollectionModModel _model;
        private readonly IChangeCollectionModEnableState _changeModEnableState;
        public CollectionSettingsViewModel(CollectionModModel model, IChangeCollectionModEnableState changeModEnableState)
        {
            _model = model;
            _changeModEnableState = changeModEnableState;
        }
        public string Name => _model.Name;
        public string Author => _model.Author;
        public bool Enabled
        {
            get => _model.IsEnabled;
            set
            {
                _model.IsEnabled = value;
                _changeModEnableState.CollectionModEnableStateChanged();
                OnPropertyChanged();
            }
        }

    }
}
