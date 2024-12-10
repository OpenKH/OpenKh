using System.Linq;
using System.Windows;

namespace OpenKh.Tools.ModsManager.Services
{
    public class GetActiveWindowService
    {
        public Window GetActiveWindow() => Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
    }
}
