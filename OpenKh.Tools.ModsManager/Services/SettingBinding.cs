using System.Windows.Data;

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
