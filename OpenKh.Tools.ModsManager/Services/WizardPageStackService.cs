using System.Collections.Generic;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.Services
{
    // Tracks the visited wizard pages so "previous" follows the path the user
    // actually took through the branching wizard. Pages are opaque tokens
    // (WPF passes Xceed WizardPage instances, Avalonia its own page controls),
    // which keeps this service and the SetupWizardViewModel UI-framework
    // agnostic.
    public class WizardPageStackService : BaseNotifyPropertyChanged
    {
        private readonly List<object> _pages = new List<object>();
        private object _back;

        public object Back
        {
            get => _back;
            set
            {
                _back = value;
                OnPropertyChanged();
            }
        }

        public void OnPageChanged(object page)
        {
            int found = _pages.IndexOf(page);
            if (found != -1)
            {
                _pages.RemoveRange(found + 1, _pages.Count - (found + 1));
            }
            else
            {
                _pages.Add(page);
            }

            Back = (_pages.Count <= 1) ? null : _pages[_pages.Count - 2];
        }
    }
}
