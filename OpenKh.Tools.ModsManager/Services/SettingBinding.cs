using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

using OpenKh.Tools.ModsManager;

namespace OpenKh.Tools.ModsManager.Services
{
    public class SettingBinding : Binding
    {
        public SettingBinding()
        {
            Initialize();
        }

        public SettingBinding(string path)
            : base(path)
        {
            Initialize();
        }

        private void Initialize()
        {
            this.Source = WinSettings.Default;
            this.Mode = BindingMode.TwoWay;
        }
    }
}
