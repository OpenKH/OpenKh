using OpenKh.Common;
using System.Windows;

namespace OpenKh.Tools.ModsManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            Log.Close();
            base.OnExit(e);
        }
    }
}
