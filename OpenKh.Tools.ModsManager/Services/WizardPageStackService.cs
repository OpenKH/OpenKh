using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Xceed.Wpf.Toolkit;

namespace OpenKh.Tools.ModsManager.Services
{
    public class WizardPageStackService : DependencyObject
    {
        public static readonly DependencyProperty BackProperty = DependencyProperty.Register(
            "Back",
            typeof(WizardPage),
            typeof(WizardPageStackService),
            new PropertyMetadata(null)
        );

        private readonly List<WizardPage> _pages = new List<WizardPage>();

        public WizardPage Back
        {
            get => (WizardPage)GetValue(BackProperty);
            set => SetValue(BackProperty, value);
        }

        internal void OnPageChanged(WizardPage page)
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
