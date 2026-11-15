using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModManager.ViewModels
{
    public partial class WizardViewModel : ViewModelBase
    {
        [ObservableProperty]
        private object? _currentWizardPage;

        [ObservableProperty]
        private ObservableCollection<object?> _pastWizardPages;

        [ObservableProperty]
        private ObservableCollection<object?> _futureWizardPages;
    }
}
